using System.Collections.Generic;

namespace DealerManagement.Model.KendoCommon
{
    public class GridOptions
    {
        public int skip { get; set; }
        public int take { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<AzFilter.GridSort>? sort { get; set; }
        public AzFilter.GridFilters? filter { get; set; }
    }

    public class GridColumns
    {
        public string? field { get; set; }
        public string? title { get; set; }
        public string? width { get; set; }
        public bool filterable { get; set; }
        public bool sortable { get; set; }
        public bool hidden { get; set; }
    }

    public class GridEntity<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public List<GridColumns> Columnses { get; set; } = new List<GridColumns>();
    }

    public class GridResult<T>
    {
        public GridEntity<T> Data(List<T> list, int totalCount)
        {
            return new GridEntity<T>
            {
                Items = list ?? new List<T>(),
                TotalCount = totalCount,
                Columnses = new List<GridColumns>()
            };
        }
    }
}

