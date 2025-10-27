using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Xml.Linq;
using TMS_NET8.Controller;
using TMS_NET8.Model;

namespace TMS_NET8.View.Tender_Team
{
    /// <summary>
    /// Interaction logic for SingleTenderDetails.xaml
    /// </summary>
    public partial class SingleTenderDetails : Window
    {
        private readonly MTender currentTender; // Field to hold the tender data
        private readonly CTender controller = new CTender();
        public SingleTenderDetails(MTender mtender)
        {
            InitializeComponent();
            currentTender = mtender;
            LoadcmbTenderIssuerDashboardData();
            LoadDataComboTenderItems();
            PopulateTenderDetails();

        }

        private void LoadcmbTenderIssuerDashboardData()
        {
            List<string> tenderIssuer = controller.LoadDataComboTenderIssuer();
            cmbBuyingAuthName.ItemsSource = tenderIssuer;
        }

        private void LoadDataComboTenderItems()
        {
            List<string> tenderItem = controller.LoadDataComboTenderItems();
            cmbTenderItem.ItemsSource = tenderItem;
        }

        //Load brand names by item selection
        private void cmbTenderItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTenderItem.SelectedItem != null)
            {
                string? selectedItem = cmbTenderItem.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedItem))
                {
                    List<string> brandNames = controller.GetItemBrandsByItem(selectedItem);
                    cmbItemBrandName.ItemsSource = brandNames;
                    cmbItemBrandName.SelectedIndex = -1; // clear previous selection
                }
            }
        }

        //Get unit price by brand selection
        private void cmbTenderItemBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbItemBrandName.SelectedItem != null)
            {
                string? selectedBrand = cmbItemBrandName.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(selectedBrand))
                {
                    CTender controller = new CTender();
                    double unitPrice = controller.GetItemBrandUnitPrice(selectedBrand);
                    txtTenderUnitPrice.Text = unitPrice.ToString("0.00");
                }
            }
        }

        private void PopulateTenderDetails()
        {
            if (currentTender != null)
            {
                // Assign tender details to corresponding form controls
                txtTenderId.Text = currentTender.TenderId;
                txtTenderTitle.Text = currentTender.TenderTitle;
                txtTenderDesc.Text = currentTender.Description;
                txtTenderStatus.Text = currentTender.TenderStatus;

                // Handle dates (convert string to DateTime if possible)
                if (DateTime.TryParse(currentTender.StartDate, out DateTime startDate))
                    dateTenderStartDate.SelectedDate = startDate;

                if (DateTime.TryParse(currentTender.CloseDate, out DateTime closeDate))
                    dateTenderCloseDate.SelectedDate = closeDate;

                if (DateTime.TryParse(currentTender.TenderDeadline, out DateTime deadline))
                    dateTenderDeadline.SelectedDate = deadline;

                // ComboBoxes
                cmbBuyingAuthName.Text = currentTender.BuyingAuthorityName;
                cmbTenderItem.Text = currentTender.TenderItem;
                cmbItemBrandName.Text = currentTender.BrandName;

                // Numeric fields
                txtTenderBidAmt.Text = currentTender.TenderBidAmount.ToString("F2");
                txtTenderTotalAmt.Text = currentTender.TenderTotalAmount.ToString("F2");
                txtTenderItemQty.Text = currentTender.TenderItemQty.ToString();
                txtTenderUnitPrice.Text = currentTender.TenderUnitPrice.ToString("F2");
            }
        }

        //******************************************************************************************************************************
        //                                                       Validations Start
        //******************************************************************************************************************************

        private void txtTenderItemQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _); // Blocks non-numeric input
        }

        private void txtTenderBidAmt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null)
            {
                e.Handled = true; // Block input if sender is not a TextBox
                return;
            }
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !double.TryParse(newText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _);
        }

        private void txtTenderUnitPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox == null)
            {
                e.Handled = true; // Block input if sender is not a TextBox
                return;
            }
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !double.TryParse(newText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _);
        }

        private void HighlightField(Control control, bool isValid)
        {
            control.BorderBrush = isValid ? Brushes.Gray : Brushes.Red;
        }
        private bool ValidateFields()
        {
            // Tender Title
            bool titleValid = !string.IsNullOrWhiteSpace(txtTenderTitle.Text);
            HighlightField(txtTenderTitle, titleValid);
            if (!titleValid)
            {
                MessageBox.Show("Tender Title cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenderTitle.Focus();
                return false;
            }

            // Tender Issuer (ComboBox)
            bool issuerValid = cmbBuyingAuthName.SelectedItem != null;
            HighlightField(cmbBuyingAuthName, issuerValid);
            if (!issuerValid)
            {
                MessageBox.Show("Please select a Tender Issuer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbBuyingAuthName.Focus();
                return false;
            }

            // Tender Item (ComboBox)
            bool itemValid = cmbTenderItem.SelectedItem != null;
            HighlightField(cmbTenderItem, itemValid);
            if (!itemValid)
            {
                MessageBox.Show("Please select a Tender Item.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTenderItem.Focus();
                return false;
            }

            // Tender Item Brand (ComboBox)
            bool brandValid = cmbItemBrandName.SelectedItem != null;
            HighlightField(cmbItemBrandName, brandValid);
            if (!brandValid)
            {
                MessageBox.Show("Please select a Tender Item Brand.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbItemBrandName.Focus();
                return false;
            }

            // Unit Price
            bool unitPriceValid = !string.IsNullOrWhiteSpace(txtTenderUnitPrice.Text);
            HighlightField(txtTenderUnitPrice, unitPriceValid);
            if (!unitPriceValid)
            {
                MessageBox.Show("Unit Price cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenderUnitPrice.Focus();
                return false;
            }

            // Bid Amount
            bool bidValid = !string.IsNullOrWhiteSpace(txtTenderBidAmt.Text);
            HighlightField(txtTenderBidAmt, bidValid);
            if (!bidValid)
            {
                MessageBox.Show("Bid Amount cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenderBidAmt.Focus();
                return false;
            }

            // Quantity
            bool qtyValid = !string.IsNullOrWhiteSpace(txtTenderItemQty.Text);
            HighlightField(txtTenderItemQty, qtyValid);
            if (!qtyValid)
            {
                MessageBox.Show("Quantity cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenderItemQty.Focus();
                return false;
            }

            // Deadline
            bool deadlineValid = dateTenderDeadline.SelectedDate != null;
            HighlightField(dateTenderDeadline, deadlineValid);
            if (!deadlineValid)
            {
                MessageBox.Show("Please select a deadline date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                dateTenderDeadline.Focus();
                return false;
            }

            // Deadline cannot be in the past
            if (dateTenderDeadline.SelectedDate < DateTime.Today)
            {
                MessageBox.Show("Deadline cannot be a past date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                dateTenderDeadline.Focus();
                return false;
            }

            // Description
            bool descValid = !string.IsNullOrWhiteSpace(txtTenderDesc.Text);
            HighlightField(txtTenderDesc, descValid);
            if (!descValid)
            {
                MessageBox.Show("Tender Description cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTenderDesc.Focus();
                return false;
            }

            // ✅ All good
            return true;
        }
        //******************************************************************************************************************************
        //                                                     Validations End
        //******************************************************************************************************************************


        public bool NeedsRefresh { get; set; } = false;

        private void btnUpdate(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            try
            {
                // Update tender object with new values
                currentTender.TenderTitle = txtTenderTitle.Text.Trim();
                currentTender.TenderDeadline = dateTenderDeadline.SelectedDate?.ToString("yyyy-MM-dd") ?? currentTender.TenderDeadline;
                currentTender.Description = txtTenderDesc.Text.Trim();
                currentTender.BuyingAuthorityName = cmbBuyingAuthName.SelectedItem.ToString();
                currentTender.TenderItem = cmbTenderItem.SelectedItem.ToString();
                currentTender.BrandName = cmbItemBrandName.SelectedItem.ToString();
                currentTender.TenderUnitPrice = double.Parse(txtTenderUnitPrice.Text);
                currentTender.TenderBidAmount = double.Parse(txtTenderBidAmt.Text);
                currentTender.TenderItemQty = int.Parse(txtTenderItemQty.Text);

                // Call controller to update the database
                bool success = controller.UpdateTender(currentTender);

                if (success)
                {
                    MessageBox.Show("Tender updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NeedsRefresh = true; // trigger refresh in parent
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Update failed. Please check your database connection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Tender delete
        private void btnDelete(object sender, RoutedEventArgs e)
        {
            if (currentTender == null || string.IsNullOrWhiteSpace(currentTender.TenderId))
            {
                MessageBox.Show("No tender selected for deletion.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm delete
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this tender?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    CTender controller = new CTender();
                    bool success = controller.DeleteTender(currentTender.TenderId);

                    if (success)
                    {
                        MessageBox.Show("Tender deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.NeedsRefresh = true;
                        this.Close(); // Close popup and trigger refresh in dashboard
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the tender. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting tender: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
