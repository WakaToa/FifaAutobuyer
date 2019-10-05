using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.PriceCalculation
{
    public static class DifferenceTable
    {
        private static List<Tuple<int, int, double>> _goldRangeTuple;
        private static List<Tuple<int, int, double>> _silverRangeTuple;

        static DifferenceTable()
        {
            _goldRangeTuple = new List<Tuple<int, int, double>>();
            _goldRangeTuple.Add(new Tuple<int, int, double>(0, 1, 0.4));
            _goldRangeTuple.Add(new Tuple<int, int, double>(1, 2, 0.375));
            _goldRangeTuple.Add(new Tuple<int, int, double>(2, 3, 0.35));
            _goldRangeTuple.Add(new Tuple<int, int, double>(3, 4, 0.33));
            _goldRangeTuple.Add(new Tuple<int, int, double>(4, 5, 0.3));
            _goldRangeTuple.Add(new Tuple<int, int, double>(5, 7, 0.275));
            _goldRangeTuple.Add(new Tuple<int, int, double>(7, 10, 0.25));
            _goldRangeTuple.Add(new Tuple<int, int, double>(10, 15, 0.22));
            _goldRangeTuple.Add(new Tuple<int, int, double>(15, 20, 0.2));
            _goldRangeTuple.Add(new Tuple<int, int, double>(20, 30, 0.175));
            _goldRangeTuple.Add(new Tuple<int, int, double>(30, 40, 0.16));
            _goldRangeTuple.Add(new Tuple<int, int, double>(40, 50, 0.15));
            _goldRangeTuple.Add(new Tuple<int, int, double>(50, 75, 0.125));
            _goldRangeTuple.Add(new Tuple<int, int, double>(75, 100, 0.1));
            _goldRangeTuple.Add(new Tuple<int, int, double>(100, 150, 0.08));
            _goldRangeTuple.Add(new Tuple<int, int, double>(150, 200, 0.07));

            _silverRangeTuple = new List<Tuple<int, int, double>>();
            _silverRangeTuple.Add(new Tuple<int, int, double>(0, 1, 0.4));
            _silverRangeTuple.Add(new Tuple<int, int, double>(1, 2, 0.375));
            _silverRangeTuple.Add(new Tuple<int, int, double>(2, 3, 0.35));
            _silverRangeTuple.Add(new Tuple<int, int, double>(3, 4, 0.33));
            _silverRangeTuple.Add(new Tuple<int, int, double>(4, 5, 0.3));
            _silverRangeTuple.Add(new Tuple<int, int, double>(5, 7, 0.275));
            _silverRangeTuple.Add(new Tuple<int, int, double>(7, 10, 0.25));
            _silverRangeTuple.Add(new Tuple<int, int, double>(10, 15, 0.22));
            _silverRangeTuple.Add(new Tuple<int, int, double>(15, 20, 0.2));
            _silverRangeTuple.Add(new Tuple<int, int, double>(20, 30, 0.175));
            _silverRangeTuple.Add(new Tuple<int, int, double>(30, 40, 0.16));
            _silverRangeTuple.Add(new Tuple<int, int, double>(40, 50, 0.15));
            _silverRangeTuple.Add(new Tuple<int, int, double>(50, 75, 0.125));
            _silverRangeTuple.Add(new Tuple<int, int, double>(75, 100, 0.1));
            _silverRangeTuple.Add(new Tuple<int, int, double>(100, 150, 0.08));
            _silverRangeTuple.Add(new Tuple<int, int, double>(150, 200, 0.07));
        }

        public static double GetPercentage(int value, int rating)
        {
            var useTable = rating >= 75 ? _goldRangeTuple : _silverRangeTuple;


            return (from tuple in useTable where value >= (tuple.Item1 * 1000) && value < (tuple.Item2 * 1000) select tuple.Item3).FirstOrDefault();
        }
    }
}
