﻿using Microsoft.AspNet.Identity;
using Proiect.Models;
using Proiect_final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proiect_final.Controllers
{
    [Authorize(Roles = "Member,Organiser,Admin")]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Mesage = TempData["message"];
            }
            string UserId = User.Identity.GetUserId();
            ViewBag.Teams = listTeams(UserId);
            ViewBag.Projects = listProjects(UserId);
            return View();

        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [NonAction]
        private List<Project> listProjects (string UserId)
        {
            List<Project> list = new List<Project>();
            List<Team> listTeam = listTeams(UserId);
            var query = from x in db.Projects
                        where x.UserId == UserId
                        select x;
            foreach(var project in query)
            {
                foreach(var elem in listTeam)
                {
                    if (elem.TeamId == project.TeamId)

                        list.Add(project);
                } 
            }
            return list;
        }
        [NonAction]
        private List <Team> listTeams (string UserId)
        {
            List<Team> list = new List<Team>();
            var query = from x in db.TeamUsers
                        where x.Id == UserId
                        select x;
            foreach (var elem in query)
            {
                Team team = db.Teams.Find(elem.TeamId);
                list.Add(team);
            }
            return list;
        }
    }
}