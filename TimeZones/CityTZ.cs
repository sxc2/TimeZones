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
	public class CityTZ
	{
        public string City { get; set; }
        public double Offset { get; set; }

        public override string ToString()
        {
            return City; 
        }
	}
}