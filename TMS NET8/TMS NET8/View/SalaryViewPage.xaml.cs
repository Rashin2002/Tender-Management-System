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
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Shapes;

using MigraDocVA = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment;
using PdfColors = MigraDoc.DocumentObjectModel.Colors;





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

        private void SalaryGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SalaryGrid.SelectedItem is MSalary selected)
            {
                try
                {
                    // Fetch full employee object using EmpId from selected salary
                    MEmployee? emp = MEmployee.GetEmployeeById(selected.EmpId!);
                    if (emp == null)
                    {
                        MessageBox.Show("Employee data not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return; // stop if employee not found
                    }

                    // Provide logo path if you want a logo included
                    string? logoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "login_banner1.jpeg");
                    if (!File.Exists(logoPath))
                    {
                        MessageBox.Show("Logo not found at: " + logoPath);
                        logoPath = null;
                    }// skip logo if not present

                    // Pass the full employee object to the PDF generation method
                    string? savedPath = GeneratePolishedPaySheetPdf(selected, emp, logoPath);
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        MessageBox.Show("Pay sheet saved to:\n" + savedPath, "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Open automatically
                        Process.Start(new ProcessStartInfo(savedPath) { UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating pay sheet: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        /// <summary>
        /// Generate a polished pay-sheet PDF for the given salary record and employee name.
        /// Returns the full path to the saved PDF (or null if cancelled/failed).
        /// </summary>
        private string? GeneratePolishedPaySheetPdf(MSalary s, MEmployee emp, string? logoPath = null)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            // 1️⃣ Create document
            var doc = new Document();
            doc.Info.Title = $"PaySheet - {s.SalaryId}";
            doc.Info.Author = "Your Company Name";

            // 2️⃣ Section and PageSetup
            var section = doc.AddSection();
            section.PageSetup = doc.DefaultPageSetup.Clone(); // clone default setup
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(1.5);
            section.PageSetup.RightMargin = Unit.FromCentimeter(1.5);

            // Add watermark image to the header (centered)
            string watermarkPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "WatermarkReport.png");
            if (File.Exists(watermarkPath))
            {
                var headerSection = section.Headers.Primary; // renamed
                var img = headerSection.AddImage(watermarkPath);
                img.Width = Unit.FromCentimeter(15);
                img.LockAspectRatio = true;
                img.Left = ShapePosition.Center;
                img.Top = ShapePosition.Center;
                img.WrapFormat.Style = WrapStyle.None; // behind text
            }



            // 3️⃣ Culture info and currency
            var culture = CultureInfo.CurrentCulture;
            string currencySymbol = "LKR ";

            // 4️⃣ Styles
            var style = doc.Styles["Normal"];
            style!.Font.Name = "Segoe UI";
            style.Font.Size = 10;

            var headingStyle = doc.Styles.AddStyle("Heading", "Normal");
            headingStyle.Font.Size = 14;
            headingStyle.Font.Bold = true;

            var smallBold = doc.Styles.AddStyle("SmallBold", "Normal");
            smallBold.Font.Size = 9;
            smallBold.Font.Bold = true;

            // 5️⃣ Header: logo + company details
            var headerTable = section.AddTable();
            headerTable.Borders.Width = 0;
            headerTable.AddColumn(Unit.FromCentimeter(3.5)); // logo
            headerTable.AddColumn(Unit.FromCentimeter(12));  // company details

            var headerRow = headerTable.AddRow();
            headerRow.TopPadding = Unit.FromCentimeter(0.1);
            headerRow.BottomPadding = Unit.FromCentimeter(0.1);

            // Logo cell
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    var imgCell = headerRow.Cells[0];
                    var image = imgCell.AddImage(logoPath);
                    image.LockAspectRatio = true;
                    image.Width = Unit.FromCentimeter(3);
                    imgCell.VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

                }
                catch
                {
                    headerRow.Cells[0].AddParagraph();
                }
            }
            else
            {
                headerRow.Cells[0].AddParagraph();
            }

            // Company details
            var compPara = headerRow.Cells[1].AddParagraph();
            compPara.AddFormattedText("DSK Enterprises Pvt Ltd", TextFormat.Bold);
            compPara.AddLineBreak();
            compPara.AddText("HR Management Division");
            compPara.AddLineBreak();
            compPara.AddText("No. 123, Corporate Road, Colombo");
            compPara.Format.Font.Size = 10;
            headerRow.Cells[1].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.5);

            // 6️⃣ Title
            var titlePara = section.AddParagraph("PAY SHEET");
            titlePara.Style = "Heading";
            titlePara.Format.Alignment = ParagraphAlignment.Center;
            titlePara.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            // 7️⃣ Employee info table
            var infoTable = section.AddTable();
            infoTable.Borders.Width = 0;
            infoTable.AddColumn(Unit.FromCentimeter(6));
            infoTable.AddColumn(Unit.FromCentimeter(9));

            Row r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Salary ID:").Style = "SmallBold";
            r.Cells[1].AddParagraph(s.SalaryId ?? string.Empty);

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Employee ID:").Style = "SmallBold";
            r.Cells[1].AddParagraph(emp.EmpId ?? string.Empty);

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Employee Name:").Style = "SmallBold";
            r.Cells[1].AddParagraph(emp.EmpFullName ?? string.Empty);

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Basic Salary:").Style = "SmallBold";
            r.Cells[1].AddParagraph("LKR " + emp.BasicSalary.ToString("N2"));

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Employee Name:").Style = "SmallBold";
            r.Cells[1].AddParagraph(empName ?? string.Empty);

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Month / Year:").Style = "SmallBold";
            r.Cells[1].AddParagraph($"{s.Month} / {s.Year}");

            r = infoTable.AddRow();
            r.Cells[0].AddParagraph("Generated On:").Style = "SmallBold";
            r.Cells[1].AddParagraph(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.5);

            // 8️⃣ Salary breakdown table
            var table = section.AddTable();
            table.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            table.Borders.Width = 0.8;
            table.Borders.Color = PdfColors.Black;
            table.AddColumn(Unit.FromCentimeter(10));
            table.AddColumn(Unit.FromCentimeter(5));

            var header = table.AddRow();
            header.Shading.Color = PdfColors.LightGray;
            header.Format.Font.Bold = true;
            header.Cells[0].AddParagraph("Description");
            header.Cells[1].AddParagraph("Amount");

            void AddMoneyRow(string label, double? amount)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(label);
                string formatted = amount.HasValue ? currencySymbol + amount.Value.ToString("N2", culture) : "-";
                var p = row.Cells[1].AddParagraph(formatted);
                row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
            }

            AddMoneyRow("Allowance", s.Allowance);
            AddMoneyRow("Gross Salary", s.GrossSalary);
            AddMoneyRow("EPF Amount", s.EPFAmt);
            AddMoneyRow("ETF Amount", s.ETFAmt);
            AddMoneyRow("Income Tax", s.IncomeTaxAmt);
            AddMoneyRow("Absence Deduction", s.AbsenceDeduction);

            // Net salary row
            var netRow = table.AddRow();
            netRow.Format.Font.Bold = true;
            netRow.Cells[0].AddParagraph("Net Salary");
            netRow.Cells[1].AddParagraph(currencySymbol + s.NetSalary.ToString("N2", culture));
            netRow.Cells[1].Format.Alignment = ParagraphAlignment.Right;

            section.AddParagraph().Format.SpaceAfter = Unit.FromCentimeter(0.8);

            // Signature table
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
            section.Footers.Primary.AddParagraph($"This is a computer generated paysheet.").Format.Font.Size = 8;

            // 9️⃣ Render PDF
            var renderer = new PdfDocumentRenderer { Document = doc };
            renderer.RenderDocument();

            // 10️⃣ Save dialog
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Pay Sheet",
                Filter = "PDF Files|*.pdf",
                FileName = $"Paysheet_{s.SalaryId}_{s.EmpId}_{s.Month}_{s.Year}.pdf",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (saveFileDialog.ShowDialog() != true) return null;

            string filePath = saveFileDialog.FileName;
            renderer.PdfDocument.Save(filePath);

            return filePath;
        }




    }
}
