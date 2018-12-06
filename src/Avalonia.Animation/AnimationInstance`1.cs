using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Animation.Utils;
using Avalonia.Data;
using Avalonia.Reactive;

namespace Avalonia.Animation
{
    /// <summary>
    /// Handles interpolation and time-related functions 
    /// for keyframe animations.
    /// </summary>
    internal class AnimationInstance<T> : SingleSubscriberObservableBase<T>
    {
        private T _lastInterpValue;
        private T _firstKFValue;
        private ulong? _iterationCount;
        private ulong _currentIteration;
        private bool _gotFirstKFValue;
        private FillMode _fillMode;
        private PlaybackDirection _animationDirection;
        private Animator<T> _parent;
        private Animatable _targetControl;
        private T _neutralValue;
        private double _speedRatio;
        private TimeSpan _initialDelay;
        private TimeSpan _iterationDelay;
        private TimeSpan _duration;
        private Easings.Easing _easeFunc;
        private Action _onCompleteAction;
        private Func<double, T, T> _interpolator;
        private IDisposable _timerSub, _speedRatioSub;
        private readonly IClock _baseClock;
        private IClock _clock;

        public AnimationInstance(Animation animation, Animatable control, Animator<T> animator, IClock baseClock, Action OnComplete, Func<double, T, T> Interpolator)
        {
            if (animation.SpeedRatio <= 0)
                throw new InvalidOperationException("Speed ratio cannot be negative or zero.");

            if (animation.Duration.TotalSeconds <= 0)
                throw new InvalidOperationException("Duration cannot be negative or zero.");

            _parent = animator;
            _easeFunc = animation.Easing;
            _targetControl = control;
            _neutralValue = (T)_targetControl.GetValue(_parent.Property);

            _speedRatioSub = animation.GetObservable(Animation.SpeedRatioProperty)
                                      .Subscribe(p => _speedRatio = p);

            _initialDelay = animation.Delay;
            _duration = animation.Duration;
            _iterationDelay = animation.DelayBetweenIterations;

            if (animation.IterationCount.RepeatType == IterationType.Many)
                _iterationCount = animation.IterationCount.Value;

            _animationDirection = animation.PlaybackDirection;
            _fillMode = animation.FillMode;
            _onCompleteAction = OnComplete;
            _interpolator = Interpolator;
            _baseClock = baseClock;
        }

        protected override void Unsubscribed()
        {
            // Animation may have been stopped before it has finished.
            ApplyFinalFill();

            _timerSub?.Dispose();
            _speedRatioSub?.Dispose();
            _clock.PlayState = PlayState.Stop;
        }

        protected override void Subscribed()
        {
            _clock = new Clock(_baseClock);
            _timerSub = _clock.Subscribe(Step);
        }

        public void Step(TimeSpan frameTick)
        {
            try
            {
                InternalStep(frameTick);
            }
            catch (Exception e)
            {
                PublishError(e);
            }
        }

        private void ApplyFinalFill()
        {
            if (_fillMode == FillMode.Forward || _fillMode == FillMode.Both)
                _targetControl.SetValue(_parent.Property, _lastInterpValue, BindingPriority.LocalValue);
        }

        private void DoComplete()
        {
            ApplyFinalFill();
            _onCompleteAction?.Invoke();
            PublishCompleted();
        }

        private void DoDelay()
        {
            if (_fillMode == FillMode.Backward || _fillMode == FillMode.Both)
                if (_currentIteration == 0)
                    PublishNext(_firstKFValue);
                else
                    PublishNext(_lastInterpValue);
        }

        private void DoPlayStates()
        {
            if (_clock.PlayState == PlayState.Stop || _baseClock.PlayState == PlayState.Stop)
                DoComplete();

            if (!_gotFirstKFValue)
            {
                _firstKFValue = (T)_parent.First().Value;
                _gotFirstKFValue = true;
            }
        }

        private void InternalStep(TimeSpan time)
        {
            DoPlayStates();

            // Scale timebases according to speedratio.
            var indexTime = time.Ticks;
            var iterDuration = _duration.Ticks * _speedRatio;
            var iterDelay = _iterationDelay.Ticks * _speedRatio;
            var initDelay = _initialDelay.Ticks * _speedRatio;

            if (indexTime > 0 & indexTime <= initDelay)
            {
                DoDelay();
            }
            else
            {
                // Calculate timebases.
                var iterationTime = iterDuration + iterDelay;
                var opsTime = indexTime - initDelay;
                var playbackTime = opsTime % iterationTime;

                _currentIteration = (ulong)(opsTime / iterationTime);

                // Stop animation when the current iteration is beyond the iteration count.
                if ((_currentIteration + 1) > _iterationCount)
                    DoComplete();

                if (playbackTime <= iterDuration)
                {
                    // Normalize time for interpolation.
                    var normalizedTime = playbackTime / iterDuration;

                    // Check if normalized time needs to be reversed.
                    bool isCurIterReverse = _animationDirection == PlaybackDirection.Normal ? false :
                                            _animationDirection == PlaybackDirection.Alternate ? (_currentIteration % 2 == 0) ? false : true :
                                            _animationDirection == PlaybackDirection.AlternateReverse ? (_currentIteration % 2 == 0) ? true : false :
                                            _animationDirection == PlaybackDirection.Reverse ? true : false;
                    if (isCurIterReverse)
                        normalizedTime = 1 - normalizedTime;

                    // Ease and interpolate
                    var easedTime = _easeFunc.Ease(normalizedTime);
                    _lastInterpValue = _interpolator(easedTime, _neutralValue);

                    PublishNext(_lastInterpValue);
                }
                else if (playbackTime > iterDuration &
                         playbackTime <= iterationTime &
                         iterDelay > 0)
                {
                    // The last iteration's trailing delay should be skipped.
                    if ((_currentIteration + 1) < _iterationCount)
                        DoDelay();
                    else
                        DoComplete();
                }
            }
        }
    }
}