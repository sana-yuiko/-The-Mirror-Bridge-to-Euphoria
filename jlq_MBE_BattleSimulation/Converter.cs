using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    [ValueConversion(typeof(Size), typeof(Thickness))]
    class ConverterMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Size size = (Size) value;
            return new Thickness(size.Width*2/5, size.Height/5, size.Width/5, size.Height/5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
