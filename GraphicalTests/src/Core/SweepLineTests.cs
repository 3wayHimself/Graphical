using Graphical.Core;
using Graphical.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Graphical.Core.Tests
{
    [TestFixture]
    public class SweepLineTests
    {
        //[Test]
        public void ByEdgesTest()
        {

        }

        //[Test]
        public void ByPolygonsTest()
        {

        }

        [Test]
        public void FindIntersectionsTest()
        {
            // Similar example as in http://www.webcitation.org/6ahkPQIsN
            gVertex a1 = gVertex.ByCoordinates(1, 3);
            gVertex a2 = gVertex.ByCoordinates(9, 8);
            gVertex b1 = gVertex.ByCoordinates(5, 1);
            gVertex b2 = gVertex.ByCoordinates(9, 4);
            gVertex c1 = gVertex.ByCoordinates(3, 5);
            gVertex c2 = gVertex.ByCoordinates(7, 1);
            gVertex d1 = gVertex.ByCoordinates(3, 9);
            gVertex d2 = gVertex.ByCoordinates(10, 2);

            List<gEdge> edges = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(b1, b2),
                gEdge.ByStartVertexEndVertex(c1, c2),
                gEdge.ByStartVertexEndVertex(d1, d2)
            };

            SweepLine swLine = SweepLine.ByEdges(edges);
            Assert.AreEqual(true, swLine.HasIntersection);
            Assert.AreEqual(4, swLine.Intersections.Count);
        }

        [Test]
        public void FindIntersectionsNotHorizontalTest()
        {
            gVertex a1 = gVertex.ByCoordinates(0, 0, 0);
            gVertex a2 = gVertex.ByCoordinates(10, 10, 10);
            gVertex b1 = gVertex.ByCoordinates(0, 10, 0);
            gVertex b2 = gVertex.ByCoordinates(10, 0, 10);
            //gVertex c1 = gVertex.ByCoordinates(0, 0, 0);
            //gVertex c2 = gVertex.ByCoordinates(0, 10, 10);
            //gVertex d1 = gVertex.ByCoordinates(0, 10, 0);
            //gVertex d2 = gVertex.ByCoordinates(0, 0, 10);

            List<gEdge> edges = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(b1, b2)
                // Error when trying non coplanar lines and intersecting on extremes.
                //gEdge.ByStartVertexEndVertex(c1, c2),
                //gEdge.ByStartVertexEndVertex(d1, d2)
            };

            SweepLine swLine = SweepLine.ByEdges(edges);
            Assert.AreEqual(true, swLine.HasIntersection);
            Assert.AreEqual(1, swLine.Intersections.Count);
            //Assert.AreEqual(2, swLine.Intersections.Count);
        }

        [Test]
        public void IntersectionCoincidentLinesTest()
        {
            gVertex a1 = gVertex.ByCoordinates(0, 0);
            gVertex a2 = gVertex.ByCoordinates(10, 10);
            gVertex b1 = gVertex.ByCoordinates(5, 5);
            gVertex b2 = gVertex.ByCoordinates(15, 15);
            gVertex c1 = gVertex.ByCoordinates(3, 5);
            gVertex c2 = gVertex.ByCoordinates(7, 1);
            gVertex d1 = gVertex.ByCoordinates(3, 9);
            gVertex d2 = gVertex.ByCoordinates(10, 2);

            List<gEdge> sameEdges = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(a2, a1)
            };
            SweepLine slSameEdges = SweepLine.ByEdges(sameEdges);
            Assert.AreEqual(1, slSameEdges.Intersections.Count);
            Assert.AreEqual(1, slSameEdges.coincidentCase[0]);

            List<gEdge> sameStart = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(b1, a1)
            };
            SweepLine slSameStart = SweepLine.ByEdges(sameStart);
            Assert.AreEqual(1, slSameStart.Intersections.Count);
            Assert.AreEqual(2, slSameStart.coincidentCase[0]);

            List<gEdge> sameEnd = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(a2, b1)
            };
            SweepLine slSameEnd = SweepLine.ByEdges(sameEnd);
            Assert.AreEqual(1, slSameEnd.Intersections.Count);
            Assert.AreEqual(new List<int>() { 3, 1 }, slSameEnd.coincidentCase);

            List<gEdge> overlapping = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(b2, b1)
            };
            SweepLine slOverlapping = SweepLine.ByEdges(overlapping);
            Assert.AreEqual(1, slOverlapping.Intersections.Count);
            Assert.AreEqual(new List<int>() { 4, 1 }, slOverlapping.coincidentCase);

            List<gEdge> containing = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, b2),
                gEdge.ByStartVertexEndVertex(a2, b1)
            };
            SweepLine slContaining = SweepLine.ByEdges(containing);
            Assert.AreEqual(1, slContaining.Intersections.Count);
            Assert.AreEqual(new List<int>() { 5, 1 }, slContaining.coincidentCase);





        }
    }
}