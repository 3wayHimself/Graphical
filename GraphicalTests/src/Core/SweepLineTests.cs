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
        public void SetIntersectionsTest()
        {
            gVertex a1 = gVertex.ByCoordinates(0, 0);
            gVertex a2 = gVertex.ByCoordinates(10, 10);
            gVertex b1 = gVertex.ByCoordinates(0, 10);
            gVertex b2 = gVertex.ByCoordinates(10, 0);

            List<gEdge> edges = new List<gEdge>()
            {
                gEdge.ByStartVertexEndVertex(a1, a2),
                gEdge.ByStartVertexEndVertex(b1,b2)
            };

            SweepLine swLine = SweepLine.ByEdges(edges);
            swLine.SetIntersections();

            Assert.AreEqual(1, swLine.Intersections.Count);
        }
    }
}