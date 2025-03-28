using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.AppService.Model
{
    /// <summary>
    /// Base class for objects that want to expose their properties to events when they change value
    /// </summary>
    public abstract class KidPixDependecyObject
    {
        public delegate void ObjectValueChangedEventHandler(KidPixDependecyObject Owner, string PropertyName, object? NewValue);
        /// <summary>
        /// Occurs whenever any property on this object has been updated to a new value
        /// </summary>
        public event ObjectValueChangedEventHandler ObjectValueChanged;

        protected static KidPixDependencyProperty<T> RegisterProperty<T>(T InitValue = default) => new KidPixDependencyProperty<T>(InitValue);

        protected KidPixDependecyObject()
        {
            RegisterInternal();
        }

        /// <summary>
        /// Uses cursed reflection to get around hurdles I imposed because I wanted pretty code when making properties that are <see cref="KidPixDependencyProperty{T}"/> :(
        /// </summary>
        private void RegisterInternal()
        {
            foreach (PropertyInfo propertyTypeInfo in GetType().GetProperties().Where(x => x.PropertyType.IsAssignableTo(typeof(IKidPixDependencyProperty))))
            {
                IKidPixDependencyProperty property = propertyTypeInfo.GetValue(this) as IKidPixDependencyProperty;
                property.SetTypeInfo(this, propertyTypeInfo.Name);

                //hack
                property.ValueChanged += async delegate
                {
                    ObjectValueChanged?.Invoke(this, property.Name, property.GetValue());
                };
            }
        }
    }
}
