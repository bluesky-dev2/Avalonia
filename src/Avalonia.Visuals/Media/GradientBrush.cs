// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Collections;
using Avalonia.Metadata;

namespace Avalonia.Media
{
    /// <summary>
    /// Base class for brushes that draw with a gradient.
    /// </summary>
    public abstract class GradientBrush : Brush, IGradientBrush
    {
        /// <summary>
        /// Defines the <see cref="SpreadMethod"/> property.
        /// </summary>
        public static readonly StyledProperty<GradientSpreadMethod> SpreadMethodProperty =
            AvaloniaProperty.Register<GradientBrush, GradientSpreadMethod>(nameof(SpreadMethod));

        /// <summary>
        /// Defines the <see cref="GradientStops"/> property.
        /// </summary>
        public static readonly StyledProperty<GradientStops> GradientStopsProperty =
            AvaloniaProperty.Register<GradientBrush, GradientStops>(nameof(GradientStops));

        private IDisposable _gradientStopsSubscription;

        static GradientBrush()
        {
            GradientStopsProperty.Changed.Subscribe(GradientStopsChanged);
            AffectsRender<LinearGradientBrush>(SpreadMethodProperty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GradientBrush"/> class.
        /// </summary>
        public GradientBrush()
        {
            this.GradientStops = new GradientStops();
        }

        /// <inheritdoc/>
        public GradientSpreadMethod SpreadMethod
        {
            get { return GetValue(SpreadMethodProperty); }
            set { SetValue(SpreadMethodProperty, value); }
        }

        /// <inheritdoc/>
        [Content]
        public GradientStops GradientStops
        {
            get { return GetValue(GradientStopsProperty); }
            set { SetValue(GradientStopsProperty, value); }
        }

        /// <inheritdoc/>
        IReadOnlyList<IGradientStop> IGradientBrush.GradientStops => GradientStops;

        private static void GradientStopsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is GradientBrush brush)
            {
                var oldValue = (GradientStops)e.OldValue;
                var newValue = (GradientStops)e.NewValue;

                if (oldValue != null)
                {
                    oldValue.CollectionChanged -= brush.GradientStopsChanged;
                    brush._gradientStopsSubscription.Dispose();
                }

                if (newValue != null)
                {
                    newValue.CollectionChanged += brush.GradientStopsChanged;
                    brush._gradientStopsSubscription = newValue.TrackItemPropertyChanged(brush.GradientStopChanged);
                }

                brush.RaiseChanged(EventArgs.Empty);
            }
        }

        private void GradientStopsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseChanged(EventArgs.Empty);
        }

        private void GradientStopChanged(Tuple<object, PropertyChangedEventArgs> e)
        {
            RaiseChanged(EventArgs.Empty);
        }
    }
}
