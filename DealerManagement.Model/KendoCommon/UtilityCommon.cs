using System;
using System.Collections.Generic;

namespace DealerManagement.Model.KendoCommon
{
    public class UtilityCommon
    {
        public static string BuildWhereClause<T>(int index, string logic, AzFilter.GridFilter filter, List<object> parameters)
        {
            if (string.IsNullOrEmpty(filter.Field) || string.IsNullOrEmpty(filter.Operator) || filter.Value == null)
            {
                return "1=1";
            }

            var safeField = filter.Field.Replace("'", "''");
            var safeValue = filter.Value.Trim().Replace("'", "''");

            switch (filter.Operator.ToLower())
            {
                case "eq":
                    return $"({safeField} = '{safeValue}')";
                case "neq":
                    return $"({safeField} != '{safeValue}')";
                case "gte":
                    return $"({safeField} >= '{safeValue}')";
                case "gt":
                    return $"({safeField} > '{safeValue}')";
                case "lte":
                    return $"({safeField} <= '{safeValue}')";
                case "lt":
                    return $"({safeField} < '{safeValue}')";
                case "startswith":
                    return $"({safeField} LIKE '{safeValue}%')";
                case "endswith":
                    return $"({safeField} LIKE '%{safeValue}')";
                case "contains":
                    return $"({safeField} LIKE '%{safeValue}%')";
                case "doesnotcontain":
                    return $"({safeField} NOT LIKE '%{safeValue}%')";
                default:
                    return $"({safeField} LIKE '%{safeValue}%')";
            }
        }

        public static string ToLinqOperator(string? logic)
        {
            return (logic != null && logic.ToLower() == "or") ? "OR" : "AND";
        }
    }
}

