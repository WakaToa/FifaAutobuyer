using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.PriceCalculation
{
    public static class FirstAverageTable
    {
        private static List<Tuple<int, int, int>> _goldRangeTuple;
        private static List<Tuple<int, int, int>> _silverRangeTuple;
        static FirstAverageTable()
        {
            _goldRangeTuple = new List<Tuple<int, int, int>>();
            _goldRangeTuple.Add(new Tuple<int, int, int>(0, 1, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(1, 2, 40));
            _goldRangeTuple.Add(new Tuple<int, int, int>(2, 3, 30));
            _goldRangeTuple.Add(new Tuple<int, int, int>(3, 4, 20));
            _goldRangeTuple.Add(new Tuple<int, int, int>(4, 5, 15));
            _goldRangeTuple.Add(new Tuple<int, int, int>(5, 7, 10));
            _goldRangeTuple.Add(new Tuple<int, int, int>(7, 10, 7));
            _goldRangeTuple.Add(new Tuple<int, int, int>(10, 15, 7));
            _goldRangeTuple.Add(new Tuple<int, int, int>(15, 20, 7));
            _goldRangeTuple.Add(new Tuple<int, int, int>(20, 30, 6));
            _goldRangeTuple.Add(new Tuple<int, int, int>(30, 40, 6));
            _goldRangeTuple.Add(new Tuple<int, int, int>(40, 50, 6));
            _goldRangeTuple.Add(new Tuple<int, int, int>(50, 75, 6));
            _goldRangeTuple.Add(new Tuple<int, int, int>(75, 100, 5));
            _goldRangeTuple.Add(new Tuple<int, int, int>(100, 150, 5));
            _goldRangeTuple.Add(new Tuple<int, int, int>(150, 200, 5));

            _silverRangeTuple = new List<Tuple<int, int, int>>();
            _silverRangeTuple.Add(new Tuple<int, int, int>(0, 1, 20));
            _silverRangeTuple.Add(new Tuple<int, int, int>(1, 2, 18));
            _silverRangeTuple.Add(new Tuple<int, int, int>(2, 3, 16));
            _silverRangeTuple.Add(new Tuple<int, int, int>(3, 4, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(4, 5, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(5, 7, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(7, 10, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(10, 15, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(15, 20, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(20, 30, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(30, 40, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(40, 50, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(50, 75, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(75, 100, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(100, 150, 15));
            _silverRangeTuple.Add(new Tuple<int, int, int>(150, 200, 15));
        }

        public static int GetRange(int value, int rating)
        {
            var useTable = rating >= 75 ? _goldRangeTuple : _silverRangeTuple;


            return (from tuple in useTable where value >= (tuple.Item1 * 1000) && value < (tuple.Item2 * 1000) select tuple.Item3).FirstOrDefault();
        }
    }
   
}
