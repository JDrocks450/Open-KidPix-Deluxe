using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KidPix.ResourceExplorer.Controls
{
    /// <summary>
    /// Interaction logic for BreakdownTextBlock.xaml
    /// </summary>
    public partial class BreakdownTextBlock : TextBlock
    {
        public static readonly DependencyProperty PreviewObjectProperty = 
            DependencyProperty.Register(nameof(PreviewObject), typeof(Object), typeof(BreakdownTextBlock));

        public Object PreviewObject
        {
            get => GetValue(PreviewObjectProperty);
            set => Breakdown(value);                                  
        }

        public BreakdownTextBlock()
        {
            InitializeComponent();            
        }

        public void Breakdown(object? Object)
        {
            Inlines.Clear();
            SetValue(PreviewObjectProperty, Object);
            if (Object == null) return;

            foreach (var property in Object.GetType().GetProperties())
            {
                Inlines.Add(new Run(property.Name.PadRight(20))
                {
                    FontStyle = FontStyles.Italic
                });
                Inlines.Add(new Run(property.GetValue(Object)?.ToString() ?? "null"));
                Inlines.Add(new LineBreak());
            }
        }
    }
}
