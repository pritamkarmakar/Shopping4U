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
using System.Device.Location;
using Microsoft.Phone.Tasks;
using Microsoft.Advertising.Mobile.UI;

namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class ProductSortPage : PhoneApplicationPage
    {
        private GeoCoordinateWatcher gcw = null;

        public ProductSortPage()
        {

            InitializeComponent();
            AdControl.TestMode = false;

            sortList.SelectedIndex = (Application.Current as App).SelectedSortListItemIndex;

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

        private void Pricelowest_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (Application.Current as App).SelectedSortListItemIndex = 1;
            sortList.SelectedIndex = (Application.Current as App).SelectedSortListItemIndex;

            if (!(Application.Current as App).IsApplicationExited)
            {                
                (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderBy(price => price.Price).ToList();
            }
            else
                (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderBy(price => price.Price).ToList();
            Uri pritam = new Uri("/ResultsPage.xaml?param1=ProductSortPage", UriKind.Relative);
            NavigationService.Navigate(pritam);
                                                               
        }

        private void PriceHighest_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
            (Application.Current as App).SelectedSortListItemIndex = 2;
            sortList.SelectedIndex = (Application.Current as App).SelectedSortListItemIndex;

            if (!(Application.Current as App).IsApplicationExited)
            {               
                (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderByDescending(price => price.Price).ToList();
            }
            else
                (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderByDescending(price => price.Price).ToList();

            Uri pritam = new Uri("/ResultsPage.xaml?param1=ProductSortPage", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void NewlyAdded_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (Application.Current as App).SelectedSortListItemIndex = 3;
            sortList.SelectedIndex = (Application.Current as App).SelectedSortListItemIndex;

            if (!(Application.Current as App).IsApplicationExited)
            {
                (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderByDescending(date => date.UpdatedDate).ToList();
            }
            else
                (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderByDescending(date => date.UpdatedDate).ToList();
            
            Uri pritam = new Uri("/ResultsPage.xaml?param1=ProductSortPage", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }        

        private void BestMatch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (Application.Current as App).SelectedSortListItemIndex = 0;
            sortList.SelectedIndex = (Application.Current as App).SelectedSortListItemIndex;

            if (!(Application.Current as App).IsApplicationExited)
            {
                (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList;
            }
            else
                (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList;

            Uri pritam = new Uri("/ResultsPage.xaml?param1=ProductSortPage", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void ListBoxItem_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            (Application.Current as App).HasPressedBackButton = true;
        }
    }
}