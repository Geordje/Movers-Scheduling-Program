using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Movers_Scheduling_Program
{
    public partial class LogIn : Form
    {
        private int failedAttempts = 0;
        private const int maxFailedAttempts = 5;

        public LogIn()
        {
            InitializeComponent();
        }

        private void LogIn_MouseDown(object sender, MouseEventArgs e)
        {
            PasswordField.PasswordChar = '\0';
            Show.Image = Properties.Resources.eye;
        }
        private void Show_MouseLeave(object sender, EventArgs e)
        {
            PasswordField.PasswordChar = '⛟';
            Show.Image = Properties.Resources.irisless;

        }
        private void Show_MouseUp(object sender, MouseEventArgs e)
        {
            PasswordField.PasswordChar = '⛟';
            Show.Image = Properties.Resources.irisless;
        }

        private void firstLabel_Click(object sender, EventArgs e)
        {
            helpPopUp popUp = new helpPopUp();
            popUp.ShowDialog();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Username, FirstName, SecondName, Email, PhoneNo, Role FROM Staff WHERE Username = @Username AND Password = @Password";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", UsernameField.Text);
                    command.Parameters.AddWithValue("@Password", PasswordField.Text);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string username = reader.GetString("Username");
                            string firstName = reader.GetString("FirstName");
                            string secondName = reader.GetString("SecondName");
                            string email = reader.GetString("Email");
                            string phoneNo = reader.GetString("PhoneNo");
                            string role = reader.GetString("Role");

                            SessionManager.InitializeSession(username, firstName, secondName, email, phoneNo, role);

                            Home home = new Home();
                            this.Hide();
                            home.ShowDialog();
                            this.Close();
                        }
                        else
                        {
                            failedAttempts++;
                            MessageBox.Show("Invalid username or password.");

                            if (failedAttempts >= maxFailedAttempts)
                            {
                                MessageBox.Show("Maximum failed attempts reached. The application will now close.");
                                Application.Exit();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Home home = new Home();
            this.Hide();
            home.ShowDialog();
            this.Close();
        }
    }
}
