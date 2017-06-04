using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Tweetinvi;
using Tweetinvi.Models;
using System.IO;

namespace Nottifier
{
    public partial class LoginDialog : Window
    {
        private string credentialsFileName = "cred.bin";

        private string consumerKey = "";
        private string consumerSecret = "";
        IAuthenticationContext authContext;

        public LoginDialog()
        {
            if (!UseCredentialsFromFile()) SendAuthRequest();
            else GoToConfig();
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                string pin = ((TextBox)sender).Text;
                EnteredPin(pin);
                GoToConfig();
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            string pin = insertPinBox.Text;
            EnteredPin(pin);
            GoToConfig();
        }

        private void GoToConfig()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Application.Current.MainWindow = new ConfigWindow();
            this.Close();
        }

        private void SendAuthRequest()
        {
            authContext = AuthFlow.InitAuthentication(new TwitterCredentials(consumerKey, consumerSecret));
            Process.Start(authContext.AuthorizationURL);
        }

        private void EnteredPin (string pin)
        {
            var userCredentials = AuthFlow.CreateCredentialsFromVerifierCode(pin, authContext);
            Auth.SetCredentials(userCredentials);

            CreateUserCredentialsFile(userCredentials.AccessToken, userCredentials.AccessTokenSecret);
        }

        private bool UseCredentialsFromFile()
        {
            try
            {
                FileStream fs = new FileStream(credentialsFileName, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                string accessToken = br.ReadString();
                string accessTokenSecret = br.ReadString();
                Auth.SetUserCredentials(consumerKey, consumerSecret, accessToken, accessTokenSecret);
                return true;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        private bool CreateUserCredentialsFile(string accessToken, string accessTokenSecret)
        {
            try
            {
                FileStream fs = new FileStream(credentialsFileName, FileMode.OpenOrCreate);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(accessToken);
                bw.Write(accessTokenSecret);
                bw.Close();
                fs.Close();
                return true;
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
    }
}
