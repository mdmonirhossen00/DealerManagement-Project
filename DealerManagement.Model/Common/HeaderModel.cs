using System.Collections.Generic;

namespace DealerManagement.Model.Common
{
    public class HeaderModel
    {
        public string HeaderName { get; set; } = string.Empty;
        public Dictionary<string, string> BreadCrums { get; set; } = new();
    }
}

