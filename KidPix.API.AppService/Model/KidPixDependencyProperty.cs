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
        void SetTypeInfo(KidPixDependecyObject Parent, string PropertyName);

        public delegate void ValueChangedEventHandler(KidPixDependecyObject Parent, IKidPixDependencyProperty Property);
        public event ValueChangedEventHandler ValueChanged;
    }

    /// <summary>
    /// A property which other objects can listen for changes to its stored value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class KidPixDependencyProperty<T> : IKidPixDependencyProperty
    {
        private KidPixDependecyObject parent;
        private T? _value;

        /// <summary>
        /// Gets (or sets) the value of this <see cref="KidPixDependencyProperty{T}"/>
        /// </summary>
        /// <returns></returns>
        public T? Value
        {
            get => _value;
            set
            {
                _value = value;
                ValueChanged?.Invoke(parent, this);
            }
        }
        public string Name { get; private set; }
        
        public event IKidPixDependencyProperty.ValueChangedEventHandler ValueChanged;
        
        internal KidPixDependencyProperty(T? myValue = default)
        {           
            _value = myValue;
        }

        event IKidPixDependencyProperty.ValueChangedEventHandler IKidPixDependencyProperty.ValueChanged
        {
            add => this.ValueChanged += value;
            remove => this.ValueChanged -= value;
        }

        public void SetTypeInfo(KidPixDependecyObject Parent, string PropertyName)
        {
            if (parent != null) throw new InvalidOperationException("Do not call SetTypeInfo() in your code.");
            Name = PropertyName;
            parent = Parent;
        }

        public object? GetValue() => _value;

        public Type GetPropertyType() => typeof(T);
    } 
}
