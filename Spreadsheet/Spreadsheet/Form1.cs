/*
// Aleks Zemlyanskiy
// CS321
// HW5 Spreadsheet v1.0
// 3/14/2018
// Form1.cs
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using SpreadSheet;

namespace Spreadsheet
{
    public partial class Form1 : Form
    {
        static MySpreadsheet MySpreadsheet;
        public Form1()
        {
            InitializeComponent();
            MySpreadsheet = new MySpreadsheet(50, 26);
            MySpreadsheet.CellPropertyChanged += new PropertyChangedEventHandler(SpreadSheetPropertyChanged);
            //Move the following into data-grid initialization
            //Populates the data grid with columns 'A' through 'Z'
            for (char col = 'A'; col <= 'Z'; ++col)
            {
                dataGridView1.Columns.Add("Column" + col, col.ToString());
            }
            //Creates 50 rows in the datagridview and numbers each row
            dataGridView1.Rows.Add(50);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }

            //Fires when an event occurs which requires updating the UI cells, hacky use of PropertyChangedEventArgs parameter
            //used to pass the row and column of the cell which needs to be updated
            void SpreadSheetPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                AbstractCell cell = (AbstractCell)sender;
                int row = cell.RowIndex;
                int column = cell.ColumnIndex;
                dataGridView1[column, row].Value = MySpreadsheet.getCell(row, column).EvaluatedValue;
            }
        }

        //Demo button generates cell content in 50 random cells along with demonstrating relational integrity in first two columns
        private void button2_Click_1(object sender, EventArgs e)
        {
            Random rndNumber = new Random();
            for (int count = 0; count < 50; count++)
            {
                int row = rndNumber.Next(0, 50);
                int col = rndNumber.Next(0, 26);
                MySpreadsheet.getCell(row, col).TextValue = "Not another random cell";
            }
            //for (int row = 0; row < MySpreadsheet.RowCount; row++)
            //{
            //    MySpreadsheet.getCell(row, 1).TextValue = "This is cell B" + (row + 1);
            //    MySpreadsheet.getCell(row, 0).TextValue = "=B" + (row + 1); //Used to demonstrate cells can respond appropriately to having value set to content of another cell
            //    //i.e. ex. B1_cell.value = "=B2" should evaluate to the value of the b2 cell 
            //}
        }

        //Light user UI-level cell editting, still needs further error validation
        private void dataGridView1_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = ((DataGridView)sender).CurrentCell;
            if (cell.Value == null) return;
            if (MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).EvaluatedValue != cell.Value.ToString())
            {
                MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue = cell.Value.ToString();
            }
        }



        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (dataGridView1.CurrentCell == null) return;
                DataGridViewCell cell = dataGridView1.CurrentCell;
                MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue = textBox1.Text;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = ((DataGridView)sender).CurrentCell;
            if (MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue != null)
            {
                textBox1.Text = MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue;
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            //DataGridViewCell cell = dataGridView1.CurrentCell;
            //if (cell.Value != null)
            //{
            //    Form1.MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue = cell.Value.ToString();
            //}
        }

        private void dataGridView1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyCode == Keys.Return)
            //{

            //    DataGridViewCell cell = ((DataGridView)sender).CurrentCell;
            //    //Debug.WriteLine("Left Cell " + cell.RowIndex + "," + cell.ColumnIndex);
            //    if (cell.Value == null) return;
            //    MySpreadsheet.getCell(cell.RowIndex, cell.ColumnIndex).TextValue = cell.Value.ToString();
            //}
        }

    }
}
