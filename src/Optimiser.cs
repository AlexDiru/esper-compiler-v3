﻿using System;
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
        /// <summary>
        /// The root node of the parse tree (output from parser)
        /// </summary>
        public Node Root;

        /// <summary>
        /// Optimises the parse tree
        /// </summary>
        public void Optimise()
        {
            OptimiseOperatorsAndValues();
            OptimiseConditionals();
        }

        /// <summary>
        /// Optimises the following:
        /// 3 + 2 * a ==> 5 * a
        /// b = 7(8/4) ==> b = 14
        /// </summary>
        private void OptimiseOperatorsAndValues()
        {
            OptimiseOperatorsAndValuesOfNode(Root);
        }

        /// <summary>
        /// Traverses the tree in post-order (bottom-up) and optimises any arithmetic
        /// </summary>
        private void OptimiseOperatorsAndValuesOfNode(Node currentNode)
        {
            if (currentNode.Left != null)
            {
                OptimiseOperatorsAndValuesOfNode(currentNode.Left);
            }

            if (currentNode.Right != null)
            {
                OptimiseOperatorsAndValuesOfNode(currentNode.Right);
            }

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

                    currentNode.Attributes[1] = "VALUE";
                    currentNode.Attributes[0] = currentNode.Attributes[2] = "";
                    currentNode.Left = null;
                    currentNode.Right = null;
                }
            }
        }

        /// <summary>
        /// Optimises the following
        /// b = 31 == 19 => b = 1
        /// b = 12 >= 13 => b = 0
        /// </summary>
        private void OptimiseConditionals()
        {
            OptimiseConditionalsOfNode(Root);
        }

        /// <summary>
        /// Traverses the tree in post-order (bottom-up) and optimises any conditions
        /// </summary>
        private void OptimiseConditionalsOfNode(Node currentNode)
        {
            if (currentNode.Left != null)
            {
                OptimiseConditionalsOfNode(currentNode.Left);
            }

            if (currentNode.Right != null)
            {
                OptimiseConditionalsOfNode(currentNode.Right);
            }

            if (Parser.IsConditionalOperator(currentNode.Attributes[0]))
            {
                //If left node and right node are both values
                Int32 leftValue;
                Int32 rightValue;
                if (currentNode.Left != null && Int32.TryParse(currentNode.Left.Value, out leftValue) &&
                    currentNode.Right != null && Int32.TryParse(currentNode.Right.Value, out rightValue) &&
                    currentNode.Left.Attributes[1] == "VALUE" && currentNode.Right.Attributes[1] == "VALUE")
                {
                    if (currentNode.Attributes[0] == "==")
                        currentNode.Value = (leftValue == rightValue ? 1 : 0).ToString();
                    else if (currentNode.Attributes[0] == "<=")
                        currentNode.Value = (leftValue <= rightValue ? 1 : 0).ToString();
                    else if (currentNode.Attributes[0] == ">=")
                        currentNode.Value = (leftValue >= rightValue ? 1 : 0).ToString();
                    else
                        currentNode.Value = (leftValue != rightValue ? 1 : 0).ToString();

                    currentNode.Attributes[1] = "VALUE";
                    currentNode.Attributes[0] = currentNode.Attributes[2] = "";
                    currentNode.Left = null;
                    currentNode.Right = null;
                }
            }
        }
    }
}
