using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphical.Geometry;
using Graphical.DataStructures;
using Graphical.Extensions;

namespace Graphical.Core
{
    /// <summary>
    /// Helper class to implement Bentley-Ottmann Algorithm for
    /// polygon self-intersections and boolean operations.
    /// </summary>
    public class SweepLine
    {
        #region Internal Properties
        internal MinPriorityQ<SweepEvent> EventsQ;
        internal List<SweepEvent> ActiveEvents;
        #endregion

        #region Public Properties
        public List<gVertex> Intersections = new List<gVertex>();
        #endregion

        #region Internal Constructors
        internal SweepLine (List<gEdge> edges)
        {
            EventsQ = new MinPriorityQ<SweepEvent>(edges.Count * 2);
            ActiveEvents = new List<SweepEvent>(edges.Count * 2);

            foreach(gEdge e in edges)
            {
                SweepEvent swStart = new SweepEvent(e.StartVertex, e);
                SweepEvent swEnd = new SweepEvent(e.EndVertex, e);
                swStart.Pair = swEnd;
                swEnd.Pair = swStart;
                swStart.IsLeft = swStart < swEnd;
                swEnd.IsLeft = !swStart.IsLeft;

                EventsQ.Add(swStart);
                EventsQ.Add(swEnd);
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


        // TODO: Check if BoundingBoxes intersect first to avoid unnecessary computation if they don't
        public void SetIntersections()
        {
            while (EventsQ.Any())
            {
                SweepEvent nextEvent = EventsQ.Take();
                if (!ActiveEvents.Any())
                {
                    ActiveEvents.Add(nextEvent);
                }
                else if (nextEvent.IsLeft)
                {
                    int index = ActiveEvents.BisectIndex(nextEvent);
                    ActiveEvents.Insert(index, nextEvent);
                    // If has event below
                    if(index > 0)
                    {
                        SweepEvent belowEvent = ActiveEvents[index - 1];
                        gBase intersection = nextEvent.Edge.Intersection(belowEvent.Edge);
                        // If intersection exists and is not any of the edges extremes, update events
                        if(intersection != null)
                        {
                            ProcessIntersection(nextEvent, belowEvent, intersection);
                        }
                    }

                    // If has event above
                    if(index + 1 < ActiveEvents.Count)
                    {
                        SweepEvent aboveEvent = ActiveEvents[index + 1];
                        gBase intersection = nextEvent.Edge.Intersection(aboveEvent.Edge);
                        if(intersection != null)
                        {
                            ProcessIntersection(nextEvent, aboveEvent, intersection);
                        }
                    }
                }
                else
                {
                    int pairIndex = ActiveEvents.BisectIndex(nextEvent.Pair) - 1;
                    SweepEvent belowEvent = (pairIndex > 0) ? ActiveEvents[pairIndex - 1] : null;
                    SweepEvent aboveEvent = (pairIndex + 1 < ActiveEvents.Count) ? ActiveEvents[pairIndex + 1] : null;

                    ActiveEvents.RemoveAt(pairIndex);
                    if (belowEvent != null && aboveEvent != null)
                    {
                        gBase intersection = belowEvent.Edge.Intersection(aboveEvent.Edge);
                        if (intersection != null)
                        {
                            ProcessIntersection(belowEvent, aboveEvent, intersection);
                        } 
                    }
                }
            }
        }

        private void ProcessIntersection(SweepEvent swEvent, SweepEvent swOther, gBase intersection)
        {
            if(intersection is gVertex)
            {
                gVertex vtx = intersection as gVertex;
                // Intersection is between extremes vertices
                foreach (SweepEvent sw in new List<SweepEvent>() { swEvent, swOther })
                {
                    if (!sw.Edge.Contains(vtx))
                    {
                        if (!this.Intersections.Contains(vtx)) { this.Intersections.Add(vtx); }

                        var pairEvent = sw.Pair;
                        int index = EventsQ.IndexOf(pairEvent);

                        sw.UpdatePairVertex(vtx);
                        pairEvent.UpdatePairVertex(vtx);

                        //EventsQ.UpdateItem(sw);
                        EventsQ.UpdateAtIndex(index);
                        EventsQ.Add(sw.Pair);
                        EventsQ.Add(pairEvent.Pair);
                    }
                }
            }
            else
            {
                throw new Exception("Coincident edges not implemented just yet");
            }
        }
    }

    public class SweepEvent : IEquatable<SweepEvent>, IComparable<SweepEvent>
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

        public void UpdatePairVertex(gVertex newPairVertex)
        {
            this.Edge = gEdge.ByStartVertexEndVertex(this.Vertex, newPairVertex);
            this.Pair = new SweepEvent(newPairVertex, this.Edge)
            {
                Pair = this,
                IsLeft = !this.IsLeft
            };
        }

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

        public override string ToString()
        {
            return String.Format("(Vertex:{0}, Pair:{1})", this.Vertex.ToString(), this.Pair.Vertex.ToString());
        }
    }
}
