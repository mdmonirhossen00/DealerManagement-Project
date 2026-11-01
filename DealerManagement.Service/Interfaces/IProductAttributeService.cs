using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface IProductAttributeService
    {
        // Color
        Task<ProductColor?> GetColorByIdAsync(int id);
        Task<(List<ProductColor> Items, int TotalCount)> GetColorGridDataAsync(GridOptions options);
        Task<ResultModel<ProductColor>> SaveColorAsync(ProductColor model);
        Task<ResultModel<bool>> DeleteColorAsync(int id);

        // Size
        Task<ProductSize?> GetSizeByIdAsync(int id);
        Task<(List<ProductSize> Items, int TotalCount)> GetSizeGridDataAsync(GridOptions options);
        Task<ResultModel<ProductSize>> SaveSizeAsync(ProductSize model);
        Task<ResultModel<bool>> DeleteSizeAsync(int id);

        // Grade
        Task<ProductGrade?> GetGradeByIdAsync(int id);
        Task<(List<ProductGrade> Items, int TotalCount)> GetGradeGridDataAsync(GridOptions options);
        Task<ResultModel<ProductGrade>> SaveGradeAsync(ProductGrade model);
        Task<ResultModel<bool>> DeleteGradeAsync(int id);
    }
}

