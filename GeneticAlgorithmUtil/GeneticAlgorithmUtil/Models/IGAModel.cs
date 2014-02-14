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
using GeneticAlgorithmUtil.Config;
using GeneticAlgorithmUtil.GA;

namespace GeneticAlgorithmUtil.Models
{
    /// <summary>
    /// A model for a GA.  This specifies how the genomes are used, as well as
    /// how they are scored and mutated/crossed over.  If you want to define
    /// your own model, for use with this program, you should create a
    /// new class that implements this interface.
    /// 
    /// See NetworkRBF for an example.
    /// See also TypicalGAModel, as it may have some "typical" implementations of these
    /// methods, that you can use.
    /// </summary>
    public interface IGAModel
    {
        /// <summary>
        /// Setup this model.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="config">Configuration information for the model.</param>
        void Init(ConfigScript script, string config);

        /// <summary>
        /// Generate a random genome.
        /// </summary>
        /// <param name="rnd">A random number</param>
        /// <param name="script">The configuration script.</param>
        /// <returns></returns>
        Genome GenerateRandomGenome(Random rnd, ConfigScript script);

        /// <summary>
        /// Compute the genome.  This is where you implement a computation method that
        /// is unique to your "problem".  You must make use of the input to produce output
        /// that  will be scored.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="genome">The genome to use for the computation.</param>
        /// <returns>The output.</returns>
        double[] Compute(double[] input, Genome genome);

        /// <summary>
        /// Score the specified genome.
        /// </summary>
        /// <param name="testSubject">The genome to score.</param>
        /// <param name="training">The training set to use.</param>
        /// <param name="ideal">The ideal output for the training set.</param>
        /// <returns>The score.</returns>
        double Score(Genome testSubject, double[][] training, double[][] ideal);

        /// <summary>
        /// Create a child genome as a mutation of the parent.  The parent is not changed.
        /// </summary>
        /// <param name="rnd">A random number generator</param>
        /// <param name="genome">The genome</param>
        /// <returns>The new child genome</returns>
        Genome Mutate(Random rnd, Genome genome);

        /// <summary>
        /// Perform a cross over amount several parents.  
        /// There should be two or more parents.
        /// </summary>
        /// <param name="rnd">A random number generator</param>
        /// <param name="parents">The parents</param>
        /// <returns>The children</returns>
        Genome[] Crossover(Random rnd, Genome[] parents);
    }
}