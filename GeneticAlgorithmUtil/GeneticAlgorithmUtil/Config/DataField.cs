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
using System.Text;

namespace GeneticAlgorithmUtil.Config
{
    /// <summary>
    /// Holds information about one field.  These fields will be read in from Excel files.  
    /// They can be either input or output.  Input fields are used to predict, and they 
    /// predict the output fields.
    /// 
    /// This class can also normalize the fields.  For example catagorical data can be 
    /// converted to one-of-n encoding.
    /// http://www.heatonresearch.com/wiki/One_of_n
    /// 
    /// Additionally continuous data can be encoded using range normalization.
    /// http://www.heatonresearch.com/wiki/Range_Normalization
    /// 
    /// </summary>
    [Serializable]
    public class DataField
    {
        /// <summary>
        /// If this is a catagorical field, then this collection holds the individual classes.
        /// </summary>
        private readonly IList<string> _classes = new List<string>();

        /// <summary>
        /// Construct the field.
        /// </summary>
        public DataField()
        {
            Reset();
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The action to perfom on this field.
        /// </summary>
        public FieldAction Action { get; set; }

        /// <summary>
        /// If this is a catagorical field, then this collection holds the individual classes.
        /// </summary>
        public IList<string> Classes
        {
            get { return _classes; }
        }

        /// <summary>
        /// The high-value for the actual data.
        /// </summary>
        public double ActualHigh { get; set; }

        /// <summary>
        /// The low-value for the actual data.
        /// </summary>
        public double ActualLow { get; set; }

        /// <summary>
        /// The low-range to normalize into.
        /// </summary>
        public double NormalizedLow { get; set; }

        /// <summary>
        /// The high-range to normalize to.
        /// </summary>
        public double NormalizedHigh { get; set; }

        /// <summary>
        /// Reset the actual range for a new analyze.
        /// </summary>
        public void Reset()
        {
            ActualHigh = double.NegativeInfinity;
            ActualLow = double.PositiveInfinity;
        }

        /// <summary>
        /// Analyze a value for this field.  Adjust the min/max for a numeric, 
        /// or add to the classes for categorical.
        /// </summary>
        /// <param name="d">The input string.</param>
        public void Analyze(string d)
        {
            if (Action == FieldAction.Class)
            {
                if (!_classes.Contains(d))
                {
                    _classes.Add(d);
                }
            }
            else if (Action == FieldAction.Normalize)
            {
                if (!d.Equals("?"))
                {
                    double dd = double.Parse(d, CultureInfo.InvariantCulture);
                    ActualLow = Math.Min(ActualLow, dd);
                    ActualHigh = Math.Max(ActualHigh, dd);
                }
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("[DataField:");
            result.Append("name=");
            result.Append(Name);
            result.Append(";action=");
            result.Append(Action);
            result.Append(";min=");
            result.Append(ActualLow);
            result.Append(";max=");
            result.Append(ActualHigh);
            result.Append("]");
            return result.ToString();
        }

        /// <summary>
        /// Determine the number of raw fields that this one field takes.
        /// </summary>
        /// <returns>The number of fields needed.</returns>
        public int DetermineRawFieldCount()
        {
            switch (Action)
            {
                case FieldAction.Class:
                    return _classes.Count;
                case FieldAction.Direct:
                    return 1;
                case FieldAction.Ignore:
                    return 0;
                case FieldAction.Normalize:
                    return 1;
                default:
                    return 0;
            }
        }


        /// <summary>
        /// Normalize the data into the raw form that will be sent to the model.
        /// </summary>
        /// <param name="target">The array to normalize into</param>
        /// <param name="targetIndex">The target index</param>
        /// <param name="source">The source data</param>
        /// <returns>The new target index, advanced for this field</returns>
        public int Normalize(double[] target, int targetIndex, string source)
        {
            switch (Action)
            {
                case FieldAction.Class:
                    for (int i = 0; i < _classes.Count; i++)
                    {
                        if (_classes[i].Equals(source))
                        {
                            target[targetIndex + i] = NormalizedHigh;
                        }
                        else
                        {
                            target[targetIndex + i] = NormalizedLow;
                        }
                    }
                    return targetIndex + _classes.Count;
                case FieldAction.Direct:
                    target[targetIndex] = double.Parse(source, CultureInfo.InvariantCulture);
                    return targetIndex + 1;
                case FieldAction.Ignore:
                    return targetIndex;
                case FieldAction.Normalize:
                    if (source.Equals("?"))
                    {
                        target[targetIndex] = NormalizedLow + ((NormalizedHigh - NormalizedLow)/2.0);
                    }
                    else
                    {
                        target[targetIndex] = ((double.Parse(source, CultureInfo.InvariantCulture) - ActualLow)/(ActualHigh - ActualLow))
                                              *(NormalizedHigh - NormalizedLow) + NormalizedLow;
                    }
                    return targetIndex + 1;
                default:
                    return targetIndex;
            }
        }
    }
}