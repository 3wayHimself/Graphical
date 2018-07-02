using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphical.Geometry
{
    /// <summary>
    /// gPolygon class to hold graph´s polygon information in relation to its function on the graph
    /// like if it is internal or limit boundary.
    /// </summary>
    public class gPolygon : gBase, ICloneable
    {
        #region Internal Variables

        /// <summary>
        /// Polygon's id
        /// </summary>
        internal int id { get; set; }

        /// <summary>
        /// Flag to check polygons role: Internal or Boundary
        /// </summary>
        internal bool isBoundary { get; set; }

        /// <summary>
        /// Polygon's edges
        /// </summary>
        internal List<gEdge> edges = new List<gEdge>();

        /// <summary>
        /// Polygon's Vertices
        /// </summary>
        internal List<gVertex> vertices = new List<gVertex>();
        #endregion

        #region Public Variables
        /// <summary>
        /// gPolygon's vertices
        /// </summary>
        public List<gVertex> Vertices
        {
            get { return vertices; }
        }

        /// <summary>
        /// gPolygon's edges
        /// </summary>
        public List<gEdge> Edges
        {
            get
            {
                return edges;
            }
        }

        /// <summary>
        /// Determines if the gPolygon is closed.
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return this.edges.Count > 2 && (edges.First().StartVertex.OnEdge(edges.Last()) || edges.First().EndVertex.OnEdge(edges.Last()));
            }
        }
        #endregion

        #region Internal Constructors
        internal gPolygon() { }

        internal gPolygon(int _id, bool _isExternal)
        {
            id = _id;
            isBoundary = _isExternal;
        }
        #endregion

        #region Public Constructos
        /// <summary>
        /// Creates a new gPolygon by a list of ordered vertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="isExternal"></param>
        /// <returns></returns>
        public static gPolygon ByVertices(List<gVertex> vertices, bool isExternal = false)
        {
            gPolygon polygon = new gPolygon(-1, isExternal);
            polygon.vertices = vertices;
            int vertexCount = vertices.Count;
            for (var j = 0; j < vertexCount; j++)
            {
                int next_index = (j + 1) % vertexCount;
                gVertex vertex = vertices[j];
                gVertex next_vertex = vertices[next_index];
                polygon.edges.Add( new gEdge(vertex, next_vertex));
            }
            return polygon;
        }
        #endregion

        #region Internal Methods
        internal void AddVertex(gVertex vertex)
        {
            vertex.polygonId = this.id;
            vertices.Add(vertex);
        }

        internal gPolygon AddVertex(gVertex v, gEdge intersectingEdge)
        {
            //Assumes that vertex v intersects one of polygons edges.
            gPolygon newPolygon = (gPolygon)this.Clone();

            // Assign the polygon Id to the new vertex.
            v.polygonId = this.id;

            // Getting the index of the intersecting edge's start vertex and
            // inserting the new vertex at the following index.
            int index = newPolygon.vertices.IndexOf(intersectingEdge.StartVertex);
            newPolygon.vertices.Insert(index + 1, v);

            // Rebuilding edges.
            newPolygon.edges.Clear();
            int verticesCount = newPolygon.vertices.Count;
            for (var i = 0; i < verticesCount; i++)
            {
                int nextIndex = (i + 1) % verticesCount;
                newPolygon.edges.Add(new gEdge(newPolygon.vertices[i], newPolygon.vertices[nextIndex]));
            }

            return newPolygon;
        }

        
        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if a gVertex is inside the gPolygon using Fast Winding Number method
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool ContainsVertex(gVertex vertex)
        {
            gVertex maxVertex = vertices.OrderByDescending(v => v.DistanceTo(vertex)).First();
            double maxDistance = vertex.DistanceTo(maxVertex) * 1.5;
            gVertex v2 = gVertex.ByCoordinates(vertex.X + maxDistance, vertex.Y, vertex.Z);
            gEdge ray = gEdge.ByStartVertexEndVertex(vertex, v2);
            int windNumber = 0;
            foreach (gEdge edge in edges)
            {
                gBase intersection = ray.Intersection(edge);
                if (intersection  is gVertex)
                {
                    gVertex vtx = (gVertex)intersection;
                    if (edge.StartVertex.Y <= vertex.Y)
                    {
                        // If interection is a vertex from the ray edge, don't count it
                        if (edge.EndVertex.Y > vertex.Y && !ray.Contains(vtx))
                        {
                            ++windNumber;
                        }
                    }
                    else
                    {
                        if (edge.EndVertex.Y <= vertex.Y)
                        {
                            --windNumber;
                        }
                    }
                }
            }

            // If windNumber is different from 0, vertex is in polygon
            return windNumber != 0;
        }

        /// <summary>
        /// Determines if a gEdge is inside the gPolygon by comparing
        /// it's start, end and mid vertices.
        /// Note: Prone to error if polygon has edges intersecting the edge not at mid vertex?
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool ContainsEdge(gEdge edge)
        {
            // TODO: Check if edge intersects polygon in vertices different than start/end.
            return this.ContainsVertex(edge.StartVertex)
                && this.ContainsVertex(edge.EndVertex)
                && this.ContainsVertex(gVertex.MidVertex(edge.StartVertex, edge.EndVertex));
        }
        /// <summary>
        /// Checks if a polygon is planar
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <returns>boolean</returns>
        public static bool IsPlanar(gPolygon polygon)
        {
            return gVertex.Coplanar(polygon.Vertices);
        }

        /// <summary>
        /// Checks if two gPolygons are coplanar.
        /// </summary>
        /// <param name="polygon">gPolygon</param>
        /// <param name="otherPolygon">Other gPolygon</param>
        /// <returns></returns>
        public static bool Coplanar(gPolygon polygon, gPolygon otherPolygon)
        {
            List<gVertex> joinedVertices = new List<gVertex>(polygon.Vertices);
            joinedVertices.AddRange(otherPolygon.Vertices);

            return gVertex.Coplanar(joinedVertices);
        }
        #endregion

        /// <summary>
        /// Clone method for gPolygon
        /// </summary>
        /// <returns>Cloned gPolygon</returns>
        public object Clone()
        {
            gPolygon newPolygon = new gPolygon(this.id, this.isBoundary);
            newPolygon.edges = new List<gEdge>(this.edges);
            newPolygon.vertices = new List<gVertex>(this.vertices);
            return newPolygon;
        }

        internal override gBoundingBox ComputeBoundingBox()
        {
            var xCoord = new List<double>(this.vertices.Count);
            var yCoord = new List<double>(this.vertices.Count);
            var zCoord = new List<double>(this.vertices.Count);
            foreach(gVertex v in vertices)
            {
                xCoord.Add(v.X);
                yCoord.Add(v.Y);
                zCoord.Add(v.Z);
            }
            return new gBoundingBox(
                new double[3] { xCoord.Min(), yCoord.Min(), zCoord.Min() },
                new double[3] { xCoord.Max(), yCoord.Max(), zCoord.Max() }
                );
        }
    }
}
