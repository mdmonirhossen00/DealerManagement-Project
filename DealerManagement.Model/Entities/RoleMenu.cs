using System;

namespace DealerManagement.Model.Entities
{
    public class RoleMenu
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }

        // Display / Mapping properties matching ShampanERP
        public string? RoleName { get; set; }
        public string? MenuName { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Url { get; set; }
        public int ParentId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsChecked { get; set; }
    }
}

