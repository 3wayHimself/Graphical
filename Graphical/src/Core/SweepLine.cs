using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphical.Geometry;
using Graphical.DataStructures;

namespace Graphical.Core
{
    /// <summary>
    /// Helper class to implement Bentley-Ottmann Algorithm for
    /// polygon self-intersections and boolean operations.
    /// </summary>
    public static class SweepLine
    {


        
    }

    internal class SweepVertex : IComparable<SweepVertex>
    {
        public gVertex Vertex;
        public SweepVertex Pair;
        public bool IsStart;

        public int CompareTo(SweepVertex other)
        {
            throw new NotImplementedException();
        }
    }
}
