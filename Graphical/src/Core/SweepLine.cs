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
        #region Internal Properties
        internal MinPriorityQ<SweepEvent> EventsHeap = new MinPriorityQ<SweepEvent>();
        internal List<SweepEvent> ActiveEvents = new List<SweepEvent>();
        #endregion

        #region Public Properties
        public List<gVertex> Intersections = new List<gVertex>();
        #endregion

        #region Internal Constructors
        internal SweepLine (List<gEdge> edges)
        {
            foreach(gEdge e in edges)
            {
                SweepEvent swStart = new SweepEvent(e.StartVertex, e);
                SweepEvent swEnd = new SweepEvent(e.EndVertex, e);
                swStart.Pair = swEnd;
                swEnd.Pair = swStart;
                swStart.IsLeft = swStart < swEnd;
                swEnd.IsLeft = !swStart.IsLeft;

                EventsHeap.Add(swStart);
                EventsHeap.Add(swEnd);
            }
        }
        #endregion

        #region Public Constructors
        /// <summary>
        /// SweepLine constructor by a list of gEdges
        /// </summary>
        /// <param name="edges"></param>
        /// <returns>SweepLine</returns>
        public static SweepLine ByEdges(List<gEdge> edges)
        {
            return new SweepLine(edges);
        }

        /// <summary>
        /// SweepLine constructor by a list of gPolygons
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns>SweepLine</returns>
        public static SweepLine ByPolygons(List<gPolygon> polygons)
        {
            List<gEdge> edges = new List<gEdge>();
            polygons.ForEach(p => edges.AddRange(p.Edges));

            return new SweepLine(edges);
        } 
        #endregion

        public void SetIntersections()
        {
            while (EventsHeap.Any())
            {
                SweepEvent nextEvent = EventsHeap.Take();
                if (nextEvent.IsLeft)
                {

                }
            }
        }

    }

    internal class SweepEvent : IEquatable<SweepEvent>, IComparable<SweepEvent>
    {
        #region Public Properties
        public gVertex Vertex { get; set; }
        public SweepEvent Pair { get; set; }
        public gEdge Edge { get; set; }
        public bool IsLeft { get; set; }
        #endregion

        #region Constructor
        public SweepEvent(gVertex vertex, gEdge edge)
        {
            this.Vertex = vertex;
            this.Edge = edge;
        }
        #endregion

        public int CompareTo(SweepEvent other)
        {
            if(other == null) { return -1; }
            // If same Vertex, compare pairs
            if (this.Vertex.Equals(other.Vertex))
            {
                if (this.Pair.Vertex.Equals(other.Pair.Vertex))
                {
                    return 0;
                }
                else
                {
                    return this.Pair.CompareTo(other.Pair);
                }
            }
            // If same X
            else if(gBase.Threshold(this.Vertex.X, other.Vertex.X))
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

        public bool Equals(SweepEvent other)
        {
            return this.Edge.Equals(other.Edge);
        }

        public static bool operator <(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == -1;
        }

        public static bool operator >(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == 1;
        }

        public static bool operator <=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) <= 0;
        }

        public static bool operator >=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) >= 0;
        }
    }
}
