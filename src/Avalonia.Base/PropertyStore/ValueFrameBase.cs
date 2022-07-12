using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Data;
using Avalonia.Utilities;

namespace Avalonia.PropertyStore
{
    internal abstract class ValueFrameBase : IValueFrame
    {
        private readonly AvaloniaPropertyValueStore<IValueEntry> _values = new();

        public abstract bool IsActive { get; }
        public abstract BindingPriority Priority { get; }
        public int EntryCount => _values.Count;

        public IValueEntry GetEntry(int index) => _values[index];
        public virtual void SetOwner(ValueStore? owner) { }

        public bool TryGet(AvaloniaProperty property, [NotNullWhen(true)] out IValueEntry? value)
        {
            return _values.TryGetValue(property, out value);
        }

        protected void Add(IValueEntry value)
        {
            Debug.Assert(!value.Property.IsDirect);
            _values.AddValue(value.Property, value);
        }

        protected void Remove(AvaloniaProperty property) => _values.Remove(property);
        protected void Set(IValueEntry value) => _values.SetValue(value.Property, value);
    }
}
