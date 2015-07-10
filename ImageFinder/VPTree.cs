using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageFinder
{
    public class VPTree
    {
        private static int partition(List<Node> list, int left, int right, int pivotIndex, Func<Node, Node, int> comp)
        {
            var pivotValue = list[pivotIndex];
            var swap = list[pivotIndex];	// Move pivot to end
            list[pivotIndex] = list[right];
            list[right] = swap;
            var storeIndex = left;
            for (var i = left; i < right; i++)
            {
                if (comp(list[i], pivotValue) > 0)
                {
                    swap = list[storeIndex];
                    list[storeIndex] = list[i];
                    list[i] = swap;
                    storeIndex++;
                }
            }
            swap = list[right];				// Move pivot to its final place
            list[right] = list[storeIndex];
            list[storeIndex] = swap;
            return storeIndex;
        }

        private static int medianOf3(List<Node> list, int a, int b, int c, Func<Node, Node, int> comp)
        {
            var A = list[a];
            var B = list[b];
            var C = list[c];
            return comp(A, B) > 0 ?
                comp(B, C) > 0 ? b : comp(A, C) > 0 ? c : a :
                comp(A, C) > 0 ? a : comp(B, C) > 0 ? c : b;
        }

        private static Node nth_element(List<Node> list, int left, int nth, int right, Func<Node, Node, int> comp)
        {
            if (nth <= 0 || nth > (right - left + 1))
                throw new Exception("VPTree.nth_element: nth must be in range [1, right-left+1] (nth=" + nth + ")");

            var pivotIndex = 0;
            var pivotNewIndex = 0;
            var pivotDist = 0;
            for (; ; )
            {
                // select pivotIndex between left and right
                pivotIndex = medianOf3(list, left, right, (left + right) >> 1, comp);
                pivotNewIndex = partition(list, left, right, pivotIndex, comp);
                pivotDist = pivotNewIndex - left + 1;
                if (pivotDist == nth)
                {
                    return list[pivotNewIndex];
                }
                else if (nth < pivotDist)
                {
                    right = pivotNewIndex - 1;
                }
                else
                {
                    nth -= pivotDist;
                    left = pivotNewIndex + 1;
                }
            }
        }

        private static Node select(List<Node> list, int k, Func<Node, Node, int> comp)
        {
            if (k < 0 || k >= list.Count) throw new Exception("VPTree.select: k must be in range [0, list.length-1] (k=" + k + ")");

            return nth_element(list, 0, k + 1, list.Count - 1, comp);
        }

        private static Random rnd = new Random();

        private static int selectVPIndex(List<Node> list)
        {
            return rnd.Next(list.Count);
        }

        private static List<Node> recurseVPTree(ulong[] S, List<Node> list, Func<ulong, ulong, int> distance, int nb)
        {
            if (list.Count == 0) return Enumerable.Empty<Node>().ToList();
            var i = 0;

            // Is this a leaf node ?
            var listLength = list.Count;
            if (nb > 0 && listLength <= nb)
            {
                /*var bucket = new ulong[listLength];
                for(i = 0; i < listLength; i++)
                {
                    bucket[i] = list[i].i;
                }*/
                //return bucket;
                return list;
            }

            // Non-leaf node.
            // Constructs a node with the selected vantage point extracted from the set.
            var vpIndex = selectVPIndex(list);
            var node = list[vpIndex];
            list.Splice(vpIndex, 1);
            listLength--;
            // We can't use this information yet, so don't show it in the vp-tree output.
            node.dist = null;
            if (listLength == 0)
            {
                return new[] { node }.ToList();
            }

            // Adds to each item its distance to the vantage point.
            // This ensures each distance is computed only once.
            ulong vp = S[node.i];
            var dmin = int.MaxValue;
            var dmax = 0;
            Node item = null;
            int dist = 0;
            int n;
            for (i = 0, n = listLength; i < n; i++)
            {
                item = list[i];
                dist = distance(vp, S[item.i]);
                item.dist = dist;// > 0;
                //item.hist.push(dist);	// unused (yet)
                if (dmin > dist) dmin = dist;
                if (dmax < dist) dmax = dist;
            }
            node.m = dmin;
            node.M = dmax;

            // Partitions the set around the median distance.
            var medianIndex = listLength >> 1;
            var median = select(list, medianIndex, distanceComparator);

            // Recursively builds vp-trees with the 2 resulting subsets.
            var leftItems = list.Splice(0, medianIndex);
            var rightItems = list;
            node.μ = median.dist;
            node.L = recurseVPTree(S, leftItems, distance, nb);
            node.R = recurseVPTree(S, rightItems, distance, nb);
            return new[] { node }.ToList();
        }

        private static VPTree buildVPTree(ulong[] S, Func<ulong, ulong, int> distance, int nb)
        {
            var list = new List<Node>(S.Length);
            for (int i = 0, n = S.Length; i < n; i++)
            {
                list.Insert(i, new Node()
                {
                    i = i
                    //hist: []		// unused (yet)
                });
            }

            var tree = recurseVPTree(S, list, distance, nb);
            return new VPTree(S, distance, tree);
        }

        private void doSearch(ulong q, int τ, PriorityQueue W, List<Node> node)
        {
            if (!node.Any()) return;

            // Leaf node : test each element in this node's bucket.
            if (node.Count > 1)
            {
                var n = node.Count;
                for (var i = 0; i < n; i++)
                {
                    comparisons++;
                    var elementID = node[i].i;
                    var element = S[elementID];
                    var elementDist = distance(q, element);
                    if (elementDist < τ)
                    {
                        τ = W.Insert(elementID, elementDist) ?? τ;
                    }
                }
                return;
            }

            // Non-leaf node
            var id = node[0].i;
            var p = S[id];
            var dist = distance(q, p);

            comparisons++;

            // This vantage-point is close enough to q.
            if (dist < τ)
            {
                τ = W.Insert(id, dist) ?? τ;
            }

            // The order of exploration is determined by comparison with μ.
            // The sooner we find elements close to q, the smaller τ and the more nodes we skip.
            // P. Yianilos uses the middle of left/right bounds instead of μ.
            var μ = node[0].μ;
            var L = node[0].L;
            var R = node[0].R;
            if (μ == null)
                return;
            if (dist < μ)
            {
                if (L.Any() && node[0].m - τ < dist) doSearch(q, τ, W, L);
                if (R.Any() && μ - τ < dist && dist < node[0].M + τ) doSearch(q, τ, W, R);
            }
            else
            {
                if (R.Any() && dist < node[0].M + τ) doSearch(q, τ, W, R);
                if (L.Any() && node[0].m - τ < dist && dist < μ + τ) doSearch(q, τ, W, L);
            }
        }

        public IEnumerable<ImageFinder.PriorityQueue.PItem> searchVPTree(ulong q, int n = 1, int τ = int.MaxValue)
        {
            var W = new PriorityQueue(n);
            doSearch(q, τ, W, this.tree);
            return W.List();
        }

        public class Node
        {
            public Node()
            {
                this.L = new List<Node>();
                this.R = new List<Node>();
            }

            public int i { get; set; }

            public int m { get; set; }

            public int M { get; set; }

            public int? μ { get; set; }

            public List<Node> L { get; set; }

            public List<Node> R { get; set; }

            public int? dist { get; set; }
        }

        private static Func<Node, Node, int> distanceComparator = (a, b) => b.dist.GetValueOrDefault() - a.dist.GetValueOrDefault();// a.dist < b.dist;

        private ulong[] S;
        private Func<ulong, ulong, int> distance;
        private int comparisons;
        private List<Node> tree;

        private VPTree(ulong[] S, Func<ulong, ulong, int> distance, List<Node> tree = null, int nb = 0)
        {
            this.S = S;
            this.distance = distance;
            this.tree = tree;
            this.comparisons = 0;
        }

        public static VPTree Build(ulong[] S, Func<ulong, ulong, int> distance, List<Node> tree = null, int nb = 0)
        {
            if (tree == null)
            {
                return buildVPTree(S, distance, nb);
            }
            else
            {
                return new VPTree(S, distance, tree, nb);
            }
        }
    }
}