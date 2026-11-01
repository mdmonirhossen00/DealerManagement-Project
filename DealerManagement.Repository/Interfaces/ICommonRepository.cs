using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;

namespace DealerManagement.Repository.Interfaces
{
    public interface ICommonRepository
    {
        Task<List<DropdownModel>> GetCategoryDropdownAsync();
        Task<List<DropdownModel>> GetProductNameDropdownAsync(int? categoryId = null);
        Task<List<DropdownModel>> GetSizeDropdownAsync();
        Task<List<DropdownModel>> GetColorDropdownAsync();
        Task<List<DropdownModel>> GetGradeDropdownAsync();
        Task<List<DropdownModel>> GetProductDropdownAsync();
        Task<List<DropdownModel>> GetCustomerDropdownAsync();
        Task<List<DropdownModel>> GetSupplierDropdownAsync();
        Task<List<DropdownModel>> GetPurchaseOrderDropdownAsync(int? supplierId = null);
        Task<List<DropdownModel>> GetPurchaseReceiptDropdownAsync(int? supplierId = null);
        Task<List<DropdownModel>> GetCustomerOrderDropdownAsync(int? customerId = null);
        Task<List<DropdownModel>> GetCustomerDeliveryDropdownAsync(int? customerId = null);
        Task<decimal> GetProductPriceAsync(int productId, string priceType);
    }
}

