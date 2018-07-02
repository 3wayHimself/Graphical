using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphical.Extensions;

namespace Graphical.Geometry
{
    /// <summary>
    /// Axis Aligned Bounding Box
    /// </summary>
    public class gBoundingBox
    {
        #region Private Properties
        private double[] min = new double[3];
        private double[] max = new double[3];
        #endregion

        #region Public Properties
        /// <summary>
        /// Bounding Box's minimum vertex
        /// </summary>
        public gVertex MinVertex
        {
            get { return gVertex.ByCoordinatesArray(min); }
        }

        /// <summary>
        /// Bounding Box's maximum vertex
        /// </summary>
        public gVertex MaxVertex
        {
            get { return gVertex.ByCoordinatesArray(max); }
        }
        #endregion

        #region Private Constructor
        internal gBoundingBox(double[] min, double[] max)
        {
            if(min.Count() != 3 || max.Count() != 3) { throw new ArgumentException("Arrays must contain 3 coordinates each"); }
            this.min = min;
            this.max = max;
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// Creates a new Bounding Box from a minimum and maximum vertices
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static gBoundingBox ByMinVertexMaxVertex(gVertex min, gVertex max)
        {
            return new gBoundingBox(new double[3] { min.X, min.Y, min.Z }, new double[3] { max.X, max.Y, max.Z });
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines if two Axis Aligned Bounding Boxes intersect
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(gBoundingBox other)
        {
            return
                (min[0] <= other.max[0]) && (max[0] >= other.min[0]) &&
                (min[1] <= other.max[1]) && (max[1] >= other.min[1]) &&
                (min[2] <= other.max[2]) && (max[2] >= other.min[2]);
        } 
        #endregion

    }
}
