using Resto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Resto.Google_Api;

namespace Resto.Controllers
{
    public class SearchController : Controller
    {
        private RestoDB db = new RestoDB();

        /*
         * Hi Jaya,
         * We weren't able to search by city, since we did not includ that field in our Restaurant table, we've tried to workthrough the problem
         * but we did not succeed. Sorry for that.
         * 
         * Thank you for your time and have a great day!!
         * 
         * Regards, 
         * Frank Birikundavyi
         * Brandon Balala         
         */
        // GET: Search
        public ActionResult Search(String search)
        {
            List<Restaurant> query = (from s in db.Restaurants
                                      where s.RestoName.Contains(search) ||
                                      s.Genre.Contains(search) ||
                                      s.PostalCode.Contains(search) ||
                                      s.StreetName.Contains(search) ||
                                      s.Price.Equals(search)
                                      select s).ToList(); //||
                                      //Google.getCity(s.PostalCode).Contains(search)
                                      //select s).ToList();
            ViewBag.searchedValue = search;
            return View(query);
        }
    }
}