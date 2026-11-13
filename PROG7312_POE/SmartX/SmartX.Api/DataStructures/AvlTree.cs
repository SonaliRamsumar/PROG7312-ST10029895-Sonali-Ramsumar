
namespace SmartX.Api.DataStructures
{
    // A generic self-balancing AVL tree.
    // Keys must support comparison so entries can be kept in order.
    public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private class Node
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
            public int Height { get; set; } = 1;

            public Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        private Node? _root;
        private int _count;

        public int Count => _count;

        public int Height => GetHeight(_root);

        // Inserts a new key or updates the value if the key already exists.
        public void Insert(TKey key, TValue value)
        {
            bool added = false;
            _root = Insert(_root, key, value, ref added);

            if (added)
            {
                _count++;
            }
        }

        private Node Insert(
            Node? node,
            TKey key,
            TValue value,
            ref bool added)
        {
            if (node == null)
            {
                added = true;
                return new Node(key, value);
            }

            int comparison = key.CompareTo(node.Key);

            if (comparison < 0)
            {
                node.Left = Insert(node.Left, key, value, ref added);
            }
            else if (comparison > 0)
            {
                node.Right = Insert(node.Right, key, value, ref added);
            }
            else
            {
                node.Value = value;
                return node;
            }

            UpdateHeight(node);

            return Balance(node);
        }

        // Returns all values in ascending key order.
        public List<TValue> GetInOrder()
        {
            var results = new List<TValue>();
            TraverseInOrder(_root, results);
            return results;
        }

        private void TraverseInOrder(Node? node, List<TValue> results)
        {
            if (node == null)
            {
                return;
            }

            TraverseInOrder(node.Left, results);
            results.Add(node.Value);
            TraverseInOrder(node.Right, results);
        }

        // Returns values whose keys fall within the inclusive range.
        // Branches outside the requested range are skipped.
        public List<TValue> GetRange(TKey minimum, TKey maximum)
        {
            if (minimum.CompareTo(maximum) > 0)
            {
                throw new ArgumentException(
                    "The minimum key cannot be greater than the maximum key.");
            }

            var results = new List<TValue>();
            SearchRange(_root, minimum, maximum, results);
            return results;
        }

        private void SearchRange(
            Node? node,
            TKey minimum,
            TKey maximum,
            List<TValue> results)
        {
            if (node == null)
            {
                return;
            }

            int minimumComparison = minimum.CompareTo(node.Key);
            int maximumComparison = maximum.CompareTo(node.Key);

            // Only search the left branch if it may contain matching keys.
            if (minimumComparison < 0)
            {
                SearchRange(node.Left, minimum, maximum, results);
            }

            // Include the current node when it is inside the range.
            if (minimumComparison <= 0 && maximumComparison >= 0)
            {
                results.Add(node.Value);
            }

            // Only search the right branch if it may contain matching keys.
            if (maximumComparison > 0)
            {
                SearchRange(node.Right, minimum, maximum, results);
            }
        }

        private Node Balance(Node node)
        {
            int balanceFactor = GetBalanceFactor(node);

            // Left-heavy tree.
            if (balanceFactor > 1)
            {
                if (GetBalanceFactor(node.Left) < 0)
                {
                    node.Left = RotateLeft(node.Left!);
                }

                return RotateRight(node);
            }

            // Right-heavy tree.
            if (balanceFactor < -1)
            {
                if (GetBalanceFactor(node.Right) > 0)
                {
                    node.Right = RotateRight(node.Right!);
                }

                return RotateLeft(node);
            }

            return node;
        }

        private Node RotateRight(Node oldRoot)
        {
            Node newRoot = oldRoot.Left!;
            Node? transferredSubtree = newRoot.Right;

            newRoot.Right = oldRoot;
            oldRoot.Left = transferredSubtree;

            UpdateHeight(oldRoot);
            UpdateHeight(newRoot);

            return newRoot;
        }

        private Node RotateLeft(Node oldRoot)
        {
            Node newRoot = oldRoot.Right!;
            Node? transferredSubtree = newRoot.Left;

            newRoot.Left = oldRoot;
            oldRoot.Right = transferredSubtree;

            UpdateHeight(oldRoot);
            UpdateHeight(newRoot);

            return newRoot;
        }

        private static void UpdateHeight(Node node)
        {
            node.Height = 1 + Math.Max(
                GetHeight(node.Left),
                GetHeight(node.Right));
        }

        private static int GetHeight(Node? node)
        {
            return node?.Height ?? 0;
        }

        private static int GetBalanceFactor(Node? node)
        {
            if (node == null)
            {
                return 0;
            }

            return GetHeight(node.Left) - GetHeight(node.Right);
        }
    }
}
