using Microsoft.AspNet.Identity;
using Proiect.Models;
using Proiect_final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proiect.Controllers
{
    public class TasksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Tasks = db.Tasks;
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.Message = TempData["message"];
                }
                return View();
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul de a accesa ruta";
                return Redirect("/Home/Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Member,Organiser,Admin")]
        public ActionResult Show(int id)
        {
            Task task = db.Tasks.Find(id);
            List<ApplicationUser> list = listUsersOfTeam(task.ProjectId);
            bool ok = false;
            foreach (var elem in list)
            {
                if (elem.Id == User.Identity.GetUserId())
                {
                    ok = true;
                    break;
                }
            }
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            if (User.IsInRole("Admin") || ok)
            {
                if (task.WorkerId != null)
                    ViewBag.WorkerName = db.Users.Find(task.WorkerId).UserName;
                setAccessRights();
                return View(task);
            }
            else
            {
                TempData["message"] = "Nu ai permisiunea sa vizualizezi acest task ";
                return Redirect("/Home/Index");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult New(string ProiectId)
        {
            int ProjId = Convert.ToInt32(ProiectId);
            string userId = GetUserIdFromProjectId(ProjId);
            
            if (userId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                Task task = new Task();
                task.UserId = userId;
                task.ProjectId = Convert.ToInt32(ProiectId);
                return View(task);
            }
            else
            {
                TempData["message"] = "Nu ai permisiunea sa adaugi un task nou";
                return Redirect("/Projects/Show/" + ProiectId);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult New(Task task)
        { 
            try
            {
                if (ModelState.IsValid)
                {
                    if (User.Identity.GetUserId() ==  task.UserId || User.IsInRole("Admin"))
                    {
                        task.Status = "Not started";
                        db.Tasks.Add(task);
                        db.SaveChanges();
                        TempData["message"] = "Taskul a fost adaugat!";
                        return Redirect("/Projects/Show/" + task.ProjectId.ToString());
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti permisiunea sa adaugati un nou task";
                        return Redirect("/Projects/Index");
                    }
                }
                else
                    return View(task);
            }
            catch (Exception e)
            {
                return View(task);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Edit(int id)
        {
            Task task = db.Tasks.Find(id);
            task.Statusuri = GetAllStatus();
            string userId = GetUserIdFromProjectId(task.ProjectId);
            if (userId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                return View(task);
            }
            else
            {
                TempData["message"] = "NU poti edita taskul unui proiet in care nu esti organizator";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllStatus()
        {
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "Not started",
                Text = "Not started"
            });
            selectList.Add(new SelectListItem
            {
                Value = "In progress",
                Text = "In progress"
            });
            selectList.Add(new SelectListItem
            {
                Value = "Done",
                Text = "Done"
            });

            return selectList;
        }

        [HttpPut]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Edit(int id, Task requestTask)
        {
            requestTask.Statusuri = GetAllStatus();
            try
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(requestTask))
                    {

                        Task task = db.Tasks.Find(id);
                        string userId = GetUserIdFromProjectId(task.ProjectId);
                        if (userId == User.Identity.GetUserId() || User.IsInRole("Admin"))
                        {
                            task.TaskName = requestTask.TaskName;
                            task.TaskDescription = requestTask.TaskDescription;
                            task.StartDate = requestTask.StartDate;
                            task.EndDate = requestTask.EndDate;
                            task.Status = requestTask.Status;
                            task.ProjectId = requestTask.ProjectId;
                            db.SaveChanges();
                            TempData["message"] = "Taskul a fost editat cu succes!";
                            return Redirect("/Tasks/Show/" + id.ToString());
                        }
                        else
                        {
                            TempData["message"] = "NU poti edita taskul unui proiet in care nu esti organizator";
                            return Redirect("/Tasks/Show/" + id.ToString());
                        }
                    }
                    else
                        View(requestTask);
                }

                return View(requestTask);
            }
            catch (Exception e)
            {
                return View(requestTask);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Delete(int id)
        {
            Task task = db.Tasks.Find(id);
            string userId = GetUserIdFromProjectId(task.ProjectId);
            if (userId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Tasks.Remove(task);
                db.SaveChanges();
                TempData["message"] = "Taskul a fost sters!";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
            else
            {
                TempData["message"] = "NU poti edita taskul unui proiet in care nu esti organizator";
                return Redirect("/Projects/Show/" + task.ProjectId);
            }
        }

        [NonAction]
        private void setAccessRights()
        {
            ViewBag.afisareButoane = false;
            if (User.IsInRole("Organiser") || User.IsInRole("Admin"))
            {
                ViewBag.afisareButoane = true;
            }
            ViewBag.isAdmin = User.IsInRole("Admin");
            ViewBag.currentUser = User.Identity.GetUserId();
        }
        [NonAction]
        private string GetUserIdFromProjectId (int id)
        {
            Project proiect = db.Projects.Find(id);
            return proiect.UserId;
        }

        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult AddWorker (int id)
        {
            Task task = db.Tasks.Find(id);
            if (User.Identity.GetUserId() == task.UserId || User.IsInRole("Admin"))
            {
                List<ApplicationUser> list = listUsersOfTeam(task.ProjectId);
                ViewBag.WorkerId = task.WorkerId;
                ViewBag.TaskId = task.TaskId;
                ViewBag.listOfUsers = list;
                return View();
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa efectuati aceasta modificare";
                return Redirect("/Tasks/Show/" + id.ToString());
            }
        }
        [HttpPost]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult AddWorker (int id, string WorkerId)
        {
            Task task = db.Tasks.Find(id);
            ViewBag.TaskId = task.TaskId;

            ViewBag.listOfUsers = listUsersOfTeam(task.ProjectId);
            try
            {
                if (User.Identity.GetUserId() == task.UserId || User.IsInRole("Admin"))
                {
                    task.WorkerId = WorkerId;
                    db.SaveChanges();
                    TempData["message"] = "Taskul a fost asignat cu succes";
                    return Redirect("/Tasks/Show/" + id.ToString());
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa efectuati aceasta modificare";
                    return Redirect("/Tasks/Show/" + id.ToString());
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }
        [NonAction]
        private List<ApplicationUser> listUsersOfTeam (int ProjectId)
        {
            List<ApplicationUser> listofUsr = new List<ApplicationUser>();
            Project project = db.Projects.Find(ProjectId);
            var list = from x in db.TeamUsers
                       where x.TeamId == project.TeamId
                       select x;
            foreach (var elem in list)
            {
                ApplicationUser auxUser = db.Users.Find(elem.Id);
                listofUsr.Add(auxUser);
            }

            return listofUsr;
        }

    }
}