using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public class SimpleSearchItemModel
    {
        public int id { get; set; }
        public int r { get; set; }
        public int n { get; set; }
        public int RevisionID { get; set; }
        public int Rating { get; set; }
        public int RareFlag { get; set; }
        public FUTSearchParameterType Type { get; set; }
        public string f { get; set; }
        public string l { get; set; }
        public string c { get; set; }

        public int ClubID { get; set; }
        public int LeagueID { get; set; }
        public int NationID { get; set; }

        public string GetName()
        {
            if (!string.IsNullOrEmpty(c))
            {
                return c;
            }
            if (string.IsNullOrEmpty(l))
            {
                return f;
            }
            if (string.IsNullOrEmpty(f))
            {
                return l;
            }
            return f + " " + l;
        }
    }
}
