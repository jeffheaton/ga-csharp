using System;
using System.Collections.Generic;
using GeneticAlgorithmUtil.Config;
using Microsoft.Office.Interop.Excel;

namespace GeneticAlgorithmUtil
{
    public class DataFile
    {
        /// <summary>
        ///     The block size for batch reading.
        /// </summary>
        private const int BlockSize = 1000;

        private readonly ConfigScript _config;
        private readonly IList<string[]> _data = new List<string[]>();
        private readonly IList<string> _headers = new List<string>();

        public DataFile(ConfigScript config)
        {
            _config = config;
        }

        /// <summary>
        ///     The column headers.
        /// </summary>
        public IList<string> Headers
        {
            get { return _headers; }
        }

        /// <summary>
        ///     The loaded data.
        /// </summary>
        public IList<string[]> Data
        {
            get { return _data; }
        }

        /// <summary>
        ///     Load a Microsoft Excel file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        public void LoadExcel(string filename)
        {
            // Setup COM connection to Excel
            _Application excel = new Application();
            Workbook wkbk = excel.Workbooks.Open(filename, ReadOnly: true);
            Worksheet wksht = wkbk.Worksheets.Item[1];

            // Determine the headers.
            dynamic headers = wksht.get_Range("A1:ALL1").Value;
            int idx = 1;

            while (headers[1, idx] != null)
            {
                string str = headers[1, idx++].ToString();
                _headers.Add(str);
            }

            // Load in the data.  Read the data in blocks (ranges) for 
            // better performance.  
            bool done = false;
            int row = 2;

            while (!done)
            {
                Range c1 = wksht.Cells[row, 1];
                Range c2 = wksht.Cells[row + BlockSize, _headers.Count];
                Range loadingRange = wksht.get_Range(c1, c2);
                dynamic data = loadingRange.Value;

                // read in one block
                for (int i = 1; i <= BlockSize; i++)
                {
                    // have we hit the end?
                    if (data[i, 1] == null)
                    {
                        done = true;
                        break;
                    }

                    // Allocate space for a loaded row
                    var loadedRow = new string[_headers.Count];
                    for (int j = 0; j < loadedRow.Length; j++)
                    {
                        loadedRow[j] = data[i, j + 1].ToString();
                    }
                    _data.Add(loadedRow);
                }

                row += BlockSize;
            }

            wkbk.Close();
        }

        public double[][] GenerateInputData()
        {
            var result = new double[_data.Count][];
            int rawColCount = _config.DetermineRawInputCount();
            int colCount = _headers.Count;

            for (int row = 0; row < _data.Count; row++)
            {
                result[row] = new double[rawColCount];

                int targetIndex = 0;
                for (int col = 0; col < colCount; col++)
                {
                    string columnName = _headers[col];
                    if (string.Compare(columnName, _config.PredictField, true) != 0)
                    {
                        DataField df = _config.FieldMap[columnName];
                        targetIndex = df.Normalize(result[row], targetIndex, _data[row][col]);
                    }
                }
                Console.WriteLine(targetIndex);
            }

            return result;
        }

        public double[][] GenerateIdealData()
        {
            var result = new double[_data.Count][];
            int rawColCount = _config.DetermineRawOutputCount();
            int colCount = _headers.Count;

            for (int row = 0; row < _data.Count; row++)
            {
                result[row] = new double[rawColCount];

                int targetIndex = 0;
                for (int col = 0; col < colCount; col++)
                {
                    string columnName = _headers[col];
                    if (string.Compare(columnName, _config.PredictField, true) == 0)
                    {
                        DataField df = _config.FieldMap[columnName];
                        targetIndex = df.Normalize(result[row], targetIndex, _data[row][col]);
                    }
                }
            }

            return result;
        }

        private object[,] PrepareRegression(double[][] evalOutput)
        {
            var dataForExcel = new object[evalOutput.Length + 1, _headers.Count + evalOutput[0].Length];

            int colCount = 0;
            for (int i = 0; i < _headers.Count; i++)
            {
                dataForExcel[0, colCount++] = _headers[i];
            }

            for (int i = 0; i < evalOutput[0].Length; i++)
            {
                dataForExcel[0, colCount++] = "Output " + i;
            }

            for (int row = 0; row < evalOutput.Length; row++)
            {
                int idx = 0;
                for (int col = 0; col < _headers.Count; col++)
                {
                    dataForExcel[row + 1, idx++] = _data[row][col];
                }
                for (int col = 0; col < evalOutput[row].Length; col++)
                {
                    dataForExcel[row + 1, idx++] = evalOutput[row][col];
                }
            }

            return dataForExcel;
        }

        private object[,] PrepareClassification(double[][] evalOutput)
        {
            var dataForExcel = new object[evalOutput.Length + 1, _headers.Count + 1];

            int colCount = 0;
            for (int i = 0; i < _headers.Count; i++)
            {
                dataForExcel[0, colCount++] = _headers[i];
            }

            var cls = _config.FieldMap[_config.PredictField].Classes;

            dataForExcel[0, colCount] = "Output ";

            for (int row = 0; row < evalOutput.Length; row++)
            {
                int idx = 0;
                for (int col = 0; col < _headers.Count; col++)
                {
                    dataForExcel[row + 1, idx++] = _data[row][col];
                }

                double maxValue = 0;
                int maxIndex = -1;
                for (int col = 0; col < evalOutput[row].Length; col++)
                {
                    if (maxIndex == -1 || evalOutput[row][col]>maxValue )
                    {
                        maxValue = evalOutput[row][col];
                        maxIndex = col;
                    }
                    
                }
                dataForExcel[row + 1, idx] = cls[maxIndex];
            }

            return dataForExcel;
        }

        public void WriteExcel(string filename, double[][] evalOutput)
        {
            bool classification = _config.DetermineRawOutputCount() > 1;

            object[,] dataForExcel;

            if (classification)
            {
                dataForExcel = PrepareClassification(evalOutput);
            }
            else
            {
                dataForExcel = PrepareRegression(evalOutput);
            }

            // creates a workbook, populates it, and saves it
            Application excel = null;
            Workbook wkb = null;
            Worksheet wks = null;
            Range rng = null;

            try
            {
                excel = new Application();
                excel.Visible = true;
                wkb = excel.Workbooks.Add();
                wks = wkb.Worksheets.get_Item("Sheet1");
                Range c1 = wks.Cells[1, 1];
                Range c2 = wks.Cells[evalOutput.Length + 1, dataForExcel.GetUpperBound(1)+1];
                rng = wks.get_Range(c1, c2);
                rng.Value = dataForExcel;
                excel.DisplayAlerts = false;
                wkb.SaveAs(filename, ConflictResolution: false);
                excel.DisplayAlerts = true;
                wkb.Close();
                excel.Quit();
            }
            finally
            {
                rng = null;
                wks = null;
                wkb = null;
                excel = null;
            }
        }
    }
}