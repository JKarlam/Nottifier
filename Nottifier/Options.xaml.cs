using System.Windows;
using Tweetinvi;

namespace Nottifier
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
            textBoxFadeTime.Text = ConfigManager.dic["timeForAlertToFade"].ToString();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveAll();

            var l1 = User.GetFriends(User.GetAuthenticatedUser());
            foreach (var item in l1)
            {
                DBHelper.AddName(item.ScreenName);
            }
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveAll();
        }

        private void textBoxFadeTime_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.dic["timeForAlertToFade"] = int.Parse(textBoxFadeTime.Text);
            ConfigManager.WriteConfigFile();
        }
    }
}
