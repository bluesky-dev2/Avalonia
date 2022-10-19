﻿using System;

namespace Avalonia.Platform
{
    /// <summary>
    /// Represents a single display screen.
    /// </summary>
    public class Screen
    {
        /// <summary>
        /// Gets the scaling factor applied to the screen by the operating system.
        /// </summary>
        /// <remarks>
        /// Multiply this value by 100 to get a percentage.
        /// Both X and Y scaling factors are assumed uniform.
        /// </remarks>
        public double Scale { get; }

        /// <inheritdoc cref="Scale"/>
        [Obsolete("Use the Scale property instead.")]
        public double PixelDensity => Scale;

        /// <summary>
        /// Gets the overall pixel-size of the screen.
        /// </summary>
        /// <remarks>
        /// This generally is the raw pixel counts in both the X and Y direction.
        /// </remarks>
        public PixelRect Bounds { get; }

        /// <summary>
        /// Gets the actual working-area pixel-size of the screen.
        /// </summary>
        /// <remarks>
        /// This area may be smaller than <see href="Bounds"/> to account for notches and
        /// other block-out areas such as taskbars etc.
        /// </remarks>
        public PixelRect WorkingArea { get; }

        /// <summary>
        /// Gets a value indicating whether the screen is the primary one.
        /// </summary>
        public bool IsPrimary { get; }

        /// <inheritdoc cref="IsPrimary"/>
        [Obsolete("Use the IsPrimary property instead.")]
        public bool Primary => IsPrimary;

        public Screen(double scale, PixelRect bounds, PixelRect workingArea, bool isPrimary)
        {
            this.Scale = scale;
            this.Bounds = bounds;
            this.WorkingArea = workingArea;
            this.IsPrimary = isPrimary;
        } 
    }
}
