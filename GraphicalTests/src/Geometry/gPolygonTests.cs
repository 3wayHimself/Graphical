using Graphical.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Graphical.Geometry.Tests
{
    [TestFixture]
    public class gPolygonTests
    {
        [Test]
        public void IsPlanarTest()
        {
            var a = gVertex.ByCoordinates(0, 0, 0);
            var b = gVertex.ByCoordinates(0, 10, 0);
            var c = gVertex.ByCoordinates(10, 10, 0);
            var d = gVertex.ByCoordinates(15, 25, 0);
            var e = gVertex.ByCoordinates(0, 50, 3);
            var f = gVertex.ByCoordinates(15, 15, 0.5);
            var g = gVertex.ByCoordinates(10, 0, 0);
            var h = gVertex.ByCoordinates(5, 0, 10);

            gPolygon pol1 = gPolygon.ByVertices(new List<gVertex>() { a, b, c, d });
            gPolygon pol2 = gPolygon.ByVertices(new List<gVertex>() { a, e, f });
            gPolygon triangleXZPlane = gPolygon.ByVertices(new List<gVertex>() { a, g, h });

            Assert.IsTrue(gPolygon.IsPlanar(pol1));
            Assert.IsTrue(gPolygon.IsPlanar(pol2));
            Assert.IsTrue(gPolygon.IsPlanar(triangleXZPlane));


        }
    }
}