using System;
using System.Collections.Generic;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class SingleValueFrame<T> : List<IValueEntry>, 
        IValueFrame,
        IValueEntry,
        IDisposable
    {
        private readonly T _value;
        private ValueStore? _owner;

        public SingleValueFrame(
            StyledPropertyBase<T> property,
            T value,
            BindingPriority priority)
        {
            Property = property;
            Priority = priority;
            _value = value;
            Add(this);
        }

        public bool HasValue => true;
        public bool IsActive => true;
        public BindingPriority Priority { get; }
        public AvaloniaProperty Property { get; }
        public IList<IValueEntry> Values => this;

        public object? GetValue() => _value;

        public void Dispose()
        {
            _owner?.RemoveFrame(this);
            _owner = null;
        }

        public void SetOwner(ValueStore? owner) => _owner = owner;

        public bool TryGetValue(out object? value)
        {
            value = _value;
            return true;
        }
    }
}
