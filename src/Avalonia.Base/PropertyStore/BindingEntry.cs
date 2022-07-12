using System;
using Avalonia.Data;

namespace Avalonia.PropertyStore
{
    internal class BindingEntry : BindingValueEntryBase,
        IValueFrame,
        IDisposable
    {
        private ValueStore? _owner;

        public BindingEntry(
            AvaloniaProperty property,
            IObservable<object?> source,
            BindingPriority priority)
            : base(property, source)
        {
            Priority = priority;
        }

        public BindingPriority Priority { get; }
        public int EntryCount => 1;

        public IValueEntry GetEntry(int index) => this;
        public void SetOwner(ValueStore? owner) => _owner = owner;
        protected override void ValueChanged(object? oldValue) => _owner!.ValueChanged(this, this, oldValue);
        protected override void Completed(object? oldValue) => _owner!.RemoveBindingEntry(this, oldValue);
        protected override AvaloniaObject GetOwner() => _owner!.Owner;
    }
}
