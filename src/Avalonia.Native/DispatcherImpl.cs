#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using Avalonia.Native.Interop;
using Avalonia.Threading;
using MicroCom.Runtime;

namespace Avalonia.Native;

internal class DispatcherImpl : IControlledDispatcherImpl, IDispatcherClock
{
    private readonly IAvnPlatformThreadingInterface _native;
    private Thread? _loopThread;
    private Stack<RunLoopFrame> _managedFrames = new();

    public DispatcherImpl(IAvnPlatformThreadingInterface native)
    {
        _native = native;
        using var events = new Events(this);
        _native.SetEvents(events);
    }
    
    public event Action Signaled;
    public event Action Timer;
    
    private class Events : NativeCallbackBase, IAvnPlatformThreadingInterfaceEvents
    {
        private readonly DispatcherImpl _parent;

        public Events(DispatcherImpl parent)
        {
            _parent = parent;
        }
        public void Signaled() => _parent.Signaled?.Invoke();

        public void Timer() => _parent.Timer?.Invoke();
    }

    public bool CurrentThreadIsLoopThread
    {
        get
        {
            if (_loopThread != null)
                return Thread.CurrentThread == _loopThread;
            if (_native.CurrentThreadIsLoopThread == 0)
                return false;
            _loopThread = Thread.CurrentThread;
            return true;
        }
    }

    public void Signal() => _native.Signal();

    public void UpdateTimer(int? dueTimeInTicks)
    {
        var ms = dueTimeInTicks == null ? -1 : Math.Max(1, dueTimeInTicks.Value - TickCount);
        _native.UpdateTimer(ms);
    }

    public bool CanQueryPendingInput => true;
    public bool HasPendingInput => _native.HasPendingInput() != 0;

    class RunLoopFrame : IDisposable
    {
        public ExceptionDispatchInfo? Exception;
        public CancellationTokenSource CancellationTokenSource = new();

        public RunLoopFrame(CancellationToken token)
        {
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        }

        public void Dispose() => CancellationTokenSource.Dispose();
    }
    
    public void RunLoop(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        object l = new();
        var exited = false;
        
        using var frame = new RunLoopFrame(token);
        
        using var cancel = _native.CreateLoopCancellation();
        frame.CancellationTokenSource.Token.Register(() =>
        {
            lock (l)
                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once AccessToDisposedClosure
                if (!exited)
                    cancel.Cancel();
        });
        
        try
        {
            _managedFrames.Push(frame);
            _native.RunLoop(cancel);
        }
        finally
        {
            lock (l)
                exited = true;
            _managedFrames.Pop();
            if (frame.Exception != null)
                frame.Exception.Throw();
        }
    }

    public int TickCount => Environment.TickCount;

    public void PropagateCallbackException(ExceptionDispatchInfo capture)
    {
        if (_managedFrames.Count == 0)
        {
            Debug.Assert(false, "We should never get here");
            return;
        }

        var frame = _managedFrames.Peek();
        frame.Exception = capture;
        frame.CancellationTokenSource.Cancel();
    }
}
