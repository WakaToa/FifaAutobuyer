using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FifaAutobuyer.Fifa.Database;

namespace FifaAutobuyer.WebServer.Models
{
    class ManageListModel : BaseModel
    {
        public List<FUTListItem> FutListItems { get; set; }

        public bool DisplayError { get; set; }
        public string DisplayErrorStyle => DisplayError ? "" : "display:none;";
        public string ErrorMessage { get; set; }

        public ManageListModel()
        {
            ManageListActive = "active";
        }
    }
}
