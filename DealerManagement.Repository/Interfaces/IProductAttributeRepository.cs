using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface IProductAttributeRepository
    {
        // Color
        Task<ProductColor?> GetColorByIdAsync(int id);
        Task<(List<ProductColor> Items, int TotalCount)> GetColorGridDataAsync(GridOptions options);
        Task<int> InsertColorAsync(ProductColor model);
        Task<bool> UpdateColorAsync(ProductColor model);
        Task<bool> DeleteColorAsync(int id);

        // Size
        Task<ProductSize?> GetSizeByIdAsync(int id);
        Task<(List<ProductSize> Items, int TotalCount)> GetSizeGridDataAsync(GridOptions options);
        Task<int> InsertSizeAsync(ProductSize model);
        Task<bool> UpdateSizeAsync(ProductSize model);
        Task<bool> DeleteSizeAsync(int id);

        // Grade
        Task<ProductGrade?> GetGradeByIdAsync(int id);
        Task<(List<ProductGrade> Items, int TotalCount)> GetGradeGridDataAsync(GridOptions options);
        Task<int> InsertGradeAsync(ProductGrade model);
        Task<bool> UpdateGradeAsync(ProductGrade model);
        Task<bool> DeleteGradeAsync(int id);
    }
}

