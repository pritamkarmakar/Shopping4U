using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using System.Device.Location;
using Facebook;
using Microsoft.Phone.Tasks;
using Microsoft.Advertising.Mobile.UI;

namespace Pritam.W7MobileApp.Shopping4U
{  
    
    public partial class ProductDetails : PhoneApplicationPage
    {
        private GeoCoordinateWatcher gcw = null;

        public ProductDetails()
        {
            InitializeComponent();
            AdControl.TestMode = false;

            if ((Application.Current as App).EnableLocationAd)
            {
                this.gcw = new GeoCoordinateWatcher();
                this.gcw.Start();
                this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            }
        }

        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.gcw.Stop();
            adControl.Location = new Microsoft.Advertising.Mobile.UI.Location(e.Position.Location.Latitude, e.Position.Location.Longitude);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.performanceProgressBar.Visibility = Visibility.Visible;
            this.performanceProgressBar.IsIndeterminate = true;
            base.OnNavigatedTo(e);
        }

        private void webBrowser1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            this.performanceProgressBar.IsIndeterminate = false;
            this.performanceProgressBar.Visibility = Visibility.Collapsed;
        }
        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            string lastVisitedUrl = (Application.Current as App).LastVisitedUrl;
            if (lastVisitedUrl != string.Empty)
            {
                webBrowser1.Navigate(new Uri((Application.Current as App).LastVisitedUrl));
                this.webBrowser1.Tag = lastVisitedUrl;
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {          
            (Application.Current as App).HasPressedBackButton = true;
            //NavigationService.GoBack();
        }


        private void RefreshApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            this.performanceProgressBar.Visibility = Visibility.Visible;
            this.performanceProgressBar.IsIndeterminate = true;

            string lastVisitedUrl = (Application.Current as App).LastVisitedUrl;
            if (lastVisitedUrl != string.Empty)
            {
                webBrowser1.Navigate(new Uri((Application.Current as App).LastVisitedUrl));
                this.webBrowser1.Tag = lastVisitedUrl;
            }
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            LoginToFacebook();
        }

        private void InfoApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Info.xaml?param1=" + "/MainPage.xaml", UriKind.Relative));
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "pritam83@gmail.com";
            emailComposeTask.Subject = "Shopping4U feedback";
            emailComposeTask.Show();
        }

        private string link, title;

        #region Facebook       

        
        private readonly string[] _extendedPermissions = new[] { "publish_stream" };

        private bool _loggedIn = false;

        private FacebookClient _fbClient;

        private void LoginToFacebook()
        {
            browserAuth.IsScriptEnabled = true;
            browserAuth.Navigated += FacebookLoginBrowser_Navigated;


            //InfoPanel.Visibility = Visibility.Collapsed;

            performanceProgressBar.IsIndeterminate = true;

            var loginParameters = new Dictionary<string, object>
                                      {
                                          { "response_type", "token" },
                                          { "display", "touch" } // by default for wp7 builds only (in Facebook.dll), display is set to touch.
                                      };

            var navigateUrl = FacebookOAuthClient.GetLoginUrl("213946495309561", null, _extendedPermissions, loginParameters);

            browserAuth.Navigate(navigateUrl);
        }

        private void FacebookLoginBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //FacebookOAuthResult.TryParse()
            FacebookOAuthResult oauthResult;
            if (FacebookOAuthResult.TryParse(e.Uri, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    _fbClient = new FacebookClient(oauthResult.AccessToken);
                    _loggedIn = true;
                    ShareToFacebook();                    
                }
                else
                {
                    //listSharing.Visibility = System.Windows.Visibility.Visible;
                    browserAuth.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                //listSharing.Visibility = System.Windows.Visibility.Collapsed;
                browserAuth.Visibility = Visibility.Visible;
            }
            performanceProgressBar.IsIndeterminate = false;
        }

        private void ShareToFacebook()
        {
            performanceProgressBar.IsIndeterminate = true;

            browserAuth.Visibility = System.Windows.Visibility.Collapsed;
            performanceProgressBar.Visibility = System.Windows.Visibility.Visible;
            var parameters = new Dictionary<string, object>
                 {
                     //{"description", title},
                     {"link", (Application.Current as App).LastVisitedUrl},
                     //{"name", title}
                 };
            _fbClient.PostCompleted += new EventHandler<FacebookApiEventArgs>(fbApp_PostCompleted);
            _fbClient.PostAsync("me/feed", parameters);
        }

        void fbApp_PostCompleted(object sender, FacebookApiEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                performanceProgressBar.IsIndeterminate = false;
                MessageBox.Show("You have succesfully posted product details to your facebook profile.");

                browserAuth.Navigated -= FacebookLoginBrowser_Navigated;
            });
        }

        #endregion

        private void browserAuth_Navigating(object sender, NavigatingEventArgs e)
        {
            performanceProgressBar.IsIndeterminate = true;
        }

        private void RateApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void EmailApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();        
            emailComposeTask.Subject = (Application.Current as App).LastVisitedProductDetails + " from: " + (Application.Current as App).LastVisitedRetailer;
            emailComposeTask.Body = "Found this product from windows phone 7 app Shopping4u \n" + (Application.Current as App).LastVisitedUrl;
            emailComposeTask.Show();
        }
    }
}