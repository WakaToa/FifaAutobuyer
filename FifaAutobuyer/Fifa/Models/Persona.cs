using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class Persona
    {
        public int personaId { get; set; }
        public string personaName { get; set; }
        public int returningUser { get; set; }
        public bool trial { get; set; }
        public object userState { get; set; }
        public List<UserClubList> userClubList { get; set; }
    }
}
