using System.Windows;
using Tweetinvi;
using Tweetinvi.Events;

namespace Nottifier
{
    public partial class DebugTools : Window
    {
        public DebugTools()
        {
            InitializeComponent();
        }

        private void buttonRecibirTweetAncho_Click(object sender, RoutedEventArgs e)
        {
            new AlertWindow(new TweetReceivedEventArgs(Tweet.GetTweet(866685387124736001), "Debug: imagen ancha"));
        }

        private void buttonRecibirTweetAlto_Click(object sender, RoutedEventArgs e)
        {
            new AlertWindow(new TweetReceivedEventArgs(Tweet.GetTweet(866376603235999745), "Debug: imagen alta"));
        }

        private void buttonRecibirTweetVideo_Click(object sender, RoutedEventArgs e)
        {
            new AlertWindow(new TweetReceivedEventArgs(Tweet.GetTweet(866428900058038272), "Debug: gifv"));
        }
    }
}
