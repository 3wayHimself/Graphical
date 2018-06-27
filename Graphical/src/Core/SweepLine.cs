﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphical.Geometry;
using Graphical.DataStructures;
using Graphical.Extensions;

namespace Graphical.Core
{
    public enum SweepLineType
    {
        Linear,
        Polygonal
    }

    /// <summary>
    /// Helper class to implement Bentley-Ottmann Algorithm for
    /// polygon self-intersections and boolean operations.
    /// </summary>
    public class SweepLine
    {
        #region Internal Properties
        internal SweepLineType type;
        internal MinPriorityQ<SweepEvent> eventsQ;
        internal List<SweepEvent> activeEvents;
        internal List<gBase> intersections = null;
        internal IComparer<SweepEvent> verticalAscEventsComparer = new SortEventsVerticalAscendingComparer();
        #endregion

        #region Public Properties
        /// <summary>
        /// Intersections found on SweepLine method.
        /// </summary>
        public List<gBase> Intersections
        {
            get
            {
                if(intersections == null ) { intersections = FindIntersections(); }
                return intersections;
            }
        }

        /// <summary>
        /// Determines if edges intersects
        /// </summary>
        public bool HasIntersection
        {
            get
            {
                return Intersections.Any();
            }
        }

        /// <summary>
        /// Returns the event below SweepEvent on index, null if none
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SweepEvent BelowEvent(int index) { return (activeEvents.Any() && index > 0) ? activeEvents[index - 1] : null; }

        /// <summary>
        /// Returns the event above SweepEvent on index, null if none
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public SweepEvent AboveEvent(int index) { return (activeEvents.Any() && index + 1 < activeEvents.Count) ? activeEvents[index + 1] : null; }
        #endregion

        #region Internal Constructors
        internal SweepLine (List<gEdge> edges, SweepLineType type)
        {
            this.type = type;
            this.eventsQ = new MinPriorityQ<SweepEvent>(edges.Count * 2);
            this.activeEvents = new List<SweepEvent>(edges.Count);

            foreach(gEdge e in edges)
            {
                SweepEvent swStart = new SweepEvent(e.StartVertex, e);
                SweepEvent swEnd = new SweepEvent(e.EndVertex, e);
                swStart.Pair = swEnd;
                swEnd.Pair = swStart;
                swStart.IsLeft = swStart < swEnd;
                swEnd.IsLeft = !swStart.IsLeft;

                eventsQ.Add(swStart);
                eventsQ.Add(swEnd);
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
            return new SweepLine(edges, SweepLineType.Linear);
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

            return new SweepLine(edges, SweepLineType.Polygonal);
        } 
        #endregion

        // TODO: Check if BoundingBoxes intersect first to avoid unnecessary computation if they don't
        internal List<gBase> FindIntersections()
        {
            List<gBase> tempIntersections = new List<gBase>();

            while (eventsQ.Any())
            {
                SweepEvent nextEvent = eventsQ.Take();
                if (!activeEvents.Any())
                {
                    activeEvents.Add(nextEvent);
                }
                else if (nextEvent.IsLeft)
                {
                    int index = activeEvents.BisectIndex(nextEvent, verticalAscEventsComparer);
                    activeEvents.Insert(index, nextEvent);
                    SweepEvent belowEvent = BelowEvent(index);
                    SweepEvent aboveEvent = AboveEvent(index);

                    if (belowEvent != null) { ProcessIntersection(nextEvent, belowEvent, tempIntersections);}
                    if (aboveEvent != null) { ProcessIntersection(nextEvent, aboveEvent, tempIntersections);}
                }
                else
                {
                    int pairIndex = activeEvents.BisectIndex(nextEvent.Pair, verticalAscEventsComparer) - 1;
                    SweepEvent belowEvent = BelowEvent(pairIndex);
                    SweepEvent aboveEvent = AboveEvent(pairIndex);

                    activeEvents.RemoveAt(pairIndex);
                    if (belowEvent != null && aboveEvent != null) { ProcessIntersection(belowEvent, aboveEvent, tempIntersections); }
                }
            }

            return tempIntersections;
        }
        
        internal void ProcessIntersection(SweepEvent swEvent, SweepEvent swOther, List<gBase> intersections)
        {
            gBase intersection = swEvent.Edge.Intersection(swOther.Edge);
            if(intersection == null) { return; }
            if(intersection is gVertex)
            {
                gVertex vtx = intersection as gVertex;
                // Intersection is between extremes vertices
                foreach (SweepEvent sw in new List<SweepEvent>() { swEvent, swOther })
                {
                    if (!sw.Edge.Contains(vtx))
                    {
                        if (!intersections.Contains(vtx)) { intersections.Add(vtx); }

                        var pairEvent = sw.Pair;
                        int index = eventsQ.IndexOf(pairEvent);

                        sw.UpdatePairVertex(vtx);
                        pairEvent.UpdatePairVertex(vtx);

                        //eventsQ.UpdateItem(sw);
                        eventsQ.UpdateAtIndex(index);
                        eventsQ.Add(sw.Pair);
                        eventsQ.Add(pairEvent.Pair);
                    }
                }
            }
            else
            {
                throw new Exception("Coincident edges not implemented just yet");
            }
        }

    }

    /// <summary>
    /// Class to hold information about Vertex and Edges on 
    /// the SweepLine algorithm. SweepEvents are compared by X, then Y coordinates
    /// of the vertex. If same vertex, Pairs are compared insted.
    /// </summary>
    public class SweepEvent : IEquatable<SweepEvent>, IComparable<SweepEvent>
    {
        #region Public Properties
        /// <summary>
        /// Vertex associated with the event
        /// </summary>
        public gVertex Vertex { get; set; }

        /// <summary>
        /// SweepEvent pair
        /// </summary>
        public SweepEvent Pair { get; set; }

        /// <summary>
        /// Edge associated with the event
        /// </summary>
        public gEdge Edge { get; set; }

        /// <summary>
        /// Determines if SweepEvent comes first on a left to right direction
        /// </summary>
        public bool IsLeft { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// SweepEvent default constructor
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="edge"></param>
        public SweepEvent(gVertex vertex, gEdge edge)
        {
            this.Vertex = vertex;
            this.Edge = edge;
        }
        #endregion

        /// <summary>
        /// Updates the edge and Pair event with a new gVertex
        /// </summary>
        /// <param name="newPairVertex"></param>
        public void UpdatePairVertex(gVertex newPairVertex)
        {
            this.Edge = gEdge.ByStartVertexEndVertex(this.Vertex, newPairVertex);
            this.Pair = new SweepEvent(newPairVertex, this.Edge)
            {
                Pair = this,
                IsLeft = !this.IsLeft
            };
        }

        /// <summary>
        /// SweepEvent comparer.
        /// A SweepEvent is considered less than other if having smaller X, then Y and then Z.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
                    return this.Vertex.Z.CompareTo(other.Vertex.Z);
                }
                else
                {
                    return this.Vertex.Y.CompareTo(other.Vertex.Y);
                }
            }else
            {
                return this.Vertex.X.CompareTo(other.Vertex.X);
            }
        }

        /// <summary>
        /// SweepEvent equality comparer. SweepEvents are considered equals if have the same edge.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SweepEvent other)
        {
            return this.Edge.Equals(other.Edge);
        }

        /// <summary>
        /// Less Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator <(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == -1;
        }

        /// <summary>
        /// Greater Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator >(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) == 1;
        }

        /// <summary>
        /// Less or Equal Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator <=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) <= 0;
        }

        /// <summary>
        /// Greater or Equal Than operator
        /// </summary>
        /// <param name="sw1"></param>
        /// <param name="sw2"></param>
        /// <returns></returns>
        public static bool operator >=(SweepEvent sw1, SweepEvent sw2)
        {
            return sw1.CompareTo(sw2) >= 0;
        }

        /// <summary>
        /// SweepEvent string override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("(Vertex:{0}, Pair:{1})", this.Vertex.ToString(), this.Pair.Vertex.ToString());
        }
    }
    
    /// <summary>
    /// Custom Vertical Ascending IComparer for SweepEvent.
    /// Lower SweepEvent has lowest X. At same X, lowest Y and finally lowest Z.
    /// </summary>
    public class SortEventsVerticalAscendingComparer : IComparer<SweepEvent>
    {
        /// <summary>
        /// Custom SweepEvent Vertical Ascending Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(SweepEvent x, SweepEvent y)
        {
            if (x.Vertex.Equals(y.Vertex))
            {
                if (x.Pair.Vertex.Equals(y.Pair.Vertex))
                {
                    return 0;
                }
                else
                {
                    return Compare(x.Pair, y.Pair);
                }
            }
            // If same Y, below is the one with lower X
            else if (gBase.Threshold(x.Vertex.Y, y.Vertex.Y))
            {
                // If same X, below is the one with lower Z
                if (gBase.Threshold(x.Vertex.X, y.Vertex.X))
                {
                    return x.Vertex.Z.CompareTo(y.Vertex.Z);
                }
                else
                {
                    return x.Vertex.X.CompareTo(y.Vertex.X);
                }
            }
            else
            {
                return x.Vertex.Y.CompareTo(y.Vertex.Y);
            }
        }
    }
}
