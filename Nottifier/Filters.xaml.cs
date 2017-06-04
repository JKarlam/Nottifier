using System.Windows;
using System.Windows.Controls;

namespace Nottifier
{
    public partial class Filters : Window
    {
        private string name;

        public Filters(string name)
        {
            this.name = name;

            InitializeComponent();
            labelName.Content += name;

            RefreshFilterList();
            this.Show();
        }

        private void RefreshFilterList()
        {
            listView.ItemsSource = DBHelper.GenerateFilterList(name);
        }

        private void buttonRemoveFilter_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveFilter(name, (string)(sender as Button).DataContext);
            RefreshFilterList();
        }

        private void buttonAddFilter_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.AddFilter(name, textBoxNewFilter.Text);
            RefreshFilterList();
        }
    }
}
