using System;
using System.Collections.Generic;
namespace CAMTrak.Utilities
{
    public class MathUtils
    {
        public const double kEpsilon = 1E-05f;
        public const double kOutOfWorld = -20000f;
        public const double kInvalid = double.PositiveInfinity;
        private static readonly long[] kPrimeNumbers = new long[]
		{
			2L, 
			3L, 
			5L, 
			7L, 
			11L, 
			13L, 
			17L, 
			19L, 
			23L, 
			29L, 
			31L, 
			37L, 
			41L, 
			43L, 
			47L, 
			53L, 
			59L, 
			61L, 
			67L, 
			71L, 
			73L, 
			79L, 
			83L, 
			89L, 
			97L, 
			101L, 
			103L, 
			107L, 
			109L, 
			113L, 
			127L, 
			131L, 
			137L, 
			139L, 
			149L, 
			151L, 
			157L, 
			163L, 
			167L, 
			173L, 
			179L, 
			181L, 
			191L, 
			193L, 
			197L, 
			199L, 
			211L, 
			223L, 
			227L, 
			229L, 
			233L, 
			239L, 
			241L, 
			251L, 
			257L, 
			263L, 
			269L, 
			271L, 
			277L, 
			281L, 
			283L, 
			293L, 
			307L, 
			311L, 
			313L, 
			317L, 
			331L, 
			337L, 
			347L, 
			349L, 
			353L, 
			359L, 
			367L, 
			373L, 
			379L, 
			383L, 
			389L, 
			397L, 
			401L, 
			409L, 
			419L, 
			421L, 
			431L, 
			433L, 
			439L, 
			443L, 
			449L, 
			457L, 
			461L, 
			463L, 
			467L, 
			479L, 
			487L, 
			491L, 
			499L, 
			503L, 
			509L, 
			521L, 
			523L, 
			541L, 
			547L, 
			557L, 
			563L, 
			569L, 
			571L, 
			577L, 
			587L, 
			593L, 
			599L, 
			601L, 
			607L, 
			613L, 
			617L, 
			619L, 
			631L, 
			641L, 
			643L, 
			647L, 
			653L, 
			659L, 
			661L, 
			673L, 
			677L, 
			683L, 
			691L, 
			701L, 
			709L, 
			719L, 
			727L, 
			733L, 
			739L, 
			743L, 
			751L, 
			757L, 
			761L, 
			769L, 
			773L, 
			787L, 
			797L, 
			809L, 
			811L, 
			821L, 
			823L, 
			827L, 
			829L, 
			839L, 
			853L, 
			857L, 
			859L, 
			863L, 
			877L, 
			881L, 
			883L, 
			887L, 
			907L, 
			911L, 
			919L, 
			929L, 
			937L, 
			941L, 
			947L, 
			953L, 
			967L, 
			971L, 
			977L, 
			983L, 
			991L, 
			997L
		};
        public static double Floor(double value)
        {
            return (double)Math.Floor((double)value);
        }
        public static int Clamp(int value, int minValue, int maxValue)
        {
            if (value < minValue)
            {
                return minValue;
            }
            if (value > maxValue)
            {
                return maxValue;
            }
            return value;
        }
        public static double Clamp(double value, double minValue, double maxValue)
        {
            if (value < minValue)
            {
                return minValue;
            }
            if (value > maxValue)
            {
                return maxValue;
            }
            return value;
        }
        public static double Degree2Radian(double deg)
        {
            return 3.14159274f * deg / 180f;
        }
        public static double Radian2Degree(double radian)
        {
            return 180f * radian / 3.14159274f;
        }
        public static bool IsSimilar(double x, double y)
        {
            return Math.Abs(x - y) < 1E-05f;
        }
        public static bool IsSimilar(double x, double y, double tolerance)
        {
            return Math.Abs(x - y) < tolerance;
        }
        public static List<Vector2> FactorsOf(int value)
        {
            List<Vector2> list = new List<Vector2>();
            if (value == 0)
            {
                return list;
            }
            long a = (long)value;
            double value2 = Math.Sqrt((double)value);
            long num = (long)Math.Floor(value2);
            int num2 = 1;
            while ((long)num2 <= num)
            {
                long num4;
                long num3 = Math.DivRem(a, (long)num2, out num4);
                if (num4 == 0L)
                {
                    list.Add(new Vector2((double)num2, (double)num3));
                }
                num2++;
            }
            return list;
        }
        public static bool FactorsOfUnitTest()
        {
            bool result = true;
            List<Vector2> list = MathUtils.FactorsOf(0);
            if (list.Count != 0)
            {
                result = false;
            }
            list = MathUtils.FactorsOf(1);
            if (list.Count != 1 || list[0].x != 1f || list[0].y != 1f)
            {
                result = false;
            }
            list = MathUtils.FactorsOf(2);
            if (list.Count != 1 || list[0].x != 1f || list[0].y != 2f)
            {
                result = false;
            }
            list = MathUtils.FactorsOf(6);
            if (list.Count != 2 || list[0].x != 1f || list[0].y != 6f || list[1].x != 2f || list[1].y != 3f)
            {
                result = false;
            }
            list = MathUtils.FactorsOf(24);
            if (list.Count != 4 || list[0].x != 1f || list[0].y != 24f || list[1].x != 2f || list[1].y != 12f || list[2].x != 3f || list[2].y != 8f || list[3].x != 4f || list[3].y != 6f)
            {
                result = false;
            }
            list = MathUtils.FactorsOf(100);
            if (list.Count != 5 || list[0].x != 1f || list[0].y != 100f || list[1].x != 2f || list[1].y != 50f || list[2].x != 4f || list[2].y != 25f || list[3].x != 5f || list[3].y != 20f || list[4].x != 10f || list[4].y != 10f)
            {
                result = false;
            }
            return result;
        }
        public static List<long> PrimeFactorsOf(long longValue)
        {
            List<long> list = new List<long>();
            while (longValue > 1L)
            {
                int i;
                for (i = 0; i < MathUtils.kPrimeNumbers.Length; i++)
                {
                    long num = MathUtils.kPrimeNumbers[i];
                    long num3;
                    long num2 = Math.DivRem(longValue, num, out num3);
                    if (num3 == 0L)
                    {
                        list.Add(num);
                        longValue = num2;
                        break;
                    }
                }
                if (i >= MathUtils.kPrimeNumbers.Length)
                {
                    break;
                }
            }
            return list;
        }
        public static bool PrimeFactorsOfUnitTest()
        {
            bool result = true;
            List<long> list = MathUtils.PrimeFactorsOf(0L);
            if (list.Count != 0)
            {
                result = false;
            }
            list = MathUtils.PrimeFactorsOf(1L);
            if (list.Count != 0)
            {
                result = false;
            }
            list = MathUtils.PrimeFactorsOf(2L);
            if (list.Count != 1 || list[0] != 2L)
            {
                result = false;
            }
            list = MathUtils.PrimeFactorsOf(6L);
            if (list.Count != 2 || list[0] != 2L || list[1] != 3L)
            {
                result = false;
            }
            list = MathUtils.PrimeFactorsOf(24L);
            if (list.Count != 4 || list[0] != 2L || list[1] != 2L || list[2] != 2L || list[3] != 3L)
            {
                result = false;
            }
            list = MathUtils.PrimeFactorsOf(100L);
            if (list.Count != 4 || list[0] != 2L || list[1] != 2L || list[2] != 5L || list[3] != 5L)
            {
                result = false;
            }
            return result;
        }
        public static int CountBits(uint x)
        {
            x -= (x >> 1 & 1431655765u);
            x = (x & 858993459u) + (x >> 2 & 858993459u);
            x = (x + (x >> 4) & 252645135u);
            return (int)(x * 16843009u >> 24);
        }
        public static uint TurnOffLowestBit(uint x)
        {
            return x & x - 1u;
        }
        public static uint IsolateLowestBit(uint x)
        {
            return x & 0u - x;
        }
    }
}
