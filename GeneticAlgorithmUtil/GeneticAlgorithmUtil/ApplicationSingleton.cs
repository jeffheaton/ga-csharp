using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeneticAlgorithmUtil.GA;
using GeneticAlgorithmUtil.Config;

namespace GeneticAlgorithmUtil
{
    public class ApplicationSingleton
    {
        private static ApplicationSingleton _instance;

        private ApplicationSingleton() { }

        public static ApplicationSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApplicationSingleton();
                }
                return _instance;
            }
        }

        public Population GenomePopulation { get; set; }
        public ConfigScript Config { get; set; }
        public GeneticTraining Train { get; set; }
    }
}
