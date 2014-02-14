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
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithmUtil.Config;
using GeneticAlgorithmUtil.GA;

namespace GeneticAlgorithmUtil.Models
{
    /// <summary>
    /// A GA model that implements a Radial Basis Function (RBF) Network.  
    /// http://en.wikipedia.org/wiki/Radial_basis_function_network
    /// 
    /// The RBF network can perform both classification and regression.
    /// To perform regression, specify a single output.  To perform
    /// classification, one-of-n encoding is used on multiple outputs.
    /// http://www.heatonresearch.com/wiki/One_of_n
    /// </summary>
    [Serializable]
    public class NetworkRBF : IGAModel
    {
        /// <summary>
        /// The cut length for crossover.
        /// </summary>
        private int _cutLength;

        /// <summary>
        /// The genome size.
        /// </summary>
        private int _genomeSize;

        /// <summary>
        /// Index to where the input weights start in the gene array.
        /// </summary>
        private int _indexInputWeights;

        /// <summary>
        /// Index to where the output weights start in the gene array.
        /// </summary>
        private int _indexOutputWeights;

        /// <summary>
        /// Index to where the RBF parameters start in the gene array.
        /// </summary>
        private int _indexRBFParams;

        /// <summary>
        /// The number of inputs.
        /// </summary>
        private int _inputCount;

        /// <summary>
        /// The number of outputs.
        /// </summary>
        private int _outputCount;

        /// <summary>
        /// The number of RBF functions to use.  This is set as a parameter in the script.  
        /// Higher values take longer to train, but increase the complexity of what can be learned.
        /// </summary>
        private int _rbfCount;

        public void Init(ConfigScript script, string config)
        {
            _inputCount = script.DetermineRawInputCount();
            _outputCount = script.DetermineRawOutputCount();

            // parse out name-value pairs
            Dictionary<string, string> pairs =
                config.Split(',')
                    .Select(x => x.Split('='))
                    .ToDictionary(y => y[0], y => y[1]);

            _rbfCount = int.Parse(pairs["rbf"]);

            // calculate input and output weight counts
            // add 1 to output to account for an extra bias node
            int inputWeightCount = _inputCount*_rbfCount;
            int outputWeightCount = (_rbfCount + 1)*_outputCount;
            int rbfParams = (_inputCount + 1)*_rbfCount;
            _genomeSize = inputWeightCount + outputWeightCount + rbfParams;

            _indexInputWeights = 0;
            _indexRBFParams = inputWeightCount;
            _indexOutputWeights = _indexRBFParams + rbfParams;

            _cutLength = _genomeSize/script.MaxParents;
            if (_cutLength < 1)
            {
                _cutLength = 1;
            }
        }

        public double[] Compute(double[] input, Genome genome)
        {
            // first, compute the output values of each of the RBFs.
            // Add in one additional RBF output for bias (always set to one).
            var rbfOutput = new double[_rbfCount + 1];
            rbfOutput[rbfOutput.Length - 1] = 1; // bias

            for (int rbfIndex = 0; rbfIndex < _rbfCount; rbfIndex++)
            {
                // weight the input
                var weightedInput = new double[input.Length];

                for (int inputIndex = 0; inputIndex < input.Length; inputIndex++)
                {
                    int memoryIndex = _indexInputWeights + (rbfIndex*_inputCount) + inputIndex;
                    weightedInput[inputIndex] = input[inputIndex]*genome.Genes[memoryIndex];
                }

                // calculate the rbf
                rbfOutput[rbfIndex] = CalculateGaussian(genome, rbfIndex, weightedInput);
            }

            // second, calculate the output, which is the result of the weighted result of the RBF's.
            var result = new double[_outputCount];

            for (int outputIndex = 0; outputIndex < result.Length; outputIndex++)
            {
                double sum = 0;
                for (int rbfIndex = 0; rbfIndex < rbfOutput.Length; rbfIndex++)
                {
                    int memoryIndex = _indexOutputWeights + (outputIndex*rbfOutput.Length) + rbfIndex;
                    sum += rbfOutput[rbfIndex]*genome.Genes[memoryIndex];
                }
                result[outputIndex] = sum;
            }

            return result;
        }

        public double Score(Genome testSubject, double[][] input, double[][] ideal)
        {
            return TypicalGAModel.Score(this, testSubject, input, ideal);
        }

        public Genome Mutate(Random rnd, Genome parent)
        {
            return TypicalGAModel.Mutate(rnd, parent, -0.01, +0.01);
        }

        public Genome[] Crossover(Random rnd, Genome[] parents)
        {
            return TypicalGAModel.Crossover(rnd, parents, _cutLength);
        }


        public Genome GenerateRandomGenome(Random rnd, ConfigScript script)
        {
            return TypicalGAModel.GenerateRandomGenome(rnd, script, _genomeSize);
        }

        private double CalculateGaussian(Genome genome, int rbfIndex, double[] xValues)
        {
            int rbfArrayIndex = _indexRBFParams + ((_inputCount + 1)*rbfIndex);
            double value = 0;
            double width = genome.Genes[rbfArrayIndex];

            for (int i = 0; i < _inputCount; i++)
            {
                double center = genome.Genes[rbfArrayIndex + i + 1];
                value += Math.Pow(xValues[i] - center, 2)/(2.0*width*width);
            }

            return Math.Exp(-value);
        }
    }
}