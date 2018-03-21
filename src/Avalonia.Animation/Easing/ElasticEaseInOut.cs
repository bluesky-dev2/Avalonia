// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;

namespace Avalonia.Animation
{
    /// <summary>
    /// Eases a <see cref="double"/> value 
    /// using a piecewise damped sine function.
    /// </summary>
    public class ElasticEaseInOut : Easing
    {
        /// <inheritdoc/>
        public override double Ease(double progress)
        {
            double p = progress;

            if (p < 0.5d)
            {
                return 0.5d * Math.Sin(13d * EasingConstants.HALFPI * (2d * p)) * Math.Pow(2d, 10d * ((2d * p) - 1d));
            }
            else
            {
                return 0.5d * (Math.Sin(-13d * EasingConstants.HALFPI * ((2d * p - 1d) + 1d)) * Math.Pow(2d, -10d * (2d * p - 1d)) + 2d);
            }            
        }

    }
}
