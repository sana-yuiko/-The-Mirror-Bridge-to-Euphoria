﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;

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

    [ValueConversion(typeof(Brush), typeof(string))]
    class ConverterContentToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "准备阶段":
                    return Brushes.Red;
                case "行动阶段":
                    return Brushes.DarkOrange;
                case "结束阶段":
                    return Brushes.CornflowerBlue;
                default:
                    return Brushes.Blue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "准备阶段":
                    return Brushes.Red;
                case "行动阶段":
                    return Brushes.DarkOrange;
                case "结束阶段":
                    return Brushes.CornflowerBlue;
                default:
                    return Brushes.Blue;
            }
        }
    }
}