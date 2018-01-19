using Resto.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Resto.Google_Api
{
    public class Google
    {
        public static XElement GetGeocodingSearchResults(string address) //XElement holds the XML result, withing the System.Xml.Linq namespace
        {
            var url = String.Format("http://maps.google.com/maps/api/geocode/xml?address={0}&sensor=false", HttpUtility.UrlEncode(address));  //Url encode since it was provided by user

            // Load the XML into an XElement object
           
                var results = XElement.Load(url);
            
            
            return results;
        }

        public static DbGeography getLocation(string postalCode) {
            var result = Google.GetGeocodingSearchResults(postalCode);
            Location location = (from l in result.Descendants("location")
                                 select new Location
                                 {
                                     Latitude = System.Convert.ToDouble(l.Element("lat").Value),
                                     Longitude = System.Convert.ToDouble(l.Element("lng").Value)
                                 }).First();

            double lat = location.Latitude;
            double lng = location.Longitude;
            DbGeography loc = DbGeography.FromText(string.Format("POINT({1} {0})", lat, lng), 4326);
            return loc;
        }

        public static String getCity(string postalCode)
        {
            var result = Google.GetGeocodingSearchResults(postalCode);
            String address = (from a in result.Descendants("formatted_address")
                              select a).FirstOrDefault().Value;

            return address;
        }

    }
}