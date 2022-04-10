// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Avalonia.Controls
{
    /// <summary>
    /// Specifies date formats for a
    /// <see cref="T:Avalonia.Controls.CalendarDatePicker" />.
    /// </summary>
    public enum CalendarDatePickerFormat
    {
        /// <summary>
        /// Specifies that the date should be displayed using unabbreviated days
        /// of the week and month names.
        /// </summary>
        Long = 0,

        /// <summary>
        /// Specifies that the date should be displayed using abbreviated days
        /// of the week and month names.
        /// </summary>
        Short = 1,

        /// <summary>
        /// Specifies that the date should be displayed using a custom format string.
        /// </summary>
        Custom = 2
    }

    [TemplatePart(ElementButton,   typeof(Button))]
    [TemplatePart(ElementCalendar, typeof(Calendar))]
    [TemplatePart(ElementPopup,    typeof(Popup))]
    [TemplatePart(ElementTextBox,  typeof(TextBox))]
    public partial class CalendarDatePicker : TemplatedControl
    {
        private const string ElementTextBox = "PART_TextBox";
        private const string ElementButton = "PART_Button";
        private const string ElementPopup = "PART_Popup";
        private const string ElementCalendar = "PART_Calendar";

        private Calendar? _calendar;
        private string _defaultText;
        private Button? _dropDownButton;
        //private Canvas _outsideCanvas;
        //private Canvas _outsidePopupCanvas;
        private Popup? _popUp;
        private TextBox? _textBox;
        private IDisposable? _textBoxTextChangedSubscription;
        private IDisposable? _buttonPointerPressedSubscription;

        private DateTime? _onOpenSelectedDate;
        private bool _settingSelectedDate;

        private DateTime _displayDate;
        private DateTime? _displayDateStart;
        private DateTime? _displayDateEnd;
        private bool _isDropDownOpen;
        private DateTime? _selectedDate;
        private string? _text;
        private bool _suspendTextChangeHandler = false;
        private bool _isPopupClosing = false;
        private bool _ignoreButtonClick = false;

        /// <summary>
        /// Occurs when the drop-down
        /// <see cref="T:Avalonia.Controls.Calendar" /> is closed.
        /// </summary>
        public event EventHandler? CalendarClosed;

        /// <summary>
        /// Occurs when the drop-down
        /// <see cref="T:Avalonia.Controls.Calendar" /> is opened.
        /// </summary>
        public event EventHandler? CalendarOpened;

        /// <summary>
        /// Occurs when <see cref="P:Avalonia.Controls.DatePicker.Text" />
        /// is assigned a value that cannot be interpreted as a date.
        /// </summary>
        public event EventHandler<CalendarDatePickerDateValidationErrorEventArgs>? DateValidationError;

        /// <summary>
        /// Occurs when the
        /// <see cref="P:Avalonia.Controls.CalendarDatePicker.SelectedDate" />
        /// property is changed.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectedDateChanged;

        static CalendarDatePicker()
        {
            FocusableProperty.OverrideDefaultValue<CalendarDatePicker>(true);
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Avalonia.Controls.DatePicker" /> class.
        /// </summary>
        public CalendarDatePicker()
        {
            FirstDayOfWeek = DateTimeHelper.GetCurrentDateFormat().FirstDayOfWeek;
            _defaultText = string.Empty;
            DisplayDate = DateTime.Today;
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (_calendar != null)
            {
                _calendar.DayButtonMouseUp -= Calendar_DayButtonMouseUp;
                _calendar.DisplayDateChanged -= Calendar_DisplayDateChanged;
                _calendar.SelectedDatesChanged -= Calendar_SelectedDatesChanged;
                _calendar.PointerReleased -= Calendar_PointerReleased;
                _calendar.KeyDown -= Calendar_KeyDown;
            }
            _calendar = e.NameScope.Find<Calendar>(ElementCalendar);
            if (_calendar != null)
            {
                _calendar.SelectionMode = CalendarSelectionMode.SingleDate;

                _calendar.DayButtonMouseUp += Calendar_DayButtonMouseUp;
                _calendar.DisplayDateChanged += Calendar_DisplayDateChanged;
                _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
                _calendar.PointerReleased += Calendar_PointerReleased;
                _calendar.KeyDown += Calendar_KeyDown;

                var currentBlackoutDays = BlackoutDates;
                BlackoutDates = _calendar.BlackoutDates;
                if(currentBlackoutDays != null)
                {
                    foreach (var range in currentBlackoutDays)
                    {
                        BlackoutDates.Add(range);
                    }
                }
            }

            if (_popUp != null)
            {
                _popUp.Child = null;
                _popUp.Closed -= PopUp_Closed;
            }
            _popUp = e.NameScope.Find<Popup>(ElementPopup);
            if(_popUp != null)
            {
                _popUp.Closed += PopUp_Closed;
                if (IsDropDownOpen)
                {
                    OpenDropDown();
                }
            }

            if(_dropDownButton != null)
            {
                _dropDownButton.Click -= DropDownButton_Click;
                _buttonPointerPressedSubscription?.Dispose();
            }
            _dropDownButton = e.NameScope.Find<Button>(ElementButton);
            if(_dropDownButton != null)
            {
                _dropDownButton.Click += DropDownButton_Click;
                _buttonPointerPressedSubscription =
                    _dropDownButton.AddDisposableHandler(PointerPressedEvent, DropDownButton_PointerPressed, handledEventsToo: true);
            }

            if (_textBox != null)
            {
                _textBox.KeyDown -= TextBox_KeyDown;
                _textBox.GotFocus -= TextBox_GotFocus;
                _textBoxTextChangedSubscription?.Dispose();
            }
            _textBox = e.NameScope.Find<TextBox>(ElementTextBox);

            if(!SelectedDate.HasValue)
            {
                SetWaterMarkText();
            }

            if(_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
                _textBox.GotFocus += TextBox_GotFocus;
                _textBoxTextChangedSubscription = _textBox.GetObservable(TextBox.TextProperty).Subscribe(txt => TextBox_TextChanged());

                if(SelectedDate.HasValue)
                {
                    _textBox.Text = DateTimeToString(SelectedDate.Value);
                }
                else if(!String.IsNullOrEmpty(_defaultText))
                {
                    _textBox.Text = _defaultText;
                    SetSelectedDate();
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            // CustomDateFormatString
            if (change.Property == CustomDateFormatStringProperty)
            {
                if (SelectedDateFormat == CalendarDatePickerFormat.Custom)
                {
                    OnDateFormatChanged();
                }
            }
            // IsDropDownOpen
            else if (change.Property == IsDropDownOpenProperty)
            {
                var oldValue = change.OldValue.GetValueOrDefault<bool>();
                var value = change.NewValue.GetValueOrDefault<bool>();

                if (_popUp != null && _popUp.Child != null)
                {
                    if (value != oldValue)
                    {
                        if (_calendar!.DisplayMode != CalendarMode.Month)
                        {
                            _calendar.DisplayMode = CalendarMode.Month;
                        }

                        if (value)
                        {
                            OpenDropDown();
                        }
                        else
                        {
                            _popUp.IsOpen = false;
                            OnCalendarClosed(new RoutedEventArgs());
                        }
                    }
                }
            }
            // SelectedDate
            else if (change.Property == SelectedDateProperty)
            {
                var addedDate = change.NewValue.GetValueOrDefault() as DateTime?;
                var removedDate = change.OldValue.GetValueOrDefault() as DateTime?;

                if (SelectedDate != null)
                {
                    DateTime day = SelectedDate.Value;

                    // When the SelectedDateProperty change is done from
                    // OnTextPropertyChanged method, two-way binding breaks if
                    // BeginInvoke is not used:
                    Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _settingSelectedDate = true;
                        Text = DateTimeToString(day);
                        _settingSelectedDate = false;
                        OnDateSelected(addedDate, removedDate);
                    });

                    // When DatePickerDisplayDateFlag is TRUE, the SelectedDate
                    // change is coming from the Calendar UI itself, so, we
                    // shouldn't change the DisplayDate since it will automatically
                    // be changed by the Calendar
                    if ((day.Month != DisplayDate.Month || day.Year != DisplayDate.Year) && (_calendar == null || !_calendar.CalendarDatePickerDisplayDateFlag))
                    {
                        DisplayDate = day;
                    }
                    if(_calendar != null)
                        _calendar.CalendarDatePickerDisplayDateFlag = false;
                }
                else
                {
                    _settingSelectedDate = true;
                    SetWaterMarkText();
                    _settingSelectedDate = false;
                    OnDateSelected(addedDate, removedDate);
                }
            }
            // SelectedDateFormat
            else if (change.Property == SelectedDateFormatProperty)
            {
                OnDateFormatChanged();
            }
            // Text
            else if (change.Property == TextProperty)
            {
                var oldValue = change.OldValue.GetValueOrDefault() as string;
                var value = change.NewValue.GetValueOrDefault() as string;

                if (!_suspendTextChangeHandler)
                {
                    if (value != null)
                    {
                        if (_textBox != null)
                        {
                            _textBox.Text = value;
                        }
                        else
                        {
                            _defaultText = value;
                        }

                        if (!_settingSelectedDate)
                        {
                            SetSelectedDate();
                        }
                    }
                    else
                    {
                        if (!_settingSelectedDate)
                        {
                            _settingSelectedDate = true;
                            SelectedDate = null;
                            _settingSelectedDate = false;
                        }
                    }
                }
                else
                {
                    SetWaterMarkText();
                }
            }

            base.OnPropertyChanged(change);
        }

        protected override void UpdateDataValidation<T>(AvaloniaProperty<T> property, BindingValue<T> value)
        {
            if (property == SelectedDateProperty)
            {
                DataValidationErrors.SetError(this, value.Error);
            }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            if (!e.Handled && SelectedDate.HasValue && _calendar != null)
            {
                DateTime selectedDate = this.SelectedDate.Value;
                DateTime? newDate = DateTimeHelper.AddDays(selectedDate, e.Delta.Y > 0 ? -1 : 1);
                if (newDate.HasValue && Calendar.IsValidDateSelection(_calendar, newDate.Value))
                {
                    SelectedDate = newDate;
                    e.Handled = true;
                }
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            if(IsEnabled && _textBox != null && e.NavigationMethod == NavigationMethod.Tab)
            {
                _textBox.Focus();
                var text = _textBox.Text;
                if(!string.IsNullOrEmpty(text))
                {
                    _textBox.SelectionStart = 0;
                    _textBox.SelectionEnd = text.Length;
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            SetSelectedDate();
        }

        private void OnDateFormatChanged()
        {
            if (_textBox != null)
            {
                if (SelectedDate.HasValue)
                {
                    Text = DateTimeToString(SelectedDate.Value);
                }
                else if (string.IsNullOrEmpty(_textBox.Text))
                {
                    SetWaterMarkText();
                }
                else
                {
                    DateTime? date = ParseText(_textBox.Text);

                    if (date != null)
                    {
                        string? s = DateTimeToString((DateTime)date);
                        Text = s;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the
        /// <see cref="E:Avalonia.Controls.CalendarDatePicker.DateValidationError" />
        /// event.
        /// </summary>
        /// <param name="e">
        /// A
        /// <see cref="T:Avalonia.Controls.CalendarDatePickerDateValidationErrorEventArgs" />
        /// that contains the event data.
        /// </param>
        protected virtual void OnDateValidationError(CalendarDatePickerDateValidationErrorEventArgs e)
        {
            DateValidationError?.Invoke(this, e);
        }

        private void OnDateSelected(DateTime? addedDate, DateTime? removedDate)
        {
            EventHandler<SelectionChangedEventArgs>? handler = this.SelectedDateChanged;
            if (null != handler)
            {
                Collection<DateTime> addedItems = new Collection<DateTime>();
                Collection<DateTime> removedItems = new Collection<DateTime>();

                if (addedDate.HasValue)
                {
                    addedItems.Add(addedDate.Value);
                }

                if (removedDate.HasValue)
                {
                    removedItems.Add(removedDate.Value);
                }

                handler(this, new SelectionChangedEventArgs(SelectingItemsControl.SelectionChangedEvent, removedItems, addedItems));
            }
        }

        private void OnCalendarClosed(EventArgs e)
        {
            CalendarClosed?.Invoke(this, e);
        }

        private void OnCalendarOpened(EventArgs e)
        {
            CalendarOpened?.Invoke(this, e);
        }

        private void Calendar_DayButtonMouseUp(object? sender, PointerReleasedEventArgs e)
        {
            Focus();
            IsDropDownOpen = false;
        }

        private void Calendar_DisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
        {
            if (e.AddedDate != this.DisplayDate)
            {
                SetValue(DisplayDateProperty, (DateTime) e.AddedDate!);
            }
        }

        private void Calendar_SelectedDatesChanged(object? sender, SelectionChangedEventArgs e)
        {
            Debug.Assert(e.AddedItems.Count < 2, "There should be less than 2 AddedItems!");

            if (e.AddedItems.Count > 0 && SelectedDate.HasValue && DateTime.Compare((DateTime)e.AddedItems[0]!, SelectedDate.Value) != 0)
            {
                SelectedDate = (DateTime?)e.AddedItems[0];
            }
            else
            {
                if (e.AddedItems.Count == 0)
                {
                    SelectedDate = null;
                    return;
                }

                if (!SelectedDate.HasValue)
                {
                    if (e.AddedItems.Count > 0)
                    {
                        SelectedDate = (DateTime?)e.AddedItems[0];
                    }
                }
            }
        }

        private void Calendar_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
             
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                e.Handled = true;
            }
        }

        private void Calendar_KeyDown(object? sender, KeyEventArgs e)
        {
            Calendar? c = sender as Calendar ?? throw new ArgumentException("Sender must be Calendar.", nameof(sender));

            if (!e.Handled && (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Escape) && c.DisplayMode == CalendarMode.Month)
            {
                Focus();
                IsDropDownOpen = false;

                if (e.Key == Key.Escape)
                {
                    SelectedDate = _onOpenSelectedDate;
                }
            }
        }

        private void TextBox_GotFocus(object? sender, RoutedEventArgs e)
        {
            IsDropDownOpen = false;
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = ProcessDatePickerKey(e);
            }
        }

        private void TextBox_TextChanged()
        {
            if (_textBox != null)
            {
                _suspendTextChangeHandler = true;
                Text = _textBox.Text;
                _suspendTextChangeHandler = false;
            }
        }

        private void DropDownButton_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _ignoreButtonClick = _isPopupClosing;
        }

        private void DropDownButton_Click(object? sender, RoutedEventArgs e)
        {
            if (!_ignoreButtonClick)
            {
                HandlePopUp();
            }
            else
            {
                _ignoreButtonClick = false;
            }
        }

        private void PopUp_Closed(object? sender, EventArgs e)
        {
            IsDropDownOpen = false;

            if(!_isPopupClosing)
            {
                _isPopupClosing = true;
                Threading.Dispatcher.UIThread.InvokeAsync(() => _isPopupClosing = false);
            }
        }

        private void HandlePopUp()
        {
            if (IsDropDownOpen)
            {
                Focus();
                IsDropDownOpen = false;
            }
            else
            {
                ProcessTextBox();
            }
        }

        private void OpenDropDown()
        {
            if (_calendar != null)
            {
                _calendar.Focus();
                OpenPopUp();
                _calendar.ResetStates();
                OnCalendarOpened(new RoutedEventArgs());
            }
        }

        private void OpenPopUp()
        {
            _onOpenSelectedDate = SelectedDate;
            _popUp!.IsOpen = true;
        }

        /// <summary>
        /// Input text is parsed in the correct format and changed into a
        /// DateTime object.  If the text can not be parsed TextParseError Event
        /// is thrown.
        /// </summary>
        /// <param name="text">Inherited code: Requires comment.</param>
        /// <returns>
        /// IT SHOULD RETURN NULL IF THE STRING IS NOT VALID, RETURN THE
        /// DATETIME VALUE IF IT IS VALID.
        /// </returns>
        private DateTime? ParseText(string text)
        {
            DateTime newSelectedDate;

            // TryParse is not used in order to be able to pass the exception to
            // the TextParseError event
            try
            {
                newSelectedDate = DateTime.Parse(text, DateTimeHelper.GetCurrentDateFormat());

                if (Calendar.IsValidDateSelection(this._calendar!, newSelectedDate))
                {
                    return newSelectedDate;
                }
                else
                {
                    var dateValidationError = new CalendarDatePickerDateValidationErrorEventArgs(new ArgumentOutOfRangeException(nameof(text), "SelectedDate value is not valid."), text);
                    OnDateValidationError(dateValidationError);

                    if (dateValidationError.ThrowException)
                    {
                        throw dateValidationError.Exception;
                    }
                }
            }
            catch (FormatException ex)
            {
                CalendarDatePickerDateValidationErrorEventArgs textParseError = new CalendarDatePickerDateValidationErrorEventArgs(ex, text);
                OnDateValidationError(textParseError);

                if (textParseError.ThrowException)
                {
                    throw textParseError.Exception;
                }
            }

            return null;
        }

        private string? DateTimeToString(DateTime d)
        {
            DateTimeFormatInfo dtfi = DateTimeHelper.GetCurrentDateFormat();

            switch (SelectedDateFormat)
            {
                case CalendarDatePickerFormat.Short:
                    return string.Format(CultureInfo.CurrentCulture, d.ToString(dtfi.ShortDatePattern, dtfi));
                case CalendarDatePickerFormat.Long:
                    return string.Format(CultureInfo.CurrentCulture, d.ToString(dtfi.LongDatePattern, dtfi));
                case CalendarDatePickerFormat.Custom:
                    return string.Format(CultureInfo.CurrentCulture, d.ToString(CustomDateFormatString, dtfi));
            }

            return null;
        }

        private bool ProcessDatePickerKey(KeyEventArgs e)
        {
            
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        SetSelectedDate();
                        return true;
                    }
                case Key.Down:
                    { 
                        if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
                        {
                            HandlePopUp();
                            return true;
                        }
                        break;
                    }
            }

            return false;
        }

        private void ProcessTextBox()
        {
            SetSelectedDate();
            IsDropDownOpen = true;
            _calendar!.Focus();
        }

        private void SetSelectedDate()
        {
            if (_textBox != null)
            {
                if (!string.IsNullOrEmpty(_textBox.Text))
                {
                    string s = _textBox.Text;

                    if (SelectedDate != null)
                    {
                        // If the string value of the SelectedDate and the
                        // TextBox string value are equal, we do not parse the
                        // string again if we do an extra parse, we lose data in
                        // M/d/yy format.
                        // ex: SelectedDate = DateTime(1008,12,19) but when
                        // "12/19/08" is parsed it is interpreted as
                        // DateTime(2008,12,19)
                        string? selectedDate = DateTimeToString(SelectedDate.Value);
                        if (selectedDate == s)
                        {
                            return;
                        }
                    }
                    DateTime? d = SetTextBoxValue(s);
                    
                    if (SelectedDate != d)
                    {
                        SelectedDate = d;
                    }
                }
                else
                {
                    if (SelectedDate != null)
                    {
                        SelectedDate = null;
                    }
                }
            }
            else
            {
                DateTime? d = SetTextBoxValue(_defaultText);

                if (SelectedDate != d)
                {
                    SelectedDate = d;
                }
            }
        }

        private DateTime? SetTextBoxValue(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                SetValue(TextProperty, s);
                return SelectedDate;
            }
            else
            {
                DateTime? d = ParseText(s);
                if (d != null)
                {
                    SetValue(TextProperty, s);
                    return d;
                }
                else
                {
                    // If parse error: TextBox should have the latest valid
                    // SelectedDate value:
                    if (SelectedDate != null)
                    {
                        string? newtext = this.DateTimeToString(SelectedDate.Value);
                        SetValue(TextProperty, newtext);
                        return SelectedDate;
                    }
                    else
                    {
                        SetWaterMarkText();
                        return null;
                    }
                }
            }
        }

        private void SetWaterMarkText()
        {
            if (_textBox != null)
            {
                if (string.IsNullOrEmpty(Watermark) && !UseFloatingWatermark)
                {
                    DateTimeFormatInfo dtfi = DateTimeHelper.GetCurrentDateFormat();
                    Text = string.Empty;
                    _defaultText = string.Empty;
                    var watermarkFormat = "<{0}>";
                    string watermarkText;

                    switch (SelectedDateFormat)
                    {
                        case CalendarDatePickerFormat.Long:
                            {
                                watermarkText = string.Format(CultureInfo.CurrentCulture, watermarkFormat, dtfi.LongDatePattern.ToString());
                                break;
                            }
                        case CalendarDatePickerFormat.Short:
                        default:
                            {
                                watermarkText = string.Format(CultureInfo.CurrentCulture, watermarkFormat, dtfi.ShortDatePattern.ToString());
                                break;
                            }
                    }
                    _textBox.Watermark = watermarkText;
                }
                else
                {
                    _textBox.ClearValue(TextBox.WatermarkProperty);
                }
            }
        }

        private static bool IsValidSelectedDateFormat(CalendarDatePickerFormat value)
        {
            return value == CalendarDatePickerFormat.Long
                || value == CalendarDatePickerFormat.Short
                || value == CalendarDatePickerFormat.Custom;
        }

        private static bool IsValidDateFormatString(string formatString)
        {
            return !string.IsNullOrWhiteSpace(formatString);
        }
    }
}
