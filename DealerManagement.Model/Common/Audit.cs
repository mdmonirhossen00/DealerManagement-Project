using System;

namespace DealerManagement.Model.Common
{
    public class Audit
    {
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedFrom { get; set; }
        public string? LastUpdateBy { get; set; }
        public DateTime? LastUpdateAt { get; set; }
        public string? LastUpdateFrom { get; set; }
    }
}

