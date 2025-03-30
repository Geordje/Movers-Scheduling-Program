using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



namespace Movers_Scheduling_Program
{
    public partial class Home : Form
    {
        private Cloudinary cloudinary;
        private bool isNext;
        private int timeValue;
        private string timeUnit;
        public int selectedJobId;

        public Home()
        {
            InitializeComponent();
            InitializeCloudinary();
            SetWelcomeMessage();
            LoadProfilePicture();
            InitializeVariables();
            AttachEventHandlers();
        }

        private void InitializeCloudinary()
        {
            Account account = new Account("dvkxm0kra", "345524937891459", "aYs2jg_r0hLSzQ2m8UD_7syEGxU");
            cloudinary = new Cloudinary(account);
        }

        private void SetWelcomeMessage()
        {
            label1.Text = "Welcome " + SessionManager.FirstName + " " + SessionManager.SecondName;
        }

        private void LoadProfilePicture()
        {
            try
            {
                string imageUrl = cloudinary.Api.UrlImgUp.BuildUrl($"{SessionManager.Username}.jpg");
                using (WebClient webClient = new WebClient())
                {
                    byte[] imageBytes = webClient.DownloadData(imageUrl);
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        Image image = Image.FromStream(ms);
                        bunifuButton8.IdleIconLeftImage = new Bitmap(image);
                        bunifuButton8.OnPressedState.IconLeftImage = new Bitmap(image);
                        bunifuButton8.OnIdleState.IconLeftImage = new Bitmap(image);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private void InitializeVariables()
        {
            isNext = true;
            timeValue = 4;
            timeUnit = "days";
        }

        private void AttachEventHandlers()
        {
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            AssignedEmployees.CellContentClick += AssignedEmployees_CellContentClick;
        }

        private void SetProfilePicture(Cloudinary cloudinary)
        {
            string imageUrl = cloudinary.Api.UrlImgUp.BuildUrl("profilepictures/" + SessionManager.Username + ".jpg");
            bunifuButton8.IdleIconLeftImage = Image.FromStream(new System.Net.WebClient().OpenRead(imageUrl));
        }

        private void rjButton1_Click(object sender, EventArgs e) { }

        private void AssignedEmployees_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && AssignedEmployees.Columns[e.ColumnIndex].Name == "ProfileButton")
            {
                var cellValue = AssignedEmployees.Rows[e.RowIndex].Cells["Username"].Value;
                string username = cellValue.ToString();
                DisplayProfile(username);
                
               
            }
        }

        private void LoadEmployees()
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT Username, 
                           CONCAT(FirstName, ' ', SecondName) AS FullName, 
                           Role 
                    FROM Staff";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (!AssignedEmployees.Columns.Contains("ProfileButton"))
                {
                    DataGridViewButtonColumn profileButtonColumn = new DataGridViewButtonColumn
                    {
                        Name = "ProfileButton",
                        HeaderText = "Profile",
                        Text = "View Profile",
                        UseColumnTextForButtonValue = true
                    };
                    AssignedEmployees.Columns.Add(profileButtonColumn);
                }
                AssignedEmployees.DataSource = dataTable;
                AssignedEmployees.BackgroundColor = Color.White;
                AssignedEmployees.RowTemplate.Height = 40;
                AssignedEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                AssignedEmployees.Columns["ProfileButton"].DisplayIndex = AssignedEmployees.Columns.Count - 1;
            }
        }

        private void DisplayProfile(string username)
        {
            SessionManager.Username = username;
            Page.SetPage(prof);
            pageName.Text = "Profile";
            LoadProfileInformation(username);
        }

        private void rjButton2_Click(object sender, EventArgs e)
        {
            string tenseValue = tense.SelectedItem.ToString().ToLower();
            int amountValue = (int)amount.Value;
            string unitValue = unit.SelectedItem.ToString().ToLower();
            isNext = tenseValue == "next";
            timeValue = amountValue;
            timeUnit = unitValue;
            switch (timeUnit)
            {
                case "hours":
                    timeUnit = "HOUR";
                    break;
                case "days":
                    timeUnit = "DAY";
                    break;
                case "weeks":
                    timeUnit = "WEEK";
                    break;
                case "months":
                    timeUnit = "MONTH";
                    break;
                default:
                    throw new ArgumentException("Invalid time unit");
            }
            LoadJobs();
        }

        private void LoadJobs()
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string dateCondition = isNext
                    ? $"CURDATE() AND DATE_ADD(CURDATE(), INTERVAL {timeValue} {timeUnit})"
                    : $"DATE_SUB(CURDATE(), INTERVAL {timeValue} {timeUnit}) AND CURDATE()";
                string query = $@"
                        SELECT Job.idJob, 
                               CONCAT(Customer.CustomerForename, ' ', Customer.CustomerSurname) AS CustomerFullName, 
                               buildingsA.Address AS BuildingAAddress, 
                               buildingsB.Address AS BuildingBAddress, 
                               JobTimeInstance.DayOccurance AS TimeInstance
                        FROM Job
                        JOIN JobTimeInstance ON Job.idJob = JobTimeInstance.JobID
                        JOIN Building AS buildingsA ON Job.BuildingA = buildingsA.BuildingID
                        JOIN Building AS buildingsB ON Job.BuildingB = buildingsB.BuildingID
                        JOIN Customer ON Job.CustomerID = Customer.idCustomer
                        WHERE JobTimeInstance.DayOccurance BETWEEN {dateCondition}";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (!dataGridView1.Columns.Contains("DetailsButton"))
                {
                    DataGridViewButtonColumn detailsButtonColumn = new DataGridViewButtonColumn
                    {
                        Name = "DetailsButton",
                        HeaderText = "Details",
                        Text = "View Details",
                        UseColumnTextForButtonValue = true
                    };
                    dataGridView1.Columns.Add(detailsButtonColumn);
                }
                dataGridView1.DataSource = dataTable;
                dataGridView1.BackgroundColor = Color.White;
                dataGridView1.RowTemplate.Height = 40;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView1.Columns["DetailsButton"].DisplayIndex = dataGridView1.Columns.Count - 1;
            }
        }

        private void LoadProfileInformation(string username)
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                        SELECT Username, 
                               CONCAT(FirstName, ' ', SecondName) AS FullName, 
                               Email, 
                               Role
                        FROM Staff 
                        WHERE Username = @Username";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    FullName.Text = reader["FullName"].ToString();
                    Username.Text = reader["Username"].ToString();
                    Email.Text = reader["Email"].ToString();
                    Role.Text = reader["Role"].ToString();
                    AccessLevel.Text = (reader["Role"].ToString().ToLower() == "it admin" || reader["Role"].ToString().ToLower() == "manager") ? "Higher" : "typical";
                }
                reader.Close();
                LoadProfilePicture(username);
            }
        }

        private void LoadProfilePicture(string username)
        {
            string imageUrl = cloudinary.Api.UrlImgUp.BuildUrl("profilepictures/" + username + ".jpg");
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    byte[] imageBytes = webClient.DownloadData(imageUrl);
                    using (var ms = new System.IO.MemoryStream(imageBytes))
                    {
                        Image image = Image.FromStream(ms);
                        userpfp.Image = new Bitmap(image);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) { }

        private void Home_Load(object sender, EventArgs e) { }

        private void pictureBox12_Click(object sender, EventArgs e) { }

        private void pictureBox4_Click(object sender, EventArgs e) { }

        private void Recents_Click(object sender, EventArgs e)
        {
            Page.SetPage(Recemts);
            pageName.Text = "Recents";
        }

        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            Page.SetPage(Availability);
            pageName.Text = "Availability";
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            Page.SetPage(Employees);
            pageName.Text = "Employees";
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            Page.SetPage(Customers);
            pageName.Text = "Customers";
        }

        private void bunifuButton4_Click(object sender, EventArgs e)
        {
            Page.SetPage(Date);
            pageName.Text = "Date Filter";
        }

        private void bunifuButton5_Click(object sender, EventArgs e)
        {
            Page.SetPage(Customer);
            pageName.Text = "Customer Filter";
        }

        private void bunifuButton6_Click(object sender, EventArgs e)
        {
            Page.SetPage(Details);
            pageName.Text = "Details Filter";
        }

        private void bunifuButton7_Click(object sender, EventArgs e)
        {
            Page.SetPage(Advanced);
            pageName.Text = "Advanced Filter";
        }

        private void bunifuButton8_Click(object sender, EventArgs e)
        {
            Page.SetPage(prof);
            pageName.Text = "My Profile";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["DetailsButton"].Index && e.RowIndex >= 0)
            {
                selectedJobId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["idJob"].Value);
                UpdateJobDetails(selectedJobId);
                Page.SetPage(JobInfo);
            }
        }

        private void UpdateJobDetails(int jobId)
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"
                        SELECT Job.idJob, 
                               CONCAT(Customer.CustomerForename, ' ', Customer.CustomerSurname) AS CustomerFullName, 
                               Customer.CustomerPhoneNo, 
                               Customer.CustomerEmail, 
                               buildingsA.Address AS BuildingAAddress, 
                               buildingsA.BuildingType AS BuildingABuildingType, 
                               buildingsA.NoOfRooms AS BuildingARooms, 
                               buildingsA.Postcode AS BuildingAPostcode, 
                               buildingsB.Address AS BuildingBAddress, 
                               buildingsB.BuildingType AS BuildingBType, 
                               buildingsB.NoOfRooms AS BuildingBRooms, 
                               buildingsB.Postcode AS BuildingBPostcode, 
                               DATE_FORMAT(JobTimeInstance.DayOccurance, '%Y-%m-%d') AS TimeInstance, 
                               Job.Fragiles, 
                               Job.Packed, 
                               Job.Disassembly, 
                               Job.Reassembly, 
                               Job.JobNotes,
                               Job.NoOfBoxes,
                               Job.Packed,
                               Job.Disassembly,
                               Job.Reassembly,
                               (SELECT COUNT(*) FROM JobTimeInstance WHERE JobTimeInstance.JobID = Job.idJob) AS NoOfDays
                        FROM Job
                        JOIN JobTimeInstance ON Job.idJob = JobTimeInstance.JobID
                        JOIN Building AS buildingsA ON Job.BuildingA = buildingsA.BuildingID
                        JOIN Building AS buildingsB ON Job.BuildingB = buildingsB.BuildingID
                        JOIN Customer ON Job.CustomerID = Customer.idCustomer
                        WHERE Job.idJob = {jobId}";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    CustomerName.Text = reader["CustomerFullName"].ToString();
                    CustomerPhoneNumber.Text = reader["CustomerPhoneNo"].ToString();
                    CustomerEmail.Text = reader["CustomerEmail"].ToString();
                    BuildingAAddress.Text = reader["BuildingAAddress"].ToString();
                    BuildingABuildingType.Text = reader["BuildingABuildingType"].ToString();
                    BuildingARooms.Text = reader["BuildingARooms"].ToString() + " rooms";
                    label3.Text = reader["BuildingAPostcode"].ToString();
                    BuildingBAddress.Text = reader["BuildingBAddress"].ToString();
                    BuildingBType.Text = reader["BuildingBType"].ToString();
                    BuildingBRooms.Text = reader["BuildingBRooms"].ToString() + " rooms";
                    BuildingBPostcode.Text = reader["BuildingBPostcode"].ToString();
                    DateStarting.Text = reader["TimeInstance"].ToString();
                    Fragiles.Text = Convert.ToBoolean(reader["Fragiles"]) ? "Fragiles" : "None";
                    JobNotes.Text = string.IsNullOrEmpty(reader["JobNotes"].ToString()) ? "None" : reader["JobNotes"].ToString();
                    NoOfBoxes.Text = reader["NoOfBoxes"].ToString() + " Boxes";
                    BuildingAPacking.Text = Convert.ToBoolean(reader["Packed"]) ? "Packed" : "Not Packed";
                    BuildingADissasembly.Text = Convert.ToBoolean(reader["Disassembly"]) ? "Disassembly" : "No Disassembly";
                    BuildingBReassembly.Text = Convert.ToBoolean(reader["Reassembly"]) ? "Reassembly" : "No Reassembly";
                    NoOfDays.Text = reader["NoOfDays"].ToString() + " Days";
                }
                reader.Close();
                LoadTimeInstances(jobId);
                LoadAssignedStaff(jobId);
            }
        }

        private void LoadTimeInstances(int jobId)
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"
                        SELECT DATE_FORMAT(DayOccurance, '%Y-%m-%d') AS DateOnly
                        FROM JobTimeInstance
                        WHERE JobID = {jobId}";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                StringBuilder builder = new StringBuilder();
                while (reader.Read())
                {
                    builder.AppendLine(reader["DateOnly"].ToString());
                }
                TimeInstances.Text = builder.ToString();
                reader.Close();
            }
        }

        private void LoadAssignedStaff(int jobId)
        {
            string connectionString = "Server=bf0aazuktscfjlzc79lq-mysql.services.clever-cloud.com;Database=bf0aazuktscfjlzc79lq;User Id=uwxdwzdrkyehbgba;Password=sKTDtXyMidMIIfXhi8yl;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"
                        SELECT CONCAT(Staff.FirstName, ' ', Staff.SecondName) AS StaffFullName, Staff.Role, Staff.Username
                        FROM StaffAssign
                        JOIN Staff ON StaffAssign.StaffUsername = Staff.Username
                        WHERE StaffAssign.JobID = {jobId}";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                DataTable table = new DataTable();
                table.Load(reader);
                if (!AssignedEmployees.Columns.Contains("ProfileButton"))
                {
                    DataGridViewButtonColumn profileButtonColumn = new DataGridViewButtonColumn
                    {
                        Name = "ProfileButton",
                        HeaderText = "Profile",
                        Text = "View Profile",
                        UseColumnTextForButtonValue = true
                    };
                    AssignedEmployees.Columns.Add(profileButtonColumn);
                }
                AssignedEmployees.DataSource = table;
                AssignedEmployees.BackgroundColor = Color.White;
                AssignedEmployees.RowTemplate.Height = 40;
                AssignedEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                AssignedEmployees.Columns["ProfileButton"].DisplayIndex = AssignedEmployees.Columns.Count - 1;
            }
        }

        private void Username_Click(object sender, EventArgs e) { }
    }
}
