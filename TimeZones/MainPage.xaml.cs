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
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace TimeZones
{
	/// <summary>
	/// Main page displaying time zones
	/// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
		/// Timer to keep track of refreshes
		/// </summary>
		private DispatcherTimer _timer;

		/// <summary>
		/// List of saved time instances
		/// </summary>
		private List<SavedTimeInstance> instances;
		
		/// <summary>
		/// Lock for threading 
		/// </summary>
		private Object thisLock = new Object();

        /// <summary>
        /// Number of maximum customized timezones
        /// </summary>
        private const byte MaxNumberOfTimeZones = 20;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        	CreateUI(this, null);
			RefreshUI(this, null);
	        _timer = new DispatcherTimer();
        	_timer.Interval = TimeSpan.FromSeconds(1);
        	_timer.Tick += RefreshUI;
        	_timer.Start();
        }
		
		protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			if (_timer != null)
			{
				_timer.Stop();
			}
		}

		public void CreateUI(object sender, EventArgs e)
		{
			if (IsolatedStorageHelper.ReadFromTimeZoneFile() == null)
			{	
				SavedTimeInstance defaultInstance2 = new SavedTimeInstance();
				defaultInstance2.Description = "The Big Apple & Bob lives here.";
                defaultInstance2.Location = "New York City, NY, US";
				defaultInstance2.UTCOffset = -5;
				defaultInstance2.Id = Guid.NewGuid();
			
                SavedTimeInstance defaultInstance = new SavedTimeInstance();
                defaultInstance.Description = "Asakusa Shrine & Jiro's Sushi!";
                defaultInstance.Location = "Tokyo, JP";
                defaultInstance.UTCOffset = 9;
                defaultInstance.Hearted = true;
                defaultInstance.Id = Guid.NewGuid();
                
                IsolatedStorageHelper.AddToTimeZoneFile(defaultInstance);
                IsolatedStorageHelper.AddToTimeZoneFile(defaultInstance2);      
            }
			
			this.instances = 
				IsolatedStorageHelper.ReadFromTimeZoneFile();
			
			// clean out children
            while (this.ListOfZones.Items.Count > 0)
			{
                this.ListOfZones.Items.RemoveAt(0);
			}
				
			// add new children
			for (int i = 0; i < this.instances.Count; i++)
			{
				TimeInstance timeInstance = new TimeInstance();
				timeInstance.SetTimerTime(
					(Style)this.Resources["TimerTimeStyle"], 
					"12:34 PM");
				timeInstance.SetTimerDate(
					(Style)this.Resources["TimerDateStyle"], 
					"12/12/12");
				timeInstance.SetTimerLabel(
					(Style)this.Resources["TimerLabelStyle"], 
					this.instances[i].Description);
				timeInstance.SetTimerLabelLoc(
					(Style)this.Resources["TimerLabelLocStyle"], 
					this.instances[i].Location);						
				timeInstance.SetDividerStyles(
					(Style)this.Resources["DividerStyle"],
					(Style)this.Resources["DividerTopStyle"]);	
				timeInstance.SetGridStyles(
					(Style)this.Resources["DayNightGridStyle"],
					(Style)this.Resources["HeartGridStyle"]);
				timeInstance.EnableHeart(this.instances[i].Hearted);
				timeInstance.militaryTime = this.instances[i].Military;
				timeInstance.Id = this.instances[i].Id;
					
                Thickness prevThick = timeInstance.Margin;
                prevThick.Bottom = 25;
                timeInstance.Margin = prevThick;
                timeInstance.UTCOffset = this.instances[i].UTCOffset;
					
				ContextMenu contextMenu = new ContextMenu();
				MenuItem removeMenuItem = new MenuItem() 
					{ Header = "Remove", Tag = "Remove"};
				removeMenuItem.Click += new RoutedEventHandler(ConMenuRemove_Click);
				
				contextMenu.Items.Add(removeMenuItem);
				contextMenu.Tag = timeInstance.Id;
				ContextMenuService.SetContextMenu(timeInstance,contextMenu);
                this.ListOfZones.Items.Add(timeInstance);
			}

			RefreshUI(this, null);
		}
		
		private void ConMenuRemove_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				lock (thisLock)
				{
					ContextMenu menu = ((ContextMenu)(sender as MenuItem).Parent);
					SavedTimeInstance source = null;
					TimeInstance instanceToDelete = null;
					for (int i = 0; i < this.instances.Count; i++)
					{
						if (this.instances[i].Id.ToString().Equals(menu.Tag.ToString()))
						{
							source = this.instances[i];
						}

                        if (((TimeInstance)this.ListOfZones.Items[i]).Id.ToString().Equals(
							menu.Tag.ToString()))
						{
                            instanceToDelete = (TimeInstance)this.ListOfZones.Items[i];
						}
					}
					
					this.instances.Remove(source);
                    this.ListOfZones.Items.Remove(instanceToDelete);
					IsolatedStorageHelper.WriteTimeZoneFile(this.instances);
					CreateUI(this, null);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show("Remove exception:" + exception.Message);
			}
		}
		
		public void RefreshUI(object sender, EventArgs e)
		{
			lock (thisLock)
			{
					for (int i = 0; i < this.instances.Count; i++)
					{
						RefreshOneTimeInstance(
                            (TimeInstance)this.ListOfZones.Items[i]);
					}
			}
		}
		
		private void RefreshOneTimeInstance(TimeInstance timer)
		{
			double offset = timer.UTCOffset;
			DateTime thisDate = DateTime.UtcNow.AddHours(offset);
            if (timer.ifCurrentDayLight())
            {
                thisDate = thisDate.AddHours(timer.ObserveDayLightSavings());
            }

			string currentTime;
			string dayofWeek = 
				thisDate.DayOfWeek.ToString();
			dayofWeek += '\n' + thisDate.Date.ToString().Substring(
				0, 
				thisDate.Date.ToString().IndexOf(" "));
			if (timer.militaryTime)
			{
				currentTime = thisDate.ToString("HH:mm");
			}
			else 
			{
				currentTime = thisDate.ToString("hh:mm tt");
			}
	
			timer.UpdateTimerTimeText(currentTime);
			timer.UpdateTimerDateText(dayofWeek);
			bool isNight = (thisDate.Hour < 7 || (thisDate.Hour > (12+7)));
			timer.UpdateDayNightIcon(isNight);
		}

		
		private void MenuAdd_Click(object sender, System.EventArgs e)
		{
            if (this.instances.Count < MaxNumberOfTimeZones)
            {
                NavigationService.Navigate(new Uri("/AddCityPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show(
                    "Reached max custom timezones: " + MaxNumberOfTimeZones 
                    + "\nPlease delete existing timezones to add more.");
            }
		}

        private void MenuAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void MenuRemoveAll_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to remove all custom time zones?", 
                "Remove All",
                MessageBoxButton.OKCancel);
            if (result.Equals(MessageBoxResult.OK) || result.Equals(MessageBoxResult.Yes))
            {
                try
                {
                    lock (thisLock)
                    {
                        int count = this.instances.Count - 1;
                        while (count >= 0)
                        {
                            this.instances.Remove(this.instances[count]);
                            this.ListOfZones.Items.Remove(this.ListOfZones.Items[count]);
                            count--;
                        }

                        IsolatedStorageHelper.WriteTimeZoneFile(this.instances);
                        CreateUI(this, null);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Remove exception:" + exception.Message);
                }
            }
        }
    }
	
	/// <summary>
	/// Helper class for each time instance
	/// </summary>
	public class TimeInstance : Grid
	{
		/// <summary>
		/// Private fields for text
		/// </summary>
		private TextBlock TimerTime;
		private TextBlock TimerLabel;
		private TextBlock TimerDate;
		private TextBlock TimerLabelLoc;
		private Rectangle Divider;
		private Rectangle DividerThin;
		private Grid DayNightGrid;
		private Grid HeartGrid;
		private bool setToNight;
        private int observeDayLightSavings;
        private string geoSector;
        
		/// <summary>
		/// Constructor
		/// </summary>
		public TimeInstance()
		{
			this.TimerTime = new TextBlock();
			this.TimerLabel = new TextBlock();
			this.TimerDate = new TextBlock();
			this.TimerLabelLoc = new TextBlock();
			this.Divider = new Rectangle();
			this.DividerThin = new Rectangle();
			this.DayNightGrid = new Grid();
			this.HeartGrid = new Grid();
			this.setToNight = true;
            this.Width = 450;
			this.Height = 105;
			this.VerticalAlignment = VerticalAlignment.Top;
			this.Children.Add(this.TimerLabel);
			this.Children.Add(this.TimerLabelLoc);
			this.Children.Add(this.TimerTime);
			this.Children.Add(this.TimerDate);
			this.Children.Add(this.Divider);
			this.Children.Add(this.DividerThin);
			this.Children.Add(this.DayNightGrid);
			this.Children.Add(this.HeartGrid);
            this.observeDayLightSavings = this.observeDLS();
      }
		
		public void UpdateTimerTimeText(string text)
		{
			if (this.TimerTime.Text != text)
			{
				this.TimerTime.Text = text;
			}
		}
		
		public void UpdateTimerDateText(string text)
		{
			if (this.TimerDate.Text != text)
			{
				this.TimerDate.Text = text;
			}
		}
		
		public void UpdateTimerLabelLocText(string text)
		{
			if (this.TimerLabelLoc.Text != text)
			{
				this.TimerLabelLoc.Text = text;
			}
		}
		
		public void UpdateDayNightIcon(bool isNight)
		{
            if (this.setToNight != isNight)
            {
                this.setToNight = isNight;
            }
			
            if (isNight)
			{
                if ((Visibility)App.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                {
                    this.DayNightGrid.Background =
                        new ImageBrush
                        {
                            ImageSource = (ImageSource)new
                                ImageSourceConverter().ConvertFromString(
                                    "appbar.moon.rest.png")
                        };
                }
                else
                {
                    this.DayNightGrid.Background =
                        new ImageBrush
                        {
                            ImageSource = (ImageSource)new
                                ImageSourceConverter().ConvertFromString(
                                    "appbar.moonlt.rest.png")
                        };
                }
			}
			else
			{
                if ((Visibility)App.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible)
                {
                    this.DayNightGrid.Background =
                        new ImageBrush
                        {
                            ImageSource = (ImageSource)new
                                ImageSourceConverter().ConvertFromString(
                                    "appbar.sun.rest.png")
                        };
                }
                else
                {
                    this.DayNightGrid.Background =
                    new ImageBrush
                    {
                        ImageSource = (ImageSource)new
                            ImageSourceConverter().ConvertFromString(
                                "appbar.sunlt.rest.png")
                    };
                }

			}
		}
		
		
		public double UTCOffset
		{
			set; 
			get;
		}
		
		public Guid Id
		{
			set; 
			get;
		}
		
		public bool militaryTime
		{
			set; 
			get;
		}

        public int ObserveDayLightSavings()
        {
            return this.observeDayLightSavings;
        }

        private DateTime findXthSunday(int xth, int month, int year)
        {
            DateTime sunday = new DateTime(year, month, (xth-1) * 7 + 1);

            while (sunday.DayOfWeek != DayOfWeek.Sunday)
            {
                sunday = sunday.AddDays(1);
            }

            return sunday;
        }

        private DateTime findLastSunday(int month, int year)
        {
            DateTime sunday = new DateTime(year, month, 31);

            while (sunday.DayOfWeek != DayOfWeek.Sunday)
            {
                sunday = sunday.AddDays(-1);
            }

            return sunday;
        }

        public bool ifCurrentDayLight()
        {
            DateTime current = DateTime.Now.ToUniversalTime();
            
            if (this.geoSector.ToUpper().Equals("NA"))
            {
                // second sunday march, to first sunday nov
                if ((current.CompareTo(findXthSunday(2, 3, current.Year)) > 0) &&
                    (current.CompareTo(findXthSunday(1, 11, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("EU"))
            {
                // last sunday march, to last sunday october
                if ((current.CompareTo(findLastSunday(3, current.Year)) > 0) &&
                   (current.CompareTo(findLastSunday(10, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("MX"))
            {
                // first sunday april, last sunday october
                if ((current.CompareTo(findXthSunday(1, 4, current.Year)) > 0) &&
                   (current.CompareTo(findLastSunday(10, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("BR"))
            {
                // third sunday october, third sunday feb
                if ((current.CompareTo(findXthSunday(3, 2, current.Year)) > 0) &&
                  (current.CompareTo(findXthSunday(3, 10, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("CL"))
            {
                // second sunday sep, last sunday apri
                if ((current.CompareTo(findLastSunday(4, current.Year)) > 0) &&
                   (current.CompareTo(findXthSunday(2, 9, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("SA"))
            {
                // first oct, fourth march
                if ((current.CompareTo(findXthSunday(4, 3, current.Year)) > 0) &&
                   (current.CompareTo(findXthSunday(1, 10, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("NZ"))
            {
                // last sunday sep, first sunday apr
                if ((current.CompareTo(findXthSunday(1, 4, current.Year)) > 0) &&
                   (current.CompareTo(findLastSunday(9, current.Year)) < 0))
                {
                    return true;
                }
            }

            if (this.geoSector.ToUpper().Equals("AF"))
            {
                // last april, last sep
                if ((current.CompareTo(findLastSunday(4, current.Year)) > 0) &&
                   (current.CompareTo(findLastSunday(9, current.Year)) < 0))
                {
                    return true;
                }
            }

            return false;
        }

        private int observeDLS()
        {
            // default to US, EU, CA, 
            // AU special case Oct, Apr
            // BR special case Oct, Apr
            string location = this.TimerLabelLoc.Text.Trim();
            string[] splits = location.Split(',');
            location = splits[splits.Length - 1].Trim();

            // north america
            if (location.ToUpper().Equals("US") || location.ToUpper().Equals("CA") || location.ToUpper().Equals("CU"))
            {
                this.geoSector = "NA";
                return 1;
            }

            // mexico
            if (location.ToUpper().Equals("MX"))
            {
                this.geoSector = "MX"; 
                return 1;
            }

            // europe and greenland
            if (location.ToUpper().Equals("GL"))
            {
                this.geoSector = "EU"; 
                return 1;
            } 
            
            if (location.ToUpper().Equals("FR") || location.ToUpper().Equals("GB") || location.ToUpper().Equals("ES"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("PT") || location.ToUpper().Equals("IE") || location.ToUpper().Equals("BE"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("NL") || location.ToUpper().Equals("DE") || location.ToUpper().Equals("CH"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("IT") || location.ToUpper().Equals("AT") || location.ToUpper().Equals("CZ"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("PL") || location.ToUpper().Equals("SK") || location.ToUpper().Equals("HU"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("HR") || location.ToUpper().Equals("BA") || location.ToUpper().Equals("RS"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("AL") || location.ToUpper().Equals("MK") || location.ToUpper().Equals("GR"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("BG") || location.ToUpper().Equals("RO") || location.ToUpper().Equals("MD"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("UA") || location.ToUpper().Equals("LT") || location.ToUpper().Equals("EE"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("LV") || location.ToUpper().Equals("SE") || location.ToUpper().Equals("FI"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("SK") || location.ToUpper().Equals("TR") || location.ToUpper().Equals("SY"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("JO") || location.ToUpper().Equals("LB") || location.ToUpper().Equals("IR"))
            {
                this.geoSector = "EU";
                return 1;
            }

            if (location.ToUpper().Equals("IL") || location.ToUpper().Equals("AZ") || location.ToUpper().Equals("IR"))
            {
                this.geoSector = "EU";
                return 1;
            }

            // israel and libya
            if (location.ToUpper().Equals("IS") || location.ToUpper().Equals("LY") )
            {
                this.geoSector = "EU";
                return 1;
            }

            // africa
            if (location.ToUpper().Equals("MA") || location.ToUpper().Equals("EH"))
            {
                this.geoSector = "AF";
                return 1;
            }

            if (location.ToUpper().Equals("NA"))
            {
                this.geoSector = "AF";
                return -1;
            }

            // south america
            if (location.ToUpper().Equals("CL")) 
            {
                this.geoSector = "CL";
                return -1;
            }

            if (location.ToUpper().Equals("BR"))
            {
                this.geoSector = "BR";
                return -1;
            }

            if (location.ToUpper().Equals("NZ"))
            {
                this.geoSector = "NZ";
                return -1;
            }

            if (location.ToUpper().Equals("PY") || location.ToUpper().Equals("UY") || location.ToUpper().Equals("WS"))
            {
                this.geoSector = "SA";
                return -1;
            }

            this.geoSector = "OTHER";
            return 0;
        }

		#region Initialization Styles and Text
		public void SetTimerTime(Style style, string text)
		{
			this.TimerTime.Style = style;
			this.TimerTime.Text = text;
		}
		
		public void SetTimerLabel(Style style, string text)
		{
			this.TimerLabel.Style = style;
			this.TimerLabel.Text = text;
		}
		
		public void SetTimerDate(Style style, string text)
		{
			this.TimerDate.Style = style;
			this.TimerDate.Text = text;
		}
		
		public void SetTimerLabelLoc(Style style, string text)
		{
			this.TimerLabelLoc.Style = style;
			this.TimerLabelLoc.Text = text;
            this.observeDayLightSavings = observeDLS();
		}
		
		public void SetDividerStyles(Style style, Style styleThin)
		{
			this.Divider.Style = style;
			this.DividerThin.Style = styleThin;
		}
		
		public void SetGridStyles(Style styleDay, Style styleHeart)
		{
			this.DayNightGrid.Style = styleDay;
			this.HeartGrid.Style = styleHeart;
		}
		
		public void EnableHeart(bool enable)
		{
			if (!enable)
			{
				this.HeartGrid.Visibility = Visibility.Collapsed;
			}
			else
			{
				this.HeartGrid.Visibility = Visibility.Visible;	
			}
		}
		#endregion
	}
}
