using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    /// <summary>
    /// Represents the node of a binary tree
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The value of the node
        /// </summary>
        public String Value;

        /// <summary>
        /// Three additional attributes that the node can hold - makes code gen easier
        /// </summary>
        public String[] Attributes = new String[3];

        /// <summary>
        /// The left child node of this node
        /// </summary>
        public Node Left;

        /// <summary>
        /// The right child node of this node
        /// </summary>
        public Node Right;

        /// <summary>
        /// Given a node, deletes all the children
        /// </summary>
        public static void DeleteTree(Node Root)
        {
            if (Root.Left != null)
                DeleteTree(Root.Left);

            if (Root.Right != null)
                DeleteTree(Root.Right);

            Root = null;
        }

        /// <summary>
        /// Recursively searches through the child nodes 
        /// </summary>
        public void NullifyEmptyChildren()
        {
            if (Left != null)
            {
                Left.NullifyEmptyChildren();

                if (Left.IsEmpty())
                    Left = null;
            }

            if (Right != null)
            {
                Right.NullifyEmptyChildren();

                if (Right.IsEmpty())
                    Right = null;
            }
        }

        /// <summary>
        /// Checks whether the node has any data
        /// </summary>
        private bool IsEmpty()
        {
            return Left == null && Right == null && String.IsNullOrEmpty(Attributes[0]) && String.IsNullOrEmpty(Attributes[1]) && String.IsNullOrEmpty(Attributes[2]);
        }

        /// <summary>
        /// Returns a cloned node with all the same properties
        /// </summary>
        public Node Clone()
        {
            Node clone = new Node();
            clone.Left = Left;
            clone.Right = Right;
            clone.Value = Value;
            clone.Attributes = Attributes;

            return clone;
        }

        /// <summary>
        /// Outputs the parse tree in a readable format
        /// </summary>
        public static void DisplayTree(Node node, String inner)
        {
            if (node == null)
                return;

            Console.Write(inner + ">" + node.Value);

            String Attrib = "";

            foreach (String Attribute in node.Attributes)
                if (Attribute != "")
                    Attrib += " " + Attribute;

            if (Attrib != "" && node.Value != "STATEMENTS")
                Console.Write("  (Attributes: " + Attrib + ")");

            Console.Write("\n");

            DisplayTree(node.Left, inner + "-");
            DisplayTree(node.Right, inner + "-");
        }
    }
}
