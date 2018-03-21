// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

namespace Avalonia.Animation
{
    /// <summary>
    /// Eases out a <see cref="double"/> value 
    /// using a quartic equation.
    /// </summary>
    public class QuarticEaseOut : Easing
    {
        /// <inheritdoc/>
        public override double Ease(double progress)
        {
            double f = (progress - 1d);
            return f * f * f * (1d - progress) + 1d;
        }

    }
}
