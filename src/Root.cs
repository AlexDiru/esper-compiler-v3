using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    class Root : Node
    {

        public Node CurrentNode;

        public Root()
        {
            CurrentNode = this;
        }

        public void Insert(String value, Node leftNode)
        {
            CurrentNode.Value = value;
            CurrentNode.Left = new Node();
            CurrentNode.Right = new Node();
            CurrentNode.Left = leftNode;
            CurrentNode = CurrentNode.Right;
        }

        public void InsertFinal(Node node)
        {
            CurrentNode.Value = "STATEMENTS";
            CurrentNode.Left = node;
        }
    }
}
