using System.Collections.Generic;

namespace DealerManagement.Model.ViewModels
{
    public class RoleMenuVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<RoleMenuItemVM> RoleMenuList { get; set; } = new();
    }

    public class RoleMenuItemVM
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string? ParentMenuName { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Url { get; set; }
        public string? IconClass { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsChecked { get; set; }
    }

    public class RoleMenuSaveModel
    {
        public int RoleId { get; set; }
        public List<int> SelectedMenuIds { get; set; } = new();
    }
}

