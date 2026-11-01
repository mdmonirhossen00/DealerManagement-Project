using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DealerManagement.Repository.Base
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        public BaseRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DealerManagementDB")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DealerManagementDB' not found in configuration.");
        }

        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected SqlCommand CreateCommand(string query, SqlConnection connection, SqlTransaction? transaction = null)
        {
            return new SqlCommand(query, connection, transaction);
        }

        protected SqlDataAdapter CreateAdapter(SqlCommand command)
        {
            return new SqlDataAdapter(command);
        }

        public string StringReplacing(string stringToReplace)
        {
            string newString = stringToReplace;
            if (stringToReplace.Contains("."))
            {
                newString = Regex.Replace(stringToReplace, @"^[^.]*\.", "", RegexOptions.IgnorePatternWhitespace);
            }
            newString = newString.Replace(">", "From");
            newString = newString.Replace("<", "To");
            newString = newString.Replace("!", "");
            newString = newString.Replace("[", "");
            newString = newString.Replace("]", "");
            return newString;
        }

        public string ApplyConditions(string sqlText, string[]? conditionalFields, string[]? conditionalValue, bool orOperator = false)
        {
            if (conditionalFields == null || conditionalValue == null || conditionalFields.Length != conditionalValue.Length)
            {
                return sqlText;
            }

            var hasAnyValue = conditionalValue.Any(x => !string.IsNullOrWhiteSpace(x));
            if (!hasAnyValue) return sqlText;

            if (orOperator)
            {
                sqlText += " AND (";
            }

            bool conditionFlag = true;
            for (int i = 0; i < conditionalFields.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(conditionalFields[i]) || string.IsNullOrWhiteSpace(conditionalValue[i]))
                {
                    continue;
                }

                string cField = StringReplacing(conditionalFields[i]);
                string operand = " AND ";

                if (orOperator)
                {
                    operand = conditionFlag ? " " : " OR ";
                    conditionFlag = false;
                }

                if (conditionalFields[i].ToLower().Contains("like"))
                {
                    sqlText += operand + conditionalFields[i] + " '%' + @" + cField.Replace("like", "", StringComparison.OrdinalIgnoreCase).Trim() + " + '%'";
                }
                else if (conditionalFields[i].Contains(">") || conditionalFields[i].Contains("<"))
                {
                    sqlText += operand + conditionalFields[i] + " @" + cField;
                }
                else
                {
                    sqlText += operand + conditionalFields[i] + " = @" + cField;
                }
            }

            if (orOperator)
            {
                sqlText += " )";
            }

            return sqlText;
        }

        public SqlCommand ApplyParameters(SqlCommand objComm, string[]? conditionalFields, string[]? conditionalValue)
        {
            if (conditionalFields == null || conditionalValue == null || conditionalFields.Length != conditionalValue.Length)
            {
                return objComm;
            }

            for (int j = 0; j < conditionalFields.Length; j++)
            {
                if (string.IsNullOrWhiteSpace(conditionalFields[j]) || string.IsNullOrWhiteSpace(conditionalValue[j]))
                {
                    continue;
                }

                string cField = StringReplacing(conditionalFields[j]);

                if (conditionalFields[j].ToLower().Contains("like"))
                {
                    objComm.Parameters.AddWithValue("@" + cField.Replace("like", "", StringComparison.OrdinalIgnoreCase).Trim(), conditionalValue[j]);
                }
                else
                {
                    objComm.Parameters.AddWithValue("@" + cField, conditionalValue[j]);
                }
            }

            return objComm;
        }
    }
}
