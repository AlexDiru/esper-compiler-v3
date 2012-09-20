using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace esper_compiler_v3.src
{
    /// <summary>
    /// Given the output of the parser, optimises the code
    /// </summary>
    class Optimiser
    {
        public Node Root;

        public void Optimise()
        {
            OptimiseOperatorsAndValues();
        }

        /// <summary>
        /// 3 + 2 * a ==> 5 * a
        /// b = 7(8/4) ==> b = 14
        /// Currently only checks a node's child nodes (doesn;t go deeper)
        /// </summary>
        private void OptimiseOperatorsAndValues()
        {
            //Iterate through tree, if operator is found, recursively optimise children
            Node currentNode = Root;
            Node parentNode = null;

            //Nodes found which are operator nodes (so the child nodes know)
            List<Node> operatorNodes = new List<Node>();

            OptimiseOperatorsAndValuesOfNode(Root);
        }

        private void OptimiseOperatorsAndValuesOfNode(Node currentNode)
        {
            if ("+-*/".Contains(currentNode.Value))
            {
                //If left node and right node are both values
                Int32 leftValue;
                Int32 rightValue;
                if (currentNode.Left != null && Int32.TryParse(currentNode.Left.Value, out leftValue) &&
                    currentNode.Right != null && Int32.TryParse(currentNode.Right.Value, out rightValue) &&
                    currentNode.Left.Attributes[1] == "VALUE" && currentNode.Right.Attributes[1] == "VALUE")
                {
                    if (currentNode.Value == "+")
                        currentNode.Value = (leftValue + rightValue).ToString();
                    else if (currentNode.Value == "-")
                        currentNode.Value = (leftValue - rightValue).ToString();
                    else if (currentNode.Value == "*")
                        currentNode.Value = (leftValue * rightValue).ToString();
                    else
                        currentNode.Value = (leftValue / rightValue).ToString();

                    currentNode.Attributes[0] = "INT";
                    currentNode.Attributes[1] = currentNode.Attributes[2] = "";
                    currentNode.Left = null;
                    currentNode.Right = null;
                }
                //operatorNodes.Add(currentNode);
            }

            if (currentNode.Left != null)
            {
                OptimiseOperatorsAndValuesOfNode(currentNode.Left);
            }

            if (currentNode.Right != null)
            {
                OptimiseOperatorsAndValuesOfNode(currentNode.Right);
            }
        }
    }
}
