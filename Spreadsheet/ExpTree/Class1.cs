using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CptS321
{
    public class ExpTree
    {
        public string storedExpression { private set; get; }
        private Node root;
        //All ExpTree's have access to the same dictionary, will need to change this later so each has
        //its own dictionary to reference
        private static Dictionary<string, double> variableDictionary = new Dictionary<string, double>();
        public ExpTree(string expression)
        {
            //Stripping escape characters and whitespace
            storedExpression = expression.Replace(" ", "").Replace(@"\", "");
            root = generateParseTree(storedExpression);

        }

        private Node generateParseTree(string storedExpression)
        {
            Queue<string> outputQueue = expressionToPostFixQueue(storedExpression);
            Node treeRoot = treeFromPostFixQueue(outputQueue);
            return treeRoot;
        }

        private Node treeFromPostFixQueue(Queue<string> outputQueue)
        {
            Stack<Node> nodeStack = new Stack<Node>();
            foreach (string str in outputQueue)
            {
                if (Regex.IsMatch(str, "[a-zA-Z]+\\d*"))
                {
                    nodeStack.Push(new VarNode(str));
                }
                else if (Regex.IsMatch(str, "[0-9]+([\\,\\.][0-9]+)?"))
                {
                    nodeStack.Push(new ValNode(Double.Parse(str)));
                }
                else if (Regex.IsMatch(str, "[+\\-/*]"))
                {
                    switch (str.First().ToString())
                    {
                        case "+":
                            nodeStack.Push(new OpNode('+', nodeStack.Pop(), nodeStack.Pop()));
                            break;
                        case "-":
                            nodeStack.Push(new OpNode('-', nodeStack.Pop(), nodeStack.Pop()));
                            break;
                        case "*":
                            nodeStack.Push(new OpNode('*', nodeStack.Pop(), nodeStack.Pop()));
                            break;
                        case "/":
                            nodeStack.Push(new OpNode('/', nodeStack.Pop(), nodeStack.Pop()));
                            break;
                        default:
                            return new ValNode(0.0);
                    }
                }
            }
            return nodeStack.Pop();
        }
        private Queue<string> expressionToPostFixQueue(string storedExpression)
        {
            //80% implemented Shunting Algorithm, missing precedence management
            //supports all assignment requirements for HW6
            string op = "[+\\-/*]";
            string variable = "[a-zA-Z]+\\d*";
            string value = "[0-9]+([\\,\\.][0-9]+)?";
            string pattern = value + "|" + op + "|" + variable;
            Regex regex = new Regex(pattern);
            MatchCollection matchCollection = regex.Matches(storedExpression);
            //Framework for implementing precedence management
            Stack<string> operatorStack = new Stack<string>();
            Queue<string> outputQueue = new Queue<string>();
            foreach (Match match in matchCollection)
            {
                if (new Regex(op).IsMatch(match.Value))
                {
                    //This will need to be changed when implementing precedence
                    if (operatorStack.Count == 0)
                    {
                        operatorStack.Push(match.Value.First().ToString());
                    }
                    else
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                        operatorStack.Push(match.Value.First().ToString());
                    }
                }
                else if (new Regex(variable).IsMatch(match.Value))
                {
                    outputQueue.Enqueue(match.Value);
                }
                else if (new Regex(value).IsMatch(match.Value))
                {
                    if (Double.TryParse(match.Value, out double dbl))
                    {
                        outputQueue.Enqueue(dbl.ToString());
                    }
                }
            }
            while (operatorStack.Count != 0)
            {
                outputQueue.Enqueue(operatorStack.Pop());
            }
            return outputQueue;
        }

        public void SetVar(string varName, double varValue)
        {
            if (variableDictionary.ContainsKey(varName))
            {
                variableDictionary[varName] = varValue;
            }

        }
        public double Eval()
        {
            return root.Eval();
        }

        private abstract class Node
        {
            protected Node lchild, rchild;
            public abstract double Eval();
        }

        private class ValNode : Node
        {
            double value;
            public ValNode(double val)
            {
                value = val;
                lchild = null;
                rchild = null;
            }
            public override double Eval()
            {
                return value;
            }
        }

        private class VarNode : Node
        {
            string varName;
            public VarNode(string var)
            {
                varName = var;
                variableDictionary[varName] = 0.0;
                lchild = null;
                rchild = null;
            }
            public override double Eval()
            {
                return variableDictionary[varName];
            }
        }
        private class OpNode : Node
        {
            char op;
            public OpNode(char op)
            {
                this.op = op;
            }
            public OpNode(char op, Node right, Node left)
            {
                this.op = op;
                lchild = left;
                rchild = right;
            }

            public override double Eval()
            {
                switch (op)
                {
                    case '+':
                        return lchild.Eval() + rchild.Eval();
                    case '-':
                        return lchild.Eval() - rchild.Eval();
                    case '*':
                        return lchild.Eval() * rchild.Eval();
                    case '/':
                        return lchild.Eval() / rchild.Eval();
                    default:
                        return 0.0;

                }
            }
        }

        //private Queue<Node> expressionToPostFixNodeQueue(string storedExpression)
        //{

        //    string op = "[+\\-/*]";
        //    string variable = "[a-zA-Z]+\\d*";
        //    string value = "[0-9]+([\\,\\.][0-9]+)?";
        //    string pattern = value + "|" + op + "|" + variable;
        //    Regex regex = new Regex(pattern);
        //    MatchCollection matchCollection = regex.Matches(storedExpression);
        //    //Framework for implementing precedence management
        //    Stack<Node> operatorStack = new Stack<Node>();
        //    Queue<Node> outputQueue = new Queue<Node>();
        //    foreach (Match match in matchCollection)
        //    {
        //        if (new Regex(op).IsMatch(match.Value))
        //        {
        //            OpNode opNode = new OpNode(match.Value.First());

        //            if (operatorStack.Count == 0)
        //            {
        //                operatorStack.Push(opNode);
        //            }
        //            else
        //            {
        //                outputQueue.Enqueue(operatorStack.Pop());
        //                operatorStack.Push(opNode);
        //            }
        //        }
        //        else if (new Regex(variable).IsMatch(match.Value))
        //        {
        //            VarNode varNode = new VarNode(match.Value);
        //            outputQueue.Enqueue(varNode);
        //        }
        //        else if (new Regex(value).IsMatch(match.Value))
        //        {
        //            Double dbl;
        //            if (Double.TryParse(match.Value, out dbl))
        //            {
        //                ValNode valNode = new ValNode(dbl);
        //                outputQueue.Enqueue(valNode);
        //            }
        //        }
        //    }
        //    while (operatorStack.Count != 0)
        //    {
        //        outputQueue.Enqueue(operatorStack.Pop());
        //    }
        //    return outputQueue;
        //}
    }
}
