using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Diagnostics;

namespace SlavApp.Resembler.Tests
{
    [TestClass]
    public class HammingDistanceTests
    {
        [TestMethod]
        public void GetDistance_ReturnCorectDistance0()
        {
            ulong h1 = 123456789;
            ulong h2 = 123456789;

            Assert.IsTrue(HammingDistance.GetDistance(h1, h2) == 0);
        }

        [TestMethod]
        public void GetDistance_ReturnCorectDistance1()
        {
            var h1 = ulong.MinValue;
            var h2 = ulong.MaxValue;

            Assert.IsTrue(HammingDistance.GetDistance(h1, h2) == 64);
        }

        [TestMethod]
        public void GetDistance_ReturnCorectDistance2()
        {
            ulong h1 = 1;
            ulong h2 = 2;

            var result = HammingDistance.GetDistance(h1, h2);

            Assert.IsTrue(result == 2);
        }

        [TestMethod]
        public void GetDistance_ReturnCorectDistance3()
        {
            ulong h1 = 1;
            ulong h2 = 3;

            var result = HammingDistance.GetDistance(h1, h2);

            Assert.IsTrue(result == 1);
        }
    }
}
