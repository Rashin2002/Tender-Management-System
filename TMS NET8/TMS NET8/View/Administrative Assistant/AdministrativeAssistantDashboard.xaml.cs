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

namespace TMS_NET8.View.Administrative_Assistant
{
    /// <summary>
    /// Interaction logic for AdministrativeAssistantDashboard.xaml
    /// </summary>
    public partial class AdministrativeAssistantDashboard : Window
    {
        private string empId;
        private string empName;
        public AdministrativeAssistantDashboard(string empId, string empName)
        {
            InitializeComponent();
            this.empId = empId;
            this.empName = empName;
        }
    }
}
