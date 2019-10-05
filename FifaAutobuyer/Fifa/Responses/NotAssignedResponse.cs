using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class NotAssignedResponse : FUTError
    {
        public List<ItemData> itemData { get; set; }
    }
}
