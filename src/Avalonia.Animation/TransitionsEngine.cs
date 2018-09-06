// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using Avalonia.Metadata;
using System;
using System.Reactive.Linq;
using Avalonia.Animation.Easings;
using Avalonia.Animation.Utils;
using Avalonia.Reactive;

namespace Avalonia.Animation
{
    /// <summary>
    /// Handles the timing and lifetime of a <see cref="Transition{T}"/>.
    /// </summary>
    public class TransitionsEngine : SingleSubscriberObservableBase<double>
    {
        private IDisposable timerSubscription;
        private readonly TimeSpan startTime;
        private readonly TimeSpan duration;

        public TransitionsEngine(TimeSpan Duration)
        {
            startTime = Timing.GetTickCount();
            duration = Duration;
        }

        private void TimerTick(TimeSpan t)
        {
            var interpVal = (double)(t.Ticks - startTime.Ticks) / duration.Ticks;

            if (interpVal > 1d
             || interpVal < 0d)
            {
                PublishCompleted();
                return;
            }

            PublishNext(interpVal);
        }
  
        protected override void Unsubscribed()
        {
            timerSubscription?.Dispose();
        }

        protected override void Subscribed()
        {
            timerSubscription = Timing
                                .AnimationsTimer
                                .Subscribe(t => TimerTick(t));
        }
    }
}