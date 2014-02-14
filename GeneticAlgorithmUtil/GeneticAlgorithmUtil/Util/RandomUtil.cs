// Diagnosing Breast Tumor Malignancy with a Genetic Algorithm and RBF Network 
// Contest entry for “Forecasting & Futurism 3rd Annual iPad Contest”, by Jeff Heaton
//
// Jeff Heaton
// jheaton@rgare.com
// Reinsurance Group of America
// 1370 Timberlake Manner Parkway
// Chesterfield, MO 63017
//
using System;

namespace GeneticAlgorithmUtil.Util
{
    public class RandomUtil
    {
        public static double RangeDouble(Random rnd, double min, double max)
        {
            return (rnd.NextDouble() * (max - min)) + min;
        }

        public static int RangeInt(Random rnd, int min, int max)
        {
            return (int) ((rnd.NextDouble() * (max - min)) + min);
        }
    }
}
