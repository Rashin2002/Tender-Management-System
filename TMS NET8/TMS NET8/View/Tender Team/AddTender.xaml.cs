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
    /// Interaction logic for AddTender.xaml
    /// </summary>
    public partial class AddTender : Window
    {
        private CTender controller = new CTender();
        public bool NeedsRefresh { get; private set; } = false;
        public AddTender()
        {
            InitializeComponent();
            LoadDataComboTenderIssuer();
            LoadDataComboTenderItems();
            MTender model = new MTender();
            txtTenderId.Text = model.GetNextTenderId();
            txtTenderId.IsReadOnly = true;
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
            bool issuerValid = cmbTenderIssuer.SelectedItem != null;
            HighlightField(cmbTenderIssuer, issuerValid);
            if (!issuerValid)
            {
                MessageBox.Show("Please select a Tender Issuer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTenderIssuer.Focus();
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
            bool brandValid = cmbTenderItemBrand.SelectedItem != null;
            HighlightField(cmbTenderItemBrand, brandValid);
            if (!brandValid)
            {
                MessageBox.Show("Please select a Tender Item Brand.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTenderItemBrand.Focus();
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

            // Deadline DatePicker
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

            // ✅ All good
            return true;
        }

        //******************************************************************************************************************************
        //                                                     Validations End
        //******************************************************************************************************************************



        // Load unique tender issuers for combo box
        private void LoadDataComboTenderIssuer()
        {
            List<string> tenderIssuer = controller.LoadDataComboTenderIssuer();
            cmbTenderIssuer.ItemsSource = tenderIssuer;
        }

        // Load unique tender items for combo box
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
                    cmbTenderItemBrand.ItemsSource = brandNames;
                    cmbTenderItemBrand.SelectedIndex = -1; // clear previous selection
                }
            }
        }

        //Get unit price by brand selection
        private void cmbTenderItemBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTenderItemBrand.SelectedItem != null)
            {
                string? selectedBrand = cmbTenderItemBrand.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(selectedBrand))
                {
                    CTender controller = new CTender();
                    double unitPrice = controller.GetItemBrandUnitPrice(selectedBrand);
                    txtTenderUnitPrice.Text = unitPrice.ToString("0.00");
                }
            }
        }


        //new tender adding
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            try
            {
                string tenderId = txtTenderId.Text;
                string tenderTitle = txtTenderTitle.Text;
                string tenderIssuer = cmbTenderIssuer.SelectedItem?.ToString() ?? string.Empty;
                string tenderItem = cmbTenderItem.SelectedItem?.ToString() ?? string.Empty;
                string tenderItemBrand = cmbTenderItemBrand.SelectedItem?.ToString() ?? string.Empty;
                double tenderUnitPrice = double.Parse(txtTenderUnitPrice.Text);
                double tenderBidAmount = double.Parse(txtTenderBidAmt.Text);
                int tenderQuantity = int.Parse(txtTenderItemQty.Text);
                string tenderDeadline = (dateTenderDeadline.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd");
                string tenderDesc = txtTednerDesc.Text;
                string tenderStatus = "Open";
                string tenderStartDate = DateTime.Today.ToString("yyyy-MM-dd");

                CTender controller = new CTender();
                controller.RegisterTender(tenderId, tenderTitle, tenderIssuer, tenderItem, tenderItemBrand, 
                    tenderUnitPrice, tenderBidAmount, tenderQuantity, tenderDeadline, tenderDesc, tenderStatus, tenderStartDate);

                MessageBox.Show("Tender added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                NeedsRefresh = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
