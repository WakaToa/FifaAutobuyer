using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futcoins")]
    public class FUTCoins
    {
        [Key]
        public string EMail { get; set; }
        public int Coins { get; set; }

        public FUTCoins()
        {
            EMail = "";
            Coins = -1;
        }
    }
}
