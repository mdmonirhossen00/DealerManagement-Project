using System;
using System.Collections.Generic;

namespace DealerManagement.Model.KendoCommon
{
    public static class GridQueryBuilder<T>
    {
        public static string Query(GridOptions options, string query, string orderBy, string gridCondition = "")
        {
            string condition = "";
            if (options != null && options.filter != null)
            {
                condition = FilterCondition(options.filter);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                condition = " WHERE " + condition;
            }

            if (!string.IsNullOrEmpty(gridCondition))
            {
                if (string.IsNullOrEmpty(condition))
                {
                    condition = " WHERE " + gridCondition;
                }
                else
                {
                    condition += " AND " + gridCondition;
                }
            }

            string orderby = "";
            if (options != null && options.sort != null && options.sort.Count > 0)
            {
                foreach (var gridSort in options.sort)
                {
                    string cleanField = CleanColumnPrefix(gridSort.field);
                    if (string.IsNullOrEmpty(orderby))
                    {
                        orderby += "ORDER BY " + cleanField + " " + gridSort.dir;
                    }
                    else
                    {
                        orderby += ", " + cleanField + " " + gridSort.dir;
                    }
                }
            }

            if (string.IsNullOrEmpty(orderby))
            {
                if (!string.IsNullOrEmpty(orderBy))
                {
                    orderby = " ORDER BY " + CleanColumnPrefix(orderBy);
                }
                else
                {
                    orderby = " ORDER BY (SELECT 1)";
                }
            }

            int skip = options?.skip ?? 0;
            int pageSize = options?.pageSize ?? 0;
            int pageUpperBound = skip + (options?.take ?? 10);

            if (pageSize == 0)
            {
                return $"{query} {condition} {orderby}";
            }

            return $@"SELECT * FROM (
                SELECT ROW_NUMBER() OVER({orderby}) AS ROWINDEX, T.* FROM ({query}) T {condition}
            ) tbl WHERE ROWINDEX > {skip} AND ROWINDEX <= {pageUpperBound}";
        }

        public static string FilterCondition(AzFilter.GridFilters filter)
        {
            string whereClause = "";

            if (filter != null && filter.Filters != null && filter.Filters.Count > 0)
            {
                var parameters = new List<object>();
                var filters = filter.Filters;

                for (var i = 0; i < filters.Count; i++)
                {
                    if (string.IsNullOrEmpty(filters[i].Value))
                        continue;

                    var clause = UtilityCommon.BuildWhereClause<T>(i, filter.Logic ?? "and", filters[i], parameters);

                    if (string.IsNullOrEmpty(whereClause))
                    {
                        whereClause += clause;
                    }
                    else
                    {
                        whereClause += $" {UtilityCommon.ToLinqOperator(filter.Logic)} {clause}";
                    }
                }
            }
            return whereClause;
        }

        private static string CleanColumnPrefix(string? field)
        {
            if (string.IsNullOrWhiteSpace(field)) return "";
            return System.Text.RegularExpressions.Regex.Replace(field, @"\b[a-zA-Z0-9_]+\.([a-zA-Z0-9_]+)\b", "$1");
        }
    }
}

