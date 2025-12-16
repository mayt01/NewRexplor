using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rexplor.Models;
using System.Reflection.Emit;

namespace Rexplor.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // جداول اصلی پروژه
        public DbSet<DataFile> DataFiles { get; set; }
        public DbSet<DataFileCategory> DataFileCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // جداول دیگه (اگر لازم نیستن می‌تونی حذفشون کنی)
        public DbSet<ContactUsMessage> ContactUsMessages { get; set; }
        public DbSet<MainPageSliderImage> MainPageSliderImages { get; set; }
        public DbSet<MainPageVoice> MainPageVoices { get; set; }
        public DbSet<PI> PIs { get; set; }
        //public DbSet<Consultation> Consultations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ✅ این خط خیلی مهمه! حتماً اول باشه
            base.OnModelCreating(builder);

            // ✅ ایجاد ایندکس یونیک برای UniqueCode
            builder.Entity<DataFile>()
                .HasIndex(p => p.UniqueCode)
                .IsUnique();

            // ✅ مقداردهی اولیه برای کد یکتا
            builder.Entity<DataFile>()
                .Property(p => p.UniqueCode)
                .HasDefaultValueSql("'DF' + LEFT(NEWID(), 8)");

            // ۵. تنظیمات جداول دیگه (اگر لازمه)
            builder.Entity<MainPageSliderImage>()
                  .Ignore(i => i.ImageFile);

            builder.Entity<MainPageVoice>()
                  .Ignore(i => i.VoiceFile);
        }
    }
}