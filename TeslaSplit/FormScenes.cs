using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeslaSplit
{
    public partial class FormScenes : Form
    {
        public FormScenes(DataTable dt)
        {
            InitializeComponent();
            dataGridView1.DataSource = dt;
        }
    }
}
