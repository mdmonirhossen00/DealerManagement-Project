using System.Collections.Generic;
using System.Threading.Tasks;
using DealerManagement.Model.Entities;
using DealerManagement.Model.KendoCommon;

namespace DealerManagement.Repository.Interfaces
{
    public interface ISampleDeliveryRepository
    {
        Task<SampleDeliveryMaster?> GetByIdWithDetailsAsync(long id);
        Task<(List<SampleDeliveryMaster> Items, int TotalCount)> GetGridDataAsync(GridOptions options);
        Task<long> InsertAsync(SampleDeliveryMaster master);
        Task<bool> UpdateAsync(SampleDeliveryMaster master);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsByNoAsync(string sampleNo, long excludeId = 0);
    }
}

