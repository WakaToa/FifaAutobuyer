using FifaAutobuyer.Fifa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Responses
{
    public class WebPlayerItemResponse : FUTError
    {
        public ItemModel Item { get; set; }

        public string GetName()
        {
            if(Item == null)
            {
                return "";
            }
            if(!string.IsNullOrEmpty(Item.CommonName))
            {
                return Item.CommonName;
            }
            if(string.IsNullOrEmpty(Item.LastName))
            {
                return Item.FirstName;
            }
            if (string.IsNullOrEmpty(Item.FirstName))
            {
                return Item.LastName;
            }
            return Item.FirstName + " " + Item.LastName;
        }

        public WebPlayerItemResponse()
        {
            Item = new ItemModel();
            Item.CommonName = "";
            Item.FirstName = "";
            Item.LastName = "";
        }
    }
}
