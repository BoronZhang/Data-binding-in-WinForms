using Oracle.ManagedDataAccess.Client;
using Practice5.Properties;
using System;
using System.Data;
using System.Drawing;
using System.IO;
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


        public Form1()
        {
            InitializeComponent();
            this.chart1.Palette = ChartColorPalette.SeaGreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = bindingSource1;
            bindingNavigator1.BindingSource = bindingSource1;
            dataGridView1.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(BindingVisible);

         
            GetData("select * from PLANETS");

        }

        private void BindingVisible(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView1.Columns["IMAGE"].Visible = false;

            // Disable sorting for the DataGridView.
            foreach (DataGridViewColumn i in dataGridView1.Columns)
            {
                i.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            dataGridView1.AutoResizeColumns();
        }

        private void GetData(string selectCommand)
        {
            try
            {
                dataAdapter = new OracleDataAdapter(selectCommand, connectionStr);

                OracleCommandBuilder commandBuilder = new OracleCommandBuilder(dataAdapter);

                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                table.TableName = "PLANETS";
                bindingSource1.DataSource = table;

                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);


                chart1.Visible = true;
                chart1.DataSource = bindingSource1;
                chart1.Series[0].XValueMember = "NAME";

                chart1.Series[0].YValueMembers = "DIAMETER KM";
                bindingSource1.CurrentChanged += (o, e) => chart1.DataBind();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow.Cells[dataGridView1.Columns["IMAGE"].Index].Value == DBNull.Value)
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
                //MessageBox.Show(ex.ToString());
            }
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            GetData(dataAdapter.SelectCommand.CommandText);
        }

        private void commitButton_Click(object sender, EventArgs e)
        {
            dataAdapter.Update((DataTable)bindingSource1.DataSource);
        }

        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {


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
            string filename = saveFileDialog1.FileName;
           // string tableName = 

            Stream sr = new FileStream(filename, FileMode.Create);

            XmlSerializer xmlSer = new XmlSerializer(typeof(DataTable));

            xmlSer.Serialize(sr, (DataTable)bindingSource1.DataSource);
            sr.Close();

            MessageBox.Show("Файл сохранен");
        }

        private void loadButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "XML files(*.xml)|*.xml|All files(*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

         
            string filename = openFileDialog1.FileName;
            DataTable table = new DataTable(filename);

            table.ReadXmlSchema(filename);
            table.ReadXml(filename);
          
            bindingSource1.DataSource = table;

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

        }
    }
}
