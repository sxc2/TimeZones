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
using Microsoft.Phone.Tasks;

namespace TimeZones
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
		}

        private void RateReviewText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                marketplaceReviewTask.Show();
            }
            catch
            {
                MessageBox.Show("Marketplace cannot be reached and app cannot be found. Please try to review later - Thank you!");
            }
        }

        private void EmailSupportText_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "sophiaxcui@gmail.com";
            emailComposeTask.Body = "My Timezones Support Email";
            emailComposeTask.Subject = "[WP]My Timezones Support Email";
            emailComposeTask.Show(); 
        }
    }
}
