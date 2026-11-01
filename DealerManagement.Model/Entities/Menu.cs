using System;

namespace DealerManagement.Model.Entities
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Url { get; set; }
        public string? IconClass { get; set; }
        public int ParentId { get; set; }
        public int SubParentId { get; set; }
        public int SubChildId { get; set; }
        public int DisplayOrder { get; set; }
        public string? Module { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Computed / UI properties matching ShampanERP
        public int TotalChild { get; set; }
        public int TotalSubChild { get; set; }
    }
}

