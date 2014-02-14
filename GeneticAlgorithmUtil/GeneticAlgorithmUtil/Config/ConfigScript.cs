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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using GeneticAlgorithmUtil.Models;

namespace GeneticAlgorithmUtil.Config
{
    /// <summary>
    /// Loads in the configuration script that governs how the Genetic Algorithm will function.
    /// </summary>
    [Serializable]
    public class ConfigScript
    {
        /// <summary>
        /// A map that provides quick lookup to the fields.
        /// </summary>
        private readonly IDictionary<string, DataField> _fieldMap = new Dictionary<string, DataField>();

        /// <summary>
        /// The fields in the training set Excel file.
        /// </summary>
        private readonly IList<DataField> _fields = new List<DataField>();

        /// <summary>
        /// The filename of the evaluation file.
        /// </summary>
        private string _evaluationFile;

        /// <summary>
        /// The evaluation output file.
        /// </summary>
        private string _evaluationOutputFile;

        /// <summary>
        /// True, if the goal is to maximize.
        /// </summary>
        private bool _maxGoal;

        /// <summary>
        /// The maximum number of parents, typically 2 or higher.
        /// </summary>
        private int _maxParents;

        /// <summary>
        /// The model that defines what the Genetic Algorithm must optimize.
        /// </summary>
        private IGAModel _model;

        /// <summary>
        /// The name of the model to use.
        /// </summary>
        private string _modelConfig;

        /// <summary>
        /// The percent of new genomes that are from mutation. 
        /// (as opposed to crossover)
        /// </summary>
        private double _mutationPercent;

        /// <summary>
        /// The high value to normalize into.
        /// </summary>
        private double _normalizeHigh;

        /// <summary>
        /// The low value to normalize into.
        /// </summary>
        private double _normalizeLow;

        /// <summary>
        /// The file that stores the population.
        /// </summary>
        private string _populationFile;

        /// <summary>
        /// The desired size of the population.
        /// </summary>
        private int _populationSize;

        /// <summary>
        /// The field to predict.
        /// </summary>
        private string _predictField;
     
        /// <summary>
        /// The training file.
        /// </summary>
        private string _trainingFile;

        /// <summary>
        /// The desired size of the population.
        /// </summary>
        public int PopulationSize
        {
            get { return _populationSize; }
        }

        /// <summary>
        /// The maximum number of parents, typically 2 or higher.
        /// </summary>
        public int MaxParents
        {
            get { return _maxParents; }
        }

        /// <summary>
        /// The percent of new genomes that are from mutation. 
        /// (as opposed to crossover)
        /// </summary>
        public double MutationPercent
        {
            get { return _mutationPercent; }
        }

        /// <summary>
        /// The model that defines what the Genetic Algorithm must optimize.
        /// </summary>
        public IGAModel Model
        {
            get { return _model; }
        }

        /// <summary>
        /// True, if the goal is to maximize.
        /// </summary>
        public bool MaxGoal
        {
            get { return _maxGoal; }
        }

        /// <summary>
        /// The file that stores the population.
        /// </summary>
        public string PopulationFile
        {
            get { return _populationFile; }
        }

        /// <summary>
        /// The training file.
        /// </summary>
        public string TrainingFile
        {
            get { return _trainingFile; }
        }

        /// <summary>
        /// The filename of the evaluation file.
        /// </summary>
        public string EvaluationFile
        {
            get { return _evaluationFile; }
        }

        
        /// <summary>
        /// The evaluation output file.
        /// </summary>
        public string EvaluationOutputFile
        {
            get { return _evaluationOutputFile; }
        }

        /// <summary>
        /// The field to predict.
        /// </summary>
        public string PredictField
        {
            get { return _predictField; }
        }

        /// <summary>
        /// The high value to normalize into.
        /// </summary>
        public double NormalizeHigh
        {
            get { return _normalizeHigh; }
        }

        /// <summary>
        /// The low value to normalize into.
        /// </summary>
        public double NormalizeLow
        {
            get { return _normalizeLow; }
        }

        /// <summary>
        /// The name of the model to use.
        /// </summary>
        public string ModelConfig
        {
            get { return _modelConfig; }
        }

        /// <summary>
        /// The fields we are processing.
        /// </summary>
        public IList<DataField> Fields
        {
            get { return _fields; }
        }

        /// <summary>
        /// A map that provides quick lookup to the fields.
        /// </summary>
        public IDictionary<string, DataField> FieldMap
        {
            get { return _fieldMap; }
        }

        /// <summary>
        /// Load specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void Load(string filename)
        {
            XDocument xml = XDocument.Load(filename);

            XElement root = xml.Element("GeneticAlgorithmUtil");

            // read in attributes from the GA node
            XElement ga = root.Element("GA");
            _populationSize = int.Parse(ga.Element("PopulationSize").Value);
            _maxParents = int.Parse(ga.Element("MaxParents").Value);
            _mutationPercent = double.Parse(ga.Element("MutationPercent").Value, CultureInfo.InvariantCulture);

            // determine the score goal
            _maxGoal = (string.Compare(ga.Element("ScoreGoal").Value, "max", true) == 0);

            // use reflection to create the model class
            string typeStr = ga.Element("Model").Value;

            if (ga.Element("Model").Attribute("config") != null)
            {
                _modelConfig = ga.Element("Model").Attribute("config").Value;
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            Type type = assembly.GetTypes()
                .First(t => t.Name == typeStr);

            _model = (IGAModel) Activator.CreateInstance(type);

            // read in information from the files node
            XElement files = root.Element("Files");

            _populationFile = files.Element("PopulationFile").Value;
            _trainingFile = files.Element("TrainingFile").Value;
            _evaluationFile = files.Element("EvaluationFile").Value;
            _evaluationOutputFile = files.Element("EvaluationOutputFile").Value;

            // make sure that the filenames have path information
            string scriptPath = Path.GetDirectoryName(filename);

            if (scriptPath.Length == 0)
            {
                scriptPath = Directory.GetCurrentDirectory();
            }

            if (Path.GetDirectoryName(_populationFile).Length == 0)
            {
                _populationFile = Path.Combine(scriptPath, _populationFile);
            }

            if (Path.GetDirectoryName(_trainingFile).Length == 0)
            {
                _trainingFile = Path.Combine(scriptPath, _trainingFile);
            }

            if (Path.GetDirectoryName(_evaluationFile).Length == 0)
            {
                _evaluationFile = Path.Combine(scriptPath, _evaluationFile);
            }

            if (Path.GetDirectoryName(_evaluationOutputFile).Length == 0)
            {
                _evaluationOutputFile = Path.Combine(scriptPath, _evaluationOutputFile);
            }

            // read in normalization field data
            XElement fmt = root.Element("DataFileFormat");

            _predictField = fmt.Element("PredictField").Value;
            _normalizeHigh = double.Parse(fmt.Element("Normalize").Attribute("max").Value, CultureInfo.InvariantCulture);
            _normalizeLow = double.Parse(fmt.Element("Normalize").Attribute("min").Value, CultureInfo.InvariantCulture);

            // read in the normalized fields
            XElement fields = root.Element("DataFileFormat");
            foreach (XElement fieldElement in fields.Elements("Fields").Elements("Field"))
            {
                var dataField = new DataField();
                string actionStr = fieldElement.Attribute("action").Value;
                string nameStr = fieldElement.Attribute("name").Value;

                dataField.Name = nameStr;

                if (string.Compare(actionStr, "norm", true) == 0)
                {
                    dataField.Action = FieldAction.Normalize;
                }
                else if (string.Compare(actionStr, "class", true) == 0)
                {
                    dataField.Action = FieldAction.Class;
                }
                else if (string.Compare(actionStr, "ignore", true) == 0)
                {
                    dataField.Action = FieldAction.Ignore;
                }
                else if (string.Compare(actionStr, "direct", true) == 0)
                {
                    dataField.Action = FieldAction.Direct;
                }

                dataField.NormalizedLow = NormalizeLow;
                dataField.NormalizedHigh = NormalizeHigh;

                _fields.Add(dataField);
                _fieldMap.Add(dataField.Name, dataField);
            }
        }

        /// <summary>
        /// Analyze the specified file and determine ranges for normalization.
        /// </summary>
        /// <param name="filename"></param>
        public void Analyze(string filename)
        {
            var file = new DataFile(this);
            file.LoadExcel(filename);

            foreach (var line in file.Data)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    string d = line[i];
                    DataField df = _fieldMap[file.Headers[i]];
                    df.Analyze(d);
                }
            }

            // init the model
            _model.Init(this, _modelConfig);
        }

        /// <summary>
        /// Determine the number of input fields.
        /// </summary>
        /// <returns>The number of input fields.</returns>
        public int DetermineRawInputCount()
        {
            return _fields.Where(df => string.Compare(PredictField, df.Name, true) != 0).Sum(df => df.DetermineRawFieldCount());
        }

        /// <summary>
        /// Determine the number of output fields.
        /// </summary>
        /// <returns>The output field count.</returns>
        public int DetermineRawOutputCount()
        {
            int result = _fields.Where(df => string.Compare(PredictField, df.Name, true) == 0).Sum(df => df.DetermineRawFieldCount());

            if (result == 0)
            {
                throw new InvalidDataException("Can't find predict field: " + PredictField);
            }

            return result;
        }
    }
}