using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Rexplor.Data;
using Rexplor.Models;
using Rexplor.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Session ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Identity ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // ✅ اصلاح ۲: RequireConfirmedAccount = false برای توسعه
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // ✅ تنظیمات ساده پسورد
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // ✅ تنظیمات کاربر
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();


//builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
//{
//    // ✅ اصلاح ۲: RequireConfirmedAccount = false برای توسعه
//    options.SignIn.RequireConfirmedAccount = false;
//    options.SignIn.RequireConfirmedEmail = false;
//    options.SignIn.RequireConfirmedPhoneNumber = false;

//    // ✅ تنظیمات ساده پسورد
//    options.Password.RequireDigit = false;
//    options.Password.RequiredLength = 6;
//    options.Password.RequireNonAlphanumeric = false;
//    options.Password.RequireUppercase = false;
//    options.Password.RequireLowercase = false;

//    // ✅ تنظیمات کاربر
//    options.User.RequireUniqueEmail = true;
//})
//.AddEntityFrameworkStores<ApplicationDbContext>();

// ✅ اصلاح ۳: پیکربندی Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// --- MVC & Razor Pages ---
builder.Services.AddControllersWithViews();
// بعد از builder.Services.AddControllersWithViews()

// برای دسترسی به HttpContext
builder.Services.AddHttpContextAccessor(); // برای دسترسی به HttpContext در سرویس
builder.Services.AddScoped<IZarinPalService, ZarinPalService>();

builder.Services.AddRazorPages();

// --- Email Service (اگر واقعاً نیاز داری) ---
// اگر EmailService نداری، این خطوط رو کامنت کن
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<EmailService>();
builder.Services.AddTransient<IEmailSender, EmailService>();

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // ✅ اضافه کن
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // ✅ بعد از Routing
app.UseAuthentication(); // ✅ قبل از Authorization
app.UseAuthorization();

// --- Routing ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapControllerRoute(
    name: "codeSearch",
    pattern: "file/{code}",
    defaults: new { controller = "DataFiles", action = "SearchByCode" });

app.Run();