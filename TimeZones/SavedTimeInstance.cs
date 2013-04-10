using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeZones
{
	public class SavedTimeInstance
	{
		public double UTCOffset { get; set; }
		public string Description { get; set; }
        public bool Hearted { get; set; }
        public bool Military { get; set; }
        public string Location { get; set; }
        public Guid Id { get; set; }
	}
}