using System.Collections.Generic;

namespace DealerManagement.Model.KendoCommon
{
    public class AzFilter
    {
        public class GridFilter
        {
            public string? Operator { get; set; }
            public string? Field { get; set; }
            public string? Value { get; set; }
        }

        public class GridFilters
        {
            public List<GridFilter>? Filters { get; set; }
            public string? Logic { get; set; }
        }

        public class GridSort
        {
            public string? field { get; set; }
            public string? dir { get; set; }
        }
    }
}

