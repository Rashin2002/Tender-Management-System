using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMS_NET8.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TMS_NET8.Model
{
    public class MTender
    {
        public string GetNextTenderId()
        {
            string nextId = "T001"; // Default ID if no records exist

            try
            {
                using (SqlConnection con = Database.DBConnection.CreateConnection())
                {
                    string sql = "SELECT TOP 1 tender_id FROM Tender ORDER BY tender_id DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, con))
                    {
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            string? lastId = result.ToString(); // e.g. "T005"
                            if (!string.IsNullOrEmpty(lastId) && lastId.Length > 1)
                            {
                                int numericPart = int.Parse(lastId.Substring(1)); // get "005" → 5
                                numericPart++; // increment → 6
                                nextId = $"T{numericPart:D3}"; // format → "T006"
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating Tender ID: " + ex.Message);
            }

            return nextId;
        }

        //Add new tender
        public void RegisterTender(string tenderId, string tenderTitle, string tenderIssuer, string tenderItem, string tenderItemBrand,
                                    double tenderUnitPrice, double tenderBidAmount, int tenderQuantity, string tenderDeadline, string tenderDesc, 
                                    string tenderStatus, string tenderStartDate)
        {
            try
            {
                using (SqlConnection con = Database.DBConnection.CreateConnection())
                {
                    string tenderBuyingAuthId = string.Empty;
                    string tenderItemBrandId = string.Empty;

                    // 🔹 Step 1: Get buying_auth_id from BuyingAuthority table
                    string queryAuth = "SELECT buying_auth_id FROM BuyingAuthority WHERE buying_auth_name = @name";
                    using (SqlCommand cmdAuth = new SqlCommand(queryAuth, con))
                    {
                        cmdAuth.Parameters.AddWithValue("@name", tenderIssuer);
                        object resultAuth = cmdAuth.ExecuteScalar();
                        if (resultAuth != null && resultAuth != DBNull.Value)
                            tenderBuyingAuthId = resultAuth.ToString()!;
                        else
                            throw new Exception("Buying Authority not found for: " + tenderIssuer);
                    }

                    // 🔹 Step 2: Get brand_id from ItemBrand table
                    string queryBrand = "SELECT brand_id FROM ItemBrand WHERE item_brand_name = @brandName";
                    using (SqlCommand cmdBrand = new SqlCommand(queryBrand, con))
                    {
                        cmdBrand.Parameters.AddWithValue("@brandName", tenderItemBrand);
                        object resultBrand = cmdBrand.ExecuteScalar();
                        if (resultBrand != null && resultBrand != DBNull.Value)
                            tenderItemBrandId = resultBrand.ToString()!;
                        else
                            throw new Exception("Item Brand not found for: " + tenderItemBrand);
                    }

                    // 🔹 Step 3: Insert Tender record
                    string queryInsert = @"INSERT INTO Tender 
                                            (tender_id, tender_title, tender_description, tender_start_date, tender_status, buying_auth_id,
                                            tender_bid_amt, tender_item_qty, tender_item, brand_id, tender_deadline, tender_unit_price)
                                            VALUES 
                                            (@id, @title, @desc, @startDate, @status, @buyingAuthId, @bidAmount, @quantity, @item, @brandId, @deadline, @unitPrice)";

                    using (SqlCommand cmdInsert = new SqlCommand(queryInsert, con))
                    {
                        cmdInsert.Parameters.AddWithValue("@id", tenderId);
                        cmdInsert.Parameters.AddWithValue("@title", tenderTitle);
                        cmdInsert.Parameters.AddWithValue("@desc", tenderDesc);
                        cmdInsert.Parameters.AddWithValue("@startDate", tenderStartDate);
                        cmdInsert.Parameters.AddWithValue("@status", tenderStatus);
                        cmdInsert.Parameters.AddWithValue("@buyingAuthId", tenderBuyingAuthId);
                        cmdInsert.Parameters.AddWithValue("@bidAmount", tenderBidAmount);
                        cmdInsert.Parameters.AddWithValue("@quantity", tenderQuantity);
                        cmdInsert.Parameters.AddWithValue("@item", tenderItem);
                        cmdInsert.Parameters.AddWithValue("@brandId", tenderItemBrandId);
                        cmdInsert.Parameters.AddWithValue("@deadline", tenderDeadline);
                        cmdInsert.Parameters.AddWithValue("@unitPrice", tenderUnitPrice);

                        int rows = cmdInsert.ExecuteNonQuery();

                        if (rows > 0)
                            MessageBox.Show("Tender Registered Successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        //Update tender

        public void UpdateTender(string tenderId, string tenderTitle, string tenderDeadline, string tenderDesc,
                         string tenderIssuer, string tenderItem, string tenderItemBrand,
                         double tenderUnitPrice, double tenderBidAmount, int tenderQuantity)
        {
            try
            {
                using (SqlConnection con = Database.DBConnection.CreateConnection())
                {
                    string tenderBuyingAuthId = string.Empty;
                    string tenderItemBrandId = string.Empty;

                    // 🔹 Step 1: Get buying_auth_id from BuyingAuthority table
                    string queryAuth = "SELECT buying_auth_id FROM BuyingAuthority WHERE buying_auth_name = @name";
                    using (SqlCommand cmdAuth = new SqlCommand(queryAuth, con))
                    {
                        cmdAuth.Parameters.AddWithValue("@name", tenderIssuer);
                        object resultAuth = cmdAuth.ExecuteScalar();
                        if (resultAuth != null && resultAuth != DBNull.Value)
                            tenderBuyingAuthId = resultAuth.ToString()!;
                        else
                            throw new Exception("Buying Authority not found for: " + tenderIssuer);
                    }

                    // 🔹 Step 2: Get brand_id from ItemBrand table
                    string queryBrand = "SELECT brand_id FROM ItemBrand WHERE item_brand_name = @brandName";
                    using (SqlCommand cmdBrand = new SqlCommand(queryBrand, con))
                    {
                        cmdBrand.Parameters.AddWithValue("@brandName", tenderItemBrand);
                        object resultBrand = cmdBrand.ExecuteScalar();
                        if (resultBrand != null && resultBrand != DBNull.Value)
                            tenderItemBrandId = resultBrand.ToString()!;
                        else
                            throw new Exception("Item Brand not found for: " + tenderItemBrand);
                    }

                    // 🔹 Step 3: Update the Tender record
                    string queryUpdate = @"UPDATE Tender 
                                   SET tender_title = @title,
                                       tender_description = @desc,
                                       tender_deadline = @deadline,
                                       buying_auth_id = @buyingAuthId,
                                       tender_item = @item,
                                       brand_id = @brandId,
                                       tender_unit_price = @unitPrice,
                                       tender_bid_amt = @bidAmount,
                                       tender_item_qty = @quantity
                                   WHERE tender_id = @id";

                    using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, con))
                    {
                        cmdUpdate.Parameters.AddWithValue("@id", tenderId);
                        cmdUpdate.Parameters.AddWithValue("@title", tenderTitle);
                        cmdUpdate.Parameters.AddWithValue("@desc", tenderDesc);
                        cmdUpdate.Parameters.AddWithValue("@deadline", tenderDeadline);
                        cmdUpdate.Parameters.AddWithValue("@buyingAuthId", tenderBuyingAuthId);
                        cmdUpdate.Parameters.AddWithValue("@item", tenderItem);
                        cmdUpdate.Parameters.AddWithValue("@brandId", tenderItemBrandId);
                        cmdUpdate.Parameters.AddWithValue("@unitPrice", tenderUnitPrice);
                        cmdUpdate.Parameters.AddWithValue("@bidAmount", tenderBidAmount);
                        cmdUpdate.Parameters.AddWithValue("@quantity", tenderQuantity);

                        int rows = cmdUpdate.ExecuteNonQuery();

                        if (rows > 0)
                            MessageBox.Show("Tender updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        else
                            MessageBox.Show("No records were updated. Check Tender ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating Tender: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool DeleteTender(string tenderId)
        {
            bool isDeleted = false;

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = "DELETE FROM Tender WHERE tender_id = @TenderId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TenderId", tenderId);

                    int rows = cmd.ExecuteNonQuery();
                    isDeleted = rows > 0;
                }

                con.Close();
            }

            return isDeleted;
        }


        // Load unique tender issuers for combo box
        public static List<string> GetUniqueTenderIssuers()
        {
            List<string> tenderIssuers = new List<string>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = "SELECT DISTINCT buying_auth_name FROM BuyingAuthority";
                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tenderIssuers.Add(reader.GetString(0));
                }
            }

            return tenderIssuers;
        }

        // Load unique tender Status for combo box
        public static List<string> GetUniqueTenderStatus()
        {
            List<string> tenderStatus = new List<string>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = "SELECT DISTINCT tender_status FROM Tender";
                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tenderStatus.Add(reader.GetString(0));
                }
            }

            return tenderStatus;
        }

        // Load unique tender items for combo box
        public static List<string> GetUniqueTenderItems()
        {
            List<string> tenderItems = new List<string>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = "SELECT DISTINCT item_name FROM Item";
                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tenderItems.Add(reader.GetString(0));
                }
            }

            return tenderItems;
        }

        // Load item brands based on selected item
        public static List<string> GetItemBrandsByItem(string itemName)
        {
            List<string> brandNames = new List<string>();

            try
            {
                using (SqlConnection con = DBConnection.CreateConnection())
                {
                    // Step 1: Get item_id from Item table
                    string itemId = string.Empty;
                    string queryItem = "SELECT item_id FROM Item WHERE item_name = @name";
                    SqlCommand cmdItem = new SqlCommand(queryItem, con);
                    cmdItem.Parameters.AddWithValue("@name", itemName);

                    object result = cmdItem.ExecuteScalar();

                    if (result != null)
                        itemId = result.ToString()!;

                    // Step 2: Get all brand names linked to that item_id
                    if (!string.IsNullOrEmpty(itemId))
                    {
                        string queryBrands = "SELECT item_brand_name FROM ItemBrand WHERE item_id = @itemId";
                        SqlCommand cmdBrand = new SqlCommand(queryBrands, con);
                        cmdBrand.Parameters.AddWithValue("@itemId", itemId);

                        SqlDataReader reader = cmdBrand.ExecuteReader();

                        while (reader.Read())
                        {
                            brandNames.Add(reader.GetString(0));
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return brandNames;
        }

        

        public static double GetItemBrandUnitPrice(string brandName)
        {
            double unitPrice = 0;

            try
            {
                using (SqlConnection con = DBConnection.CreateConnection())
                {
                    string query = "SELECT item_brand_price FROM ItemBrand WHERE item_brand_name = @brandName";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@brandName", brandName);

                    object? result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                        unitPrice = Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching unit price: " + ex.Message);
            }

            return unitPrice;
        }


        public string? TenderId { get; set; }
        public string? TenderTitle { get; set; }
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public string? CloseDate { get; set; }
        public string? TenderStatus { get; set; }
        public string? BrandName { get; set; }
        public string? BuyingAuthorityName { get; set; }
        public double TenderBidAmount { get; set; }
        public double TenderTotalAmount { get; set; }
        public int TenderItemQty { get; set; }
        public string? TenderItem { get; set; }
        public string? TenderDeadline { get; set; }
        public double TenderUnitPrice { get; set; }

        // ✅ Get all tenders
        public static List<MTender> GetAllTenders()
        {
            List<MTender> tenders = new List<MTender>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = @"SELECT 
                            t.tender_id,
                            t.tender_title,
                            t.tender_description,
                            t.tender_start_date,
                            t.tender_close_date,
                            t.tender_status,
                            b.item_brand_name,
                            a.buying_auth_name,
                            t.tender_bid_amt,
                            t.tender_total_amt,
                            t.tender_item_qty,
                            t.tender_item,
                            t.tender_deadline,
                            t.tender_unit_price
                         FROM Tender t
                         INNER JOIN ItemBrand b ON t.brand_id = b.brand_id
                         INNER JOIN BuyingAuthority a ON t.buying_auth_id = a.buying_auth_id";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MTender tender = new MTender
                    {
                        TenderId = reader.GetString(0),
                        TenderTitle = reader.GetString(1),
                        Description = reader.GetString(2),
                        StartDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),

                        // ✅ Handle null close date safely
                        CloseDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd"),

                        TenderStatus = reader.GetString(5),
                        BrandName = reader.GetString(6),
                        BuyingAuthorityName = reader.GetString(7),
                        TenderBidAmount = reader.GetDouble(8),

                        // ✅ Handle null total amount safely
                        TenderTotalAmount = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9),

                        TenderItemQty = reader.GetInt32(10),
                        TenderItem = reader.GetString(11),
                        TenderDeadline = reader.GetDateTime(12).ToString("yyyy-MM-dd"),
                        TenderUnitPrice = reader.GetDouble(13)
                    };
                    tenders.Add(tender);
                }
            }

            return tenders;
        }


        //Get tenders grid view by issuer for filtering
        public static List<MTender> GetTendersByIssuer(string selectedIssuer)
        {
            List<MTender> tenders = new List<MTender>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = @"SELECT 
                            t.tender_id,
                            t.tender_title,
                            t.tender_description,
                            t.tender_start_date,
                            t.tender_close_date,
                            t.tender_status,
                            b.item_brand_name,
                            a.buying_auth_name,
                            t.tender_bid_amt,
                            t.tender_total_amt,
                            t.tender_item_qty,
                            t.tender_item,
                            t.tender_deadline,
                            t.tender_unit_price
                         FROM Tender t
                         INNER JOIN ItemBrand b ON t.brand_id = b.brand_id
                         INNER JOIN BuyingAuthority a ON t.buying_auth_id = a.buying_auth_id
                         WHERE a.buying_auth_name = @issuer"; // ✅ Filter condition

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@issuer", selectedIssuer);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MTender tender = new MTender
                    {
                        TenderId = reader.GetString(0),
                        TenderTitle = reader.GetString(1),
                        Description = reader.GetString(2),
                        StartDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),

                        // ✅ Handle null close date safely
                        CloseDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd"),

                        TenderStatus = reader.GetString(5),
                        BrandName = reader.GetString(6),
                        BuyingAuthorityName = reader.GetString(7),
                        TenderBidAmount = reader.GetDouble(8),

                        // ✅ Handle null total amount safely
                        TenderTotalAmount = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9),

                        TenderItemQty = reader.GetInt32(10),
                        TenderItem = reader.GetString(11),
                        TenderDeadline = reader.GetDateTime(12).ToString("yyyy-MM-dd"),
                        TenderUnitPrice = reader.GetDouble(13)
                    };
                    tenders.Add(tender);
                }
            }

            return tenders;
        }



        //Get tenders grid view by status for filtering
        public static List<MTender> GetTendersByStatus(string selectedStatus)
        {
            List<MTender> tenders = new List<MTender>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                string query = @"SELECT 
                            t.tender_id,
                            t.tender_title,
                            t.tender_description,
                            t.tender_start_date,
                            t.tender_close_date,
                            t.tender_status,
                            b.item_brand_name,
                            a.buying_auth_name,
                            t.tender_bid_amt,
                            t.tender_total_amt,
                            t.tender_item_qty,
                            t.tender_item,
                            t.tender_deadline,
                            t.tender_unit_price
                         FROM Tender t
                         INNER JOIN ItemBrand b ON t.brand_id = b.brand_id
                         INNER JOIN BuyingAuthority a ON t.buying_auth_id = a.buying_auth_id
                         WHERE t.tender_status = @status"; // ✅ Filter condition

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@status", selectedStatus);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MTender tender = new MTender
                    {
                        TenderId = reader.GetString(0),
                        TenderTitle = reader.GetString(1),
                        Description = reader.GetString(2),
                        StartDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),

                        // ✅ Handle null close date safely
                        CloseDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd"),

                        TenderStatus = reader.GetString(5),
                        BrandName = reader.GetString(6),
                        BuyingAuthorityName = reader.GetString(7),
                        TenderBidAmount = reader.GetDouble(8),

                        // ✅ Handle null total amount safely
                        TenderTotalAmount = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9),

                        TenderItemQty = reader.GetInt32(10),
                        TenderItem = reader.GetString(11),
                        TenderDeadline = reader.GetDateTime(12).ToString("yyyy-MM-dd"),
                        TenderUnitPrice = reader.GetDouble(13)
                    };
                    tenders.Add(tender);
                }
            }

            return tenders;
        }

        public static List<MTender> SearchTenders(string searchText)
        {
            List<MTender> tenders = new List<MTender>();

            using (SqlConnection con = Database.DBConnection.CreateConnection())
            {
                // Search by TenderId (numeric part) or Tender Title (string)
                string query = @"SELECT 
                            t.tender_id,
                            t.tender_title,
                            t.tender_description,
                            t.tender_start_date,
                            t.tender_close_date,
                            t.tender_status,
                            b.item_brand_name,
                            a.buying_auth_name,
                            t.tender_bid_amt,
                            t.tender_total_amt,
                            t.tender_item_qty,
                            t.tender_item,
                            t.tender_deadline,
                            t.tender_unit_price
                         FROM Tender t
                         INNER JOIN ItemBrand b ON t.brand_id = b.brand_id
                         INNER JOIN BuyingAuthority a ON t.buying_auth_id = a.buying_auth_id 
                         WHERE CAST(t.tender_id AS NVARCHAR(10)) LIKE @search OR t.tender_title LIKE @search";// ✅ Filter condition 

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        MTender tender = new MTender
                        {
                            TenderId = reader.GetString(0),
                            TenderTitle = reader.GetString(1),
                            Description = reader.GetString(2),
                            StartDate = reader.GetDateTime(3).ToString("yyyy-MM-dd"),

                            // ✅ Handle null close date safely
                            CloseDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd"),

                            TenderStatus = reader.GetString(5),
                            BrandName = reader.GetString(6),
                            BuyingAuthorityName = reader.GetString(7),
                            TenderBidAmount = reader.GetDouble(8),

                            // ✅ Handle null total amount safely
                            TenderTotalAmount = reader.IsDBNull(9) ? 0.0 : reader.GetDouble(9),

                            TenderItemQty = reader.GetInt32(10),
                            TenderItem = reader.GetString(11),
                            TenderDeadline = reader.GetDateTime(12).ToString("yyyy-MM-dd"),
                            TenderUnitPrice = reader.GetDouble(13)
                        };
                        tenders.Add(tender);
                    }
                }
            }

            return tenders;
        }




    }
}
