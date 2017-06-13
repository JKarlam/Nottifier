using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tweetinvi.Events;
using Tweetinvi.Models.Entities;

namespace Nottifier
{
    public partial class AlertWindow : Window
    {
        private TweetReceivedEventArgs tea;
        private bool stopAutoClosing = false;
        private bool windowPositionMoved = false;

        public AlertWindow(TweetReceivedEventArgs e)
        {
            if (!SearchForFilters(e)) this.Close();
            else
            {
                InitializeComponent();
                tea = e;
                Debug.WriteLine(DateTime.Now + ": " + e.Json);

                BitmapImage biUserIcon = new BitmapImage(new Uri(e.Tweet.CreatedBy.ProfileImageUrl));
                userIcon.Source = biUserIcon;
                userName.Content = e.Tweet.CreatedBy.Name;

                //string m = e.Tweet.DisplayTextRange==null? e.Tweet.Text:e.Tweet.Text.Substring(e.Tweet.DisplayTextRange[0], e.Tweet.DisplayTextRange[1] - e.Tweet.DisplayTextRange[0]);
                //messageTextBlock.Text = m;
                messageTextBlock.Text = e.Tweet.Text;
                //double messageLines = Math.Ceiling((double)m.Length / 50);
                double messageLines = Math.Ceiling((double)e.Tweet.Text.Length / 50);
                messageTextBlock.Height = messageLines * 20;

                List<IMediaEntity> medias;
                if (e.Tweet.ExtendedTweet == null)
                    medias = e.Tweet.Entities.Medias;
                else medias = e.Tweet.ExtendedTweet.ExtendedEntities.Medias;

                if (medias.Count != 0)
                {
                    if (medias[0].MediaType.Contains("animated_gif")) // Video
                    {
                        Debug.WriteLine("El tweet contiene un vídeo");
                        MediaElement me = new MediaElement()
                        {
                            Source = new Uri("http" + medias[0].VideoDetails.Variants[0].URL.Substring(5), UriKind.Absolute),
                            LoadedBehavior = MediaState.Play,
                            Width = xtraGrid.Width
                        };
                        xtraGrid.Children.Add(me);
                        xtraGrid.Margin = new Thickness(10, 50 + messageTextBlock.Height, 0, 10);
                        float imageHeightWidthRelation = (float)medias[0].Sizes["medium"].Height / (float)medias[0].Sizes["medium"].Width;
                        xtraGrid.Height = xtraGrid.Width * imageHeightWidthRelation;

                        SetWindowParams();
                    }
                    else if (medias[0].MediaType.Contains("photo")) // Una imagen
                    {
                        Debug.WriteLine("El tweet contiene una imagen");
                        BitmapImage biExtraImage = new BitmapImage(new Uri(medias[0].MediaURL));
                        Image i = new Image()
                        {
                            Width = xtraGrid.Width
                        };
                        xtraGrid.Children.Add(i);
                        xtraGrid.Margin = new Thickness(10, 50 + messageTextBlock.Height, 0, 10);
                        float imageHeightWidthRelation = (float)medias[0].Sizes["medium"].Height / (float)medias[0].Sizes["medium"].Width;
                        xtraGrid.Height = xtraGrid.Width * imageHeightWidthRelation;

                        biExtraImage.DownloadCompleted += (sender, args) =>
                        {
                            i.Source = biExtraImage;
                            SetWindowParams();
                        };
                    }
                }
                else // Nada (solo texto)
                {
                    Debug.WriteLine("El tweet NO contiene imagen ni vídeo");
                    SetWindowParams();
                }
            }
        }

        private bool SearchForFilters(TweetReceivedEventArgs e)
        {
            List<string> l = DBHelper.GenerateFilterList(e.Tweet.CreatedBy.ScreenName);
            if (l.Count == 0)
            {
                Debug.WriteLine("No hay filtros para esta cuenta, se muestra el tweet");
                return true;
            }
            else
            {
                foreach (string filter in l)
                {
                    if (e.Tweet.Text.Contains(filter))
                    {
                        Debug.WriteLine("El tweet contiene: " + filter + ", se muestra el tweet");
                        return true;
                    }
                }
                Debug.WriteLine("El tweet NO contiene ninguno de los filtros, no se muestra");
                return false;
            }
        }

        // Establece parámetros relacionados con la ventana
        private void SetWindowParams()
        {
            this.Show();
            double offset = Screen.PrimaryScreen.WorkingArea.Height - (ConfigManager.dic["alertWindowPositionY"] + aWindow.Height);

            aWindow.Left = ConfigManager.dic["alertWindowPositionX"];
            aWindow.Top = ConfigManager.dic["alertWindowPositionY"] + (offset < 0? offset : 0);

            string[] colors = DBHelper.GetColors(tea.Tweet.CreatedBy.ScreenName);
            aWindow.Background = (Brush)new BrushConverter().ConvertFromString(colors[0]);
            userName.Foreground = (Brush)new BrushConverter().ConvertFromString(colors[1]);
            messageTextBlock.Foreground = (Brush)new BrushConverter().ConvertFromString(colors[1]);

            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(ConfigManager.dic["timeForAlertToFade"]);
                if(!stopAutoClosing) this.Dispatcher.Invoke(Close);
            })).Start();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void messageTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Abriendo web: mensaje");
            Process.Start(tea.Tweet.Url);
        }

        private void userName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Abriendo web: autor");
            Process.Start("https://twitter.com/" + tea.Tweet.CreatedBy.ScreenName);
        }

        private void userIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Abriendo web: autor");
            Process.Start("https://twitter.com/" + tea.Tweet.CreatedBy.ScreenName);
        }

        private void aWindow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("Thread que cierra pausado");
            stopAutoClosing = true;
        }
        
        private void aWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (windowPositionMoved)
            {
                Debug.WriteLine("La ventana se ha movido, reescribiendo config.ini");
                ConfigManager.dic["alertWindowPositionX"] = (int)aWindow.Left;
                ConfigManager.dic["alertWindowPositionY"] = (int)aWindow.Top;
                ConfigManager.WriteConfigFile();
            }
        }

        // Arrastrar para mover la ventana
        private void dragGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            windowPositionMoved = true;
        }
    }
}
