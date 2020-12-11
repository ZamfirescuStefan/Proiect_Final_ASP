using Proiect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proiect_final.Models
{
    public class TeamUser
    { 
        public string Id { get; set; } // de la User.Id
        public int TeamId { get; set; }

        public virtual ApplicationUser Users { get; set; }
        public virtual Team Teams { get; set; }
    }
}