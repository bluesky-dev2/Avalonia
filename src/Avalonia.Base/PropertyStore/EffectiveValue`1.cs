﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    /// <summary>
    /// Represents the active value for a property in a <see cref="ValueStore"/>.
    /// </summary>
    /// <remarks>
    /// Stores the active value in an <see cref="AvaloniaObject"/>'s <see cref="ValueStore"/>
    /// for a single property, when the value is not inherited or unset/default.
    /// </remarks>
    internal sealed class EffectiveValue<T> : EffectiveValue
    {
        private T? _baseValue;

        public EffectiveValue(T value, BindingPriority priority)
        {
            Value = value;
            Priority = priority;

            if (priority >= BindingPriority.LocalValue && priority < BindingPriority.Inherited)
            {
                _baseValue = value;
                BasePriority = priority;
            }
            else
            {
                _baseValue = default;
                BasePriority = BindingPriority.Unset;
            }
        }

        /// <summary>
        /// Gets the current effective value.
        /// </summary>
        public new T Value { get; private set; }

        public override void SetAndRaise(
            ValueStore owner,
            AvaloniaProperty property,
            object? value, 
            BindingPriority priority)
        {
            // `value` should already have been converted to the correct type and
            // validated by this point.
            SetAndRaise(owner, (StyledPropertyBase<T>)property, (T)value!, priority);
        }

        public override void SetAndRaise(
            ValueStore owner,
            AvaloniaProperty property,
            object? value,
            BindingPriority priority,
            object? baseValue,
            BindingPriority basePriority)
        {
            SetAndRaise(owner, (StyledPropertyBase<T>)property, (T)value!, priority, (T)baseValue!, basePriority);
        }

        public override void SetAndRaise(
            ValueStore owner,
            IValueEntry entry,
            BindingPriority priority)
        {
            var value = entry is IValueEntry<T> typed ? typed.GetValue() : (T)entry.GetValue()!;
            SetAndRaise(owner, (StyledPropertyBase<T>)entry.Property, value, priority);
        }

        /// <summary>
        /// Sets the value and base value, raising <see cref="AvaloniaObject.PropertyChanged"/>
        /// where necessary.
        /// </summary>
        /// <param name="owner">The object on which to raise events.</param>
        /// <param name="property">The property being changed.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="priority">The priority of the new value.</param>
        public void SetAndRaise(
            ValueStore owner,
            StyledPropertyBase<T> property,
            T value,
            BindingPriority priority)
        {
            Debug.Assert(priority < BindingPriority.Inherited);

            var oldValue = Value;
            var valueChanged = false;
            var baseValueChanged = false;

            if (priority <= Priority)
            {
                valueChanged = !EqualityComparer<T>.Default.Equals(Value, value);
                Value = value;
                Priority = priority;
            }

            if (priority <= BasePriority && priority >= BindingPriority.LocalValue)
            {
                baseValueChanged = !EqualityComparer<T>.Default.Equals(_baseValue, value);
                _baseValue = value;
                BasePriority = priority;
            }

            if (valueChanged)
            {
                owner.Owner.RaisePropertyChanged(property, oldValue, Value, Priority, true);
                if (property.Inherits)
                    owner.OnInheritedEffectiveValueChanged(property, oldValue, this);
            }
            else if (baseValueChanged)
            {
                owner.Owner.RaisePropertyChanged(property, default, _baseValue!, BasePriority, false);
            }
        }

        /// <summary>
        /// Sets the value and base value, raising <see cref="AvaloniaObject.PropertyChanged"/>
        /// where necessary.
        /// </summary>
        /// <param name="owner">The object on which to raise events.</param>
        /// <param name="property">The property being changed.</param>
        /// <param name="value">The new value of the property.</param>
        /// <param name="priority">The priority of the new value.</param>
        /// <param name="baseValue">The new base value of the property.</param>
        /// <param name="basePriority">The priority of the new base value.</param>
        public void SetAndRaise(
            ValueStore owner,
            StyledPropertyBase<T> property,
            T value,
            BindingPriority priority,
            T baseValue,
            BindingPriority basePriority)
        {
            Debug.Assert(priority < BindingPriority.Inherited);
            Debug.Assert(basePriority > BindingPriority.Animation);

            var oldValue = Value;
            var valueChanged = false;
            var baseValueChanged = false;

            if (!EqualityComparer<T>.Default.Equals(Value, value))
            {
                Value = value;
                valueChanged = true;
            }

            if (BasePriority == BindingPriority.Unset || 
                !EqualityComparer<T>.Default.Equals(_baseValue, baseValue))
            {
                _baseValue = value;
                baseValueChanged = true;
            }

            Priority = priority;
            BasePriority = basePriority;

            if (valueChanged)
            {
                owner.Owner.RaisePropertyChanged(property, oldValue, Value, Priority, true);
                if (property.Inherits)
                    owner.OnInheritedEffectiveValueChanged(property, oldValue, this);
            }
            else if (baseValueChanged)
            {
                owner.Owner.RaisePropertyChanged(property, default, _baseValue!, BasePriority, false);
            }
        }

        public bool TryGetBaseValue([MaybeNullWhen(false)] out T value)
        {
            value = _baseValue!;
            return BasePriority != BindingPriority.Unset;
        }

        public override void RaiseInheritedValueChanged(
            AvaloniaObject owner,
            AvaloniaProperty property,
            EffectiveValue? oldValue,
            EffectiveValue? newValue)
        {
            Debug.Assert(oldValue is not null || newValue is not null);

            var p = (StyledPropertyBase<T>)property;
            var o = oldValue is not null ? ((EffectiveValue<T>)oldValue).Value : p.GetDefaultValue(owner.GetType());
            var n = newValue is not null ? ((EffectiveValue<T>)newValue).Value : p.GetDefaultValue(owner.GetType());
            var priority = newValue is not null ? BindingPriority.Inherited : BindingPriority.Unset;

            if (!EqualityComparer<T>.Default.Equals(o, n))
            {
                owner.RaisePropertyChanged(p, o, n, priority, true);
            }
        }

        public override void DisposeAndRaiseUnset(ValueStore owner, AvaloniaProperty property)
        {
            DisposeAndRaiseUnset(owner, (StyledPropertyBase<T>)property);
        }

        public void DisposeAndRaiseUnset(ValueStore owner, StyledPropertyBase<T> property)
        {
            var defaultValue = property.GetDefaultValue(owner.GetType());

            if (!EqualityComparer<T>.Default.Equals(defaultValue, Value))
            {
                owner.Owner.RaisePropertyChanged(property, Value, defaultValue, BindingPriority.Unset, true);
                if (property.Inherits)
                    owner.OnInheritedEffectiveValueDisposed(property, Value);
            }
        }

        protected override object? GetBoxedValue() => Value;
        
        protected override object? GetBoxedBaseValue()
        {
            return BasePriority != BindingPriority.Unset ? _baseValue : AvaloniaProperty.UnsetValue;
        }
    }
}
