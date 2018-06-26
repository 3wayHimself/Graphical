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
    public class SweepLine
    {
        

        
    }

    internal class SweepVertex : IComparable<SweepVertex>
    {
        public gVertex Vertex;
        public SweepVertex Pair;
        public gEdge Edge;
        public bool IsStart;

        public int CompareTo(SweepVertex other)
        {
            if(other == null) { return -1; }
            if (this.Vertex.Equals(other.Vertex)) { return 0; }
            // If same X
            if(gBase.Threshold(this.Vertex.X, other.Vertex.X))
            {
                // If same Y
                if(gBase.Threshold(this.Vertex.Y, other.Vertex.Y))
                {
                    return 0;
                }
                else
                {
                    return (this.Vertex.Y < other.Vertex.Y) ? -1 : 1;
                }
            }else
            {
                return (this.Vertex.X < other.Vertex.X) ? -1 : 1;
            }
        }

        public static bool operator <(SweepVertex sw1, SweepVertex sw2)
        {
            return sw1.CompareTo(sw2) == -1;
        }

        public static bool operator >(SweepVertex sw1, SweepVertex sw2)
        {
            return sw1.CompareTo(sw2) == 1;
        }

        public static bool operator <=(SweepVertex sw1, SweepVertex sw2)
        {
            return sw1.CompareTo(sw2) <= 0;
        }

        public static bool operator >=(SweepVertex sw1, SweepVertex sw2)
        {
            return sw1.CompareTo(sw2) >= 0;
        }
    }
}
