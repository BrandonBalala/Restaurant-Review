using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Resto.Models;
using System.Web.Security;
using Resto.Validators;

namespace Resto.Controllers
{
    public class AccountController : Controller
    {
        private RestoDB db = new RestoDB();

       
        // GET: Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = (from u in db.Users
                             where u.UserName.Equals(username)
                             && u.Password.Equals(password)
                             select u).FirstOrDefault();

                if (user != null)
                {
                    FormsAuthentication.RedirectFromLoginPage(username, false);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }

            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }


        // GET: Account/Register
        public ActionResult Register()
        {
            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "LastName");
            return View();
        }

        // POST: Account/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "UserId,Password,UserName,UserDetail")] User user)
        {
            if (ModelState.IsValid)
            {
                User checkUserExists = (from u in db.Users
                             where u.UserName.Equals(user.UserName)
                             select u).FirstOrDefault();

                if (checkUserExists == null)
                {
                    db.Users.Add(user);
                    user.UserDetail.UserName = user.UserName;
                    db.UserDetails.Add(user.UserDetail);
                    db.SaveChanges();

                    //FormsAuthentication.RedirectFromLoginPage(user.UserName, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Username already exists");
                }
            }

            ViewBag.UserId = new SelectList(db.UserDetails, "UserId", "LastName", user.UserName);
            return View(user);
        }
 
        // GET: Account
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Index()
        {
            var users = (from u in db.Users.Include("UserDetail")
                         where u.UserName != "admin"
                         select u).ToList();
            return View(users);
        }

        // GET: Account/Details/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        // GET: Account/Edit/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserName = new SelectList(db.UserDetails, "UserName", "LastName", user.UserName);
            return View(user);
        }

        // POST: Account/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Edit([Bind(Include = "UserName,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserName = new SelectList(db.UserDetails, "UserName", "LastName", user.UserName);
            return View(user);
        }

        // GET: Account/Delete/5
        [AdminAuthorize(Users = "admin")] 
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
        /*
         * Hi Jaya,
         * Unfortuanely, We weren't able to delete users because, we think it is because the restaurant table
         * and comments are linked to the username. Silly us! Please overlook that mistake and enjoy the 
         * rest of the code
         * 
         * Thank you for your time and have a great day!!
         * 
         * Regards, 
         * Frank Birikundavyi
         * Brandon Balala         
         */
        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [AdminAuthorize(Users = "admin")] 
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            UserDetail details = db.UserDetails.Find(id);
            User user = db.Users.Find(id);
           
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [AdminAuthorize(Users = "admin")] 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
