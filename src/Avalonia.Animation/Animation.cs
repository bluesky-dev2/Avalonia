// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Animation.Animators;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Metadata;

namespace Avalonia.Animation
{
    /// <summary>
    /// Tracks the progress of an animation.
    /// </summary>
    public class Animation : AvaloniaObject, IAnimation
    {
        /// <summary>
        /// Gets the children of the <see cref="Animation"/>.
        /// </summary>
        [Content]
        public KeyFrames Children { get; } = new KeyFrames();

        /// <summary>
        /// Gets or sets the active time of this animation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the repeat count for this animation.
        /// </summary>
        public IterationCount IterationCount { get; set; } = new IterationCount(1);

        /// <summary>
        /// Gets or sets the playback direction for this animation.
        /// </summary>
        public PlaybackDirection PlaybackDirection { get; set; }

        /// <summary>
        /// Gets or sets the value fill mode for this animation.
        /// </summary>
        public FillMode FillMode { get; set; }

        /// <summary>
        /// Gets or sets the easing function to be used for this animation.
        /// </summary>
        public Easing Easing { get; set; } = new LinearEasing();

        /// <summary> 
        /// Gets or sets the initial delay time for this animation. 
        /// </summary> 
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;

        /// <summary> 
        /// Gets or sets the delay time in between iterations.
        /// </summary> 
        public TimeSpan DelayBetweenIterations { get; set; } = TimeSpan.Zero;

        public static readonly DirectProperty<Animation, double> SpeedRatioProperty =
            AvaloniaProperty.RegisterDirect<Animation, double>(
                nameof(_speedRatio),
                o => o._speedRatio,
                (o, v) => o._speedRatio = v,
                1d);

        private double _speedRatio = 1d;

        /// <summary>
        /// Gets or sets the speed multiple for this animation.
        /// </summary> 
        public double SpeedRatio
        {
            get { return _speedRatio; }
            set { SetAndRaise(SpeedRatioProperty, ref _speedRatio, value); }
        }

        private readonly static List<(Func<AvaloniaProperty, bool> Condition, Type Animator)> Animators = new List<(Func<AvaloniaProperty, bool>, Type)>
        {
            ( prop => typeof(double).IsAssignableFrom(prop.PropertyType), typeof(DoubleAnimator) )
        };

        public static void RegisterAnimator<TAnimator>(Func<AvaloniaProperty, bool> condition)
            where TAnimator : IAnimator
        {
            Animators.Insert(0, (condition, typeof(TAnimator)));
        }

        private static Type GetAnimatorType(AvaloniaProperty property)
        {
            foreach (var (condition, type) in Animators)
            {
                if (condition(property))
                {
                    return type;
                }
            }
            return null;
        }

        private (IList<IAnimator> Animators, IList<IDisposable> subscriptions) InterpretKeyframes(Animatable control)
        {
            var handlerList = new List<(Type type, AvaloniaProperty property)>();
            var animatorKeyFrames = new List<AnimatorKeyFrame>();
            var subscriptions = new List<IDisposable>();

            foreach (var keyframe in Children)
            {
                foreach (var setter in keyframe.Setters)
                {
                    var handler = GetAnimatorType(setter.Property);

                    if (handler == null)
                    {
                        throw new InvalidOperationException($"No animator registered for the property {setter.Property}. Add an animator to the Animation.Animators collection that matches this property to animate it.");
                    }

                    if (!handlerList.Contains((handler, setter.Property)))
                        handlerList.Add((handler, setter.Property));

                    var cue = keyframe.Cue;

                    if (keyframe.TimingMode == KeyFrameTimingMode.TimeSpan)
                    {
                        cue = new Cue(keyframe.KeyTime.Ticks / Duration.Ticks);
                    }

                    var newKF = new AnimatorKeyFrame(handler, cue);

                    subscriptions.Add(newKF.BindSetter(setter, control));

                    animatorKeyFrames.Add(newKF);
                }
            }

            var newAnimatorInstances = new List<IAnimator>();

            foreach (var (handlerType, property) in handlerList)
            {
                var newInstance = (IAnimator)Activator.CreateInstance(handlerType);
                newInstance.Property = property;
                newAnimatorInstances.Add(newInstance);
            }

            foreach (var keyframe in animatorKeyFrames)
            {
                var animator = newAnimatorInstances.First(a => a.GetType() == keyframe.AnimatorType &&
                                                             a.Property == keyframe.Property);
                animator.Add(keyframe);
            }

            return (newAnimatorInstances, subscriptions);
        }

        /// <inheritdocs/>
        public IDisposable Apply(Animatable control, IClock clock, IObservable<bool> match, Action onComplete)
        {
            var (animators, subscriptions) = InterpretKeyframes(control);
            if (animators.Count == 1)
            {
                subscriptions.Add(animators[0].Apply(this, control, clock, match, onComplete));
            }
            else
            {
                var completionTasks = onComplete != null ? new List<Task>() : null;
                foreach (IAnimator animator in animators)
                {
                    Action animatorOnComplete = null;
                    if (onComplete != null)
                    {
                        var tcs = new TaskCompletionSource<object>();
                        animatorOnComplete = () => tcs.SetResult(null);
                        completionTasks.Add(tcs.Task);
                    }
                    subscriptions.Add(animator.Apply(this, control, clock, match, animatorOnComplete));
                }

                if (onComplete != null)
                {
                    Task.WhenAll(completionTasks).ContinueWith(_ => onComplete());
                }
            }
            return new CompositeDisposable(subscriptions);
        }

        /// <inheritdocs/>
        public Task RunAsync(Animatable control, IClock clock = null)
        {
            var run = new TaskCompletionSource<object>();

            if (this.IterationCount == IterationCount.Infinite)
                run.SetException(new InvalidOperationException("Looping animations must not use the Run method."));

            IDisposable subscriptions = null;
            subscriptions = this.Apply(control, clock, Observable.Return(true), () =>
            {
                run.SetResult(null);
                subscriptions?.Dispose();
            });

            return run.Task;
        }
    }
}
