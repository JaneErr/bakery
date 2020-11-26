using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace bakery
{
    public partial class Form1 : Form
    {

        private SqlConnection sqlConnection = null;

        private SqlCommandBuilder sqlBuilder = null;

        private SqlDataAdapter sqlDataAdapter = null;

        private DataSet dataSet = null;

        private string currentTable = "Orders";

        private bool newRowAdding = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            try
            {
                sqlDataAdapter = new SqlDataAdapter("Select *, 'Delete' AS [Commands] FROM " + currentTable, sqlConnection);

                sqlBuilder = new SqlCommandBuilder(sqlDataAdapter);

                sqlBuilder.GetInsertCommand();
                sqlBuilder.GetDeleteCommand();
                sqlBuilder.GetUpdateCommand();

                dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet, currentTable);

                dataGridView1.DataSource = dataSet.Tables[currentTable];

                int c = dataGridView1.ColumnCount - 1;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[c, i] = linkCell;
                }

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadData()
        {
            try
            {
                dataSet.Tables[currentTable].Clear();

                sqlDataAdapter.Fill(dataSet, currentTable);

                dataGridView1.DataSource = dataSet.Tables[currentTable];

                int c = dataGridView1.ColumnCount - 1;

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[c, i] = linkCell;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var projectPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
            AppDomain.CurrentDomain.SetData("DataDirectory", projectPath);
            SqlConnectionStringBuilder stringBuilder = new SqlConnectionStringBuilder();
            stringBuilder["Data Source"] = "(LocalDB)\\MSSQLLocalDB";
            stringBuilder["AttachDbFilename"] = projectPath + "\\data\\bakerydb.mdf";
            stringBuilder["Integrated Security"] = true;
            sqlConnection = new SqlConnection(stringBuilder.ConnectionString);
            sqlConnection.Open();
            LoadData();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage.Name == "tabPage1") 
            {
                currentTable = "Orders";
            }
            else if (e.TabPage.Name == "tabPage2")
            {
                currentTable = "Contracts";
            }
            else if (e.TabPage.Name == "tabPage3")
            {
                currentTable = "Providers";
            }
            else if (e.TabPage.Name == "tabPage4")
            {
                currentTable = "Customers";
            }
            else if (e.TabPage.Name == "tabPage5")
            {
                currentTable = "Employees";
            }
            else if (e.TabPage.Name == "tabPage6")
            {
                currentTable = "Production";
            }
            else if (e.TabPage.Name == "tabPage7")
            {
                currentTable = "Materials";
            }
            LoadData();
        }

        private void tabControl1_Deselected(object sender, TabControlEventArgs e)
        {
            dataGridView1.DataSource = null;
        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == dataGridView1.ColumnCount - 1)
                {
                    string task = dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.ColumnCount - 1].Value.ToString();

                    if (task == "Delete")
                    {
                        if (MessageBox.Show("Удалить эту строку?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            dataGridView1.Rows.RemoveAt(e.RowIndex);

                            dataSet.Tables[currentTable].Rows[e.RowIndex].Delete();

                            sqlDataAdapter.Update(dataSet, currentTable);
                        }
                    }
                    else if (task == "Insert")
                    {
                        int rowIndex = dataGridView1.RowCount - 2;

                        DataRow row = dataSet.Tables[currentTable].NewRow();

                        for (int i = 0; i < dataGridView1.ColumnCount - 1; i++)
                        {
                            var columnName = dataGridView1.Columns[i].Name;

                            row[columnName] = dataGridView1.Rows[rowIndex].Cells[columnName].Value;
                        }

                        dataSet.Tables[currentTable].Rows.Add(row);

                        dataSet.Tables[currentTable].Rows.RemoveAt(dataSet.Tables[currentTable].Rows.Count - 1);

                        dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 2);

                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "Delete";

                        sqlDataAdapter.Update(dataSet, currentTable);

                        newRowAdding = false;
                    }
                    else if (task == "Update")
                    {
                        for (int i = 0; i < dataGridView1.ColumnCount - 1; i++)
                        {
                            var columnName = dataGridView1.Columns[i].Name;

                            dataSet.Tables[currentTable].Rows[e.RowIndex][columnName] = dataGridView1.Rows[e.RowIndex].Cells[columnName].Value;
                        }

                        sqlDataAdapter.Update(dataSet, currentTable);

                        dataGridView1.Rows[e.RowIndex].Cells[dataGridView1.ColumnCount - 1].Value = "Delete";
                    }

                    ReloadData();
                }

            } 
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                if (newRowAdding == false)
                {
                    newRowAdding = true;

                    int lastRow = dataGridView1.RowCount - 2;

                    int lastColumn = dataGridView1.ColumnCount - 1;

                    DataGridViewRow row = dataGridView1.Rows[lastRow];

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();

                    dataGridView1[lastColumn, lastRow] = linkCell;

                    row.Cells["Commands"].Value = "Insert";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (newRowAdding == false)
                {
                    int rowIndex = dataGridView1.SelectedCells[0].RowIndex;

                    DataGridViewRow editingRow = dataGridView1.Rows[rowIndex];

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();

                    dataGridView1[dataGridView1.ColumnCount - 1, rowIndex] = linkCell;

                    editingRow.Cells["Commands"].Value = "Update";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(Int_Column_KeyPress);

            e.Control.KeyPress -= new KeyPressEventHandler(Double_Column_KeyPress);

            e.Control.KeyPress -= new KeyPressEventHandler(DateTime_Column_KeyPress);

            object cellValue = dataGridView1.Rows[0].Cells[dataGridView1.CurrentCell.ColumnIndex].Value;

            TextBox textBox = e.Control as TextBox;

            if (cellValue is int || cellValue is decimal)
            {
                textBox.KeyPress += new KeyPressEventHandler(Int_Column_KeyPress);
            }
            else if (cellValue is DateTime)
            {
                textBox.KeyPress += new KeyPressEventHandler(DateTime_Column_KeyPress);
            }
            else if (cellValue is double)
            {
                textBox.KeyPress += new KeyPressEventHandler(Double_Column_KeyPress);
            }    
        }

        private void Int_Column_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }    

        }

        private void Double_Column_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != ',' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void DateTime_Column_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) && e.KeyChar != '.' && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
