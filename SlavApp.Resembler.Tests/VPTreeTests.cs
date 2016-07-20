using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Diagnostics;

namespace SlavApp.Resembler.Tests
{
    [TestClass]
    public class VPTreeTests
    {
        [TestMethod]
        public void SearchVPTree_ReturnsSameHits()
        {
            var tree = VPTree.Build(new ulong[] { 0, 1, 2, 3, 4, 5, 6, 565656345, 1, 1, ulong.MaxValue }, (a, b) => GetHammingDistance(ConvertToBinaryString(a), ConvertToBinaryString(b)));
            var result = tree.SearchVPTree(1, 10, 1).OrderBy(x => x.i).ToArray();

            Assert.IsTrue(result.Length == 3);
        }

        [TestMethod]
        public void SearchVPTree_ReturnsNoHits()
        {
            var tree = VPTree.Build(new ulong[] { 0, ulong.MaxValue }, (a, b) => GetHammingDistance(ConvertToBinaryString(a), ConvertToBinaryString(b)));
            var result = tree.SearchVPTree(1, 10, 1).OrderBy(x => x.i).ToArray();

            Assert.IsTrue(result.Length == 0);
        }

        [TestMethod]
        public void SearchVPTree_ReturnsHitsWithinDistance()
        {
            var tree = VPTree.Build(new ulong[] { 0, 1, 2, 3, 4, 5, 6, 565656345, ulong.MaxValue }, (a, b) => GetHammingDistance(ConvertToBinaryString(a), ConvertToBinaryString(b)));
            var result = tree.SearchVPTree(1, 10, 10).OrderBy(x => x.i).ToArray();

            Assert.IsTrue(result.Length == 7);
        }

        public static string ConvertToBinaryString(ulong value)
        {
            if (value == 0) return "0".PadLeft(64, '0');
            System.Text.StringBuilder b = new System.Text.StringBuilder();
            while (value != 0)
            {
                b.Insert(0, ((value & 1) == 1) ? '1' : '0');
                value >>= 1;
            }
            return b.ToString().PadLeft(64, '0');
        }

        public static int GetHammingDistance(string source, string target)
        {
            if (source.Length != target.Length)
            {
                throw new Exception("Strings must be equal length");
            }

            int distance =
                source.ToCharArray()
                .Zip(target.ToCharArray(), (c1, c2) => new { c1, c2 })
                .Count(m => m.c1 != m.c2);

            Debug.WriteLine(source);
            Debug.Write(target);
            Debug.WriteLine("= " + distance);

            return distance;
        }
    }
}
