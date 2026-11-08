using Microsoft.AspNetCore.Authentication.Cookies;
using DealerManagement.Repository.Interfaces;
using DealerManagement.Repository.Repositories;
using DealerManagement.Service.Interfaces;
using DealerManagement.Service.Services;
using DealerManagement.UI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(options =>
    {
        // Global Server-Side Menu Authorization Filter
        options.Filters.Add<MenuAuthorizeFilter>();
    })
    .AddJsonOptions(options =>
    {
        // Keep property names PascalCase or camelCase friendly
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Register Authorization Filter
builder.Services.AddScoped<MenuAuthorizeFilter>();

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleMenuRepository, RoleMenuRepository>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();

builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductNameRepository, ProductNameRepository>();
builder.Services.AddScoped<IProductSetupRepository, ProductSetupRepository>();
builder.Services.AddScoped<IPriceSetupRepository, PriceSetupRepository>();
builder.Services.AddScoped<IProductAttributeRepository, ProductAttributeRepository>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();

builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseReceiptRepository, PurchaseReceiptRepository>();
builder.Services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();

builder.Services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
builder.Services.AddScoped<ICustomerDeliveryRepository, CustomerDeliveryRepository>();
builder.Services.AddScoped<ICustomerReturnRepository, CustomerReturnRepository>();

builder.Services.AddScoped<ISampleDeliveryRepository, SampleDeliveryRepository>();
builder.Services.AddScoped<IProductionRepository, ProductionRepository>();
builder.Services.AddScoped<IProductDamageRepository, ProductDamageRepository>();

builder.Services.AddScoped<ICustomerFinancialRepository, CustomerFinancialRepository>();
builder.Services.AddScoped<ISupplierFinancialRepository, SupplierFinancialRepository>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleMenuService, RoleMenuService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ICommonService, CommonService>();

builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductNameService, ProductNameService>();
builder.Services.AddScoped<IProductSetupService, ProductSetupService>();
builder.Services.AddScoped<IPriceSetupService, PriceSetupService>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();

builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();

builder.Services.AddScoped<ICustomerOrderService, CustomerOrderService>();
builder.Services.AddScoped<ICustomerDeliveryService, CustomerDeliveryService>();
builder.Services.AddScoped<ICustomerReturnService, CustomerReturnService>();

builder.Services.AddScoped<ISampleDeliveryService, SampleDeliveryService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IProductDamageService, ProductDamageService>();

builder.Services.AddScoped<ICustomerFinancialService, CustomerFinancialService>();
builder.Services.AddScoped<ISupplierFinancialService, SupplierFinancialService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DealerManagement.Auth";
        options.LoginPath = "/Login/Index";
        options.LogoutPath = "/Login/LogOff";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

// Session Configuration (for menu or flash state)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Automatically seed and sync all menus in dbo.Menu at startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var menuRepo = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        await menuRepo.GetActiveMenusAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error initializing database menus.");
    }
}

app.Run();
