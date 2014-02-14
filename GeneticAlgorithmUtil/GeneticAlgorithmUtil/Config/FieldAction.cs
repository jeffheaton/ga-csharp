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

namespace GeneticAlgorithmUtil.Config
{
    /// <summary>
    /// The action to perform on a field.
    /// </summary>
    [Serializable]
    public enum FieldAction
    {
        /// <summary>
        /// Adjust numeric values to be in a specific range.
        /// </summary>
        Normalize,
        /// <summary>
        /// Break into a number of discrete classes.
        /// </summary>
        Class,
        /// <summary>
        /// Pass the field value directly on.
        /// </summary>
        Direct,
        /// <summary>
        /// Ignore this field.
        /// </summary>
        Ignore
    }
}