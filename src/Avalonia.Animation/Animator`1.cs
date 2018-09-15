﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Animation.Utils;
using Avalonia.Collections;
using Avalonia.Data;
using Avalonia.Reactive;

namespace Avalonia.Animation
{
    /// <summary>
    /// Base class for KeyFrames objects
    /// </summary>
    public abstract class Animator<T> : AvaloniaList<AnimatorKeyFrame>, IAnimator
    {
        /// <summary>
        /// List of type-converted keyframes.
        /// </summary>
        private readonly List<AnimatorKeyFrame> _convertedKeyframes = new List<AnimatorKeyFrame>();

        private bool _isVerifiedAndConverted;

        /// <summary>
        /// Gets or sets the target property for the keyframe.
        /// </summary>
        public AvaloniaProperty Property { get; set; }

        public Animator()
        {
            // Invalidate keyframes when changed.
            this.CollectionChanged += delegate { _isVerifiedAndConverted = false; };
        }

        /// <inheritdoc/>
        public virtual IDisposable Apply(Animation animation, Animatable control, IObservable<bool> match, Action onComplete)
        {
            if (!_isVerifiedAndConverted)
                VerifyConvertKeyFrames();

            var subject = new DisposeAnimationInstanceSubject<T>(this, animation, control, onComplete);
            return match.Subscribe(subject);
        }

        /// <summary>
        /// Get the nearest pair of cue-time ordered keyframes 
        /// according to the given time parameter that is relative to the
        /// total animation time and the normalized intra-keyframe pair time 
        /// (i.e., the normalized time between the selected keyframes, relative to the
        /// time parameter).
        /// </summary>
        /// <param name="t">The time parameter, relative to the total animation time</param>
        protected (double IntraKFTime, KeyFramePair<T> KFPair) GetKFPairAndIntraKFTime(double t)
        {
            AnimatorKeyFrame firstCue, lastCue;
            int kvCount = _convertedKeyframes.Count;
            if (kvCount > 2)
            {
                if (t <= 0.0)
                {
                    firstCue = _convertedKeyframes[0];
                    lastCue = _convertedKeyframes[1];
                }
                else if (t >= 1.0)
                {
                    firstCue = _convertedKeyframes[_convertedKeyframes.Count - 2];
                    lastCue = _convertedKeyframes[_convertedKeyframes.Count - 1];
                }
                else
                {
                    (double time, int index) maxval = (0.0d, 0);
                    for (int i = 0; i < _convertedKeyframes.Count; i++)
                    {
                        var comp = _convertedKeyframes[i].Cue.CueValue;
                        if (t >= comp)
                        {
                            maxval = (comp, i);
                        }
                    }
                    firstCue = _convertedKeyframes[maxval.index];
                    lastCue = _convertedKeyframes[maxval.index + 1];
                }
            }
            else
            {
                firstCue = _convertedKeyframes[0];
                lastCue = _convertedKeyframes[1];
            }

            double t0 = firstCue.Cue.CueValue;
            double t1 = lastCue.Cue.CueValue;
            var intraframeTime = (t - t0) / (t1 - t0);
            var firstFrameData = (firstCue.GetTypedValue<T>(), firstCue.isNeutral);
            var lastFrameData = (lastCue.GetTypedValue<T>(), lastCue.isNeutral);
            return (intraframeTime, new KeyFramePair<T>(firstFrameData, lastFrameData));
        }
 
        internal IDisposable Run(Animation animation, Animatable control, Action onComplete)
        {
            var instance = new AnimationInstance<T>(animation, control, this, onComplete, DoInterpolation);
            return control.Bind<T>((AvaloniaProperty<T>)Property, instance, BindingPriority.Animation);
        }

        /// <summary>
        /// Interpolates a value given the desired time.
        /// </summary>
        protected abstract T DoInterpolation(double time, T neutralValue);

        /// <summary>
        /// Verifies, converts and sorts keyframe values according to this class's target type.
        /// </summary>
        private void VerifyConvertKeyFrames()
        {
            foreach (AnimatorKeyFrame keyframe in this)
            {
                _convertedKeyframes.Add(keyframe);
            }

            AddNeutralKeyFramesIfNeeded();

            var copy = _convertedKeyframes.ToList().OrderBy(p => p.Cue.CueValue);
            _convertedKeyframes.Clear();

            foreach (AnimatorKeyFrame keyframe in copy)
            {
                _convertedKeyframes.Add(keyframe);
            }

            _isVerifiedAndConverted = true;
        }

        private void AddNeutralKeyFramesIfNeeded()
        {
            bool hasStartKey, hasEndKey;
            hasStartKey = hasEndKey = false;

            // Check if there's start and end keyframes.
            foreach (var frame in _convertedKeyframes)
            {
                if (frame.Cue.CueValue == 0.0d)
                {
                    hasStartKey = true;
                }
                else if (frame.Cue.CueValue == 1.0d)
                {
                    hasEndKey = true;
                }
            }

            if (!hasStartKey || !hasEndKey)
                AddNeutralKeyFrames(hasStartKey, hasEndKey);
        }

        private void AddNeutralKeyFrames(bool hasStartKey, bool hasEndKey)
        {
            if (!hasStartKey)
            {
                _convertedKeyframes.Add(new AnimatorKeyFrame(null, new Cue(0.0d)) { Value = default(T), isNeutral = true });
            }

            if (!hasEndKey)
            {
                _convertedKeyframes.Add(new AnimatorKeyFrame(null, new Cue(1.0d)) { Value = default(T), isNeutral = true });
            }
        }
    }
}