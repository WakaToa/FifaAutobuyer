using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class Squad
    {
        public int id { get; set; }
        public bool valid { get; set; }
        public object personaId { get; set; }
        public string formation { get; set; }
        public int rating { get; set; }
        public int chemistry { get; set; }
        public List<object> manager { get; set; }
        public List<object> players { get; set; }
        public object dreamSquad { get; set; }
        public object changed { get; set; }
        public string squadName { get; set; }
        public int starRating { get; set; }
        public object captain { get; set; }
        public List<object> kicktakers { get; set; }
        public List<object> actives { get; set; }
        public object newSquad { get; set; }
        public string squadType { get; set; }
        public string custom { get; set; }
    }
}
