using System;
using System.IO;
using System.Collections.Generic;
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
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace Pritam.W7MobileApp.Shopping4U
{
    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        private IsolatedStorageSettings userSetting = IsolatedStorageSettings.ApplicationSettings;        

        //private int productToDownload = 500;
        private int apiSearchStartIndex = 0;

        // Declare a private variable to store application state.
        private string _applicationDataObject;

        // Declare an event for when the application data changes.
        public event EventHandler ApplicationDataObjectChanged;

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
        }

        // Declare a public property to access the application data variable.
        public string ApplicationDataObject
        {
            get { return _applicationDataObject; }
            set
            {
                if (value != _applicationDataObject)
                {
                    _applicationDataObject = value;
                    OnApplicationDataObjectChanged(EventArgs.Empty);
                }
            }
        }

        // Create a method to raise the ApplicationDataObjectChanged event.
        protected void OnApplicationDataObjectChanged(EventArgs e)
        {
            EventHandler handler = ApplicationDataObjectChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Declare a public property to store the status of the application data.
        public string ApplicationDataStatus { get; set; }


        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            if (userSetting.Contains("EnableLocationAd"))
            {
                (Application.Current as App).EnableLocationAd = (bool)userSetting["EnableLocationAd"];
            }
            else
                (Application.Current as App).EnableLocationAd = true;

            if (userSetting.Contains("ApplicationIdle"))
            {
                (Application.Current as App).ApplicationIdleDetection = (bool)userSetting["ApplicationIdle"];
            }
            else
                (Application.Current as App).ApplicationIdleDetection = false;

            if ((Application.Current as App).ApplicationIdleDetection)
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
            }
            else
            PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Enabled;
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            (Application.Current as App).SearchTerm = Convert.ToString(userSetting["SearchTerm"]);
            (Application.Current as App).IsApplicationExited = true;
            (Application.Current as App).SortedProductList = (List<ProductInventory>)(userSetting["ProductsList"]);
            (Application.Current as App).SelectedSortListItemIndex = Convert.ToInt32(userSetting["SelectedSortIndex"]);
            (Application.Current as App).CountryCode = Convert.ToString(userSetting["CountryCode"]);
            (Application.Current as App).ProductToDownload = Convert.ToInt32(userSetting["ProductToDownLoad"]);
            (Application.Current as App).APISearchStartIndex = Convert.ToInt32(userSetting["APISearchStartIndex"]);
            (Application.Current as App).SelectedBrand = Convert.ToString(userSetting["SelectedBrand"]);
            (Application.Current as App).CachedProductList = (List<ProductInventory>)(userSetting["CacheProductForBrandList"]);
            (Application.Current as App).SelectedBrandIndex = (List<int>)(userSetting["SelectedBrandIndex"]);
            (Application.Current as App).EnableLocationAd = (bool)userSetting["EnableLocationAd"];
            (Application.Current as App).ApplicationIdleDetection = (bool)userSetting["ApplicationIdle"];
            (Application.Current as App).LastVisitedUrl = Convert.ToString(userSetting["LastVisitedUrl"]);
            (Application.Current as App).LastVisitedProductDetails = Convert.ToString(userSetting["LastProductDetails"]);
            (Application.Current as App).LastVisitedRetailer = Convert.ToString(userSetting["LastVisitedRetailer"]);
            (Application.Current as App).UpdateSortPage = (bool)userSetting["UpdateSortPage"];
            (Application.Current as App).MasterProductList = (List<ProductInventory>)(userSetting["MasterProductList"]);
            (Application.Current as App).SelectedProduct = userSetting["SelectedProduct"]; 
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            if (!userSetting.Contains("SearchTerm"))
                userSetting.Add("SearchTerm", (Application.Current as App).SearchTerm);
            else
                userSetting["SearchTerm"] = (Application.Current as App).SearchTerm;

            if (!userSetting.Contains("ProductsList"))
                userSetting.Add("ProductsList", (Application.Current as App).SortedProductList);
            else
                userSetting["ProductsList"] = (Application.Current as App).SortedProductList;

            if (!userSetting.Contains("SelectedSortIndex"))
                userSetting.Add("SelectedSortIndex", (Application.Current as App).SelectedSortListItemIndex);
            else
                userSetting["SelectedSortIndex"] = (Application.Current as App).SelectedSortListItemIndex;

            if (!userSetting.Contains("CountryCode"))
                userSetting.Add("CountryCode", (Application.Current as App).CountryCode);
            else
                userSetting["CountryCode"] = (Application.Current as App).CountryCode;

            if (!userSetting.Contains("ProductToDownLoad"))
                userSetting.Add("ProductToDownLoad", (Application.Current as App).ProductToDownload);
            else
                userSetting["ProductToDownLoad"] = (Application.Current as App).ProductToDownload;

            if (!userSetting.Contains("APISearchStartIndex"))
                userSetting.Add("APISearchStartIndex", (Application.Current as App).APISearchStartIndex);
            else
                userSetting["APISearchStartIndex"] = (Application.Current as App).APISearchStartIndex;

            if (!userSetting.Contains("SelectedBrand"))
                userSetting.Add("SelectedBrand", (Application.Current as App).SelectedBrand);
            else
                userSetting["SelectedBrand"] = (Application.Current as App).SelectedBrand;

            if (!userSetting.Contains("CacheProductForBrandList"))
                userSetting.Add("CacheProductForBrandList", (Application.Current as App).CachedProductList);
            else
                userSetting["CacheProductForBrandList"] = (Application.Current as App).CachedProductList;

            if (!userSetting.Contains("SelectedBrandIndex"))
                userSetting.Add("SelectedBrandIndex", (Application.Current as App).SelectedBrandIndex);
            else
                userSetting["SelectedBrandIndex"] = (Application.Current as App).SelectedBrandIndex;

            if (!userSetting.Contains("EnableLocationAd"))
                userSetting.Add("EnableLocationAd", (Application.Current as App).EnableLocationAd);
            else
                userSetting["EnableLocationAd"] = (Application.Current as App).EnableLocationAd;

            if (!userSetting.Contains("ApplicationIdle"))
                userSetting.Add("ApplicationIdle", (Application.Current as App).ApplicationIdleDetection);
            else
                userSetting["ApplicationIdle"] = (Application.Current as App).ApplicationIdleDetection;

            if (!userSetting.Contains("LastVisitedUrl"))
                userSetting.Add("LastVisitedUrl", (Application.Current as App).LastVisitedUrl);
            else
                userSetting["LastVisitedUrl"] = (Application.Current as App).LastVisitedUrl;

            if (!userSetting.Contains("LastProductDetails"))
                userSetting.Add("LastProductDetails", (Application.Current as App).LastVisitedProductDetails);
            else
                userSetting["LastProductDetails"] = (Application.Current as App).LastVisitedProductDetails;

            if (!userSetting.Contains("LastVisitedRetailer"))
                userSetting.Add("LastVisitedRetailer", (Application.Current as App).LastVisitedRetailer);
            else
                userSetting["LastVisitedRetailer"] = (Application.Current as App).LastVisitedRetailer;

            if (!userSetting.Contains("UpdateSortPage"))
                userSetting.Add("UpdateSortPage", (Application.Current as App).UpdateSortPage);
            else
                userSetting["UpdateSortPage"] = (Application.Current as App).UpdateSortPage;

            if (!userSetting.Contains("MasterProductList"))
                userSetting.Add("MasterProductList", (Application.Current as App).MasterProductList);
            else
                userSetting["MasterProductList"] = (Application.Current as App).MasterProductList;

            if (!userSetting.Contains("SelectedProduct"))
                userSetting.Add("SelectedProduct", (Application.Current as App).SelectedProduct);
            else
                userSetting["SelectedProduct"] = (Application.Current as App).SelectedProduct;
        }
        

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }



        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            //RootFrame = new PhoneApplicationFrame();
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion

        public List<ProductInventory> MasterProductList
        {
            get;
            set;
        }

        public List<ProductInventory> TempProductList
        {
            get;
            set;
        }

        public List<ProductInventory> CachedProductList
        {
            get;
            set;
        }

        public List<ProductInventory> SortedProductList
        {
            get;
            set;
        }

        public string SelectedBrand
        {
            get;
            set;
        }

        public string CountryCode
        {
            get;
            set;
        }

        public string SearchData
        {
            get;
            set;
        }

        public string SearchTerm
        {
            get;
            set;
        }

        public bool UpdateSortPage
        {
            get;
            set;
        }

        public bool IsApplicationExited
        {
            get;
            set;
        }

        public int SelectedSortListItemIndex
        {
            get;
            set;
        }

        public List<int> SelectedBrandIndex
        {
            get;
            set;
        }

        public int ProductToDownload
        {
            get;
            set;
        }

        public int APISearchStartIndex
        {
            get
            {
                return this.apiSearchStartIndex;
            }
            set
            {
                this.apiSearchStartIndex = value;
            }
        }

        public string LastVisitedUrl
        {
            get;
            set;
        }

        public string LastVisitedProductDetails
        {
            get;
            set;
        }

        public string LastVisitedRetailer
        {
            get;
            set;
        }

        public int ResultEndIndex
        {
            get;
            set;
        }

        public bool EnableLocationAd
        {
            get;
            set;
        }

        public bool ApplicationIdleDetection
        {
            get;
            set;
        }

        public bool HasPressedBackButton
        {
            get;
            set;
        }

        public object SelectedProduct
        {
            get;
            set;
        }
    }
}