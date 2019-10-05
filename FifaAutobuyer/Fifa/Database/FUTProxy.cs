using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Database
{
    [Table("futproxys")]
    public class FUTProxy
    {
        [Key]
        public int ID { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public FUTProxy()
        {
            Host = "";
            Port = 0;
            Username = "";
            Password = "";
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }
    }
}
