using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TMS_NET8.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TMS_NET8.Controller
{
    class CTender
    {
        //Add new tender
        public void RegisterTender(string tenderId, string tenderTitle, string tenderIssuer, string tenderItem, string tenderItemBrand, 
            double tenderUnitPrice, double tenderBidAmount, int tenderQuantity, string tenderDeadline, string tenderDesc, string tenderStatus, string tenderStartDate)
        {
            MTender mt = new MTender();
            mt.RegisterTender(tenderId, tenderTitle, tenderIssuer, tenderItem, tenderItemBrand, tenderUnitPrice,
                tenderBidAmount, tenderQuantity, tenderDeadline, tenderDesc, tenderStatus, tenderStartDate);
        }

        //Update tender
        public bool UpdateTender(MTender tender)
        {
            try
            {
                MTender mt = new MTender();
                mt.UpdateTender(
                    tender.TenderId!, // 👈 add "!" to tell compiler it's definitely not null
                    tender.TenderTitle!,
                    tender.TenderDeadline!,
                    tender.Description!,
                    tender.BuyingAuthorityName!,
                    tender.TenderItem!,
                    tender.BrandName!,
                    tender.TenderUnitPrice,
                    tender.TenderBidAmount,
                    tender.TenderItemQty);

                return true; // Return true if update succeeded
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while updating tender: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        //Delete tender
        public bool DeleteTender(string tenderId)
        {
            MTender model = new MTender();
            return model.DeleteTender(tenderId);
        }

        // Load unique tender issuers for combo box
        public List<string> LoadDataComboTenderIssuer()
        {
            return MTender.GetUniqueTenderIssuers();
        }

        public List<string> LoadDataComboTenderStatus()
        {
            return MTender.GetUniqueTenderStatus();
        }

        // Load unique tender items for combo box
        public List<string> LoadDataComboTenderItems()
        {
            return MTender.GetUniqueTenderItems();
        }

        // Load item brands based on selected item
        public List<string> GetItemBrandsByItem(string itemName)
        {
            return MTender.GetItemBrandsByItem(itemName);
        }

        //Get unit price based on selected brand
        public double GetItemBrandUnitPrice(string brandName)
        {
            return MTender.GetItemBrandUnitPrice(brandName);
        }

        //Data Grid view data load
        public List<MTender> LoadAllTenders()
        {
            return MTender.GetAllTenders();
        }
        //Get tenders grid view by issuer for filtering
        public List<MTender> GetTendersByIssuer(string selectedIssuer)
        {
            return MTender.GetTendersByIssuer(selectedIssuer);
        }

        //Get tenders grid view by status for filtering
        public List<MTender> GetTendersByStatus(string selectedStatus)
        {
            return MTender.GetTendersByStatus(selectedStatus);
        }

        //Search tenders
        public List<MTender> SearchTenders(string searchText)
        {
            return MTender.SearchTenders(searchText);
        }



    }
}
