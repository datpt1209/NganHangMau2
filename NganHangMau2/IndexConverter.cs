using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NganHangMau2
{
    public class IndexConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var row = values[0] as DataGridRow;
            if (row != null)
            {
                return (row.GetIndex() + 1).ToString(); // 1-based index as string
            }
            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

