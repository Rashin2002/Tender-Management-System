using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMS_NET8.Controller;
using TMS_NET8.Model;

namespace TMS_NET8.View.Tender_Manager
{
    /// <summary>
    /// Interaction logic for TenderManagerDashboard.xaml
    /// </summary>
    public partial class TenderManagerDashboard : Window
    {
        private CTender controller = new CTender();
        public TenderManagerDashboard()
        {
            InitializeComponent();
            LoadTenders();
            LoadcmbTenderIssuerDashboardData();
            LoadcmbTenderStatusDashboardData();

        }

        //Data Grid View Load at the form load
        private void LoadTenders()
        {
            var tenders = controller.LoadAllTenders();
            TenderGrid.ItemsSource = tenders; // Your DataGrid name
        }

        //Combo Box Data Load at the form load
        private void LoadcmbTenderIssuerDashboardData()
        {
            List<string> tenderIssuer = controller.LoadDataComboTenderIssuer();
            cmbTenderIssuerDashboard.ItemsSource = tenderIssuer;
        }

        private void cmbTenderIssuerDashboard_SelectionChanged(object sender, SelectionChangedEventArgs e)    //Combo Box Selection Changed Event to the datagrid view load
        {
            if (cmbTenderIssuerDashboard.SelectedItem is string selectedIssuer)
            {
                TenderGrid.ItemsSource = controller.GetTendersByIssuer(selectedIssuer);
            }
            else
            {
                LoadTenders();
            }
        }

        //Combo Box Data Load at the form load
        private void LoadcmbTenderStatusDashboardData()
        {
            List<string> tenderStatus = controller.LoadDataComboTenderStatus();
            cmbTenderStatusDashboard.ItemsSource = tenderStatus;
        }

        private void cmbTenderStatusDashboard_SelectionChanged(object sender, SelectionChangedEventArgs e)    //Combo Box Selection Changed Event to the datagrid view load
        {
            if (cmbTenderStatusDashboard.SelectedItem is string selectedStatus)
            {
                TenderGrid.ItemsSource = controller.GetTendersByStatus(selectedStatus);
            }
            else
            {
                LoadTenders();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TenderGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Use pattern matching to simultaneously cast 'sender' to DataGrid and check if it's not null.
            if (sender is DataGrid dataGrid)
            {
                // Now that 'dataGrid' is known to be non-null, we check the selected item.
                if (dataGrid.SelectedItem is MTender selectedTender)
                {
                    // At this point, selectedTender is guaranteed to be a non-null MTender object.
                    SingleTenderDetails std = new SingleTenderDetails(selectedTender);
                    std.Owner = this;
                    BlurEffect blurEffect = new BlurEffect
                    {
                        Radius = 5, // You can adjust the radius for stronger/softer blur
                        KernelType = KernelType.Gaussian
                    };

                    try
                    {
                        MainContentGrid.Effect = blurEffect;
                        std.ShowDialog();
                        // CHECK THE REFRESH FLAG AFTER THE DIALOG CLOSES
                        if (std.NeedsRefresh)
                        {
                            LoadTenders(); // Call your method to reload the data source
                        }
                    }
                    finally
                    {
                        // 4. Remove the blur effect once the child window is closed
                        MainContentGrid.Effect = null;
                    }
                }

                // This line is fine because dataGrid is guaranteed non-null
                dataGrid.SelectedItem = null;
            }
        }

        private void btnSearch(object sender, RoutedEventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadTenders(); // reload all tenders if search is empty
                return;
            }

            try
            {
                var results = controller.SearchTenders(searchText);

                if (results.Count > 0)
                {
                    TenderGrid.ItemsSource = results;
                }
                else
                {
                    MessageBox.Show("No tenders found for the given ID or Name.",
                                    "Search Result",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    TenderGrid.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching: " + ex.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void btnRefresh(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear search box
                txtSearch.Text = string.Empty;

                // Clear all combo boxes (if you have any)
                cmbTenderIssuerDashboard.SelectedIndex = -1;
                cmbTenderStatusDashboard.SelectedIndex = -1;

                // Reload the DataGridView
                LoadTenders();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while refreshing: " + ex.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void btnAcceptedTenders(object sender, RoutedEventArgs e)
        {

        }

        private void btnBuyingAuthDetails(object sender, RoutedEventArgs e)
        {
            BuyingAuthorityDetails bad = new();
            bad.Show();
            this.Close();
        }
    }
}
