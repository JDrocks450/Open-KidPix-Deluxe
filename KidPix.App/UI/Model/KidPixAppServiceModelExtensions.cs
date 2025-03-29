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
        
        /// <summary>
        /// Creates a One-Way binding relationship (KidPix -> WPF only) where the incoming <typeparamref name="KPPropertyType"/> value is converted using a user-defined <paramref name="Converter"/>
        /// and set to the WPF <paramref name="Property"/>
        /// </summary>
        /// <typeparam name="KPPropertyType"></typeparam>
        /// <typeparam name="WPFPropertyType"></typeparam>
        /// <param name="Property"></param>
        /// <param name="Owner"></param>
        /// <param name="KPProperty"></param>
        /// <param name="Converter"></param>
        public static void BindKPtoWPFOneWay<KPPropertyType,WPFPropertyType>(this DependencyProperty Property, 
            DependencyObject Owner, KidPixDependencyProperty<KPPropertyType> KPProperty, Func<KPPropertyType,WPFPropertyType> Converter)
        {
            Owner.SetValue(Property, Converter(KPProperty)); //so clean its scary
            KPProperty.ValueChanged += delegate {
                Owner.SetValue(Property, Converter(KPProperty));
            };
        }
    }
}
