using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphical.Extensions
{
    /// <summary>
    /// Extensions methods for numerical objects
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Double extension method.
        /// Maps a double value from a given range to a new one.
        /// </summary>
        /// <param name="value">Value to map</param>
        /// <param name="min">Original range minimum value</param>
        /// <param name="max">Original range maximum value</param>
        /// <param name="newMin">Target range minimum value</param>
        /// <param name="newMax">Target range maximum value</param>
        /// <returns name="mapped">Mapped value to target range</returns>
        public static double Map(this double value, double min, double max, double newMin, double newMax)
        {
            double normal = (value - min) / (max - min);
            return (normal * (newMax - newMin)) + newMin;
        }


    }
}
