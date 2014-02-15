using System;
using System.IO.IsolatedStorage;
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
using Microsoft.Phone.Tasks;
using System.Windows.Navigation;
using System.Device.Location;
using Microsoft.Phone.Shell;
using Microsoft.Advertising.Mobile.UI;

namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public string navigatedFrom;
        private GeoCoordinateWatcher gcw = null;

        public SettingsPage()
        {
            InitializeComponent();
            AdControl.TestMode = false;
            //NavigationInTransition navigateInTransition = new NavigationInTransition();
            //navigateInTransition.Backward = new SlideTransition { Mode = SlideTransitionMode.SlideDownFadeIn };
            //navigateInTransition.Forward = new SlideTransition { Mode = SlideTransitionMode.SlideUpFadeIn };

            //NavigationOutTransition navigateOutTransition = new NavigationOutTransition();
            //navigateOutTransition.Backward = new SlideTransition { Mode = SlideTransitionMode.SlideDownFadeOut };
            //navigateOutTransition.Forward = new SlideTransition { Mode = SlideTransitionMode.SlideUpFadeOut };
            //TransitionService.SetNavigationInTransition(this, navigateInTransition);
            //TransitionService.SetNavigationOutTransition(this, navigateOutTransition);

            // Set toggleswitch checked property as per EnableLocationAd variable
            LocalAdToggleSwitch.IsChecked = (Application.Current as App).EnableLocationAd;
            //If enable location ad is true then enable gcw_PositionChanged event 
            if ((Application.Current as App).EnableLocationAd)
            {
                this.gcw = new GeoCoordinateWatcher();
                this.gcw.Start();
                this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            }
            ApplicationIdleDetection.IsChecked = (Application.Current as App).ApplicationIdleDetection;
        }

        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.gcw.Stop();
            adControl.Location = new Microsoft.Advertising.Mobile.UI.Location(e.Position.Location.Latitude, e.Position.Location.Longitude);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigatedFrom = this.NavigationContext.QueryString["param1"];            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            (Application.Current as App).HasPressedBackButton = true;
        }

        private void LocalAdToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).EnableLocationAd = false;
            LocalAdToggleSwitch.Content = "No";
        }

        private void LocalAdToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).EnableLocationAd = true;
            LocalAdToggleSwitch.Content = "Yes";
        }

        private void ApplicationIdleDetection_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationIdleDetection.Content = "No";
            (Application.Current as App).ApplicationIdleDetection = false;
        }

        private void ApplicationIdleDetection_Checked(object sender, RoutedEventArgs e)
        {
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            ApplicationIdleDetection.Content = "Yes";
            (Application.Current as App).ApplicationIdleDetection = true;
        }

        private void InfoApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Info.xaml?param1=" + "/MainPage.xaml", UriKind.Relative));
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "pritam83@gmail.com";
            emailComposeTask.Subject = "Shopping4U feedback";
            emailComposeTask.Show();
        }

        private void RateApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
    }
}