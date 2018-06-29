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
    public enum SweepLineType
    {
        Intersects,
        Boolean
    }

    public enum BooleanType
    {
        Intersection,
        Union,
        Differenece
    }

    public enum PolygonType
    {
        Subject,
        Clip
    }

    public enum SweepEventLabel
    {
        NoContributing,
        SameTransition,
        DifferentTransition
    }

    /// <summary>
    /// Helper class to implement Bentley-Ottmann Algorithm for
    /// polygon self-intersections and boolean operations.
    /// </summary>
    public class SweepLine
    {
#if DEBUG
        public List<int> coincidentCase = new List<int>();
#endif

        #region Internal Properties
        internal SweepLineType type;
        internal MinPriorityQ<SweepEvent> eventsQ;
        internal List<SweepEvent> activeEvents;
        internal List<gBase> intersections = null;
        internal IComparer<SweepEvent> verticalAscEventsComparer = new SortEventsVerticalAscendingComparer();
        internal gPolygon subject;
        internal gPolygon clip;
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
        private SweepEvent BelowEvent(int index) { return (activeEvents.Any() && index > 0) ? activeEvents[index - 1] : null; }

        /// <summary>
        /// Returns the event above SweepEvent on index, null if none
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private SweepEvent AboveEvent(int index) { return (activeEvents.Any() && index + 1 < activeEvents.Count) ? activeEvents[index + 1] : null; }
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

        internal SweepLine(gPolygon subject, gPolygon clip)
        {
            this.type = SweepLineType.Boolean;
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
            return new SweepLine(edges, SweepLineType.Intersects);
        }

        /// <summary>
        /// SweepLine constructor by a list of gPolygons
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns>SweepLine</returns>
        public static SweepLine ByPolygons(List<gPolygon> polygons)
        {
            return new SweepLine(
                polygons.SelectMany(p => p.Edges).ToList(),
                SweepLineType.Intersects);
        }


        #endregion

        #region Public Methods

        public static List<gPolygon> Union(gPolygon main, gPolygon clip)
        {
            List<gEdge> edges = new List<gEdge>(main.Edges);
            edges.AddRange(clip.Edges);
            var swLine = new SweepLine(edges, SweepLineType.Boolean)
            {
                subject = main,
                clip = clip
            };

            List<SweepEvent> events = swLine.ComputeBooleanOperation();

            return null;
        }

        #endregion

        #region Internal Methods

        // TODO: Check if BoundingBoxes intersect first to avoid unnecessary computation if they don't
        // TODO: Check no coplanar edges.
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

                    if (belowEvent != null) { ProcessIntersection(nextEvent, belowEvent, tempIntersections); }
                    if (aboveEvent != null) { ProcessIntersection(nextEvent, aboveEvent, tempIntersections); }
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

        internal List<SweepEvent> ComputeBooleanOperation()
        {
            List<SweepEvent> computedEvents = new List<SweepEvent>();
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

                    if (belowEvent != null) { ProcessIntersection(nextEvent, belowEvent); }
                    if (aboveEvent != null) { ProcessIntersection(nextEvent, aboveEvent); }
                }
                else
                {
                    int pairIndex = activeEvents.BisectIndex(nextEvent.Pair, verticalAscEventsComparer) - 1;
                    SweepEvent belowEvent = BelowEvent(pairIndex);
                    SweepEvent aboveEvent = AboveEvent(pairIndex);

                    activeEvents.RemoveAt(pairIndex);
                    if (belowEvent != null && aboveEvent != null) { ProcessIntersection(belowEvent, aboveEvent); }
                }
            }

            return computedEvents;
        }

        internal void UpdateEventPair(SweepEvent swEvent, gVertex newVertexPair)
        {
            var pairEvent = swEvent.Pair;
            int index = eventsQ.IndexOf(pairEvent);

            // Update event and its pair with the new pair vertex 
            swEvent.UpdatePairVertex(newVertexPair);
            pairEvent.UpdatePairVertex(newVertexPair);

            // Update position of pairEvent in PriorityQ according to its new pair.
            eventsQ.UpdateAtIndex(index);

            // Add both new pairs to the PriorityQ
            eventsQ.Add(swEvent.Pair);
            eventsQ.Add(pairEvent.Pair);
        }

        internal void ProcessIntersection(SweepEvent next, SweepEvent prev, List<gBase> intersections = null)
        {
            gBase intersection = next.Edge.Intersection(prev.Edge);
            bool inserted = false;
            #region is gVertex
            if (intersection is gVertex)
            {
                gVertex v = intersection as gVertex;
                // Intersection is between extremes vertices
                foreach (SweepEvent sw in new List<SweepEvent>() { next, prev })
                {
                    if (!sw.Edge.Contains(v))
                    {
                        if (intersections != null && !inserted)
                        {
                            intersections.Add(v);
                            inserted = true;
                        }
                        UpdateEventPair(sw, v);
                    }
                }
            }
            #endregion
            #region Is gEdge
            else if (intersection is gEdge)
            {
                gEdge e = intersection as gEdge;

                // On Case 3 below, last half of prev event is added as intersection,
                // and on next loop it will be case 1 with the same edge, so this avoids duplicates
                if (intersections != null && (!intersections.Any() || !intersections.Last().Equals(e)) )
                {
                    intersections.Add(e);
                    inserted = true;
                }

                // Case 1: events are coincident (same edge)
                // (prev)--------------------(prevPair)
                // (next)--------------------(nextPair)
                if (next.Equals(prev))
                {
#if DEBUG
                    this.coincidentCase.Add(1);
#endif
                    // Setting nextEvent as not contributing instead of deleting it
                    // as doing so will make it's pair a lonely poor thing.
                    next.Label = SweepEventLabel.NoContributing;
                }
                // Case 2: same start point, prev will be always shorter
                // as on PriorityQ it must have been sorted before next
                // (prev)----------(prevPair)
                // (next)--------------------(nextPair)
                else if (prev.Vertex.Equals(next.Vertex))
                {
#if DEBUG
                    this.coincidentCase.Add(2);
#endif
                    // TODO: check this is true in all cases
                    gVertex dividingVtx = prev.Pair.Vertex;
                    UpdateEventPair(next, dividingVtx);
                }
                // Case 3: same end point, next will be always shorter
                // as on PriorityQ it must have been sorted after next
                // (prev)--------------------(prevPair)
                //        (next)-------------(nextPair)
                else if (prev.Pair.Vertex.Equals(next.Pair.Vertex))
                {
#if DEBUG
                    this.coincidentCase.Add(3);
#endif
                    // TODO: check this is true in all cases
                    gVertex dividingVtx = next.Vertex;
                    UpdateEventPair(prev, dividingVtx);
                }
                // Case 4: events overlap
                // (prev)--------------------(prevPair)
                //        (next)--------------------(nextPair)
                else if (prev < next && prev.Pair < next.Pair)
                {
#if DEBUG
                    this.coincidentCase.Add(4);
#endif
                    // TODO: check this is true in all cases
                    gVertex prevDividingVtx = next.Vertex;
                    gVertex nextDividingVtx = prev.Pair.Vertex;

                    UpdateEventPair(prev, prevDividingVtx);
                    UpdateEventPair(next, nextDividingVtx);
                }
                // Case 5: prev fully contains next
                // (prev)--------------------(prevPair)
                //        (next)---(nextPair)
                else if (prev < next && prev.Pair > next.Pair)
                {
#if DEBUG
                    this.coincidentCase.Add(5);
#endif
                    next.Label = SweepEventLabel.NoContributing;
                    gVertex dividingVtx = next.Vertex;
                    gVertex pairDividingVtx = next.Pair.Vertex;

                    // Storing reference to prevPair before updating it
                    var prevPair = prev.Pair;

                    UpdateEventPair(prev, dividingVtx);
                    UpdateEventPair(prevPair, pairDividingVtx);
                }
                else
                {
#if DEBUG
                    this.coincidentCase.Add(-1);
#endif
                    throw new Exception("Case not contemplated? Damm!");
                }
            }
            #endregion 
            #endregion

        }

    }

    /// <summary>
    /// Class to hold information about Vertex and Edges on 
    /// the SweepLine algorithm. SweepEvents are compared by X, then Y coordinates
    /// of the vertex. If same vertex, Pairs are compared insted.
    /// </summary>
    public class SweepEvent : IEquatable<SweepEvent>, IComparable<SweepEvent>
    {
        internal PolygonType polygonType;

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

        /// <summary>
        /// Label to define how a SweepEvent contributes to a polygon boolean operation.
        /// </summary>
        public SweepEventLabel Label;
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
