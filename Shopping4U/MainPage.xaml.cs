using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Json;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Advertising.Mobile.UI;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;


namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class MainPage : PhoneApplicationPage
    {
        private GeoCoordinateWatcher gcw = null;

        /// <summary>
        /// MainPage constructor
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            AdControl.TestMode = false;

            if ((Application.Current as App).SearchTerm != null)
            this.autoCompleteSearchBox.Text = (Application.Current as App).SearchTerm;
                        
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

        /// <summary>
        /// Search image click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchImage_MouseEnter(object sender, MouseEventArgs e)
        {
            (Application.Current as App).CountryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            (Application.Current as App).SearchTerm = autoCompleteSearchBox.Text;

            (Application.Current as App).SelectedBrand = string.Empty;
            (Application.Current as App).SearchData = string.Empty;
            (Application.Current as App).CachedProductList = null;
            (Application.Current as App).UpdateSortPage = true;

            if (autoCompleteSearchBox.Text != string.Empty & autoCompleteSearchBox.Text != "enter product to search")
            {
                Uri pritam = new Uri("/ResultsPage.xaml?param1=MainPage", UriKind.Relative);
                NavigationService.Navigate(pritam);
            }
            else
            {
                MessageBox.Show("Please provide product name");
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            (Application.Current as App).SearchTerm = autoCompleteSearchBox.Text;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        
        /// <summary>
        /// Setting application icon click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/SettingsPage.xaml?param1=" + "/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void InfoApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/Info.xaml?param1=" + "/MainPage.xaml", UriKind.Relative));
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "pritam83@gmail.com";
            emailComposeTask.Subject = "Shopping4U feedback";
            emailComposeTask.Show();
        }

        private void autoCompleteSearchBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (autoCompleteSearchBox.Text == "enter product to search")
            this.autoCompleteSearchBox.Text = string.Empty;
        }

        private void autoCompleteSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (Application.Current as App).CountryCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
                (Application.Current as App).SearchTerm = autoCompleteSearchBox.Text;

                (Application.Current as App).UpdateSortPage = true;
                (Application.Current as App).SelectedBrand = string.Empty;
                (Application.Current as App).SearchData = string.Empty;
                (Application.Current as App).CachedProductList = null;                

                if (autoCompleteSearchBox.Text != string.Empty & autoCompleteSearchBox.Text != "enter product to search")
                {
                    Uri pritam = new Uri("/ResultsPage.xaml?param1=MainPage", UriKind.Relative);
                    NavigationService.Navigate(pritam);
                }
                else
                {
                    MessageBox.Show("Please provide product name");
                }
            }
        }

        private void RateApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
    }
}