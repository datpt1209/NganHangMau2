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
            var dataGrid = values[1] as DataGrid;

            if (row != null && dataGrid != null)
            {
                var item = row.Item;
                var itemsSource = dataGrid.ItemsSource as IEnumerable<object>;
                if (itemsSource != null)
                {
                    int index = itemsSource.ToList().IndexOf(item);
                    return (index + 1).ToString(); // 1-based index as string
                }
            }
            return "0";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

