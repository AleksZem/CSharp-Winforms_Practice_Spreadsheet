///Aleks Zemlyanskiy
///CPTS321 HW6
///Console Application used to demo functional simple expression tree
///used to evaluate expressions supporting variables

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CptS321;

namespace ExpTreeConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExpTree expTree = new ExpTree("A+42");
            //expTree.SetVar("A", 0);
            //string expression = expTree.storedExpression;
            //bool finished = false;
            //while (!finished)
            //{
            //    Console.Write("Menu (Current expression= "+expression+")\n" +
            //    "\t1 = Enter a new expression\n" +
            //    "\t2 = Set a variable value\n" +
            //    "\t3 = Evaluate tree\n" +
            //    "\t4 = Quit\n");
            //    string input = Console.ReadLine();
            //    switch (input)
            //    {
            //        case "1":
            //            Console.Write("Enter new expression: ");
            //            expTree = new ExpTree(Console.ReadLine());
            //            expression = expTree.storedExpression;
            //            break;
            //        case "2":
            //            Console.Write("Enter variable name: ");
            //            string varname = Console.ReadLine();
            //            Console.Write("Enter variable value: ");
            //            Double varval = Double.Parse(Console.ReadLine());
            //            expTree.SetVar(varname, varval);
            //            break;
            //        case "3":
            //            Console.WriteLine(expTree.Eval());
            //            break;
            //        case "4":
            //            finished = true;
            //            break;
            //        default:
            //            Console.WriteLine("I'm just gonna ignore that");
            //            break;
            //    }
            //}
        }
    }
}
