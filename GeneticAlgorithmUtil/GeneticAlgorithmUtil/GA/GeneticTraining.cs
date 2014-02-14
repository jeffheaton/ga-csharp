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
using System.Diagnostics;
using System.Threading;

namespace GeneticAlgorithmUtil.GA
{
    /// <summary>
    /// Perform genetic multi-threaded training.
    /// 
    /// This Genetic Algorithm employs many advanced techniques from current research.  
    ///
    /// = Fully Multithreaded – This program makes use of the C# Parallel class to ensure that all available processors and 
    ///   cores are fully utilized. This results in very fast training on modern Multi Core CPU’s.
    /// 
    /// = Non-Epoch Based – Unlike many GA’s the population is not rebuilt each epoch.  Rather parents are chosen from the population 
    ///   and resulting children replace weaker genomes.  This is very efficient for multi-threading.  It is also biologically plausible.  
    ///   We do not have clear-cut lines (epochs) of when each generation begins and ends in the real world.
    /// 
    /// = Tournament Selection – When a parent must be chosen, for mutation or crossover, we choose a random member of the population 
    ///   and then try 10 more random members to get a more suited potential parent.  When unfit genomes must be removed, this process 
    ///   is run in reverse.  This is very efficient for multi-threading, and is also biologically plausible.  You don’t have to 
    ///   outrun the fastest tiger on earth, just the tigers you randomly encounter on a given day!  This also removes the need for 
    ///   a common GA technique, know as elitism.
    /// 
    /// = More than Two Parent Crossover – Why not have more than two parents?  A child can be created from several optimal parents.  
    ///   This is a very interesting technique that I first learned about from a Forecasting & Futurism by Dave Snell.
    /// </summary>
    public class GeneticTraining
    {
        /// <summary>
        /// Keep track of when the last update was sent, do not update the GUI more than once a second.
        /// </summary>
        private readonly Stopwatch _lastUpdate = new Stopwatch();

        /// <summary>
        /// Keep track of the listeners to update of progress.
        /// </summary>
        private readonly IList<ITrainingProgressListener> _listeners = new List<ITrainingProgressListener>();

        /// <summary>
        /// The population that we are training.
        /// </summary>
        private readonly Population _population;

        /// <summary>
        /// The expected output from training.  Note: use of exepcted output is optional, 
        /// depending on how the scoring function is constructed in the model.
        /// </summary>
        private readonly double[][] _trainingIdeal;

        /// <summary>
        /// The input vectors that are presented to the model.
        /// </summary>
        private readonly double[][] _trainingInput;

        /// <summary>
        /// Used to synchronize requests to stop training.
        /// </summary>
        private readonly AutoResetEvent _trainingStopEvent;

        /// <summary>
        /// Tracks how many genetic operations are performed.
        /// </summary>
        private int _operationCount;

        /// <summary>
        /// True, if training is running.
        /// </summary>
        private bool _running;

        /// <summary>
        /// Construct the trainer.
        /// </summary>
        /// <param name="population">The population to train.</param>
        /// <param name="trainingInput">The input.</param>
        /// <param name="trainingIdeal">The expected output.</param>
        public GeneticTraining(Population population, double[][] trainingInput, double[][] trainingIdeal)
        {
            _population = population;
            _trainingInput = trainingInput;
            _trainingIdeal = trainingIdeal;
            _trainingStopEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// True, if we are currently training.
        /// </summary>
        public bool IsTraining
        {
            get { return _running; }
        }

        /// <summary>
        /// All of the listeners.  Listeners recieve updates on the training progress.
        /// </summary>
        public IList<ITrainingProgressListener> Listeners
        {
            get { return _listeners; }
        }

        /// <summary>
        /// Start training.  This is an asynch opration and will return instantly, however
        /// training will be running in the background.
        /// </summary>
        public void Start()
        {
            var t = new Thread(ThreadBackgroundProcess);
            t.Start();
        }

        /// <summary>
        /// Request that training stop ASAP.  This method will block until training stops.
        /// </summary>
        public void RequestStop()
        {
            _running = false;
            _trainingStopEvent.WaitOne();
        }

        /// <summary>
        /// The background training process.
        /// </summary>
        public void ThreadBackgroundProcess()
        {
            var rnd = new Random();
            _running = true;
            _trainingStopEvent.Reset();
            _lastUpdate.Start();

            while (_running)
            {
                // first, decide if we should do mutation or crossover
                double d = rnd.NextDouble();

                if (d < _population.Config.MutationPercent)
                {
                    PerformMutation(rnd);
                }
                else
                {
                    PerformCrossover(rnd);
                }

                // housekeeping, send updates, etc.
                _operationCount++;

                if (_lastUpdate.ElapsedMilliseconds > 1000)
                {
                    _lastUpdate.Restart();
                    BroadcastListeners();
                }
            }

            _trainingStopEvent.Set();
        }

        /// <summary>
        /// Perform a crossover.
        /// </summary>
        /// <param name="rnd">The random number generator.</param>
        private void PerformCrossover(Random rnd)
        {
            // first, find the correct number of optimal parents
            int parentsNeeded = _population.Config.MaxParents;
            var parents = new Genome[parentsNeeded];
            int parentIndex = 0;
            int tries = 0;

            while (parentIndex < parentsNeeded)
            {
                // select a potential parent
                Genome parent = _population.TournamentForBest(rnd);
                bool goodParent = true;

                // do we already have this parent?
                for (int i = 0; i < parentIndex; i++)
                {
                    if (parents[i] == parent)
                    {
                        goodParent = false;
                        break;
                    }
                }

                // add the parent if acceptable
                if (goodParent)
                {
                    parents[parentIndex++] = parent;
                }

                // might get into an endless loop with a really small population, don't do that!
                tries++;
                if (tries > 10000)
                {
                    throw new InvalidOperationException("Failed to find acceptable parenents for crossover.");
                }
            }

            // we now have a good batch of parents, so call the crossover operation
            Genome[] children = _population.Config.Model.Crossover(rnd, parents);

            // integrate any children into the population
            foreach (Genome child in children)
            {
                child.Score = _population.Config.Model.Score(child, _trainingInput, _trainingIdeal);
                _population.AddChildAndReplace(rnd, child);
            }
        }

        /// <summary>
        /// Perform a mutation.
        /// </summary>
        /// <param name="rnd">Random number generator.</param>
        private void PerformMutation(Random rnd)
        {
            Genome parent = _population.TournamentForBest(rnd);
            Genome child = _population.Config.Model.Mutate(rnd, parent);
            child.Score = _population.Config.Model.Score(child, _trainingInput, _trainingIdeal);
            _population.AddChildAndReplace(rnd, child);
        }

        /// <summary>
        /// Broadcast a message to all listeners.
        /// </summary>
        private void BroadcastListeners()
        {
            foreach (ITrainingProgressListener listener in _listeners)
            {
                listener.TrainingUpdate(_operationCount, _population.BestGenome.Score);
            }
        }

        /// <summary>
        /// Add a listener.
        /// </summary>
        /// <param name="listener">The listener to add.</param>
        public void AddListener(ITrainingProgressListener listener)
        {
            _listeners.Add(listener);
        }
    }
}