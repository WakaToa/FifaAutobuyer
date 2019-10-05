using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FifaAutobuyer.Fifa.PriceCalculation
{
    public static class CardsRequiredTable
    {
        private static List<Tuple<int, int, int>> _goldRangeTuple;
        private static List<Tuple<int, int, int>> _silverRangeTuple;

        static CardsRequiredTable()
        {
            _goldRangeTuple = new List<Tuple<int, int, int>>();
            _goldRangeTuple.Add(new Tuple<int, int, int>(0, 1000, 200));
            _goldRangeTuple.Add(new Tuple<int, int, int>(1000, 2000, 175));
            _goldRangeTuple.Add(new Tuple<int, int, int>(2000, 3000, 150));
            _goldRangeTuple.Add(new Tuple<int, int, int>(3000, 4000, 125));
            _goldRangeTuple.Add(new Tuple<int, int, int>(4000, 5000, 100));
            _goldRangeTuple.Add(new Tuple<int, int, int>(5000, 7000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(7000, 10000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(10000, 15000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(15000, 20000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(20000, 30000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(30000, 40000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(40000, 50000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(50000, 75000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(75000, 100000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(100000, 150000, 50));
            _goldRangeTuple.Add(new Tuple<int, int, int>(150000, 200000, 50));

            _silverRangeTuple = new List<Tuple<int, int, int>>();
            _silverRangeTuple.Add(new Tuple<int, int, int>(0, 1000, 70));
            _silverRangeTuple.Add(new Tuple<int, int, int>(1000, 2000, 65));
            _silverRangeTuple.Add(new Tuple<int, int, int>(2000, 3000, 60));
            _silverRangeTuple.Add(new Tuple<int, int, int>(3000, 4000, 55));
            _silverRangeTuple.Add(new Tuple<int, int, int>(4000, 5000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(5000, 7000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(7000, 10000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(10000, 15000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(15000, 20000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(20000, 30000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(30000, 40000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(40000, 50000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(50000, 75000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(75000, 100000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(100000, 150000, 50));
            _silverRangeTuple.Add(new Tuple<int, int, int>(150000, 200000, 50));
        }

        public static int GetRange(int value, int rating)
        {
            var useTable = rating >= 75 ? _goldRangeTuple : _silverRangeTuple;


            return (from tuple in useTable where value >= tuple.Item1 && value < tuple.Item2 select tuple.Item3).FirstOrDefault();
        }


    }
}
