using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Resto.Models;
using Resto.Google_Api;
using System.Xml.Linq;
using System.Data.Entity.Spatial;
using Resto.Validators;


namespace Resto.Controllers
{
    public class HomeController : Controller
    {
        private RestoDB db = new RestoDB();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        /*
         *This action displays the five closest restaurant to the location
         */
        public ActionResult DisplayResto(int error, double Latitude, double Longitude, String PostalCode)
        {
            double lat = Latitude;
            double lng = Longitude;

            if (error != 0)
            {
                var result = Google.GetGeocodingSearchResults(PostalCode);

                String status = (string)(from s in result.Descendants("status")
                              select s).First();

                //Count should be 1 for it to be valid (1 <result>), if more then it found too many locations
                int count = (from c in result.Descendants("result")
                              select c).Count();
                
                if(status != "OK" || count != 1)
                {
                    //Defaut Location : DAWSON COLLEGE
                    lat = 45.489171;
                    lng = -73.587348;

                }
                else
                {
                    
                    Location location = (from l in result.Descendants("location")
                                         select new Location
                                         {
                                             Latitude = System.Convert.ToDouble(l.Element("lat").Value),
                                             Longitude = System.Convert.ToDouble(l.Element("lng").Value)
                                         }).First();

                    lat = location.Latitude;
                    lng = location.Longitude;
                }
            }

            DbGeography userLoc = DbGeography.FromText(string.Format("POINT({1} {0})", lat, lng), 4326);

            var closest = (from r in db.Restaurants
                           orderby r.Location.Distance(userLoc)  //userLoc is a DbGeography that represents the user’s location
                           select r).Take(5).ToList();




            return View(closest);
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);

            restaurant.Views = restaurant.Views + 1;
            db.Entry(restaurant).State = EntityState.Modified;
            db.SaveChanges();

            if (restaurant == null)
            {
                return HttpNotFound();
            }



            return View(restaurant);
        }

        [Authorize]
        // GET: Home/Create
        public ActionResult Create()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "$", Value = "$", Selected = true });
            items.Add(new SelectListItem { Text = "$$", Value = "$$" });
            items.Add(new SelectListItem { Text = "$$$", Value = "$$$" });
            items.Add(new SelectListItem { Text = "$$$$", Value = "$$$$" });
            items.Add(new SelectListItem { Text = "$$$$$", Value = "$$$$$" });
            ViewBag.Price = items;
            
            //ViewBag.EditBy = new SelectList(db.UserDetails, "UserName", "UserName");
            //ViewBag.EnteredBy = new SelectList(db.UserDetails, "UserName", "UserName");
            return View();
        }

        [Authorize]
        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RestoId,RestoName,StreetNumber,StreetName,PostalCode,Genre,Price")] Restaurant restaurant)
        {
            double lat;
            double lng;

            //Find location through Postal Code
            var result = Google.GetGeocodingSearchResults(restaurant.PostalCode);

            String status = (string)(from s in result.Descendants("status")
                                         select s).First();

            if (status != "OK")
            {
                 ModelState.AddModelError("", "Can't find location of the restaurant. Please enter right postal code");
            }
            else
            {
                Location location = (from l in result.Descendants("location")
                                     select new Location
                                     {
                                        Latitude = System.Convert.ToDouble(l.Element("lat").Value),
                                        Longitude = System.Convert.ToDouble(l.Element("lng").Value)
                                     }).First();

                lat = location.Latitude;
                lng = location.Longitude;

                DbGeography restoLoc = DbGeography.FromText(string.Format("POINT({1} {0})", lat, lng), 4326);
                restaurant.Location = restoLoc;
                restaurant.CreateDate = DateTime.Today.Date;
                restaurant.EditDate = DateTime.Today.Date;
                restaurant.Views = 0;
                restaurant.EnteredBy = User.Identity.Name;
                restaurant.EditBy = User.Identity.Name;
      

                if (ModelState.IsValid)
                {
                    db.Restaurants.Add(restaurant);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }            

            ViewBag.EditBy = new SelectList(db.UserDetails, "UserName", "UserName", restaurant.EditBy);
            ViewBag.EnteredBy = new SelectList(db.UserDetails, "UserName", "UserName", restaurant.EnteredBy);
            return View(restaurant);
        }

        [AdminAuthorize(Users = "admin")] 
        // GET: Home/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "$", Value = "$", Selected = true });
            items.Add(new SelectListItem { Text = "$$", Value = "$$" });
            items.Add(new SelectListItem { Text = "$$$", Value = "$$$" });
            items.Add(new SelectListItem { Text = "$$$$", Value = "$$$$" });
            items.Add(new SelectListItem { Text = "$$$$$", Value = "$$$$$" });
            ViewBag.Price = items;
            ViewBag.id = (int)id;
            return View(restaurant);
        }

       [AdminAuthorize(Users = "admin")] 
        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Genre,Price")] Restaurant restaurant, int RestoId)
        {

            if (ModelState.IsValid)
            {
                var resto = (from r in db.Restaurants
                         where r.RestoId == RestoId
                         select r).FirstOrDefault();
                resto.Location = Google.getLocation(resto.PostalCode);
                resto.EditDate = DateTime.Now;
                //resto.EditBy = User.Identity.Name;
                resto.Genre = restaurant.Genre;
                resto.Price = restaurant.Price;
                db.Entry(resto).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = RestoId });
            }
            //var errors = ModelState.Values.SelectMany(v => v.Errors);  
            return View(restaurant);
        }

        [AdminAuthorize(Users = "admin")] 
        // GET: Home/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

        [AdminAuthorize(Users = "admin")] 
        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            db.Restaurants.Remove(restaurant);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult ListAllResto()
        {
            var restaurants = db.Restaurants.Include(r => r.UserDetail);
            return View(restaurants.ToList());
        }
    }

}
