using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Extensions
{
    public static class IntegerExtensions
    {
        public static int RoundOff(this int i, int count)
        {
            return ((int)Math.Floor(i / (decimal)count)) * count;
        }
        public static int RoundOff(this long i, int count)
        {
            return ((int)Math.Floor(i / (decimal)count)) * count;
        }

        private static Tuple<int, int> GetPriceInfoRow(int price)
        {
            var lst = new List<Tuple<int, int>>();
            lst.Add(new Tuple<int, int>(100000, 1000));
            lst.Add(new Tuple<int, int>(50000, 500));
            lst.Add(new Tuple<int, int>(10000, 250));
            lst.Add(new Tuple<int, int>(1000, 100));
            lst.Add(new Tuple<int, int>(0, 50));
            foreach(var loc in lst)
            {
                if(price > loc.Item1)
                {
                    return loc;
                }
            }

            return lst[lst.Count - 1];
        }
        public static int CalculateStartingBid(this int buyNow)
        {
            var loc2 = GetPriceInfoRow(buyNow);
            var loc3 = loc2.Item1;
            var loc4 = buyNow - loc2.Item1;
            loc4 = (int)Math.Ceiling((double)loc4 / loc2.Item2) * loc2.Item2;
            var steppedPrice = loc4 + loc3 - loc2.Item2;
            return Math.Min(15000000, Math.Max(150, steppedPrice));
        }

        private static int ValidateNumber(this int buyNow)
        {
            var loc2 = GetPriceInfoRow(buyNow);
            var loc3 = loc2.Item1;
            var loc4 = buyNow - loc2.Item1;
            loc4 = (int)Math.Ceiling((double)loc4 / loc2.Item2) * loc2.Item2;
            var steppedPrice = loc4 + loc3;
            return Math.Min(15000000, Math.Max(150, steppedPrice));
        }

        public static int IncrementPrice(this int price)
        {
            //var loc2 = GetPriceInfoRow(CalculateStartingBid(price) + 1);
            //return ValidateNumber(price + loc2.Item2);
            return CalculateNextPrice(price, 1);
        }

        public static int DecrementPrice(this int price)
        {
            //var loc2 = GetPriceInfoRow(CalculateStartingBid(price) - 1);
            //return ValidateNumber(price - loc2.Item2);
            return CalculatePreviousPrice(price, 1);
        }

        private static int CalculatePreviousPrice(int price, int steps)
        {
            for (int index = 0; index < steps; ++index)
            {
                if (price <= 1000)
                    price -= 50;
                else if (price > 1000 && price <= 10000)
                    price -= 100;
                else if (price > 10000 && price <= 50000)
                    price -= 250;
                else if ((price <= 50000 ? 0 : (price <= 100000 ? 1 : 0)) == 0)
                {
                    if (price > 100000)
                        price -= 1000;
                }
                else
                    price -= 500;
            }
            return price;
        }

        private static int CalculateNextPrice(int price, int steps)
        {
            for (int index = 0; index < steps; ++index)
            {
                if (price > 950)
                {
                    if ((price > 950 ? (price <= 9900 ? 1 : 0) : 0) != 0)
                        price += 100;
                    else if ((price <= 9900 ? 0 : (price <= 49750 ? 1 : 0)) != 0)
                        price += 250;
                    else if (price > 49750 && price <= 99500)
                        price += 500;
                    else if (price > 99500)
                        price += 1000;
                }
                else
                    price += 50;
            }
            return price;
        }

        public static int GetMedian(this IEnumerable<int> source)
        {
            // Create a copy of the input, and sort the copy
            int[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                return 0;
            }
            if (count == 1)
            {
                return temp[0];
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                int a = temp[count / 2 - 1];
                int b = temp[count / 2];
                return (int)((a + b) / 2m);
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }

        public static int ValidatePrice(this int iPrice, bool getStep = false)
        {
            try
            {
                int[] monStep = {50, 100, 250, 500, 1000, 1000};
                int[] monBord = {0, 1000, 10000, 50000, 100000};

                int stepid = 0;

                int walker = 0;
                int owalker = 0;

                while (true)
                {
                    owalker = walker;

                    walker += monStep[stepid];

                    if (walker > iPrice)
                    {
                        if (getStep)
                        {
                            return monStep[stepid];
                        }
                        if (Math.Abs(owalker - iPrice) < Math.Abs(walker - iPrice))
                        {
                            return owalker;
                        }
                        else
                        {
                            return walker;
                        }
                    }

                    if (stepid < monBord.Length - 1)
                    {

                        if (walker >= monBord[stepid + 1])
                        {
                            stepid += 1;

                        }
                    }
                }
            }
            catch
            {
                return 0;
            }

        }

    }
}
