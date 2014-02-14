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
using System.Text;

namespace GeneticAlgorithmUtil.GA
{
    /// <summary>
    /// A genome, is one potential solution and a member of the population.
    /// Each genome has an array of doubles.  This double array contains the genes
    /// that makeup the genome.  These genes specify the paramaters to the model.
    /// The model can use them however it wishes.  This GA assumes that the number
    /// of genes is constant per population member.
    /// </summary>
    [Serializable]
    public class Genome
    {
        /// <summary>
        /// The genes in this genome.
        /// </summary>
        private readonly double[] _genes;

        /// <summary>
        /// Construct the genome.
        /// </summary>
        /// <param name="geneCount">The number of genes.</param>
        public Genome(int geneCount)
        {
            _genes = new double[geneCount];
        }

        /// <summary>
        /// The genes.
        /// </summary>
        public double[] Genes
        {
            get { return _genes; }
        }

        /// <summary>
        /// The score.
        /// </summary>
        public double Score { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("[Genome: score=");
            result.Append(Score);
            result.Append("]");
            return result.ToString();
        }
    }
}