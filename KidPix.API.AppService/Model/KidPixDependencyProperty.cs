namespace KidPix.API.AppService.Model
{
    public interface IKidPixDependencyProperty
    {
        /// <summary>
        /// Gets the name of this property
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the value of this property
        /// </summary>
        /// <returns></returns>
        public object? GetValue();
        /// <summary>
        /// Gets the <see cref="Type"/> of the value stored in this property
        /// </summary>
        /// <returns></returns>
        public Type GetPropertyType();

        /// <summary>
        /// Internal use only. Using this function will just crash your app, no need to try it.
        /// </summary>
        void SetTypeInfo(KidPixDependencyObject Parent, string PropertyName);

        public delegate void ValueChangedEventHandler(KidPixDependencyObject Parent, IKidPixDependencyProperty Property);
        public event ValueChangedEventHandler ValueChanged;
    }

    /// <summary>
    /// A property which other objects can listen for changes to its stored value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class KidPixDependencyProperty<T> : IKidPixDependencyProperty
    {
        private KidPixDependencyObject parent;
        private T? _value;

        public delegate void OnValueChangedCallback(KidPixDependencyObject Instance, T OldValue, T NewValue);
        private readonly OnValueChangedCallback? Internal_OnValueChangedListener = null;

        /// <summary>
        /// Gets (or sets) the value of this <see cref="KidPixDependencyProperty{T}"/>
        /// </summary>
        /// <returns></returns>
        public T? Value
        {
            get => _value;
            set
            {
                Internal_OnValueChangedListener?.Invoke(parent, _value, value);
                if (AutoDispose && _value is IDisposable disposable) disposable.Dispose();
                _value = value;                
                ValueChanged?.Invoke(parent, this);
            }
        }
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value dictating if when a new <see cref="Value"/> is set to this <see cref="IKidPixDependencyProperty"/>, it will 
        /// dispose of the old value as to not create accidental memory leaks.
        /// <para/>This setting is ON by default, however if you have some special scenarios where this gets in your way, you may turn it off at your own risk.
        /// <para/>This will only guarantee the old value is disposed if a new value is set using the <see cref="Value"/> property <see langword="set"/> accessor
        /// </summary>
        public bool AutoDispose { get; set; } = true;
        
        public event IKidPixDependencyProperty.ValueChangedEventHandler ValueChanged;
        
        event IKidPixDependencyProperty.ValueChangedEventHandler IKidPixDependencyProperty.ValueChanged
        {
            add => this.ValueChanged += value;
            remove => this.ValueChanged -= value;
        }

        public static implicit operator T(KidPixDependencyProperty<T> Object) => Object.Value;
        
        internal KidPixDependencyProperty(T? myValue = default, OnValueChangedCallback OnValueChangedCallbackFunction = default)
        {           
            _value = myValue;
            Internal_OnValueChangedListener = OnValueChangedCallbackFunction;
        }

        public void SetTypeInfo(KidPixDependencyObject Parent, string PropertyName)
        {
            if (parent != null) return;// throw new InvalidOperationException("Do not call SetTypeInfo() in your code.");
            Name = PropertyName;
            parent = Parent;
        }

        public object? GetValue() => _value;

        public Type GetPropertyType() => typeof(T);
    } 
}
