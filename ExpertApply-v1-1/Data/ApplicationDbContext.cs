//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
//using Rexplor.Models;
//using System.Reflection.Emit;

//namespace Rexplor.Data
//{
//    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }

//        // جداول اصلی پروژه
//        public DbSet<DataFile> DataFiles { get; set; }
//        public DbSet<DataFileCategory> DataFileCategories { get; set; }
//        public DbSet<Order> Orders { get; set; }
//        public DbSet<OrderItem> OrderItems { get; set; }

//        // جداول دیگه (اگر لازم نیستن می‌تونی حذفشون کنی)
//        public DbSet<ContactUsMessage> ContactUsMessages { get; set; }
//        public DbSet<MainPageSliderImage> MainPageSliderImages { get; set; }
//        public DbSet<MainPageVoice> MainPageVoices { get; set; }
//        public DbSet<PI> PIs { get; set; }
//        //public DbSet<Consultation> Consultations { get; set; }

//        protected override void OnModelCreating(ModelBuilder builder)
//        {
//            // ✅ این خط خیلی مهمه! حتماً اول باشه
//            base.OnModelCreating(builder);

//            // ✅ ایجاد ایندکس یونیک برای UniqueCode
//            builder.Entity<DataFile>()
//                .HasIndex(p => p.UniqueCode)
//                .IsUnique();

//            // ✅ مقداردهی اولیه برای کد یکتا
//            builder.Entity<DataFile>()
//                .Property(p => p.UniqueCode)
//                .HasDefaultValueSql("'DF' + LEFT(NEWID(), 8)");

//            // ۵. تنظیمات جداول دیگه (اگر لازمه)
//            builder.Entity<MainPageSliderImage>()
//                  .Ignore(i => i.ImageFile);

//            builder.Entity<MainPageVoice>()
//                  .Ignore(i => i.VoiceFile);
//        }
//    }
//}


using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rexplor.Models;
using System.Reflection.Emit;

namespace Rexplor.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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


        // جداول تخفیف
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<FileDiscount> FileDiscounts { get; set; }
        public DbSet<DiscountUsage> DiscountUsages { get; set; }

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

            // ========== تنظیمات جداول تخفیف ==========

            // تنظیمات جدول Discount
            builder.Entity<Discount>()
                .HasIndex(d => d.Code)
                .IsUnique();

            builder.Entity<Discount>()
                .Property(d => d.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.Entity<Discount>()
                .Property(d => d.DiscountPercent)
                .IsRequired();

            builder.Entity<Discount>()
                .Property(d => d.StartDate)
                .IsRequired();

            builder.Entity<Discount>()
                .Property(d => d.EndDate)
                .IsRequired();

            builder.Entity<Discount>()
                .Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Entity<Discount>()
                .Property(d => d.MinPurchaseAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Discount>()
                .Property(d => d.MaxDiscountAmount)
                .HasColumnType("decimal(18,2)");

            // تنظیمات جدول FileDiscount (کلید ترکیبی)
            builder.Entity<FileDiscount>()
                .HasKey(fd => new { fd.FileId, fd.DiscountId });

            builder.Entity<FileDiscount>()
                .HasOne(fd => fd.File)
                .WithMany()
                .HasForeignKey(fd => fd.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FileDiscount>()
                .HasOne(fd => fd.Discount)
                .WithMany(d => d.FileDiscounts)
                .HasForeignKey(fd => fd.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

            // تنظیمات جدول DiscountUsage
            // تنظیمات جدول DiscountUsage
            builder.Entity<DiscountUsage>()
                .HasOne(du => du.User)
                .WithMany()
                .HasForeignKey(du => du.UserId)
                .OnDelete(DeleteBehavior.NoAction); // تغییر به NoAction

            builder.Entity<DiscountUsage>()
                .HasOne(du => du.Discount)
                .WithMany(d => d.DiscountUsages)
                .HasForeignKey(du => du.DiscountId)
                .OnDelete(DeleteBehavior.NoAction); // تغییر به NoAction

            builder.Entity<DiscountUsage>()
                .HasOne(du => du.File)
                .WithMany()
                .HasForeignKey(du => du.FileId)
                .OnDelete(DeleteBehavior.NoAction); // تغییر به NoAction

            builder.Entity<DiscountUsage>()
                .HasOne(du => du.Order)
                .WithMany()
                .HasForeignKey(du => du.OrderId)
                .OnDelete(DeleteBehavior.NoAction); // تغییر به NoAction

            // ========== تنظیمات موجود قبلی ==========

            // تنظیمات Order و OrderItem
            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.DataFile)
                .WithMany()
                .HasForeignKey(oi => oi.DataFileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            //builder.Entity<Order>()
            //    .Property(o => o.FinalAmount)
            //    .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.UsedDiscountCode)
                .HasMaxLength(50);

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // تنظیمات DataFile
            builder.Entity<DataFile>()
                .Property(f => f.Price)
                .HasColumnType("decimal(18,2)");

            // تنظیمات DataFileCategory
            builder.Entity<DataFileCategory>()
                .HasMany(c => c.DataFiles)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}