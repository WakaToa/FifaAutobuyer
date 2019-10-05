using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Models
{
    public abstract class FUTSearchParameter
    {
        public FUTSearchParameterType Type { get; set; }

        public abstract string BuildUriString();
    }
}
