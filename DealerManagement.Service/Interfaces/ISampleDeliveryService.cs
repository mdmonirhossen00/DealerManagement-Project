using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Common;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Service.Interfaces
{
    public interface ISampleDeliveryService
    {
        Task<SampleDeliveryMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<SampleDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<ResultModel<SampleDeliveryMaster>> InsertAsync(SampleDeliveryMaster master);
        Task<ResultModel<SampleDeliveryMaster>> UpdateAsync(SampleDeliveryMaster master);
        Task<ResultModel<bool>> DeleteAsync(long id);
    }
}

