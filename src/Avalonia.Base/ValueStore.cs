﻿using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia
{
    internal class ValueStore : IPriorityValueOwner
    {
        private readonly AvaloniaPropertyCollection<object> _propertyValues;
        private readonly AvaloniaPropertyCollection<object> _deferredSetters;
        private readonly AvaloniaObject _owner;

        public ValueStore(AvaloniaObject owner)
        {
            _owner = owner;
            _propertyValues = new AvaloniaPropertyCollection<object>();
            _deferredSetters = new AvaloniaPropertyCollection<object>();
        }

        public IDisposable AddBinding(
            AvaloniaProperty property,
            IObservable<object> source,
            BindingPriority priority)
        {
            PriorityValue priorityValue;

            if (_propertyValues.TryGetValue(property, out var v))
            {
                priorityValue = v as PriorityValue;

                if (priorityValue == null)
                {
                    priorityValue = CreatePriorityValue(property);
                    priorityValue.SetValue(v, (int)BindingPriority.LocalValue);
                    _propertyValues.SetValue(property, priorityValue);
                }
            }
            else
            {
                priorityValue = CreatePriorityValue(property);
                _propertyValues.AddValue(property, priorityValue);
            }

            return priorityValue.Add(source, (int)priority);
        }

        public void AddValue(AvaloniaProperty property, object value, int priority)
        {
            PriorityValue priorityValue;

            if (_propertyValues.TryGetValue(property, out var v))
            {
                priorityValue = v as PriorityValue;

                if (priorityValue == null)
                {
                    if (priority == (int)BindingPriority.LocalValue)
                    {
                        _propertyValues.SetValue(property, Validate(property, value));
                        Changed(property, priority, v, value);
                        return;
                    }
                    else
                    {
                        priorityValue = CreatePriorityValue(property);
                        priorityValue.SetValue(v, (int)BindingPriority.LocalValue);
                        _propertyValues.SetValue(property, priorityValue);
                    }
                }
            }
            else
            {
                if (value == AvaloniaProperty.UnsetValue)
                {
                    return;
                }

                if (priority == (int)BindingPriority.LocalValue)
                {
                    _propertyValues.AddValue(property, Validate(property, value));
                    Changed(property, priority, AvaloniaProperty.UnsetValue, value);
                    return;
                }
                else
                {
                    priorityValue = CreatePriorityValue(property);
                    _propertyValues.AddValue(property, priorityValue);
                }
            }

            priorityValue.SetValue(value, priority);
        }

        public void BindingNotificationReceived(AvaloniaProperty property, BindingNotification notification)
        {
            _owner.BindingNotificationReceived(property, notification);
        }

        public void Changed(AvaloniaProperty property, int priority, object oldValue, object newValue)
        {
            _owner.PriorityValueChanged(property, priority, oldValue, newValue);
        }

        public IDictionary<AvaloniaProperty, object> GetSetValues()
        {
            return _propertyValues.ToDictionary();
        }

        public void LogError(AvaloniaProperty property, Exception e)
        {
            _owner.LogBindingError(property, e);
        }

        public object GetValue(AvaloniaProperty property)
        {
            var result = AvaloniaProperty.UnsetValue;

            if (_propertyValues.TryGetValue(property, out var value))
            {
                result = (value is PriorityValue priorityValue) ? priorityValue.Value : value;
            }

            return result;
        }

        public bool IsAnimating(AvaloniaProperty property)
        {
            return _propertyValues.TryGetValue(property, out var value) && value is PriorityValue priority && priority.IsAnimating;
        }

        public bool IsSet(AvaloniaProperty property)
        {
            if (_propertyValues.TryGetValue(property, out var value))
            {
                return ((value as PriorityValue)?.Value ?? value) != AvaloniaProperty.UnsetValue;
            }

            return false;
        }

        public void Revalidate(AvaloniaProperty property)
        {
            if (_propertyValues.TryGetValue(property, out var value))
            {
                (value as PriorityValue)?.Revalidate();
            }
        }

        public void VerifyAccess() => _owner.VerifyAccess();

        private PriorityValue CreatePriorityValue(AvaloniaProperty property)
        {
            var validate = ((IStyledPropertyAccessor)property).GetValidationFunc(_owner.GetType());
            Func<object, object> validate2 = null;

            if (validate != null)
            {
                validate2 = v => validate(_owner, v);
            }

            return new PriorityValue(
                this,
                property,
                property.PropertyType,
                validate2);
        }

        private object Validate(AvaloniaProperty property, object value)
        {
            var validate = ((IStyledPropertyAccessor)property).GetValidationFunc(_owner.GetType());

            if (validate != null && value != AvaloniaProperty.UnsetValue)
            {
                return validate(_owner, value);
            }

            return value;
        }

        public DeferredSetterOptimized<T> GetDeferredSetter<T>(AvaloniaProperty<T> property)
        {
            if (_deferredSetters.TryGetValue(property, out var deferredSetter))
            {
                return (DeferredSetterOptimized<T>)deferredSetter;
            }

            var newDeferredSetter = new DeferredSetterOptimized<T>();

            _deferredSetters.AddValue(property, newDeferredSetter);

            return newDeferredSetter;
        }

        private DeferredSetter<object> _deferredSetter;

        public DeferredSetter<object> Setter
        {
            get
            {
                return _deferredSetter ??
                    (_deferredSetter = new DeferredSetter<object>());
            }
        }
    }
}
