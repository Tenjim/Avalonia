using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class BindingEntry<T> : IValueEntry<T>,
        IValueFrame,
        IObserver<T>,
        IObserver<BindingValue<T>>,
        IDisposable
    {
        private readonly object _source;
        private IDisposable? _bindingSubscription;
        private ValueStore? _owner;
        private bool _hasValue;
        private T? _value;

        public BindingEntry(
            StyledPropertyBase<T> property,
            IObservable<BindingValue<T>> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
        }

        public BindingEntry(
            StyledPropertyBase<T> property,
            IObservable<T?> source,
            BindingPriority priority)
        {
            _source = source;
            Property = property;
            Priority = priority;
        }

        public bool HasValue
        {
            get
            {
                StartIfNecessary();
                return _hasValue;
            }
        }

        public bool IsActive => true;
        public BindingPriority Priority { get; }
        public StyledPropertyBase<T> Property { get; }
        AvaloniaProperty IValueEntry.Property => Property;
        public int EntryCount => 1;

        public void Dispose()
        {
            _bindingSubscription?.Dispose();
            BindingCompleted();
        }


        public IValueEntry GetEntry(int index) => this;

        public T GetValue()
        {
            StartIfNecessary();
            if (!_hasValue)
                throw new AvaloniaInternalException("The BindingEntry<T> has no value.");
            return _value!;
        }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetEntry(AvaloniaProperty property, [NotNullWhen(true)] out IValueEntry? entry)
        {
            if (property == Property)
            {
                entry = this;
                return true;
            }

            entry = null;
            return false;
        }

        public bool TryGetValue(out T? value)
        {
            StartIfNecessary();
            value = _value;
            return _hasValue;
        }

        public bool TryGetValue(out object? value)
        {
            StartIfNecessary();
            value = _value;
            return _hasValue;
        }

        public void OnCompleted() => BindingCompleted();
        public void OnError(Exception error) => BindingCompleted();

        object? IValueEntry.GetValue()
        {
            StartIfNecessary();
            if (!_hasValue)
                throw new AvaloniaInternalException("The BindingEntry<T> has no value.");
            return _value!;
        }
        void IObserver<T>.OnNext(T value) => SetValue(value);

        void IObserver<BindingValue<T>>.OnNext(BindingValue<T> value)
        {
            if (value.HasValue)
                SetValue(value.Value);
            else
                ClearValue();
        }

        private void ClearValue()
        {
            _ = _owner ?? throw new AvaloniaInternalException("BindingEntry has no owner.");

            var oldValue = _hasValue ? new Optional<T>(_value!) : default;

            if (_bindingSubscription is null)
                _owner.RemoveBindingEntry(this, oldValue);
            else if (_hasValue)
            {
                _hasValue = false;
                _value = default;
                _owner.ValueChanged(this, this, oldValue);
            }
        }

        private void SetValue(T value)
        {
            _ = _owner ?? throw new AvaloniaInternalException("BindingEntry has no owner.");

            if (Property.ValidateValue?.Invoke(value) == false)
            {
                value = Property.GetDefaultValue(_owner.Owner.GetType());
            }

            if (!_hasValue || !EqualityComparer<T>.Default.Equals(_value, value))
            {
                var oldValue = _hasValue ? new Optional<T>(_value!) : default;
                _value = value;
                _hasValue = true;

                // Only raise a property changed notifcation if we're not currently in the process of
                // starting the binding (in this case the value will be read immediately afterwards
                // and a notification raised).
                if (_bindingSubscription != Disposable.Empty)
                    _owner.ValueChanged(this, this, oldValue);
            }
        }

        private void StartIfNecessary()
        {
            if (_bindingSubscription is null)
            {
                // Prevent reentrancy by first assigning the subscription to a dummy
                // non-null value.
                _bindingSubscription = Disposable.Empty;

                if (_source is IObservable<BindingValue<T>> bv)
                    _bindingSubscription = bv.Subscribe(this);
                else if (_source is IObservable<T> b)
                    _bindingSubscription = b.Subscribe(this);
                else
                    throw new AvaloniaInternalException("Unexpected binding source.");
            }
        }

        private void BindingCompleted()
        {
            _bindingSubscription = null;
            ClearValue();
        }
    }
}
