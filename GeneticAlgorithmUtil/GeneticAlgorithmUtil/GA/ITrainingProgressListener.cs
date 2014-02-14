// Diagnosing Breast Tumor Malignancy with a Genetic Algorithm and RBF Network 
// Contest entry for “Forecasting & Futurism 3rd Annual iPad Contest”, by Jeff Heaton
//
// Jeff Heaton
// jheaton@rgare.com
// Reinsurance Group of America
// 1370 Timberlake Manner Parkway
// Chesterfield, MO 63017
//
namespace GeneticAlgorithmUtil.GA
{
    /// <summary>
    /// Implement this interface to get updates on training.
    /// </summary>
    public interface ITrainingProgressListener
    {
        /// <summary>
        /// Called for each training update.
        /// </summary>
        /// <param name="genomes">The number of genomes created so far.</param>
        /// <param name="bestScore">The best score so far.</param>
        void TrainingUpdate(int genomes, double bestScore);
    }
}