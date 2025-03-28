using KidPix.API.AppService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KidPix.App.UI.Model
{
    internal static class KidPixAppServiceModelExtensions
    {
        /// <summary>
        /// Binds a WPF DependencyProperty to a KidPix AppService one
        /// </summary>
        /// <param name="Property"></param>
        /// <param name="KPProperty"></param>
        public static void Bind(this DependencyProperty Property, DependencyObject Owner, IKidPixDependencyProperty KPProperty)
        {
            Owner.SetValue(Property, KPProperty.GetValue());
            KPProperty.ValueChanged += delegate {
                Owner.SetValue(Property,KPProperty.GetValue());
            };
        }
    }
}
