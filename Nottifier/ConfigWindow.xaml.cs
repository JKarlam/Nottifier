using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Tweetinvi;
using Tweetinvi.Streaming;
using Xceed.Wpf.Toolkit;

namespace Nottifier
{
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            ConfigManager.Start();
            DBHelper.OpenConnection();
            //new DebugTools().Show();
            this.Show();
            
            RefreshNamesList();
            TweetinviEvents.QueryBeforeExecute += (sender, args) => Debug.WriteLine(RateLimit.GetQueryRateLimit(args.QueryURL));
            OpenUserTimelineStream();

            //ocultar al minimizar, mostrar con doble clic
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon()
            {
                Icon = Properties.Resources.icon,
                Visible = true
            };
            ni.MouseClick +=
                delegate (object sender, System.Windows.Forms.MouseEventArgs args)
                {
                    if(args.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        if (this.Visibility == Visibility.Visible) this.Hide();
                        else
                        {
                            this.Show();
                            this.WindowState = System.Windows.WindowState.Normal;
                        }
                    }
                };
            ni.ContextMenu = new System.Windows.Forms.ContextMenu(CreateTrayIconMenu());
        }

        public System.Windows.Forms.MenuItem[] CreateTrayIconMenu()
        {
            System.Windows.Forms.MenuItem[] mi = new System.Windows.Forms.MenuItem[1];
            mi[0] = new System.Windows.Forms.MenuItem(Properties.Resources.exitText);
            mi[0].Click += (sender, args) => this.Close();

            return mi;
        }

        // Minimizar al system tray
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        // Abrir flujo de tweets
        private void OpenUserTimelineStream()
        {
            IUserStream userStream = Tweetinvi.Stream.CreateUserStream();
            userStream.TweetCreatedByAnyoneButMe += (sender, e) =>
            {
                if(DBHelper.GenerateNamesList().Contains(e.Tweet.CreatedBy.ScreenName))
                    this.Dispatcher.Invoke(() => new AlertWindow(e));
            };
            userStream.StreamStopped += (sender, args) =>
            {
                Debug.WriteLine(args.Exception);
                Debug.WriteLine(args.DisconnectMessage);
            };
            userStream.StartStreamAsync();
        }

        private void RefreshNamesList()
        {
            listView.ItemsSource = DBHelper.GenerateNamesDataList();
        }

        private void colorPickerBackground_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            string[] s = (string[])((sender as ColorPicker).DataContext);
            DBHelper.UpdateBackgroundColor(s[0], e.NewValue.Value.ToString());
        }

        private void colorPickerText_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            string[] s = (string[])((sender as ColorPicker).DataContext);
            DBHelper.UpdateTextColor(s[0], e.NewValue.Value.ToString());
        }

        private void buttonAddName_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxAddName.Text != "")
            {
                DBHelper.AddName(textBoxAddName.Text);
                textBoxAddName.Text = "";
                RefreshNamesList();
            }
        }

        private void textBoxScreenName_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string[] s = (string[])((sender as TextBox).DataContext);
                DBHelper.ChangeName(s[0], (sender as TextBox).Text);
                Debug.WriteLine("Nombre cambiado");
                RefreshNamesList();
            }
        }

        private void buttonRemoveName_Click(object sender, RoutedEventArgs e)
        {
            string[] s = (string[])((sender as Button).DataContext);
            DBHelper.RemoveName(s[0]);
            Debug.WriteLine("Nombre eliminado");
            RefreshNamesList();
        }

        private void buttonFilters_Click(object sender, RoutedEventArgs e)
        {
            new Filters(((string[])((sender as Button).DataContext))[0]);
        }

        private void buttonOptions_Click(object sender, RoutedEventArgs e)
        {
            Options o = new Options();
            o.Closed += (s, args) => RefreshNamesList();
            o.Show();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveAll();

            var l1 = User.GetFriends(User.GetAuthenticatedUser());
            foreach (var item in l1)
                DBHelper.AddName(item.ScreenName);

            RefreshNamesList();
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            DBHelper.RemoveAll();
            RefreshNamesList();
        }

        private void buttonWeb_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/javierbm90/Nottifier");
        }

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            new About().Show();
        }

        private void fadeTime4_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetFadeTime(4000);
        }

        private void fadeTime6_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetFadeTime(6000);
        }

        private void fadeTime8_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetFadeTime(8000);
        }

        private void fadeTime10_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetFadeTime(10000);
        }

        private void fadeTime12_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetFadeTime(12000);
        }
    }
}
