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
using System.Threading.Tasks;
using GeneticAlgorithmUtil.Config;
using GeneticAlgorithmUtil.GA;
using GeneticAlgorithmUtil.Models;
using GeneticAlgorithmUtil.Util;

namespace GeneticAlgorithmUtil.Models
{
    /// <summary>
    /// This class provides several "typical" implementations of functions 
    /// that can be used by many different genetic models.
    /// </summary>
    public class TypicalGAModel
    {
        /// <summary>
        /// Useful for one-of-n encoding.  Given a vector of doubles,
        /// return the index of the highest value.
        /// </summary>
        /// <param name="array">The array of doubles.</param>
        /// <returns>The top index.</returns>
        public static int FindTopIndex(double[] array)
        {
            int topIndex = 0;
            double maxValue = double.NegativeInfinity;

            for (int i = 0; i < array.Length; i++)
            {
                if (topIndex == -1 || array[i] > maxValue)
                {
                    maxValue = array[i];
                    topIndex = i;
                }
            }

            return topIndex;
        }

        /// <summary>
        /// Score the model when the output is classification.  
        /// For classification, the score is the percentage correct.
        /// </summary>
        /// <param name="model">The model to score.</param>
        /// <param name="testSubject">The test subject.</param>
        /// <param name="input">The input.</param>
        /// <param name="ideal">The ideal.</param>
        /// <returns>The score.</returns>
        private static double ScoreClassification(IGAModel model, Genome testSubject, double[][] input, double[][] ideal)
        {
            double error = 0;
            double elementCount = 0;
            int badCount = 0;

            Parallel.For(0, input.Length, row =>
            {
                double[] output = model.Compute(input[row], testSubject);
                int outputIndex = FindTopIndex(output);
                int idealIndex = FindTopIndex(ideal[row]);
                if (outputIndex != idealIndex)
                {
                    badCount++;
                }
                elementCount++;
            });

            return badCount / elementCount;
        }

        /// <summary>
        /// Score the model when the output is regression.  
        /// For regression, the score is mean square error (MSE).
        /// http://www.heatonresearch.com/wiki/Mean_Square_Error
        /// </summary>
        /// <param name="model">The model to score.</param>
        /// <param name="testSubject">The test subject.</param>
        /// <param name="input">The input.</param>
        /// <param name="ideal">The ideal.</param>
        /// <returns>The score.</returns>
        private static double ScoreRegression(IGAModel model, Genome testSubject, double[][] input, double[][] ideal)
        {
            double error = 0;
            double elementCount = 0;

            Parallel.For(0, input.Length, row =>
            {
                double[] output = model.Compute(input[row], testSubject);
                for (int col = 0; col < output.Length; col++)
                {
                    double delta = output[col] - ideal[row][col];
                    error += delta * delta;
                    elementCount++;
                }
            });

            return Math.Sqrt(error / elementCount);
        }

        /// <summary>
        /// Score the model for regression or classification.
        /// For classification, the score is the percentage correct.  
        /// For regression, the score is mean square error (MSE).
        /// http://www.heatonresearch.com/wiki/Mean_Square_Error
        /// </summary>
        /// <param name="model">The model to score.</param>
        /// <param name="testSubject">The test subject.</param>
        /// <param name="input">The input.</param>
        /// <param name="ideal">The ideal.</param>
        /// <returns>The score.</returns>
        public static double Score(IGAModel model, Genome testSubject, double[][] input, double[][] ideal)
        {
            if (ideal[0].Length > 1)
            {
                return ScoreClassification(model, testSubject, input, ideal);
            }
            return ScoreRegression(model, testSubject, input, ideal);
        }

        /// <summary>
        /// Produce a new genome, by randomly changing the parent.  However, the parent is
        /// not actually changed. 
        /// </summary>
        /// <param name="rnd">Random number generator.</param>
        /// <param name="parent">The parent genome.</param>
        /// <param name="minRange">The minimum amount by which a gene will change.</param>
        /// <param name="maxRange">The maximum amount by which a gene will change.</param>
        /// <returns>The new child genome.</returns>
        public static Genome Mutate(Random rnd, Genome parent, double minRange, double maxRange)
        {
            bool didMutate = false;
            var result = new Genome(parent.Genes.Length);
            for (int i = 0; i < parent.Genes.Length; i++)
            {
                if (rnd.NextDouble() < 0.2)
                {
                    double d = RandomUtil.RangeDouble(rnd, minRange, maxRange);
                    result.Genes[i] += d;
                    didMutate = true;
                }
                else
                {
                    result.Genes[i] = parent.Genes[i];
                }
            }

            if (!didMutate)
            {
                int i = RandomUtil.RangeInt(rnd, 0, parent.Genes.Length);
                result.Genes[i] += RandomUtil.RangeDouble(rnd, minRange, maxRange);
            }

            return result;
        }

        /// <summary>
        /// Create a single child genome by performing crossover on two or more parents.
        /// </summary>
        /// <param name="rnd">A random number generator.</param>
        /// <param name="parents">The parents.</param>
        /// <param name="cutLength">The cut length for sequences of genes that crossover together.</param>
        /// <returns>The new child genome.</returns>
        public static Genome[] Crossover(Random rnd, Genome[] parents, int cutLength)
        {
            int genomeSize = parents[0].Genes.Length;
            var offspring = new Genome(genomeSize);

            var result = new Genome[1];
            result[0] = offspring;

            int currentIndex = 0;
            int cutCount = 0;
            int parentIndex = 0;
            Genome currentParent = parents[parentIndex];

            while (currentIndex < parents[0].Genes.Length)
            {
                offspring.Genes[currentIndex] = currentParent.Genes[currentIndex];
                currentIndex++;
                cutCount++;
                if (cutCount > cutLength)
                {
                    parentIndex++;
                    if (parentIndex >= parents.Length)
                    {
                        parentIndex = 0;
                    }
                    currentParent = parents[parentIndex];
                    cutCount = 0;
                }
            }

            return result;
        }


        public static Genome GenerateRandomGenome(Random rnd, ConfigScript script, int genomeSize)
        {
            var result = new Genome(genomeSize);

            for (int i = 0; i < result.Genes.Length; i++)
            {
                result.Genes[i] = rnd.NextDouble();
            }

            return result;
        }
    }
}