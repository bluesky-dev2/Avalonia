﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Avalonia.Controls
{
    /// <summary>
    /// Presents a color for user editing using a spectrum, palette and component sliders.
    /// </summary>
    [TemplatePart("PART_HexTextBox", typeof(TextBox))]
    [TemplatePart("PART_TabControl", typeof(TabControl))]
    public partial class ColorView : TemplatedControl
    {
        /// <summary>
        /// Event for when the selected color changes within the slider.
        /// </summary>
        public event EventHandler<ColorChangedEventArgs>? ColorChanged;

        // XAML template parts
        private TextBox?    _hexTextBox;
        private TabControl? _tabControl;

        private ObservableCollection<Color> _customPaletteColors = new ObservableCollection<Color>();
        private ColorToHexConverter colorToHexConverter = new ColorToHexConverter();
        protected bool ignorePropertyChanged = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorView"/> class.
        /// </summary>
        public ColorView() : base()
        {
        }

        /// <summary>
        /// Gets the value of the hex TextBox and sets it as the current <see cref="Color"/>.
        /// If invalid, the TextBox hex text will revert back to the last valid color.
        /// </summary>
        private void GetColorFromHexTextBox()
        {
            if (_hexTextBox != null)
            {
                var convertedColor = colorToHexConverter.ConvertBack(_hexTextBox.Text, typeof(Color), null, CultureInfo.CurrentCulture);

                if (convertedColor is Color color)
                {
                    Color = color;
                }

                // Re-apply the hex value
                // This ensure the hex color value is always valid and formatted correctly
                SetColorToHexTextBox();
            }
        }

        /// <summary>
        /// Sets the current <see cref="Color"/> to the hex TextBox.
        /// </summary>
        private void SetColorToHexTextBox()
        {
            if (_hexTextBox != null)
            {
                _hexTextBox.Text = colorToHexConverter.Convert(Color, typeof(string), null, CultureInfo.CurrentCulture) as string;
            }
        }

        /// <summary>
        /// Validates the tab/panel/page selection taking into account the visibility of each item
        /// as well as the current selection.
        /// </summary>
        /// <remarks>
        /// Derived controls may re-implement this based on their default style / control template
        /// and any specialized selection needs.
        /// </remarks>
        protected virtual void ValidateSelection()
        {
            if (_tabControl != null &&
                _tabControl.Items != null)
            {
                // Determine if any item is visible
                bool isAnyItemVisible = false;
                foreach (var item in _tabControl.Items)
                {
                    if (item is Control control &&
                        control.IsVisible)
                    {
                        isAnyItemVisible = true;
                        break;
                    }
                }

                if (isAnyItemVisible)
                {
                    object? selectedItem = null;

                    if (_tabControl.SelectedItem == null &&
                        _tabControl.ItemCount > 0)
                    {
                        // As a failsafe, forcefully select the first item
                        foreach (var item in _tabControl.Items)
                        {
                            selectedItem = item;
                            break;
                        }
                    }
                    else
                    {
                        selectedItem = _tabControl.SelectedItem;
                    }

                    if (selectedItem is Control selectedControl &&
                        selectedControl.IsVisible == false)
                    {
                        // Select the first visible item instead
                        foreach (var item in _tabControl.Items)
                        {
                            if (item is Control control &&
                                control.IsVisible)
                            {
                                selectedItem = item;
                                break;
                            }
                        }
                    }

                    _tabControl.SelectedItem = selectedItem;
                    _tabControl.IsVisible = true;
                }
                else
                {
                    // Special case when all items are hidden
                    // If TabControl ever properly supports no selected item /
                    // all items hidden this can be removed
                    _tabControl.SelectedItem = null;
                    _tabControl.IsVisible = false;
                }

                // Note that if externally the SelectedIndex is set to 4 or something
                // outside the valid range, the TabControl will ignore it and replace it
                // with a valid SelectedIndex. This however is not propagated back through
                // the TwoWay binding in the control template so the SelectedIndex and
                // SelectedIndex become out of sync.
                //
                // The work-around for this is done here where SelectedIndex is forcefully
                // synchronized with whatever the TabControl property value is. This is
                // possible since selection validation is already done by this method.
                SelectedIndex = _tabControl.SelectedIndex;
            }

            return;
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_hexTextBox != null)
            {
                _hexTextBox.KeyDown -= HexTextBox_KeyDown;
                _hexTextBox.LostFocus -= HexTextBox_LostFocus;
            }

            _hexTextBox = e.NameScope.Find<TextBox>("PART_HexTextBox");
            _tabControl = e.NameScope.Find<TabControl>("PART_TabControl");

            SetColorToHexTextBox();

            if (_hexTextBox != null)
            {
                _hexTextBox.KeyDown += HexTextBox_KeyDown;
                _hexTextBox.LostFocus += HexTextBox_LostFocus;
            }

            base.OnApplyTemplate(e);
            ValidateSelection();
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (ignorePropertyChanged)
            {
                base.OnPropertyChanged(change);
                return;
            }

            // Always keep the two color properties in sync
            if (change.Property == ColorProperty)
            {
                ignorePropertyChanged = true;

                HsvColor = Color.ToHsv();
                SetColorToHexTextBox();

                OnColorChanged(new ColorChangedEventArgs(
                    change.GetOldValue<Color>(),
                    change.GetNewValue<Color>()));

                ignorePropertyChanged = false;
            }
            else if (change.Property == HsvColorProperty)
            {
                ignorePropertyChanged = true;

                Color = HsvColor.ToRgb();
                SetColorToHexTextBox();

                OnColorChanged(new ColorChangedEventArgs(
                    change.GetOldValue<HsvColor>().ToRgb(),
                    change.GetNewValue<HsvColor>().ToRgb()));

                ignorePropertyChanged = false;
            }
            else if (change.Property == CustomPaletteProperty)
            {
                IColorPalette? palette = CustomPalette;

                // Any custom palette change must be automatically synced with the
                // bound properties controlling the palette grid
                if (palette != null)
                {
                    CustomPaletteColumnCount = palette.ColorCount;
                    CustomPaletteColors.Clear();

                    for (int shadeIndex = 0; shadeIndex < palette.ShadeCount; shadeIndex++)
                    {
                        for (int colorIndex = 0; colorIndex < palette.ColorCount; colorIndex++)
                        {
                            CustomPaletteColors.Add(palette.GetColor(colorIndex, shadeIndex));
                        }
                    }
                }
            }
            else if (change.Property == IsColorComponentsVisibleProperty ||
                     change.Property == IsColorPaletteVisibleProperty ||
                     change.Property == IsColorSpectrumVisibleProperty)
            {
                // When the property changed notification is received here the visibility
                // of individual tab items has not yet been updated through the bindings.
                // Therefore, the validation is delayed until after bindings update.
                Dispatcher.UIThread.Post(() =>
                {
                    ValidateSelection();
                }, DispatcherPriority.Background);
            }
            else if (change.Property == SelectedIndexProperty)
            {
                // Again, it is necessary to wait for the SelectedIndex value to
                // be applied to the TabControl through binding before validation occurs.
                Dispatcher.UIThread.Post(() =>
                {
                    ValidateSelection();
                }, DispatcherPriority.Background);
            }

            base.OnPropertyChanged(change);
        }

        /// <summary>
        /// Called before the <see cref="ColorChanged"/> event occurs.
        /// </summary>
        /// <param name="e">The <see cref="ColorChangedEventArgs"/> defining old/new colors.</param>
        protected virtual void OnColorChanged(ColorChangedEventArgs e)
        {
            ColorChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Event handler for when a key is pressed within the Hex RGB value TextBox.
        /// This is used to trigger re-evaluation of the color based on the TextBox value.
        /// </summary>
        private void HexTextBox_KeyDown(object? sender, Input.KeyEventArgs e)
        {
            if (e.Key == Input.Key.Enter)
            {
                GetColorFromHexTextBox();
            }
        }

        /// <summary>
        /// Event handler for when the Hex RGB value TextBox looses focus.
        /// This is used to trigger re-evaluation of the color based on the TextBox value.
        /// </summary>
        private void HexTextBox_LostFocus(object? sender, Interactivity.RoutedEventArgs e)
        {
            GetColorFromHexTextBox();
        }
    }
}
