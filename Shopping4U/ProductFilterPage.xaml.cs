using System;
using System.Collections;
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
using System.Device.Location;
using Microsoft.Advertising.Mobile.UI;

namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class ProductFilterPage : PhoneApplicationPage
    {
        private GeoCoordinateWatcher gcw = null;

        /// <summary>
        /// ProductFilterPage constructor
        /// </summary>
        public ProductFilterPage()
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
            if ((Application.Current as App).CachedProductList != null)
            {
                this.brandCheckList.ListBoxDataContext = from p in (Application.Current as App).CachedProductList
                                                         where p.Brand != string.Empty
                                                         select p;

                int i = 0;
                if ((Application.Current as App).SelectedBrandIndex != null)
                {
                    foreach (CheckBox checkbox in brandCheckList.Items)
                    {
                        if ((Application.Current as App).SelectedBrandIndex.Contains(i))
                        {
                            checkbox.IsChecked = true;
                        }
                        i++;
                    }
                }

            }

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
        /// this method will call on navigation from other pages
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (Application.Current as App).IsApplicationExited = false;
            if ((Application.Current as App).UpdateSortPage)
            {
                List<ProductInventory> UniqueBrand = (Application.Current as App).MasterProductList.GroupBy(Product => Product.Brand).Select(Product => Product.First()).ToList();
                this.brandCheckList.ListBoxDataContext = from p in UniqueBrand
                                                         where p.Brand != string.Empty
                                                         select p;
                (Application.Current as App).CachedProductList = UniqueBrand;
                (Application.Current as App).UpdateSortPage = false;
            }
        }

        /// <summary>
        /// filter application bar icon click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            (Application.Current as App).SelectedBrand = string.Empty;
            (Application.Current as App).UpdateSortPage = false;
            (Application.Current as App).SortedProductList = null;
            (Application.Current as App).SelectedBrandIndex = new List<int>();

            int i = 0;
            foreach (CheckBox checkbox in brandCheckList.Items)
            {
                if ((bool)checkbox.IsChecked)
                {
                    (Application.Current as App).SelectedBrand += checkbox.Content + "|";
                    (Application.Current as App).SelectedBrandIndex.Add(i);
                }
                i++;
            }
            if ((Application.Current as App).SelectedBrand != string.Empty)
            {
                (Application.Current as App).SelectedBrand = (Application.Current as App).SelectedBrand.Remove((Application.Current as App).SelectedBrand.LastIndexOf('|'));
            }
            Uri pritam = new Uri("/ResultsPage.xaml?param1=ProductFilterPage", UriKind.Relative);
            NavigationService.Navigate(pritam);
        }

        private void InfoApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Info.xaml?param1=" + "/MainPage.xaml", UriKind.Relative));
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            (Application.Current as App).HasPressedBackButton = true;
        }
    }

    /// <summary>
    /// custom checkListBox control
    /// </summary>
    public partial class CheckListBox : ListBox
    {
        public static readonly DependencyProperty ListBoxDataContextProperty =
            DependencyProperty.Register("ListBoxDataContext", typeof(IEnumerable), typeof(CheckListBox), new PropertyMetadata(new PropertyChangedCallback(ListBoxDataContextChanged)));

        public IEnumerable ListBoxDataContext
        {
            get
            {
                return (IEnumerable)base.GetValue(ListBoxDataContextProperty);
            }

            set
            {
                base.SetValue(ListBoxDataContextProperty, value);
            }
        }

        public static void ListBoxDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            List<CheckBox> dataList = new List<CheckBox>();

            CheckListBox control = d as CheckListBox;
           
            CheckBox box;
            Binding binding;

            IEnumerable list = e.NewValue as IEnumerable;

            if (list != null)
            {
                foreach (object item in list)
                {
                    box = new CheckBox();

                    Type typeofitem = item.GetType();
                    PropertyInfo[] itemproperties = typeofitem.GetProperties();                    
                    
                    foreach (PropertyInfo pi in itemproperties)
                    {
                        if (pi.Name == control.CheckBoxContentProperty)
                        {
                            binding = new Binding(control.CheckBoxContentProperty);
                            binding.Source = item;
                            binding.Mode = BindingMode.OneWay;                            
                            box.SetBinding(CheckBox.ContentProperty, binding);
                            ((CheckBox)box).Background = new SolidColorBrush(Colors.White);
                            ((CheckBox)box).BorderThickness = new Thickness(0);
                            ((CheckBox)box).Foreground = new SolidColorBrush(Colors.White);
                        }
                    }

                    dataList.Add(box);
                }
            }

            control.ItemsSource = dataList;
        }

        public string CheckBoxContentProperty { get; set; }
    }    
}