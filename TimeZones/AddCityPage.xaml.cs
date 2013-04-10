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

namespace TimeZones
{
    public partial class AddCityPage : PhoneApplicationPage
    {
        private List<CityTZ> dataSource;
		
		public AddCityPage()
        {
            InitializeComponent();
			InitializeTimeZones();
        }

		private void InitializeTimeZones()
		{
			if ((dataSource == null) || (dataSource.Count == 0))	
			{
                dataSource = IsolatedStorageHelper.ReadAllKnownTimeZones();
			}

            this.acBox.ItemsSource = dataSource;
            this.acBox.ItemFilter = SearchCities;
		}

        bool SearchCities(string search, object value)
        {
            if (value != null)
            {
                CityTZ datasourceValue = value as CityTZ;
                string name = datasourceValue.City;
                if (name.ToLower().StartsWith(search.ToLower()))
                    return true;
            }
            return false;
        }

        private void AddNewTimeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Random rand = new Random();

            string selectedCity = this.acBox.Text;
            int foundIndex = -1;

            for (int i = 0; i < dataSource.Count; i++)
            {
                if (dataSource[i].City.Equals(selectedCity, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex >= 0)
            {
                SavedTimeInstance timeInstance = new SavedTimeInstance();
                timeInstance.Location = selectedCity;
                timeInstance.Description = this.CustomTagTextBox.Text;
				timeInstance.Hearted = (bool) heartCheckBox.IsChecked;
                timeInstance.Military = (bool) militaryTimeCheckBox.IsChecked;
                timeInstance.UTCOffset = dataSource[foundIndex].Offset;
                timeInstance.Id = Guid.NewGuid();
                IsolatedStorageHelper.AddToTimeZoneFile(timeInstance);
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Must Select a Valid or Known City!");
            }
        }
    }
}
