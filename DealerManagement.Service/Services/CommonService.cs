using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Service.Interfaces;

namespace DealerManagement.Service.Services
{
    public class CommonService : ICommonService
    {
        private readonly ICommonRepository _commonRepository;

        public CommonService(ICommonRepository commonRepository)
        {
            _commonRepository = commonRepository;
        }

        public Task<List<DropdownModel>> GetCategoryDropdownAsync() => _commonRepository.GetCategoryDropdownAsync();
        public Task<List<DropdownModel>> GetProductNameDropdownAsync(int? categoryId = null) => _commonRepository.GetProductNameDropdownAsync(categoryId);
        public Task<List<DropdownModel>> GetSizeDropdownAsync() => _commonRepository.GetSizeDropdownAsync();
        public Task<List<DropdownModel>> GetColorDropdownAsync() => _commonRepository.GetColorDropdownAsync();
        public Task<List<DropdownModel>> GetGradeDropdownAsync() => _commonRepository.GetGradeDropdownAsync();
        public Task<List<DropdownModel>> GetProductDropdownAsync() => _commonRepository.GetProductDropdownAsync();
        public Task<List<DropdownModel>> GetCustomerDropdownAsync() => _commonRepository.GetCustomerDropdownAsync();
        public Task<List<DropdownModel>> GetSupplierDropdownAsync() => _commonRepository.GetSupplierDropdownAsync();
        public Task<List<DropdownModel>> GetPurchaseOrderDropdownAsync(int? supplierId = null) => _commonRepository.GetPurchaseOrderDropdownAsync(supplierId);
        public Task<List<DropdownModel>> GetPurchaseReceiptDropdownAsync(int? supplierId = null) => _commonRepository.GetPurchaseReceiptDropdownAsync(supplierId);
        public Task<List<DropdownModel>> GetCustomerOrderDropdownAsync(int? customerId = null) => _commonRepository.GetCustomerOrderDropdownAsync(customerId);
        public Task<List<DropdownModel>> GetCustomerDeliveryDropdownAsync(int? customerId = null) => _commonRepository.GetCustomerDeliveryDropdownAsync(customerId);
        public Task<decimal> GetProductPriceAsync(int productId, string priceType) => _commonRepository.GetProductPriceAsync(productId, priceType);
    }
}

