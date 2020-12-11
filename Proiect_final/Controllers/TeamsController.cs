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

    public class TeamsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Teams
        [HttpGet]
        [Authorize(Roles = "Member,Organiser,Admin")]
        public ActionResult Index()
        {
            ViewBag.Teams = db.Teams;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }

        [HttpGet]
       // [Authorize(Roles = "Member,Organiser,Admin")]
        public ActionResult Show(int id)
        {
            Team team = db.Teams.Find(id);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(team);
        }

        [HttpGet]
       // [Authorize(Roles = "Member,Organiser,Admin")]
        public ActionResult New()
        {
            Team team = new Team();
            team.UserId = User.Identity.GetUserId();
            return View(team);
        }

        [HttpPost]
        //[Authorize(Roles = "Member,Organiser,Admin")]
        public ActionResult New(Team team)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = db.Users.Find(User.Identity.GetUserId());
                    team.Users.Add(user);
                    db.Teams.Add(team);
                    db.SaveChanges();
                    TempData["message"] = "Echipa a fost adaugata!";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(team);
                }
            }
            catch (Exception e)
            {
                return View(team);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Edit(int id)
        {
            Team team = db.Teams.Find(id);
            return View(team);
        }

        [HttpPut]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Edit(int id, Team requestTeam)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Team team = db.Teams.Find(id);
                    if (TryUpdateModel(team))
                    {
                        team.TeamName = requestTeam.TeamName;
                        db.SaveChanges();
                        TempData["message"] = "Editarea echipei a fost efectuata cu succes!";
                        return RedirectToAction("Index");
                    }
                    else
                        return View(requestTeam);
                }
                else
                    return View(requestTeam);
            }
            catch (Exception e)
            {
                return View(requestTeam);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult Delete(int id)
        {
            Team team = db.Teams.Find(id);
            db.Teams.Remove(team);
            db.SaveChanges();
            TempData["message"] = "Echipa a fost stearsa!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult NewMember(string TeamId)
        {
            Team team = db.Teams.Find(Convert.ToInt32(TeamId));
            if (User.Identity.GetUserId() == team.UserId || User.IsInRole("Admin"))
            {
                team.Members = GetAllMembers(team);
                return View(team);
            }
            else
            {
                TempData["message"] = "Nu aveti permisiune sa adaugati membri";
                return Redirect("/Teams/" + TeamId);
            }
        }
        [NonAction]
        public IEnumerable<SelectListItem> GetAllMembers(Team team)
        {
            var selectList = new List<SelectListItem>();
            var members = from member in db.Users
                          select member;

            foreach (var member in members)
            {
                selectList.Add(new SelectListItem
                {
                    Value = member.Id.ToString(),
                    Text = member.UserName.ToString()
                });
                
            }
            /*foreach (var elem in team.Users)
            {
                foreach (var member in selectList)
                {
                    if (elem.Id == member.Value)
                    {
                        selectList.Remove(member);
                        break;
                    }
                }
            }*/
            return selectList;
        }
        [HttpPost]
        [Authorize(Roles = "Organiser,Admin")]
        public ActionResult NewMember(string UserId, int TeamId, ApplicationUser user)
        {
            Team team = db.Teams.Find(TeamId);
            team.Members = GetAllMembers(team);
            try
            {
                if (User.Identity.GetUserId() == team.UserId || User.IsInRole("Admin"))
                {
                    if (ModelState.IsValid)
                    {

                        team.Users.Add(user);
                        TempData["message"] = "Membrul a fost adaugat!";
                        return RedirectToAction("/Show/" + TeamId.ToString());
                    }
                    else
                    {
                        return View(team);
                    }
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul de a adauga un membru in aceasta echipa";
                    return Redirect("/Projects/Index");
                }
            }
            catch (Exception e)
            {
                return View(team);
            }
      
        }
    }
}