using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Requests
{
    public interface IFUTRequest<TResponse>
    {
        Task<TResponse> PerformRequestAsync();
    }
}
