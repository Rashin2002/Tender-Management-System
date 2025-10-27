using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMS_NET8.Controller;
using TMS_NET8.Model;


namespace TMS_NET8.View
{
    /// <summary>
    /// Interaction logic for SalaryViewPage.xaml
    /// </summary>
    public partial class SalaryViewPage : Window
    {
        private string empId;
        private string empName;
        private CSalary controller = new CSalary(); // Assuming you have a controller for Salary

        // Step 1: Add a constructor that accepts empId and empName
        public SalaryViewPage(string empId, string empName)
        {
            InitializeComponent();
            this.empId = empId;
            this.empName = empName;

            lblEmployeeName.Text = $"Salary Details for {empName}"; // Optional: show on UI
            LoadSalaries(); // Load salaries for this employee
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LoadSalaries()
        {
            try
            {
                var salaries = controller.GetSalariesByEmployee(empId); // Filter by empId
                SalaryGrid.ItemsSource = salaries; // Assuming your DataGrid is named SalaryGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while loading salaries: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
