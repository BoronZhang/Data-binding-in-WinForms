using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Practice5
{
    public partial class Form2 : Form
    {
        Form1 form1;
        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form1 form)
        {
            InitializeComponent();
            form1 = form;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                form1.tableName = textBox1.Text;
            }
            this.Close();
        }
    }
}
