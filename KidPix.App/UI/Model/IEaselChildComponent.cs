using KidPix.App.UI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace KidPix.App.UI.Model
{
    /// <summary>
    /// An interface that exposes a property to get the visual parent of this object by type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface ITypedVisualObjectChildComponent<T> where T : DependencyObject
    {
        /// <summary>
        /// A reference to this component's visual parent matching type <typeparamref name="T"/>
        /// </summary>
        public T? MyTypedParent
        {
            get
            {
                DependencyObject obj = this as DependencyObject;
                if (obj == default) throw new Exception($"You can only use {nameof(ITypedVisualObjectChildComponent<T>)} on a DependencyObject in the visual tree");
                while (obj != null)
                {
                    if (obj is T easel) return easel;
                    try
                    {
                        obj = VisualTreeHelper.GetParent(obj);                        
                    }
                    catch
                    {
                        break;
                    }
                }
                return null;
            }
        }
    }
}
