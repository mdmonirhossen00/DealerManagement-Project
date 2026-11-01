using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DealerManagement.Model.Common;
using DealerManagement.Repository.Base;
using DealerManagement.Repository.Interfaces;

namespace DealerManagement.Repository.Repositories
{
    public class CommonRepository : BaseRepository, ICommonRepository
    {
        public CommonRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<DropdownModel>> GetCategoryDropdownAsync()
        {
            const string sql = "SELECT CAST(ProductCategoryID AS NVARCHAR(30)) AS [Name], CategoryName AS [Value] FROM dbo.ProductCategory WHERE IsActive = 1 ORDER BY CategoryName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetProductNameDropdownAsync(int? categoryId = null)
        {
            string sql = "SELECT CAST(ProductNameID AS NVARCHAR(30)) AS [Name], (ProductCode + ' - ' + ProductName) AS [Value] FROM dbo.ProductName WHERE IsActive = 1";
            if (categoryId.HasValue && categoryId > 0)
            {
                sql += " AND ProductCategoryID = " + categoryId.Value;
            }
            sql += " ORDER BY ProductName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetSizeDropdownAsync()
        {
            const string sql = "SELECT CAST(ProductSizeID AS NVARCHAR(30)) AS [Name], SizeName AS [Value] FROM dbo.ProductSize WHERE IsActive = 1 ORDER BY SortOrder, SizeName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetColorDropdownAsync()
        {
            const string sql = "SELECT CAST(ProductColorID AS NVARCHAR(30)) AS [Name], ColorName AS [Value] FROM dbo.ProductColor WHERE IsActive = 1 ORDER BY ColorName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetGradeDropdownAsync()
        {
            const string sql = "SELECT CAST(ProductGradeID AS NVARCHAR(30)) AS [Name], GradeName AS [Value] FROM dbo.ProductGrade WHERE IsActive = 1 ORDER BY GradeName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetProductDropdownAsync()
        {
            const string sql = @"
                SELECT 
                    CAST(p.ProductID AS NVARCHAR(30)) AS [Name],
                    (p.ProductCode + ' - ' + pn.ProductName + ISNULL(' (' + s.SizeName + ')', '') + ISNULL(' - ' + c.ColorName, '')) AS [Value]
                FROM dbo.ProductSetup p
                INNER JOIN dbo.ProductName pn ON p.ProductNameID = pn.ProductNameID
                LEFT JOIN dbo.ProductSize s ON p.ProductSizeID = s.ProductSizeID
                LEFT JOIN dbo.ProductColor c ON p.ProductColorID = c.ProductColorID
                WHERE p.IsActive = 1
                ORDER BY pn.ProductName, p.ProductCode";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetCustomerDropdownAsync()
        {
            const string sql = "SELECT CAST(CustomerID AS NVARCHAR(30)) AS [Name], (CustomerCode + ' - ' + CustomerName + ' (' + CustomerType + ')') AS [Value] FROM dbo.Customer WHERE IsActive = 1 ORDER BY CustomerName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetSupplierDropdownAsync()
        {
            const string sql = "SELECT CAST(SupplierID AS NVARCHAR(30)) AS [Name], (SupplierCode + ' - ' + SupplierName) AS [Value] FROM dbo.Supplier WHERE IsActive = 1 ORDER BY SupplierName";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetPurchaseOrderDropdownAsync(int? supplierId = null)
        {
            string sql = "SELECT CAST(PurchaseOrderID AS NVARCHAR(30)) AS [Name], (PurchaseOrderNo + ' (' + FORMAT(OrderDate, 'yyyy-MM-dd') + ')') AS [Value] FROM dbo.PurchaseOrderMaster WHERE Status <> 'Cancelled'";
            if (supplierId.HasValue && supplierId > 0)
            {
                sql += " AND SupplierID = " + supplierId.Value;
            }
            sql += " ORDER BY OrderDate DESC";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetPurchaseReceiptDropdownAsync(int? supplierId = null)
        {
            string sql = "SELECT CAST(PurchaseReceiptID AS NVARCHAR(30)) AS [Name], (ReceiptNo + ' (' + FORMAT(ReceiptDate, 'yyyy-MM-dd') + ')') AS [Value] FROM dbo.PurchaseReceiptMaster WHERE Status <> 'Cancelled'";
            if (supplierId.HasValue && supplierId > 0)
            {
                sql += " AND SupplierID = " + supplierId.Value;
            }
            sql += " ORDER BY ReceiptDate DESC";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetCustomerOrderDropdownAsync(int? customerId = null)
        {
            string sql = "SELECT CAST(CustomerOrderID AS NVARCHAR(30)) AS [Name], (CustomerOrderNo + ' (' + FORMAT(OrderDate, 'yyyy-MM-dd') + ')') AS [Value] FROM dbo.CustomerOrderMaster WHERE Status <> 'Cancelled'";
            if (customerId.HasValue && customerId > 0)
            {
                sql += " AND CustomerID = " + customerId.Value;
            }
            sql += " ORDER BY OrderDate DESC";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<List<DropdownModel>> GetCustomerDeliveryDropdownAsync(int? customerId = null)
        {
            string sql = "SELECT CAST(CustomerDeliveryID AS NVARCHAR(30)) AS [Name], (DeliveryNo + ' (' + FORMAT(DeliveryDate, 'yyyy-MM-dd') + ')') AS [Value] FROM dbo.CustomerDeliveryMaster WHERE Status <> 'Cancelled'";
            if (customerId.HasValue && customerId > 0)
            {
                sql += " AND CustomerID = " + customerId.Value;
            }
            sql += " ORDER BY DeliveryDate DESC";
            return await ExecuteDropdownQueryAsync(sql);
        }

        public async Task<decimal> GetProductPriceAsync(int productId, string priceType)
        {
            const string sql = @"
                SELECT TOP 1 UnitPrice 
                FROM dbo.PriceSetup 
                WHERE ProductID = @ProductID AND PriceType = @PriceType AND IsActive = 1 AND EffectiveFrom <= CAST(GETDATE() AS DATE) AND (EffectiveTo IS NULL OR EffectiveTo >= CAST(GETDATE() AS DATE))
                ORDER BY EffectiveFrom DESC";

            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            cmd.Parameters.Add("@ProductID", SqlDbType.Int).Value = productId;
            cmd.Parameters.Add("@PriceType", SqlDbType.NVarChar, 30).Value = priceType;

            await conn.OpenAsync();
            var val = await cmd.ExecuteScalarAsync();
            return val != null && val != DBNull.Value ? Convert.ToDecimal(val) : 0;
        }

        private async Task<List<DropdownModel>> ExecuteDropdownQueryAsync(string sql)
        {
            var list = new List<DropdownModel>();
            using var conn = GetConnection();
            using var cmd = CreateCommand(sql, conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DropdownModel
                {
                    Name = reader.GetString(0),
                    Value = reader.GetString(1)
                });
            }
            return list;
        }
    }
}

