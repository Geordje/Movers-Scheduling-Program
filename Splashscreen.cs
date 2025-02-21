using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.TimeZoneInfo;

namespace Movers_Scheduling_Program
{
    public partial class Splashscreen : Form
    {
        private int dotCount = 0;

        public Splashscreen()
        {
            InitializeComponent();
            animationTimer.Start();
            doneTimer.Start();
        }


        private void animationTimer_Tick(object sender, EventArgs e)
        {
            dotCount = (dotCount + 1) % 4;
            loadingLabel.Text = "Loading" + new string('.', dotCount);
        }

        private void doneTimer_Tick(object sender, EventArgs e)
        {
            doneTimer.Stop();
            animationTimer.Stop();
            LogIn loginForm = new LogIn();
            loginForm.Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
