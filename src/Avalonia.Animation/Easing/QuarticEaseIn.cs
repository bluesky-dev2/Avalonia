// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

namespace Avalonia.Animation
{
    /// <summary>
    /// Eases in a <see cref="double"/> value 
    /// using a quartic equation.
    /// </summary>
    public class QuarticEaseIn : Easing
    {
        /// <inheritdoc/>
        public override double Ease(double progress)
        {
            return progress * progress * progress * progress;
            
        }

    }
}
