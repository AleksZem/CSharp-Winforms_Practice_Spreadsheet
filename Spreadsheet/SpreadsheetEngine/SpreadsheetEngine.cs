/*
// Aleks Zemlyanskiy
// CS321
// HW5 Spreadsheet v1.0
// 3/14/2018
// AbstractCell.cs
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SpreadSheet
{
    public abstract class AbstractCell : INotifyPropertyChanged
    {
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }
        protected string Text = ""; //Consider renaming to CellContents?
        protected string Value = "";
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler PropertyChangedValue;
        public List<AbstractCell> SubscribedToCells = new List<AbstractCell>();

        public void HandleValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("Text");
            NotifyPropertyChanged("Value");
        }

        public void NotifyPropertyChanged(string Text)
        {
            switch (Text)
            {
                case "Text":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Text));
                    break;
                case "Value":
                    PropertyChangedValue?.Invoke(this, new PropertyChangedEventArgs(Text));
                    break;
                default:
                    break;
            }
        }
        public string TextValue
        {
            get
            {

                return Text;
            }
            set
            {
                if (Text == value) return; //Value is unchanged, no need to fire Propery Changed Notification
                else
                {
                    Text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        public string EvaluatedValue
        {
            get
            {
                if (Text.Length == 0 || Text.First() != '=') return Text; //If the text attribute doesn't start with an '=' we assume it's already been evaluated to a singe term
                else { return Value; }  //If the cell contents start with a '=' it is assumed we have to do some calculation to determine it's calculated value
            }
            internal set
            {
                Value = value;
                NotifyPropertyChanged("Value");
            }
        }


        protected AbstractCell(int rowIndex, int columnIndex)
        {
            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;
        }
    }

    public class MySpreadsheet
    {
        private AbstractCell[,] cells; //back end logical spreadsheet
        public int RowCount{ get; private set; }
        public int ColumnCount{ get; private set; }
        public MySpreadsheet(int rows, int columns)
        {
            RowCount = rows;
            ColumnCount = columns;
            cells = new AbstractCell[rows, columns];
            //Populates spreadsheet with basic cells and assigns self as PropertyChange delegate
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    cells[i, j] = CellFactory("Basic", i, j);
                    cells[i, j].PropertyChanged += new PropertyChangedEventHandler(HandlePropertyChanged);
                }
            }
            
        }

        public event PropertyChangedEventHandler CellPropertyChanged;
        //Event used to alert UI-layer code to update the content of a cell
        public void NotifyCellPropertyChanged(AbstractCell cell)
        {
            CellPropertyChanged?.Invoke(cell, new PropertyChangedEventArgs("Cell"));
        } 

        //cell Property change handler
        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AbstractCell cell = (AbstractCell)sender;
            switch (e.PropertyName)
            {
                case "Text":
                    cell.EvaluatedValue = changedCellStringContents(cell);
                    break;
                case "Value":
                    //TODO: 
                    break;
                default:
                    break;
            }
            NotifyCellPropertyChanged(cell);
        }

        //Factory method implementation, only one concrete class for now
        private AbstractCell CellFactory(string cellType, int row, int column)
        {
            switch (cellType)
            {
                case "Basic":
                    return new BasicCell(row, column);
                default:
                    return null;
            }
        }

        //Used to update a cell's evaluated text value, defined recursively to allow for nested references (e.g. B1 = B2, B2 = B3, B3 = "5")
        private string changedCellStringContents(AbstractCell cell)
        {
            if (cell.TextValue.First() != '=')
            {
                return cell.TextValue;
            }
            else
            {
                ExpTree expTree = new ExpTree(cell.TextValue.Substring(1));
                List<ExpTree.VarNode> variableNodeList = expTree.variablesNodes();
                foreach (ExpTree.VarNode varNode in variableNodeList)
                {
                    int column = ((int)varNode.getName()[0] - 64) - 1;
                    int row = int.Parse(varNode.getName().Substring(1)) - 1;
                    string temp = cells[row, column].EvaluatedValue;
                    if (temp.Length == 0)
                    {
                        return "#REF!"; //Refernce Error
                    }
                    varNode.setValue(Double.Parse(cells[row, column].EvaluatedValue));
                }
                //Unsubscribe from the following Cells
                foreach (AbstractCell subCell in cell.SubscribedToCells)
                {
                    subCell.PropertyChangedValue -= (PropertyChangedEventHandler)cell.HandleValuePropertyChanged;
                }
                //Executed separately to ensure no cells are subrscribed if a reference error occurs
                foreach (ExpTree.VarNode varNode in variableNodeList)
                {
                    int column = ((int)varNode.getName()[0] - 64) - 1;
                    int row = int.Parse(varNode.getName().Substring(1)) - 1;
                    cells[row, column].PropertyChangedValue += new PropertyChangedEventHandler(cell.HandleValuePropertyChanged);
                    //Record which cells we're subbed to
                    cell.SubscribedToCells.Add(cells[row, column]);
                }
                return expTree.Eval().ToString();
                
                //return changedCellStringContents(cells[row, column]);
            }
        }
 
        public AbstractCell getCell(int row, int column)
        {
            if (row < 0 || row >= RowCount || column < 0 || column >= ColumnCount) return null; //Bounds check
            else return cells[row, column]; 
        }

        //Single concrete class implementation of Abstract cell
        private class BasicCell : AbstractCell
        {
            internal BasicCell(int row, int col) : base(row, col)
            {

            }
        }

        /// <summary>
        /// HW6 Assignment Below
        /// Simple Expression Tree
        /// 
        /// HW8 Update: Real hack job to get it integrated with the Spreadsheet class
        /// Requiring us to get rid of the dictionary certainly made this less elegant
        /// </summary>
        private class ExpTree
        {
            public string storedExpression { private set; get; }
            private Node root;
            //private Dictionary<string, double> variableDictionary = new Dictionary<string, double>();
            public List<VarNode> variablesNodes()
            {
                List<VarNode> varNodeList = new List<VarNode>();
                addVarNodesTraversal(root, varNodeList);
                return varNodeList;
            }

            private void addVarNodesTraversal(Node root, List<VarNode> list)
            {
                if (root != null)
                {
                    if (root is VarNode)
                    {
                        list.Add(root as VarNode);
                    }
                    if (root.getLeftChild() != null)
                    {
                        addVarNodesTraversal(root.getLeftChild(), list);
                    }
                    if (root.getLeftChild() != null)
                    {
                        addVarNodesTraversal(root.getRightChild(), list);
                    }
                }
            }

            public ExpTree(string expression)
            {
                //Stripping escape characters and whitespace
                storedExpression = expression.Replace(" ", "").Replace(@"\", "");
                //Converts expression to postfix, generates tree and returns its root
                root = generateParseTree(storedExpression);//,variableDictionary);
            }

            private Node generateParseTree(string storedExpression)//, Dictionary<string, double> variableDictionary)
            {
                //variableDictionary.Clear();
                //Converts expression to post fix notation
                Queue<string> outputQueue = expressionToPostFixQueue(storedExpression);
                //Converts post fix expresssion into a binary tree
                Node treeRoot = treeFromPostFixQueue(outputQueue);//, variableDictionary);
                return treeRoot;
            }

            private Node treeFromPostFixQueue(Queue<string> outputQueue)//, Dictionary<string, double> variableDictionary)
            {
                Stack<Node> nodeStack = new Stack<Node>();
                foreach (string str in outputQueue)
                {
                    if (Regex.IsMatch(str, @"[a-zA-Z]+\d*"))//"[a-zA-Z]+\\d*")) Should match variables starting with letters followed potentially by numbers
                    {
                        nodeStack.Push(new VarNode(str));//, variableDictionary)); 
                    }
                    else if (Regex.IsMatch(str, @"[0-9]+([\,\.][0-9]+)?"))//"[0-9]+([\\,\\.][0-9]+)?")) Should match integers and doubles
                    {
                        nodeStack.Push(new ValNode(Double.Parse(str)));
                    }
                    else if (Regex.IsMatch(str, @"[+-/*]")) //"[+\\-/*]")) Should match operators +-*/
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
                string op = @"[+-/*()]";// "[+\\-/*]";
                string variable = @"[a-zA-Z]+\d*";// "[a-zA-Z]+\\d*";
                string value = @"[0-9]+([\,\.][0-9]+)?";// "[0-9]+([\\,\\.][0-9]+)?";
                string pattern = value + "|" + op + "|" + variable;
                Regex regex = new Regex(pattern);
                MatchCollection matchCollection = regex.Matches(storedExpression);
                //Framework for implementing precedence management
                Stack<string> operatorStack = new Stack<string>();
                Queue<string> outputQueue = new Queue<string>();
                //Dictionary of precedence
                Dictionary<char, int> precedenceDictionary = new Dictionary<char, int>() { { '+', 1 }, { '-', 1 }, { '/', 2 }, { '*', 2 }, { '(', 3 }, { ')', 0 } };
                foreach (Match match in matchCollection)
                {


                    if (new Regex(op).IsMatch(match.Value))
                    {
                        if (match.Value.First() != ')')
                        {
                            while (operatorStack.Count != 0 && operatorStack.Peek().First() != '('
                                && (precedenceDictionary[operatorStack.Peek().First()] > precedenceDictionary[match.Value.First()]))
                            {
                                outputQueue.Enqueue(operatorStack.Pop());
                            }
                            operatorStack.Push(match.Value.First().ToString());
                        }
                        else
                        { // is closing parenthesis condition
                            while (operatorStack.Count != 0 && operatorStack.Peek().First() != '(')
                            {
                                outputQueue.Enqueue(operatorStack.Pop());
                            }
                            if (operatorStack.Count != 0 && operatorStack.Peek().First() == '(')
                            {
                                operatorStack.Pop();
                            }
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


            //Associates a variable in the expression to a specific value in the variable dictionary
            public void SetVar(string varName, double varValue)
            {
                //if (variableDictionary.ContainsKey(varName))
                //{
                //    variableDictionary[varName] = varValue;
                //}

            }
            public double Eval()
            {
                return root.Eval();
            }

            public abstract class Node
            {
                protected Node lchild, rchild;
                public abstract double Eval();
                public Node getLeftChild()
                {
                    return lchild;
                }
                public Node getRightChild()
                {
                    return rchild;
                }
            }

            //Single value node, leaf node in the binary tree
            public class ValNode : Node
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

            //Variable node, also a lead node in the binary tree
            //Implementation is not ideal, will need to go back and implement a cleaner handling of the
            //variable dictionary
            public class VarNode : Node
            {
                string varName;
                double value;
                //Dictionary<string, double> variableDictionary;
                public VarNode(string var)//, Dictionary<string, double> variableDictionary)
                {
                    varName = var;
                    value = 0.0;
                    //this.variableDictionary = variableDictionary;
                    //variableDictionary[varName] = 0.0;
                    lchild = null;
                    rchild = null;
                }
                public override double Eval()
                {
                    return value;//variableDictionary[varName];
                }
                public string getName()
                {
                    return varName;
                }
                public void setValue(double val)
                {
                    value = val;
                }
            }

            //Operator node, internal node in the tree, cannot be a leaf node
            public class OpNode : Node
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
        }
    }
}