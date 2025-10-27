using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TMS_NET8.Controller;
using TMS_NET8.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

// Aliases (to simplify your code)
using MigraDocVA = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment;
using PdfColors = MigraDoc.DocumentObjectModel.Colors;


namespace TMS_NET8.View.Tender_Manager
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

        private void btnGenerateTenderReport(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fetch current tender details from textboxes
                var tender = new MTender
                {
                    TenderId = txtTenderId.Text,
                    TenderTitle = txtTenderTitle.Text,
                    Description = txtTenderDesc.Text,
                    StartDate = dateTenderStartDate.SelectedDate?.ToString("yyyy-MM-dd"),
                    CloseDate = dateTenderCloseDate.SelectedDate?.ToString("yyyy-MM-dd"),
                    TenderStatus = txtTenderStatus.Text,
                    BrandName = cmbItemBrandName.Text,
                    BuyingAuthorityName = cmbBuyingAuthName.Text,
                    TenderItem = cmbTenderItem.Text,
                    TenderUnitPrice = double.TryParse(txtTenderUnitPrice.Text, out double up) ? up : 0,
                    TenderBidAmount = double.TryParse(txtTenderBidAmt.Text, out double ba) ? ba : 0,
                    TenderItemQty = int.TryParse(txtTenderItemQty.Text, out int qty) ? qty : 0,
                    TenderTotalAmount = double.TryParse(txtTenderTotalAmt.Text, out double tot) ? tot : 0,
                    TenderDeadline = dateTenderDeadline.SelectedDate?.ToString("yyyy-MM-dd")
                };

                // Logo path
                string logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "login_banner1.jpeg");

                // Generate report
                string? savedPath = GenerateTenderReportPdf(tender, logoPath);
                if (savedPath != null)
                    MessageBox.Show("Tender report generated successfully:\n" + savedPath, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating tender report: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string? GenerateTenderReportPdf(MTender tender, string? logoPath = null)
        {
            if (tender == null)
                throw new ArgumentNullException(nameof(tender));

            // 1️⃣ Create document
            var doc = new Document();
            doc.Info.Title = $"Tender Report - {tender.TenderId}";
            doc.Info.Author = "DSK Enterprises Pvt Ltd";

            // 2️⃣ Setup section and page format
            var section = doc.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            // 3️⃣ Add watermark if available
            string watermarkPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "WatermarkReport.png");
            if (File.Exists(watermarkPath))
            {
                var headerSection = section.Headers.Primary;
                var img = headerSection.AddImage(watermarkPath);
                img.Width = Unit.FromCentimeter(15);
                img.LockAspectRatio = true;
                img.Left = ShapePosition.Center;
                img.Top = ShapePosition.Center;
                img.WrapFormat.Style = WrapStyle.None;
            }

            // 4️⃣ Font & style settings
            var style = doc.Styles["Normal"];
            style!.Font.Name = "Segoe UI";
            style.Font.Size = 10;

            var headingStyle = doc.Styles.AddStyle("Heading", "Normal");
            headingStyle.Font.Size = 14;
            headingStyle.Font.Bold = true;

            var smallBold = doc.Styles.AddStyle("SmallBold", "Normal");
            smallBold.Font.Size = 9;
            smallBold.Font.Bold = true;

            // 5️⃣ Header section - Logo + Company Info
            var headerTable = section.AddTable();
            headerTable.Borders.Width = 0;
            headerTable.AddColumn(Unit.FromCentimeter(3.5));
            headerTable.AddColumn(Unit.FromCentimeter(12));

            var headerRow = headerTable.AddRow();
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    var imgCell = headerRow.Cells[0];
                    var image = imgCell.AddImage(logoPath);
                    image.LockAspectRatio = true;
                    image.Width = Unit.FromCentimeter(3);
                    imgCell.VerticalAlignment = MigraDocVA.Center;
                }
                catch { headerRow.Cells[0].AddParagraph(); }
            }
            else headerRow.Cells[0].AddParagraph();

            var compPara = headerRow.Cells[1].AddParagraph();
            compPara.AddFormattedText("DSK Enterprises Pvt Ltd", TextFormat.Bold);
            compPara.AddLineBreak();
            compPara.AddText("Tender Management Division");
            compPara.AddLineBreak();
            compPara.AddText("No. 123, Corporate Road, Colombo");
            compPara.Format.Font.Size = 10;

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.5);

            // 6️⃣ Report title
            var title = section.AddParagraph("TENDER REPORT");
            title.Style = "Heading";
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = Unit.FromCentimeter(0.5);

            // 7️⃣ Tender details table
            var infoTable = section.AddTable();
            infoTable.Borders.Width = 0;
            infoTable.AddColumn(Unit.FromCentimeter(6));
            infoTable.AddColumn(Unit.FromCentimeter(9));

            void AddInfoRow(string label, string? value)
            {
                var row = infoTable.AddRow();
                row.Cells[0].AddParagraph(label).Style = "SmallBold";
                row.Cells[1].AddParagraph(value ?? "-");
            }

            AddInfoRow("Tender ID:", tender.TenderId);
            AddInfoRow("Tender Title:", tender.TenderTitle);
            AddInfoRow("Buying Authority:", tender.BuyingAuthorityName);
            AddInfoRow("Tender Item:", tender.TenderItem);
            AddInfoRow("Item Brand:", tender.BrandName);
            AddInfoRow("Bid Amount:", $"LKR {tender.TenderBidAmount:N2}");
            AddInfoRow("Unit Price:", $"LKR {tender.TenderUnitPrice:N2}");
            AddInfoRow("Item Quantity:", tender.TenderItemQty.ToString());
            AddInfoRow("Total Amount:", $"LKR {tender.TenderTotalAmount:N2}");
            AddInfoRow("Tender Deadline:", tender.TenderDeadline);
            AddInfoRow("Tender Status:", tender.TenderStatus);
            AddInfoRow("Start Date:", tender.StartDate);
            AddInfoRow("Close Date:", tender.CloseDate);
            AddInfoRow("Generated On:", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));


            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.6);

            // 8️⃣ Description
            var descPara = section.AddParagraph("Tender Description:");
            descPara.Style = "SmallBold";
            descPara.Format.SpaceAfter = Unit.FromCentimeter(0.2);
            section.AddParagraph(tender.Description ?? "-");

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.8);

            // 9️⃣ Signature section
            var signTable = section.AddTable();
            signTable.Borders.Width = 0;
            signTable.AddColumn(Unit.FromCentimeter(7));
            signTable.AddColumn(Unit.FromCentimeter(7));

            var signRow = signTable.AddRow();
            signRow.Cells[0].AddParagraph("Prepared by:");
            signRow.Cells[0].AddParagraph("_______________________");
            signRow.Cells[1].AddParagraph("Authorized by:");
            signRow.Cells[1].AddParagraph("_______________________");

            // Footer
            section.Footers.Primary.AddParagraph("This is a computer-generated tender report.").Format.Font.Size = 8;

            // 🔟 Render PDF
            var renderer = new PdfDocumentRenderer { Document = doc };
            renderer.RenderDocument();

            // Save dialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save Tender Report",
                Filter = "PDF Files|*.pdf",
                FileName = $"TenderReport_{tender.TenderId}.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveDialog.ShowDialog() != true)
                return null;

            string filePath = saveDialog.FileName;
            renderer.PdfDocument.Save(filePath);

            // Auto-open
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

            return filePath;
        }


    }
}
