using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NganHangMau2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new InputUserControl();
            SidebarMenu.SelectedIndex = 0; // Select the first item by default
            SidebarColumn.Width = new GridLength(55); // Set initial width to match the collapsed state
        }

        private void SidebarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            SidebarColumn.Width = new GridLength(170);
        }

        private void SidebarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SidebarColumn.Width = new GridLength(55);
        }

        private void SidebarMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SidebarMenu.SelectedItem is ListBoxItem selectedItem)
            {
                switch (selectedItem.Tag)
                {
                    case "input":
                        MainContent.Content = new InputUserControl();
                        break;
                    case "output":
                        MainContent.Content = new OutputUserControl();
                        break;
                    case "search":
                        MainContent.Content = new SearchUserControl();
                        break;
                }
            }
        }
    }
}