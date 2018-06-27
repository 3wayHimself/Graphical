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
    }
}