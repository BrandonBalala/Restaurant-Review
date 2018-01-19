using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Resto.Models;
using Resto.Validators;

namespace Resto.Controllers
{
    public class CommentsController : Controller
    {
        private RestoDB db = new RestoDB();

        [Authorize]
        // GET: Comments/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                ViewBag.RestoId = (int)id;

                List<SelectListItem> items = new List<SelectListItem>();
                items.Add(new SelectListItem { Text = "*", Value = "*    ", Selected = true });
                items.Add(new SelectListItem { Text = "**", Value = "**   " });
                items.Add(new SelectListItem { Text = "***", Value = "***  " });
                items.Add(new SelectListItem { Text = "****", Value = "**** " });
                items.Add(new SelectListItem { Text = "*****", Value = "*****" });
                ViewBag.Rating = items;
            }


            return View();
        }

        [Authorize]
        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Rating,Review")] Comment comment, int RestoId)
        {
            if (ModelState.IsValid)
            {
                comment.RestoId = RestoId;
                comment.CreateDate = DateTime.Now;
                comment.UserName = User.Identity.Name;
                if (comment.Title == null) {
                    comment.Title = "My review";
                }
                db.Comments.Add(comment);
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = RestoId});
            }

            return View(comment);
        }

        [AdminAuthorize(Users = "admin")] 
        // GET: Comments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "*", Value = "*", Selected = true });
            items.Add(new SelectListItem { Text = "**", Value = "**" });
            items.Add(new SelectListItem { Text = "***", Value = "***" });
            items.Add(new SelectListItem { Text = "****", Value = "****" });
            items.Add(new SelectListItem { Text = "*****", Value = "*****" });
            ViewBag.Rating = items;
            return View(comment);
        }

        [AdminAuthorize(Users = "admin")] 
        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CommentId,RestoId,UserName,Title,CreateDate,Rating,Review")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                var com = (from r in db.Comments
                             where r.CommentId == comment.CommentId
                             select r).FirstOrDefault();
                com.Rating = comment.Rating;
                com.Review = comment.Review;
                
                if (comment.Title == null)
                {
                    com.Title = "My review";
                }
                else
                {
                    com.Title = comment.Title;
                }
                db.Entry(com).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Home", new { id = com.RestoId });
            }
            return View(comment);
        }

        [AdminAuthorize(Users = "admin")] 
        // GET: Comments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        [AdminAuthorize(Users = "admin")] 
        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            
            Comment comment = db.Comments.Find(id);
            int restoId = comment.RestoId;
            db.Comments.Remove(comment);
            db.SaveChanges();
            return RedirectToAction("Details", "Home", new { id = restoId });
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
    }
}
