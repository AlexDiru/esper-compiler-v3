using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    /// <summary>
    /// An extension of the node class, provides methods that the parser uses
    /// Keeps track of the current node the parser is on
    /// </summary>
    public class Root : Node
    {
        public Node CurrentNode;

        public Root()
        {
            CurrentNode = this;
        }

        /// <summary>
        /// Sets the current node's value and it's left child
        /// </summary>
        public void Insert(String value, Node leftNode)
        {
            CurrentNode.Value = value;
            CurrentNode.Left = new Node();
            CurrentNode.Right = new Node();
            CurrentNode.Left = leftNode;
            CurrentNode = CurrentNode.Right;
        }

        /// <summary>
        /// Inserts the final node in the parse tree
        /// </summary>
        public void InsertFinal(Node node)
        {
            CurrentNode.Value = "STATEMENTS";
            CurrentNode.Left = node;
        }
    }
}
