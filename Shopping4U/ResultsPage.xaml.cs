using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Net;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using System.Device.Location;
using Microsoft.Advertising.Mobile.UI;


namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class ResultsPage : PhoneApplicationPage
    {
        ScrollViewer scrollViewer = null;
        //public readonly DependencyProperty ListVerticalOffsetProperty = DependencyProperty.Register("ListVerticalOffset",
        //typeof(double), typeof(ResultsPage), new PropertyMetadata(new PropertyChangedCallback(OnListVerticalOffsetChanged)));
        ObservableCollection<ProductInventory> data = new ObservableCollection<ProductInventory>();
        private static bool moreDownloadComplete = true;
        private GeoCoordinateWatcher gcw = null;
        
        // LongList selectedItem object
        private object selectedItem = null;

        /// <summary>
        /// ResultPage Constructor
        /// </summary>
        public ResultsPage()
        {
            InitializeComponent();
            AdControl.TestMode = false;

            //resultList.Loaded += new RoutedEventHandler(resultList_Loaded);

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
        /// This method will call each time ResultPage navigated from different pages
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!(Application.Current as App).HasPressedBackButton)
            {
                if (this.NavigationContext.QueryString["param1"] == "MainPage" || this.NavigationContext.QueryString["param1"] == "ProductFilterPage")
                {
                    this.NavigationContext.QueryString["param1"] = string.Empty;
                    if (!(Application.Current as App).IsApplicationExited)
                    {
                        (Application.Current as App).ProductToDownload = 200;
                        (Application.Current as App).APISearchStartIndex = 1;
                        
                        this.performanceProgressBar.Visibility = Visibility.Visible;
                        this.performanceProgressBar.IsIndeterminate = true;                           
                        base.OnNavigatedTo(e);
                        SearchText.Text = "''" + (Application.Current as App).SearchTerm.ToUpper() + "''";
                        WebClient webClient = new WebClient();
                        string productName = (Application.Current as App).SearchTerm;
                        (Application.Current as App).IsApplicationExited = false;
                        webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
                        if ((Application.Current as App).SelectedBrand == string.Empty)
                            webClient.DownloadStringAsync(new Uri("https://www.googleapis.com/shopping/search/v1/public/products?key=AIzaSyDwuKYxOqWFajnd4_dSGkfmplivLKhBtOM&country=" + (Application.Current as App).CountryCode + "&thumbnails=90:*&maxResults=" + (Application.Current as App).ProductToDownload + "&q=" + productName + "&alt=atom"));
                        else
                            webClient.DownloadStringAsync(new Uri("https://www.googleapis.com/shopping/search/v1/public/products?key=AIzaSyDwuKYxOqWFajnd4_dSGkfmplivLKhBtOM&country=" + (Application.Current as App).CountryCode + "&thumbnails=90:*&restrictBy=brand=" + (Application.Current as App).SelectedBrand + "&maxResults=" + (Application.Current as App).ProductToDownload + "&q=" + productName + "&alt=atom"));
                    }
                    else
                    {
                        SearchText.Text = "''" + (Application.Current as App).SearchTerm.ToUpper() + "''";
                        resultList.ItemsSource = (Application.Current as App).SortedProductList;
                        (Application.Current as App).IsApplicationExited = false;
                        (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                        (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                    }
                }

                else if ((Application.Current as App).SortedProductList != null)
                {
                    SearchText.Text = "''" + (Application.Current as App).SearchTerm.ToUpper() + "''";
                    (Application.Current as App).IsApplicationExited = false;
                    //(Application.Current as App).ResultEndIndex = 100;
                    //resultList.ItemsSource = (Application.Current as App).SortedProductList.Take((Application.Current as App).ResultEndIndex);
                    resultList.ItemsSource = (Application.Current as App).SortedProductList;
                    //(Application.Current as App).ResultEndIndex = (Application.Current as App).ResultEndIndex + 100;
                    this.performanceProgressBar.IsIndeterminate = false;
                    this.performanceProgressBar.Visibility = Visibility.Collapsed;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                }
            }
            else
            {
                if ((Application.Current as App).SelectedProduct != null)
                {
                    SearchText.Text = "''" + (Application.Current as App).SearchTerm.ToUpper() + "''";
                    resultList.ItemsSource = (Application.Current as App).SortedProductList;
                    (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                    (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                    (Application.Current as App).IsApplicationExited = false;
                    //resultList.ScrollTo((Application.Current as App).SelectedProduct);
                }
            }
            (Application.Current as App).HasPressedBackButton = false;
        }        

        /// <summary>
        /// This event handler method will call once API data download completed by WebClient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("There is an error occured to get the data please try again");
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            (Application.Current as App).SearchData = e.Result;
            XElement xmlTweets = XElement.Parse((Application.Current as App).SearchData);
            string count = xmlTweets.Element("{http://a9.com/-/spec/opensearchrss/1.0/}totalResults").Value;

            if (Convert.ToInt32(count) > 0)
            {
                (Application.Current as App).MasterProductList = ((IEnumerable<ProductInventory>)from tweet in xmlTweets.Descendants("{http://www.w3.org/2005/Atom}entry")
                                                                                                 select new ProductInventory
                                                                                                 {
                                                                                                     Message = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}title") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}title").Value : string.Empty,
                                                                                                     ImageSource = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}images").Element("{http://www.google.com/shopping/api/schemas/2010}image").Element("{http://www.w3.org/2005/Atom}thumbnail").Attribute("link") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}images").Element("{http://www.google.com/shopping/api/schemas/2010}image").Element("{http://www.w3.org/2005/Atom}thumbnail").Attribute("link").Value : string.Empty,
                                                                                                     Url = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}link") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}link").Value : string.Empty,
                                                                                                     Price = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price") != null ? Convert.ToDouble(tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Value, CultureInfo.InvariantCulture) : 0,
                                                                                                     Currency = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Attribute("currency") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Attribute("currency").Value : string.Empty,
                                                                                                     RetailerName = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}author") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}author").Element("{http://www.google.com/shopping/api/schemas/2010}name").Value : string.Empty,
                                                                                                     Brand = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}brand") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}brand").Value : string.Empty,
                                                                                                     UpdatedDate = tweet.Element("{http://www.w3.org/2005/Atom}updated") != null ? Convert.ToDateTime(tweet.Element("{http://www.w3.org/2005/Atom}updated").Value) : DateTime.MinValue
                                                                                                 }).ToList();


                switch ((Application.Current as App).SelectedSortListItemIndex)
                {

                    case 0:
                        (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList;                        
                        resultList.ItemsSource = (Application.Current as App).SortedProductList;                        
                        break;
                    case 1:
                        (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderBy(price => price.Price).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList;                        
                        break;
                    case 2:
                        (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderByDescending(price => price.Price).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList;                        
                        break;
                    case 3:
                        (Application.Current as App).SortedProductList = (Application.Current as App).MasterProductList.OrderByDescending(date => date.UpdatedDate).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList;                        
                        break;
                }
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
            }
            else
            {
                MessageBox.Show("There is an error occured to get the data please try again");
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
            }
        }

        /// <summary>
        /// Method to download more product details once user reaches to end of listbox
        /// </summary>
        public void DownloadMore()
        {
            this.performanceProgressBar.Visibility = Visibility.Visible;
            this.performanceProgressBar.IsIndeterminate = true;

            WebClient webClient = new WebClient();
            string productName = (Application.Current as App).SearchTerm;
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadMoreDataCompleted);

            if ((Application.Current as App).SelectedBrand == string.Empty)
                webClient.DownloadStringAsync(new Uri("https://www.googleapis.com/shopping/search/v1/public/products?key=AIzaSyDwuKYxOqWFajnd4_dSGkfmplivLKhBtOM&country=" + (Application.Current as App).CountryCode + "&thumbnails=90:*&maxResults=" + (Application.Current as App).ProductToDownload + "&q=" + productName + "&alt=atom&startIndex=" + (Application.Current as App).APISearchStartIndex));
            else
                webClient.DownloadStringAsync(new Uri("https://www.googleapis.com/shopping/search/v1/public/products?key=AIzaSyDwuKYxOqWFajnd4_dSGkfmplivLKhBtOM&country=" + (Application.Current as App).CountryCode + "&thumbnails=90:*&restrictBy=brand=" + (Application.Current as App).SelectedBrand + "&maxResults=" + (Application.Current as App).ProductToDownload + "&q=" + productName + "&alt=atom&startIndex=" + (Application.Current as App).APISearchStartIndex));
        }

        /// <summary>
        ///  This event handler method will call once API data download completed by WebClient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_DownloadMoreDataCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("There is an error occured to get the data please try again");
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                return;
            }
            (Application.Current as App).SearchData = e.Result;
            XElement xmlTweets = XElement.Parse((Application.Current as App).SearchData);
            int count = xmlTweets.Elements("entry").Count();

            if ((Application.Current as App).SearchData.Contains("entry"))
            {
                (Application.Current as App).TempProductList = ((IEnumerable<ProductInventory>)from tweet in xmlTweets.Descendants("{http://www.w3.org/2005/Atom}entry")
                                                                                               select new ProductInventory
                                                                                               {
                                                                                                   Message = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}title") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}title").Value : string.Empty,
                                                                                                   ImageSource = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}images") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}images").Element("{http://www.google.com/shopping/api/schemas/2010}image").Element("{http://www.w3.org/2005/Atom}thumbnail").Attribute("link").Value : string.Empty,
                                                                                                   Url = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}link") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}link").Value : string.Empty,
                                                                                                   Price = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories") != null ? Convert.ToDouble(tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Value) : 0,
                                                                                                   Currency = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Attribute("currency") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}inventories").Element("{http://www.google.com/shopping/api/schemas/2010}inventory").Element("{http://www.google.com/shopping/api/schemas/2010}price").Attribute("currency").Value : string.Empty,
                                                                                                   RetailerName = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}author") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}author").Element("{http://www.google.com/shopping/api/schemas/2010}name").Value : string.Empty,
                                                                                                   Brand = tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}brand") != null ? tweet.Element("{http://www.google.com/shopping/api/schemas/2010}product").Element("{http://www.google.com/shopping/api/schemas/2010}brand").Value : string.Empty,
                                                                                                   UpdatedDate = Convert.ToDateTime(tweet.Element("{http://www.w3.org/2005/Atom}updated").Value)
                                                                                               }).ToList();

                switch ((Application.Current as App).SelectedSortListItemIndex)
                {

                    case 0:
                        foreach (ProductInventory PI in (Application.Current as App).TempProductList)
                            (Application.Current as App).SortedProductList.Add(PI);                        
                        resultList.ItemsSource = (Application.Current as App).SortedProductList.Skip((Application.Current as App).APISearchStartIndex - 1);
                        break;
                    case 1:
                        (Application.Current as App).TempProductList = (Application.Current as App).TempProductList.OrderBy(price => price.Price).ToList();
                        foreach (ProductInventory PI in (Application.Current as App).TempProductList)
                            (Application.Current as App).SortedProductList.Add(PI);
                        (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderBy(price => price.Price).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList.Skip((Application.Current as App).APISearchStartIndex -1);
                        break;
                    case 2:
                        (Application.Current as App).TempProductList = (Application.Current as App).TempProductList.OrderByDescending(price => price.Price).ToList();
                        foreach (ProductInventory PI in (Application.Current as App).TempProductList)
                            (Application.Current as App).SortedProductList.Add(PI);
                        (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderByDescending(price => price.Price).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList.Skip((Application.Current as App).APISearchStartIndex - 1);
                        break;
                    case 3:
                        (Application.Current as App).TempProductList = (Application.Current as App).TempProductList.OrderByDescending(date => date.UpdatedDate).ToList();
                        foreach (ProductInventory PI in (Application.Current as App).TempProductList)
                            (Application.Current as App).SortedProductList.Add(PI);
                        (Application.Current as App).SortedProductList = (Application.Current as App).SortedProductList.OrderByDescending(date => date.UpdatedDate).ToList();
                        resultList.ItemsSource = (Application.Current as App).SortedProductList.Skip((Application.Current as App).APISearchStartIndex -1);
                        break;
                }
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                moreDownloadComplete = true;
            }
            else
            {
                //MessageBox.Show("There is no more results to show");
                this.performanceProgressBar.IsIndeterminate = false;
                this.performanceProgressBar.Visibility = Visibility.Collapsed;
                (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = true;
                (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = true;
                moreDownloadComplete = true;
            }
        }

        /// <summary>
        /// click event for search image button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/MainPage.xaml", UriKind.Relative);
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

        /// <summary>
        /// click event for filter button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/ProductFilterPage.xaml", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        /// <summary>
        /// click event for sort button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SortApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/ProductSortPage.xaml", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        /// <summary>
        /// click event for setting button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            Uri pritam = new Uri("/SettingsPage.xaml?param1=" + "/ResultsPage.xaml", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void resultList_ScrollingCompleted(object sender, EventArgs e)
        {            
            double actual = ((Microsoft.Phone.Controls.LongListSelector)(sender)).ActualHeight;
            double current = ((Microsoft.Phone.Controls.LongListSelector)(sender)).DesiredSize.Height;
            double diff = actual - current;
            if ((int)diff == 0 && actual != 0)
            {
                if (moreDownloadComplete)
                {
                    moreDownloadComplete = false;
                    (Application.Current as App).APISearchStartIndex = (Application.Current as App).ProductToDownload + (Application.Current as App).APISearchStartIndex ;
                    DownloadMore();
                }
                
            }            
        }

        private void GestureListener_Tap(object sender, GestureEventArgs e)
        {            
            (Application.Current as App).LastVisitedUrl = ((Pritam.W7MobileApp.Shopping4U.ProductInventory)(((System.Windows.FrameworkElement)(sender)).DataContext)).Url;
            (Application.Current as App).LastVisitedProductDetails = ((Pritam.W7MobileApp.Shopping4U.ProductInventory)(((System.Windows.FrameworkElement)(sender)).DataContext)).Message;
            (Application.Current as App).LastVisitedRetailer = ((Pritam.W7MobileApp.Shopping4U.ProductInventory)(((System.Windows.FrameworkElement)(sender)).DataContext)).RetailerName;
            (Application.Current as App).SelectedProduct = ((Pritam.W7MobileApp.Shopping4U.ProductInventory)(((System.Windows.FrameworkElement)(sender)).DataContext));
            Uri uri = new Uri("/ProductDetails.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void RateApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }
    }

    /// <summary>
    /// ProductInventory to hold individual product details
    /// </summary>
    public class ProductInventory
    {
        public string Message { get; set; }
        public string ImageSource { get; set; }
        public string Url { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string CurrencyPrice
        {
            get
            {
                return Currency + " " + Price.ToString();
            }
        }
        public string RetailerName { get; set; }
        public string Brand { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}