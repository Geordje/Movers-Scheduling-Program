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
                string connectionString = "Server=34.142.30.150;Database=movers;User Id=ApplicationUser;Password=averygoodpassword123;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Staff WHERE Username = @Username AND Password = @Password";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Username", UsernameField.Text);
                    command.Parameters.AddWithValue("@Password", PasswordField.Text);
                    int userCount = Convert.ToInt32(command.ExecuteScalar());   
                    if (userCount == 1)
                    {
                        MessageBox.Show("Login successful!");
                        Home home = new Home();
                        home.Username = UsernameField.Text;
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
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }
    
    }
}
