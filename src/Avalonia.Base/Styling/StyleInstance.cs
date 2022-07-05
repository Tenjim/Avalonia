﻿using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Styling.Activators;

namespace Avalonia.Styling
{
    /// <summary>
    /// Stores state for a <see cref="Style"/> that has been instanced on a control.
    /// </summary>
    /// <remarks>
    /// <see cref="StyleInstance"/> implements the <see cref="IValueFrame"/> interface meaning that
    /// it is injected directly into the value store of an <see cref="AvaloniaObject"/>. Depending
    /// on the setters present on the style, it may be possible to share a single style instance
    /// among all controls that the style is applied to; meaning that a single style instance can
    /// apply to multiple controls.
    /// </remarks>
    internal class StyleInstance : ValueFrameBase, IStyleInstance, IStyleActivatorSink, IDisposable
    {
        private readonly IStyleActivator? _activator;
        private List<ISetterInstance>? _setters;
        private bool _isActivatorSubscribed;

        public StyleInstance(IStyle style, IStyleActivator? activator)
        {
            _activator = activator;
            Priority = activator is object ? BindingPriority.StyleTrigger : BindingPriority.Style;
            Source = style;
        }

        public bool HasActivator => _activator is object;

        public override bool IsActive
        {
            get
            {
                if (_activator is object && !_isActivatorSubscribed)
                {
                    _isActivatorSubscribed = true;
                    _activator.Subscribe(this);
                }

                return _activator?.IsActive ?? true;
            }
        }

        public override BindingPriority Priority { get; }
        public IStyle Source { get; }
        public ValueStore? ValueStore { get; private set; }

        public void Add(ISetterInstance instance)
        {
            if (instance is IValueEntry valueEntry)
                base.Add(valueEntry);
            else
                (_setters ??= new()).Add(instance);
        }

        public override void SetOwner(ValueStore? owner) => ValueStore = owner;

        public void Dispose()
        {
            if (_setters is object)
            {
                foreach (var setter in _setters)
                {
                    (setter as IDisposable)?.Dispose();
                }
            }

            _activator?.Dispose();
        }

        void IStyleActivatorSink.OnNext(bool value, int tag)
        {
            ValueStore?.FrameActivationChanged(this);
        }
    }
}
