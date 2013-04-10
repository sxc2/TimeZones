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
using System.Xml;
using System.Xml.Serialization;

using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Resources;

namespace TimeZones
{
	public static class IsolatedStorageHelper
	{
		/// <summary>
		/// Constant file name for list of persisted time zones
		/// </summary>
		private const string SavedTimeZoneFileName = "locList.xml";

        /// <summary>
        /// Constant file name for list of persisted time zones
        /// </summary>
        private const string ConstCityZoneFileName = "citiesFinal.xml";
        
        /// <summary>
        /// Retrieves stored time zones
        /// </summary>
        /// <returns>persisted time zones</returns>
        public static List<CityTZ> ReadAllKnownTimeZones()
        {
            List<CityTZ> allTimeZones = new List<CityTZ>();
            TextReader reader = null;
            TextWriter writer = null;
            try
            {
                using (IsolatedStorageFile isoStorage =
                    IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isoStorage.FileExists(ConstCityZoneFileName))
                    {
                        StreamResourceInfo sri = Application.GetResourceStream(
                            new Uri("/TimeZones;component/" + ConstCityZoneFileName, UriKind.Relative));
                        reader = new StreamReader(sri.Stream);
                        XmlSerializer xs = new XmlSerializer(
                            typeof(List<CityTZ>));
                        allTimeZones.AddRange(
                            (List<CityTZ>)xs.Deserialize(reader));
                        reader.Close();
                        IsolatedStorageFileStream file =
                         isoStorage.OpenFile(
                             ConstCityZoneFileName,
                             FileMode.Create);
                        writer = new StreamWriter(file);
                        xs.Serialize(writer, allTimeZones);
                        writer.Close(); 
                    }
                    else
                    {
                        IsolatedStorageFileStream file =
                            isoStorage.OpenFile(
                                ConstCityZoneFileName,
                                FileMode.Open);
                        reader = new StreamReader(file);

                        XmlSerializer xs = new XmlSerializer(
                            typeof(List<CityTZ>));
                        allTimeZones.AddRange(
                            (List<CityTZ>)xs.Deserialize(reader));
                        reader.Close();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Read from Isolated Storage Exception:" + exception.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();

                if (writer != null)
                    writer.Dispose();
            }

            return allTimeZones;
        }
		
		/// <summary>
		/// Retrieves stored time zones
		/// </summary>
		/// <returns>persisted time zones</returns>
		public static List<SavedTimeInstance> ReadFromTimeZoneFile()
		{
			List<SavedTimeInstance> savedTimeZones = null;
			TextReader reader = null;
			try
            {
                using (IsolatedStorageFile isoStorage = 
					IsolatedStorageFile.GetUserStoreForApplication())
				{
					if (isoStorage.FileExists(SavedTimeZoneFileName))
					{
                        savedTimeZones = new List<SavedTimeInstance>();
						IsolatedStorageFileStream file = 
							isoStorage.OpenFile(
								SavedTimeZoneFileName, 
								FileMode.Open);
						reader = new StreamReader(file);
						
						XmlSerializer xs = new XmlSerializer(
							typeof(List<SavedTimeInstance>));
						savedTimeZones.AddRange(
							(List<SavedTimeInstance>)xs.Deserialize(reader));
						reader.Close(); 
					}
				}
            }
            catch (Exception exception)
            {
                MessageBox.Show("Read from Isolated Storage Exception:" + exception.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose(); 
            }	
			
			return savedTimeZones;
		}
		
		/// <summary>
		/// Add an additional time zone instance to isolated storage
		/// </summary>
		/// <param name="timeInstance">time instance to add</param>
		public static void AddToTimeZoneFile(SavedTimeInstance timeInstance)
		{
			List<SavedTimeInstance> readInstances = ReadFromTimeZoneFile();

            if (readInstances == null)
            {
                readInstances = new List<SavedTimeInstance>();
            }

            if (timeInstance.Hearted)
            {
                readInstances.Insert(0, timeInstance);
            }
            else
            {
                readInstances.Add(timeInstance);
            }

            WriteTimeZoneFile(readInstances);
		}
		
		/// <summary>
		/// Saves time zone to file
		/// </summary>
		/// <param name="timeZones">timezones to save</param>
		public static void WriteTimeZoneFile(List<SavedTimeInstance> timeZones)
		{
			TextWriter writer = null;
			try
            {
                using (IsolatedStorageFile isoStorage = 
					IsolatedStorageFile.GetUserStoreForApplication())
				{
					IsolatedStorageFileStream file = 
						isoStorage.OpenFile(
							SavedTimeZoneFileName, 
							FileMode.Create);
					writer = new StreamWriter(file);
					
					XmlSerializer xs = new XmlSerializer(
						typeof(List<SavedTimeInstance>));
					xs.Serialize(writer, timeZones); 
					writer.Close(); 
				}
            }
            catch (Exception exception)
            {
                MessageBox.Show("Write to isolated storage Exception: " + exception.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Dispose(); 
            }	
		}
	}
}