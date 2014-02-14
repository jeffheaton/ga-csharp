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
using GeneticAlgorithmUtil.Config;
using GeneticAlgorithmUtil.GA;

namespace GeneticAlgorithmUtil.GA
{
    /// <summary>
    /// Holds a population of genomes. 
    /// This class is serializable.
    /// </summary>
    [Serializable]
    public class Population
    {
        /// <summary>
        /// The genomes.
        /// </summary>
        private readonly IList<Genome> _genomes = new List<Genome>();

        /// <summary>
        /// The best genome.
        /// </summary>
        private Genome _bestGenome;

        /// <summary>
        /// The config script.
        /// </summary>
        private ConfigScript _config;

        /// <summary>
        /// The genomes.
        /// </summary>
        public IList<Genome> Genomes
        {
            get { return _genomes; }
        }

        /// <summary>
        /// The config script.
        /// </summary>
        public ConfigScript Config
        {
            get { return _config; }
        }

        /// <summary>
        /// The best genome.
        /// </summary>
        public Genome BestGenome
        {
            get { return _bestGenome; }
        }

        /// <summary>
        /// Setup the population.
        /// </summary>
        /// <param name="config"></param>
        public void Init(ConfigScript config)
        {
            _config = config;
        }

        /// <summary>
        /// Generate a random starting population.
        /// </summary>
        public void Generate()
        {
            var rnd = new Random();
            _genomes.Clear();
            _bestGenome = null;

            for (int i = 0; i < _config.PopulationSize; i++)
            {
                Genome genome = _config.Model.GenerateRandomGenome(rnd, _config);
                _genomes.Add(genome);
                UpdateBestGenome(genome);
            }
        }

        /// <summary>
        /// Score all genomes.
        /// </summary>
        /// <param name="trainingInput">The training input.</param>
        /// <param name="trainingIdeal">The ideal output.</param>
        public void ScoreAll(double[][] trainingInput, double[][] trainingIdeal)
        {
            _bestGenome = null;
            foreach (Genome genome in _genomes)
            {
                UpdateBestGenome(genome);
                genome.Score = _config.Model.Score(genome, trainingInput, trainingIdeal);
            }
        }

        /// <summary>
        /// Determine if one genome is better than another.
        /// </summary>
        /// <param name="a">The first genome.</param>
        /// <param name="b">The second genome.</param>
        /// <returns>True, if a is better than b.</returns>
        public bool IsBetterThan(Genome a, Genome b)
        {
            if (_config.MaxGoal)
            {
                return a.Score > b.Score;
            }
            return a.Score < b.Score;
        }

        /// <summary>
        /// Hold a tournament (10 rounds) for the best genome.
        /// </summary>
        /// <param name="rnd">A random number generator.</param>
        /// <returns>The best genome.</returns>
        public Genome TournamentForBest(Random rnd)
        {
            Genome result = null;

            for (int i = 0; i < 10; i++)
            {
                var index = (int) (rnd.NextDouble()*_genomes.Count);
                Genome contender = _genomes[index];
                if (result == null || IsBetterThan(contender, result))
                {
                    result = contender;
                }
            }

            return result;
        }

        /// <summary>
        /// Hold a toournament (10 rounds) for the worst genome.
        /// </summary>
        /// <param name="rnd"></param>
        /// <returns>A random number generator.</returns>
        public int TournamentForWorst(Random rnd)
        {
            Genome result = null;
            int resultIndex = -1;

            for (int i = 0; i < 10; i++)
            {
                var index = (int) (rnd.NextDouble()*_genomes.Count);
                Genome contender = _genomes[index];
                if (result == null || !IsBetterThan(contender, result))
                {
                    result = contender;
                    resultIndex = index;
                }
            }

            return resultIndex;
        }

        /// <summary>
        /// Update the best genome.
        /// </summary>
        /// <param name="g">The genome to evaluate.</param>
        private void UpdateBestGenome(Genome g)
        {
            if (_config.MaxGoal)
            {
                if (_bestGenome == null || g.Score > _bestGenome.Score)
                {
                    _bestGenome = g;
                }
            }
            else
            {
                if (_bestGenome == null || g.Score < _bestGenome.Score)
                {
                    _bestGenome = g;
                }
            }
        }

        /// <summary>
        /// Add a new child genome and replace low-scoring population member.
        /// This is how unfit genomes are removed.
        /// </summary>
        /// <param name="rnd">A random number generator.</param>
        /// <param name="child">The new child to add.</param>
        public void AddChildAndReplace(Random rnd, Genome child)
        {
            int killIndex = TournamentForWorst(rnd);
            _genomes[killIndex] = child;
            UpdateBestGenome(child);
        }
    }
}