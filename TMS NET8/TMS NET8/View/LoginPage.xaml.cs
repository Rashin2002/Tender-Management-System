using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using TMS_NET8.View.Financial_Manager;
using TMS_NET8.View.Stock_Manager;
using TMS_NET8.View.Supply_Chain_Manager;
using TMS_NET8.View.Supply_Chain_Team;
using TMS_NET8.View.Tender_Manager;
using TMS_NET8.View.Tender_Team;
using TMS_NET8.View.Administrative_Assistant;
using TMS_NET8.View.Project_Manager;

namespace TMS_NET8.View
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        // Removed 'private readonly object lblMessage;' as it should be defined in XAML

        public LoginPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // Assuming txtEmail is a TextBox and txtPassword is a PasswordBox
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();

            // FIX 1: The 'lblMessage does not exist' error suggests it's missing from the XAML file.
            // Assuming it is a Label control with the name 'lblMessage'.
            // If it is NOT a Label, change '.Content' to the appropriate property (e.g., .Text for TextBlock).

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both email and password.";
                return;
            }

            try
            {
                // Assuming Database.DBConnection.CreateConnection() works and returns an open connection
                using (SqlConnection con = Database.DBConnection.CreateConnection())
                {
                    if (con.State != System.Data.ConnectionState.Open)
                    {
                        con.Open();
                    }

                    string query = @"SELECT emp_id, emp_name 
                                     FROM Employee 
                                     WHERE emp_email = @Email AND emp_password = @Password";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // FIX 2: Possible null reference return/argument warnings are fixed here:
                        // Use a null-coalescing operator (??) to ensure a non-null string is passed, 
                        // as DbDataReader can return DBNull or null for values.
                        string empId = reader["emp_id"]?.ToString() ?? string.Empty;
                        string empName = reader["emp_name"]?.ToString() ?? "User"; // Default name for safety
                        reader.Close();

                        // Pass the connection which is still open
                        string? role = GetEmployeeRole(empId, con);

                        if (role != null)
                        {
                            lblMessage.Text = ""; // Clear message

                            MessageBox.Show(
                                $"Login Successful!\n\nWelcome, {empName}.",
                                "Success",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            NavigateToDashboard(role, empId ,empName);
                        }
                        else
                        {
                            lblMessage.Text = "No assigned role found for this employee.";
                        }
                    }
                    else
                    {
                        lblMessage.Text = "Invalid email or password.";
                    }
                }
            }
            catch (SqlException sqlex)
            {
                lblMessage.Text = "Database Error: " + sqlex.Message;
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
            }
        }

        // Removed 'Possible null reference argument' warning by removing '?' from string empId parameter
        private string? GetEmployeeRole(string empId, SqlConnection con)
        {
            string[] tables = { "TenderTeam", "TenderManager", "StockManager", "SupplyChainManager", "SupplyChainTeam", "FinancialManager", "ProjectManager", "AdministrativeAssistant"};

            foreach (var table in tables)
            {
                string checkQuery = $"SELECT COUNT(*) FROM {table} WHERE emp_id = @EmpId";

                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@EmpId", empId);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (count > 0)
                    return table; // return table name as role
            }

            return null;
        }

        private void NavigateToDashboard(string role, string empId, string empName)
        {
            Window? dashboard = null;

            switch (role)
            {
                case "TenderTeam":
                    dashboard = new TenderTeamDashboard(empId, empName);
                    break;
                case "TenderManager":
                    dashboard = new TenderManagerDashboard(empId, empName);
                    break;

                case "StockManager":
                    dashboard = new StockManagerDashboard(empId, empName);
                    break;

                case "SupplyChainManager":
                    dashboard = new SupplyChainManagerDashboard(empId, empName);
                    break;

                case "SupplyChainTeam":
                    dashboard = new SupplyChainTeamDashboard(empId, empName);
                    break;

                case "FinancialManager":
                    dashboard = new FinancialManagerDashboard(empId, empName);
                    break;

                case "ProjectManager":
                    dashboard = new ProjectManagerDashboard(empId, empName);
                    break;

                case "AdministrativeAssistant":
                    dashboard = new AdministrativeAssistantDashboard(empId, empName);
                    break;

                default:
                    MessageBox.Show("Role not recognized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            dashboard.Show();
            this.Close();
        }
    }
}