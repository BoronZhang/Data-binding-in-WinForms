using Oracle.ManagedDataAccess.Client;
using Practice5.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;

namespace Practice5
{
    public partial class Form1 : Form
    {

        BindingSource bindingSource1 = new BindingSource();
        OracleDataAdapter dataAdapter = new OracleDataAdapter();

        string connectionStr = "DATA SOURCE=(DESCRIPTION =(ADDRESS = (PROTOCOL = TCP)(HOST = oraclebi.avalon.ru)(PORT = 1521))(CONNECT_DATA =(SID = orcl12)(SERVER = DEDICATED)(SERVICE_NAME = orcl12)));PASSWORD=zlaers;PERSIST SECURITY INFO=True;USER ID=zlaers";
        public string tableName = "PLANETS";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = bindingSource1;
            bindingNavigator1.BindingSource = bindingSource1;
            dataGridView1.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(BindingVisible);


            GetData("select * from " + tableName);

            chart1.Visible = true;
            chart1.DataSource = bindingSource1;

            chart1.Series[0].XValueMember = dataGridView1.Columns[0].HeaderText;
            chart1.Series[0].YValueMembers = dataGridView1.Columns[1].HeaderText;

            // chart1.DataBind();

            pictureBox1.Size = Resources.UnknownPlanet.Size;
            pictureBox1.Image = Resources.UnknownPlanet;
            pictureBox1.Invalidate();

        }

        private void BindingVisible(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dataGridView1.Columns.Contains("IMAGE"))
            {
                dataGridView1.Columns["IMAGE"].Visible = false;
            }
            // Disable sorting for the DataGridView.
            foreach (DataGridViewColumn i in dataGridView1.Columns)
            {
                i.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView1.AutoResizeColumns();
        }

        public void GetData(string selectCommand)
        {
            try
            {
                dataAdapter = new OracleDataAdapter(selectCommand, connectionStr);

                OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);

                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                table.TableName = tableName;
                bindingSource1.DataSource = table;

                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                propertyGrid1.SelectedObject = bindingSource1.Current;
                if (!dataGridView1.Columns.Contains("IMAGE") || dataGridView1.CurrentRow.Cells[dataGridView1.Columns["IMAGE"].Index].Value == DBNull.Value)
                {
                    pictureBox1.Size = Resources.UnknownPlanet.Size;
                    pictureBox1.Image = Resources.UnknownPlanet;
                    pictureBox1.Invalidate();
                    return;
                }

                Byte[] byteBLOB = (Byte[])(dataGridView1.CurrentRow.Cells[dataGridView1.Columns["IMAGE"].Index].Value);
                MemoryStream strBLOBData = new MemoryStream(byteBLOB);
                pictureBox1.Image = Image.FromStream(strBLOBData);
                pictureBox1.Size = Image.FromStream(strBLOBData).Size;
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!dataGridView1.Columns.Contains("IMAGE"))
            {
                MessageBox.Show("У этой таблицы нет столбца с изображением!");
                return;
            }
            OpenFileDialog open_dialog = new OpenFileDialog(); //создание диалогового окна для выбора файла
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*"; //формат загружаемого файла

            //int c = bindingSource1.DataSource.Tables["PLANETS"].Rows.Count;
            int c = dataGridView1.Rows.Count;
            ImageConverter _imageConverter = new ImageConverter();


            if (open_dialog.ShowDialog() == DialogResult.OK) //если в окне была нажата кнопка "ОК"
            {
                try
                {

                    Image image = new Bitmap(open_dialog.FileName);
                    byte[] xByte = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));
                    dataGridView1.CurrentRow.Cells[2].Value = xByte;

                    MemoryStream ms = new MemoryStream((byte[])dataGridView1.CurrentRow.Cells[2].Value);

                    pictureBox1.Image = Image.FromStream(ms);
                    pictureBox1.Size = Image.FromStream(ms).Size;
                    pictureBox1.Invalidate();
                    ms.Close();
                }
                catch
                {
                    DialogResult rezult = MessageBox.Show("Невозможно открыть выбранный файл",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "XML files(*.xml)|*.xml|All files(*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            string fullpath = saveFileDialog1.FileName;
            string filename = Path.GetFileNameWithoutExtension(fullpath);

            DataTable tmpTable = (DataTable)bindingSource1.DataSource;
            tmpTable.TableName = filename;

            //Stream sr = new FileStream(filename, FileMode.Create);
            Stream sr = new FileStream(fullpath, FileMode.Create);
            XmlSerializer xmlSer = new XmlSerializer(typeof(DataTable));

            xmlSer.Serialize(sr, tmpTable);
            sr.Close();

            MessageBox.Show("Файл сохранен");
        }

        private void loadButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "XML files(*.xml)|*.xml|All files(*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;


            string fullpath = openFileDialog1.FileName;
            string filename = Path.GetFileNameWithoutExtension(fullpath);

            DataTable table = new DataTable(filename);

            table.ReadXml(fullpath);
            bindingSource1.DataSource = table;

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this);
            form2.Show();
            form2.FormClosed += new FormClosedEventHandler(form2Closed);
        }

        private void form2Closed(object sender, FormClosedEventArgs e)
        {
            GetData("select * from " + tableName);
            //chart1.DataSource = bindingSource1;

            chart1.Series[0].XValueMember = dataGridView1.Columns[0].HeaderText;
            chart1.Series[0].YValueMembers = dataGridView1.Columns[1].HeaderText;
        }
        private void reloadButton_Click(object sender, EventArgs e)
        {
            GetData(dataAdapter.SelectCommand.CommandText);
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            dataAdapter.Update((DataTable)bindingSource1.DataSource);
        }



        private void panelX_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menuStripX = new ContextMenuStrip();
                panelX.ContextMenuStrip = menuStripX;
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText == "IMAGE")
                        continue;

                    ToolStripMenuItem menuItem = new ToolStripMenuItem(dataGridView1.Columns[i].HeaderText);
                    menuItem.Click += new EventHandler(menuItemX_Click);
                    menuStripX.Items.Add(menuItem);

                }
            }
        }


        private void panelY_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menuStripY = new ContextMenuStrip();
                panelY.ContextMenuStrip = menuStripY;
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    if (dataGridView1.Columns[i].HeaderText == "IMAGE" || dataGridView1.Columns[i].ValueType == typeof(string))
                        continue;

                    ToolStripMenuItem menuItem = new ToolStripMenuItem(dataGridView1.Columns[i].HeaderText);
                    menuItem.Click += new EventHandler(menuItemY_Click);
                    menuStripY.Items.Add(menuItem);
                }
            }
        }


        private void menuItemX_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem textItem = sender as ToolStripMenuItem;

            chart1.Visible = true;
            chart1.DataSource = bindingSource1;

            chart1.Series[0].XValueMember = textItem.Text;
            chart1.Invalidate();
            bindingSource1.CurrentChanged += (o, ev) => chart1.DataBind();

        }
        private void menuItemY_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem textItem = sender as ToolStripMenuItem;

            chart1.Visible = true;
            chart1.DataSource = bindingSource1;

            chart1.Series[0].YValueMembers = textItem.Text;
            chart1.Invalidate();
            bindingSource1.CurrentChanged += (o, ev) => chart1.DataBind();
        }
        private void search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                for (int i = 0; i < dataGridView1.RowCount - 2; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if (dataGridView1[i, j].FormattedValue.ToString().
                            Contains(search.Text.Trim()))
                        {
                            dataGridView1.CurrentCell = dataGridView1[i, j];
                            return;
                        }
                    }
                }
            }
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menuStrip = new ContextMenuStrip();

                ToolStripMenuItem pie = new ToolStripMenuItem("Круговая диаграмма");
                ToolStripMenuItem normal = new ToolStripMenuItem("Столбчатая диаграмма");
                menuStrip.Items.AddRange(new[] { pie, normal });
                pie.Click += pieDiagram;
                normal.Click += normalDiagram;

                chart1.ContextMenuStrip = menuStrip;
            }
        }

        private void normalDiagram(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void pieDiagram(object sender, EventArgs e)
        {

            Dictionary<string, int> dict = new Dictionary<string, int>();
            //уникальные значения департаментов по x
            List<string> list = dataGridView1.Rows
                        .OfType<DataGridViewRow>()
                        .Where(x => x.Cells[10].Value != null)
                        .Select(x => x.Cells[10].Value.ToString())
                        .Distinct()
                        .ToList();
            

            List<string> names = new List<string>();
            foreach (var l in list)
                dict.Add(l, 0);
            for(int i =0; i<dataGridView1.Rows.Count-1; i++)
            {
                if(dict.ContainsKey((string)dataGridView1.Rows[i].Cells[10].FormattedValue))
                {
                    dict[(string)dataGridView1.Rows[i].Cells[10].FormattedValue]++;
                }
            }
            //dataGridView1.
            chart1.Series.Clear();
            chart1.Series.Add("Pie");
            chart1.Series["Pie"].ChartType = SeriesChartType.Pie;
            chart1.Series["Pie"].IsValueShownAsLabel = true;

            for (int i = 1; i < dataGridView1.Rows.Count - 1; i++)
            {
                chart1.Series["Pie"].Points.AddXY(dict.Values,
                               dict.Keys);
            }
            // chart1.Series["Pie"].XValueMember = dataGridView1.Columns[0].HeaderText;
            // chart1.Series["Pie"].Points.DataBindXY(dict.Keys, dict.Values);
            chart1.Invalidate();
            bindingSource1.CurrentChanged += (o, ev) => chart1.DataBind();
        }
    }
}
