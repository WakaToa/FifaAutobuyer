using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Collection
{
    public class CustomCollection<T> : CollectionBase
    {
        public T this[int index]
        {
            get
            {
                return (T)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        private int _limit = 5;
        public CustomCollection(int limit)
        {
            _limit = limit;
        }

        public void Add(T value)
        {
            if(List.Count > _limit - 1)
            {
                List.RemoveAt(0);
            }
            List.Add(value);
        }

        public T LastOrDefault()
        {
            if(List.Count <= 0)
            {
                return default(T);
            }
            return this[List.Count - 1];
        }

        public T FirstOrDefault()
        {
            if (List.Count <= 0)
            {
                return default(T);
            }
            return this[0];
        }
    }
}
