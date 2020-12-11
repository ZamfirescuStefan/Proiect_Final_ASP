using Proiect.Models;
using Proiect_final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proiect.Controllers
{
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public ActionResult New(Comment comment)
        {
            try
            {
                comment.Date = DateTime.Now;
                db.Comments.Add(comment);
                db.SaveChanges();
                TempData["message"] = "Comentariul a fost adaugat!";
                return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
            }
            catch(Exception e)
            {
                return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
            }
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.Find(id);
            return View(comment);
        }

        [HttpPut]
        public ActionResult Edit(int id, Comment requestComment)
        {
            Comment comment = db.Comments.Find(id);
            requestComment.Task = comment.Task;
            try
            {
                if(TryUpdateModel(requestComment))
                {
                    comment.CommentContent = requestComment.CommentContent;
                    comment.Date = requestComment.Date;
                    db.SaveChanges();
                    TempData["message"] = "Comentariul a fost editat!";
                    return Redirect("/Tasks/Show/" + comment.TaskId.ToString());
                }
                return View(requestComment);
            }
            catch(Exception e)
            {
                return View(requestComment);
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Comment comment = db.Comments.Find(id);
            var x = comment.TaskId;
            db.Comments.Remove(comment);
            db.SaveChanges();
            TempData["message"] = "Comentariul a fost sters!";
            return Redirect("/Tasks/Show/" + x.ToString());
        }
    }
}