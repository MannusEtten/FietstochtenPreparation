﻿using BikeTouringGIS.Controls;
using BikeTouringGISLibrary.Enumerations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BikeTouringGIS.Converters
{
    public class LayerIsGpxRouteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var typeReturn = (string)parameter;
            var layer = (BikeTouringGISLayer)value;
            if(!string.IsNullOrEmpty(typeReturn) &&  typeReturn.Contains("boolean"))
            {
                if(layer == null)
                {
                    return false;
                }
                return layer.Type == LayerType.GPXRoute;
            }
            if (layer == null)
            {
                return Visibility.Collapsed;
            }
            return layer.Type == LayerType.GPXRoute ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}