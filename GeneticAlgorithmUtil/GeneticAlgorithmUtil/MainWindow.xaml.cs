using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using GeneticAlgorithmUtil.Config;
using GeneticAlgorithmUtil.GA;
using GeneticAlgorithmUtil.Util;
using Microsoft.Win32;

namespace GeneticAlgorithmUtil
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITrainingProgressListener
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void TrainingUpdate(int genomes, double bestScore)
        {
            Dispatcher.BeginInvoke(
                (Action) delegate
                {
                    lblGenomes.Content = genomes;
                    lblBestScore.Content = bestScore;
                    UpdateButtons();
                });
        }

        private void bttnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = txtScriptFile.Text = Directory.GetCurrentDirectory();

            // if we are running this from the IDE, then strip off extra path information
            int idx = path.ToLower().IndexOf("\\bin\\debug");
            if (idx == -1)
            {
                idx = path.ToLower().IndexOf("\\bin\\release");
            }

            if (idx != -1)
            {
                path = path.Substring(0, idx);
                idx = path.LastIndexOf('\\');
                path = path.Substring(0, idx);
            }

            // attach the script filename

            txtScriptFile.Text = Path.Combine(path, "script.xml");

            // set buttons to default state
            UpdateButtons();
        }

        private void bttnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "txt files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(txtScriptFile.Text);
            openFileDialog.FileName = Path.GetFileName(txtScriptFile.Text);

            if (openFileDialog.ShowDialog() == true)
            {
                txtScriptFile.Text = openFileDialog.FileName;
            }
        }

        private void bttnLoadScript_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var script = new ConfigScript();
                script.Load(txtScriptFile.Text);
                script.Analyze(script.TrainingFile);
                UpdateButtons();
                lblPopFile.Content = script.PopulationFile;
                lblEvaluateFilesIn.Content = "Input: " + script.EvaluationFile;
                lblEvaluateFilesOut.Content = "Output: " + script.EvaluationFile;

                ApplicationSingleton.Instance.Config = script;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name );
            }
        }

        private void bttnGeneratePop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (new WaitCursor())
                {
                    var pop = new Population();
                    pop.Init(ApplicationSingleton.Instance.Config);
                    ApplicationSingleton.Instance.GenomePopulation = pop;
                    pop.Generate();
                    lblBestScore.Content = ApplicationSingleton.Instance.GenomePopulation.BestGenome.Score;

                    var trainingFile = new DataFile(ApplicationSingleton.Instance.Config);
                    trainingFile.LoadExcel(ApplicationSingleton.Instance.Config.TrainingFile);
                    double[][] trainingInput = trainingFile.GenerateInputData();
                    double[][] trainingIdeal = trainingFile.GenerateIdealData();

                    pop.ScoreAll(trainingInput, trainingIdeal);

                    UpdateButtons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private void bttnLoadPop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(ApplicationSingleton.Instance.Config.PopulationFile,
                    FileMode.Open, FileAccess.Read, FileShare.Read);
                ApplicationSingleton.Instance.GenomePopulation = (Population) formatter.Deserialize(stream);
                ApplicationSingleton.Instance.Config = ApplicationSingleton.Instance.GenomePopulation.Config;
                stream.Close();
                lblBestScore.Content = ApplicationSingleton.Instance.GenomePopulation.BestGenome.Score;
                UpdateButtons();
                MessageBox.Show("Load successful", "Load");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private void bttnSavePop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IFormatter ser = new BinaryFormatter();
                Stream stream = new FileStream(ApplicationSingleton.Instance.Config.PopulationFile,
                    FileMode.Create, FileAccess.Write, FileShare.None);
                ser.Serialize(stream, ApplicationSingleton.Instance.GenomePopulation);
                stream.Close();
                UpdateButtons();
                MessageBox.Show("Save successful", "Save");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private void bttnTrain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var trainingFile = new DataFile(ApplicationSingleton.Instance.Config);
                trainingFile.LoadExcel(ApplicationSingleton.Instance.Config.TrainingFile);
                double[][] trainingInput = trainingFile.GenerateInputData();
                double[][] trainingIdeal = trainingFile.GenerateIdealData();

                ApplicationSingleton.Instance.Train = new GeneticTraining(
                    ApplicationSingleton.Instance.GenomePopulation,
                    trainingInput,
                    trainingIdeal);
                ApplicationSingleton.Instance.Train.Start();
                ApplicationSingleton.Instance.Train.AddListener(this);
                UpdateButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        private void bttnStopTrain_Click(object sender, RoutedEventArgs e)
        {
            ApplicationSingleton.Instance.Train.RequestStop();
            ApplicationSingleton.Instance.Train = null;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            Dispatcher.BeginInvoke(
                (Action) delegate
                {
                    bool isConfigured = ApplicationSingleton.Instance.Config != null;
                    bool isTraining = ApplicationSingleton.Instance.Train != null &&
                                      ApplicationSingleton.Instance.Train.IsTraining;
                    bool isTrainInit = ApplicationSingleton.Instance.Train != null;
                    bool hasPopulation = ApplicationSingleton.Instance.GenomePopulation != null;

                    bttnGeneratePop.IsEnabled = isConfigured && !isTraining;
                    bttnLoadPop.IsEnabled = isConfigured && !isTraining;
                    bttnSavePop.IsEnabled = hasPopulation && !isTraining;
                    bttnTrain.IsEnabled = isConfigured && !isTraining && !isTrainInit && hasPopulation;
                    bttnStopTrain.IsEnabled = isConfigured && isTraining;
                    bttnEvaluate.IsEnabled = isConfigured && !isTraining && !isTrainInit && hasPopulation;
                });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ApplicationSingleton.Instance.Train != null && ApplicationSingleton.Instance.Train.IsTraining)
            {
                ApplicationSingleton.Instance.Train.RequestStop();
            }
            Application.Current.Shutdown();
        }

        private void bttnEvaluate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var trainingFile = new DataFile(ApplicationSingleton.Instance.Config);
                trainingFile.LoadExcel(ApplicationSingleton.Instance.Config.EvaluationFile);
                double[][] evalInput = trainingFile.GenerateInputData();
                var evalOutput = new double[evalInput.Length][];

                var genome = ApplicationSingleton.Instance.GenomePopulation.BestGenome;
                double sc = ApplicationSingleton.Instance.Config.Model.Score(genome, evalInput,
                    trainingFile.GenerateIdealData());

                for (int row = 0; row < evalInput.Length; row++)
                {
                    double[] output = ApplicationSingleton.Instance.Config.Model.Compute(evalInput[row], genome);
                    for (int col = 0; col < output.Length; col++)
                    {
                        evalOutput[row] = output;
                    }
                }

                trainingFile.WriteExcel(ApplicationSingleton.Instance.Config.EvaluationOutputFile, evalOutput);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }
    }
}