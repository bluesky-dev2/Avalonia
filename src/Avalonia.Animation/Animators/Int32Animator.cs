﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;

namespace Avalonia.Animation.Animators
{
    /// <summary>
    /// Animator that handles <see cref="Int32"/> properties.
    /// </summary>
    public class Int32Animator : Animator<Int32>
    {
        static double maxVal = (double)Int32.MaxValue;

        /// <inheritdocs/>
        public override Int32 Interpolate(double progress, Int32 oldValue, Int32 newValue)
        {
            var normOV = oldValue / maxVal;
            var normNV = newValue / maxVal;
            var deltaV = normNV - normOV;
            return (Int32)Math.Round(maxVal * ((deltaV * progress) + normOV));
        }
    }
}
