using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Munk.AspNetCore;
using NxB.Allocating.Shared.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    //https://docs.microsoft.com/en-us/ef/core/modeling/relationships
    //https://ardalis.com/encapsulated-collections-in-entity-framework-core
    public class AppDbContext : BaseAppDbContext<AppDbContext>
    {
        // Login/User entities
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserTenantAccess> UserTenantAccess { get; set; }

        // Ordering entities
        public DbSet<Order> Orders { get; set; }
        public DbSet<SubOrderArticle> SubOrders { get; set; }
        public DbSet<SubOrderSection> SubOrderSections { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<AllocationState> AllocationStates { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<SubOrderDiscount> SubOrderDiscounts { get; set; }
        public DbSet<AutoAddState> AutoAdds { get; set; }
        public DbSet<Access> Accesses { get; set; }

        // Accounting entities
        public DbSet<Account> Accounts { get; set; }

        // Inventory entities
        public DbSet<Article> Articles { get; set; }

        public DbSet<AvailabilityMatrix> AvailabilityMatrices { get; set; }
        public DbSet<Allocation> Allocations { get; set; }

        // Voucher entities
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }

        // Additional inventory entities
        public DbSet<RentalUnit> RentalUnits { get; set; }
        public DbSet<TimeSpanBase> TimeSpans { get; set; }

        // Customer entities
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerGroup> CustomerGroups { get; set; }

        // Payment entities
        public DbSet<PaymentCompletion> PaymentCompletions { get; set; }
        public DbSet<PaymentCompletedLock> PaymentCapturedLocks { get; set; }


        public DbSet<Country> Countries { get; set; }

        // Tenant entities (from TenantApi)
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TextSection> TextSections { get; set; }
        public DbSet<TextSectionUser> TextSectionUsers { get; set; }
        public DbSet<BillableItem> BillableItems { get; set; }
        public DbSet<Kiosk> Kiosks { get; set; }
        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<FeatureModuleTenantEntry> FeatureModuleTenantEntries { get; set; }
        public DbSet<ExternalPaymentTransaction> ExternalPaymentTransactions { get; set; }

        // Pricing entities (from PricingApi)
        public DbSet<PriceProfile> PriceProfiles { get; set; }
        public DbSet<CostInterval> CostIntervals { get; set; }

        // TallyWebIntegration entities (from TallyWebIntegrationApi)
        public DbSet<TConMasterRadioTenantMap> TallyMasterRadioTenantMaps { get; set; }
        public DbSet<AccessGroup> AccessGroups { get; set; }
        public DbSet<RadioBilling> RadioBillings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options, null)
        {
            //fixes error https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes se Cascade deletions now happen immediately by default
            ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSession>().ToTable("UserSession");
            modelBuilder.Entity<UserSession>().HasKey(x => x.SessionId);
            modelBuilder.Entity<UserSession>().Property(x => x.SessionId).HasMaxLength(50);
            modelBuilder.Entity<UserSession>().Property(x => x.UserJson).HasMaxLength(1000);

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().Property(x => x.Username).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Password).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.CountryId).HasMaxLength(2).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.IsDisabled).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.IsDeleted).IsRequired();
            modelBuilder.Entity<User>().HasMany(x => x.UserTenantAccesses).WithOne().HasForeignKey(x => x.UserId);

            var navigation = modelBuilder.Entity<User>()
                .Metadata.FindNavigation(nameof(User.UserTenantAccesses));
            navigation.SetField("_userTenantAccesses");
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder.Entity<UserTenantAccess>().ToTable("UserTenantsAccess");
            modelBuilder.Entity<UserTenantAccess>().HasKey(x => new { x.TenantId, x.UserId });

            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<Order>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Order>().HasAlternateKey(x => new { x.FriendlyId, x.TenantId });
            modelBuilder.Entity<Order>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<Order>().Property(x => x.IsDeleted);
            modelBuilder.Entity<Order>().Ignore(x => x.Allocations);
            modelBuilder.Entity<Order>().Property(x => x.Note);

            modelBuilder.Entity<SubOrderArticle>().ToTable("SubOrder");
            modelBuilder.Entity<SubOrderArticle>().HasDiscriminator<string>("Type").HasValue<SubOrderArticle>("suborderarticle").HasValue<SubOrder>("suborder");
            modelBuilder.Entity<SubOrderArticle>().Property("Type").HasMaxLength(15);
            modelBuilder.Entity<SubOrderArticle>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<SubOrderArticle>().Property(x => x.Note).HasMaxLength(500);

            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.ArticleOrderLines);
            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.NotPersistedArticleOrderLines);
            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.DiscountOrderLines);
            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.NotPersistedDiscountOrderLines);
            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.SubOrderDiscountLines);
            modelBuilder.Entity<SubOrderArticle>().Ignore(x => x.NotPersistedSubOrderDiscountLines);
            // modelBuilder.Entity<SubOrderArticle>().HasMany(x => x.OrderLines).WithOne().HasForeignKey(x => x.SubOrderId);

            modelBuilder.Entity<SubOrder>().Ignore(x => x.DateInterval);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.AllocationOrderLines);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.Allocations);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.TimedBasedOrderLines);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.ResourceBasedOrderLines);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.NotPersistedAllocationOrderLines);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.GuestOrderLines);
            modelBuilder.Entity<SubOrder>().Ignore(x => x.NotPersistedGuestOrderLines);

            modelBuilder.Entity<SubOrderSection>().ToTable("SubOrderSection");
            modelBuilder.Entity<SubOrderSection>().Property(x => x.Id).ValueGeneratedNever();

            //when working, change to orderLine (.net core cannot handle derived types owning complex types yet)
            modelBuilder.Entity<OrderLine>();
            modelBuilder.Entity<OrderLine>().ToTable("OrderLine");
            modelBuilder.Entity<OrderLine>().HasDiscriminator<string>("Type").HasValue<OrderLine>("default").HasValue<AllocationOrderLine>("allocation").HasValue<GuestOrderLine>("guest").HasValue<ArticleOrderLine>("article").HasValue<DiscountOrderLine>("discount").HasValue<SubOrderDiscountLine>("sudiscount");
            modelBuilder.Entity<OrderLine>().Property("Type").HasMaxLength(10);
            modelBuilder.Entity<OrderLine>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<OrderLine>().Property(x => x.SubOrderId).IsRequired();
            modelBuilder.Entity<OrderLine>().Property(x => x.Text).IsRequired();
            modelBuilder.Entity<OrderLine>().Property(x => x.Index).HasPrecision(14, 6);
            modelBuilder.Entity<OrderLine>().Property(x => x.Text).HasMaxLength(100);
            modelBuilder.Entity<OrderLine>().Property(x => x.PriceProfileId).IsRequired();
            modelBuilder.Entity<OrderLine>().Property(x => x.PriceProfileName).HasMaxLength(50).IsRequired();
            modelBuilder.Entity<OrderLine>().Ignore(x => x.RevertedLineId);
            modelBuilder.Entity<OrderLine>().Ignore(x => x.Total);

            modelBuilder.Entity<TimedBasedOrderLine>().Ignore(x => x.Interval);
            modelBuilder.Entity<ResourceBasedOrderLine>().Property(x => x.ResourceId).IsRequired();

            modelBuilder.Entity<AllocationState>().ToTable("AllocationState");
            modelBuilder.Entity<AllocationState>().HasKey(x => x.SubOrderId);
            modelBuilder.Entity<AllocationState>().Property(x => x.SubOrderId).ValueGeneratedNever();
            modelBuilder.Entity<AllocationState>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<AllocationState>().Ignore(x => x.ArrivalStateLogs);
            modelBuilder.Entity<AllocationState>().Ignore(x => x.DepartureStateLogs);
            // modelBuilder.Entity<AllocationState>().HasOne<SubOrder>().WithOne().HasForeignKey<SubOrder>();

            modelBuilder.Entity<Discount>().ToTable("Discount");
            modelBuilder.Entity<Discount>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Discount>().Property(x => x.Name).HasMaxLength(50);
            modelBuilder.Entity<Discount>().Ignore(x => x.RentalCategoriesSelection);
            modelBuilder.Entity<Discount>().Ignore(x => x.GuestCategoriesSelection);
            modelBuilder.Entity<Discount>().Ignore(x => x.ArticleCategoriesSelection);
            modelBuilder.Entity<Discount>().Ignore(x => x.CustomerGroupsSelection);
            modelBuilder.Entity<Discount>().Property(x => x.IdSelectionsJson).HasColumnType("ntext");

            modelBuilder.Entity<SubOrderDiscount>().ToTable("SubOrderDiscount");
            modelBuilder.Entity<SubOrderDiscount>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<SubOrderDiscount>().Property(x => x.Text).HasMaxLength(50);

            modelBuilder.Entity<AutoAddState>().ToTable("AutoAdd");
            modelBuilder.Entity<AutoAddState>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AutoAddState>().Property(x => x.TenantId).IsRequired();

            modelBuilder.Entity<Access>().ToTable("TallyAccess");
            modelBuilder.Entity<Access>().HasKey(x => x.Id);
            modelBuilder.Entity<Access>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<Access>().Property(x => x.AccessNames).HasMaxLength(100);
            modelBuilder.Entity<Access>().Property(x => x.AccessibleItemsJson).HasMaxLength(500);
            modelBuilder.Entity<Access>().Ignore(x => x.AccessibleItems);

            Counter.AddCounterModelToContext(modelBuilder);


            modelBuilder.Entity<AvailabilityMatrix>().ToTable("AvailabilityMatrix");
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.Id).HasMaxLength(100);
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<AvailabilityMatrix>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<AvailabilityMatrix>().Ignore(x => x.AvailabilityArrays);

            modelBuilder.Entity<Allocation>().ToTable("Allocation");
            modelBuilder.Entity<Allocation>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Allocation>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<Allocation>().Property(x => x.RentalUnitName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Allocation>().Ignore(x => x.DateInterval);

            modelBuilder.Entity<RentalUnit>().ToTable("RentalUnit");
            modelBuilder.Entity<RentalUnit>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<RentalUnit>().Property(x => x.LegacyId).IsRequired();
            modelBuilder.Entity<RentalUnit>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<RentalUnit>().Ignore("Name");
            modelBuilder.Entity<RentalUnit>().Ignore(x => x.NameTranslator);
            modelBuilder.Entity<RentalUnit>().Ignore(x => x.DescriptionTranslator);

            modelBuilder.Entity<TimeSpanBase>().ToTable("TimeSpan");
            modelBuilder.Entity<TimeSpanBase>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<TimeSpanBase>().Property("Type").HasMaxLength(20);
            modelBuilder.Entity<TimeSpanBase>().HasIndex(x => new { x.TenantId });
            modelBuilder.Entity<TimeSpanBase>().Ignore(x => x.ResourceId);
            modelBuilder.Entity<TimeSpanBase>().Ignore(x => x.TimeBlock);
            modelBuilder.Entity<TimeSpanBase>().HasDiscriminator<string>("Type").HasValue<RentalCategoryTimeSpan>("rentalcategory").HasValue<TenantTimeSpan>("tenant");
            modelBuilder.Entity<TimeSpanBase>().Property(x => x.Sort).UseIdentityColumn().Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);


            //should be moved to a shared infrastructure service
            modelBuilder.Entity<Country>().ToTable("Country");
            modelBuilder.Entity<Country>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Country>().Ignore(x => x.TextTranslator);
            modelBuilder.Entity<Country>().Ignore(x => x.TextTranslations);

            // Tenant entity configurations (from TenantApi)
            modelBuilder.Entity<Tenant>().ToTable("Tenant");
            modelBuilder.Entity<Tenant>().HasAlternateKey(c => c.ClientId);
            modelBuilder.Entity<Tenant>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Tenant>().Property(x => x.ClientId).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Tenant>().Property(x => x.CompanyName).HasMaxLength(100);
            modelBuilder.Entity<Tenant>().Property(x => x.ContactName).HasMaxLength(100);
            modelBuilder.Entity<Tenant>().Property(x => x.Address).HasMaxLength(500);
            modelBuilder.Entity<Tenant>().Property(x => x.Email).HasMaxLength(200);
            modelBuilder.Entity<Tenant>().Property(x => x.Phone).HasMaxLength(50);
            modelBuilder.Entity<Tenant>().Property(x => x.LegacyId).HasMaxLength(50);
            modelBuilder.Entity<Tenant>().Property(x => x.Cvr).HasMaxLength(20);

            modelBuilder.Entity<TextSection>().ToTable("TextSection");
            modelBuilder.Entity<TextSection>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<TextSection>().Property(x => x.Summary).HasMaxLength(1000);
            modelBuilder.Entity<TextSection>().Property(x => x.Keywords).HasMaxLength(1000);
            modelBuilder.Entity<TextSection>().Property(x => x.Text).HasColumnType("ntext");
            modelBuilder.Entity<TextSection>().Property(x => x.Title).HasMaxLength(500);
            modelBuilder.Entity<TextSection>().Property(x => x.VideoUrl).HasMaxLength(250);
            modelBuilder.Entity<TextSection>().Ignore(x => x.IsRead);

            modelBuilder.Entity<TextSectionUser>().ToTable("TextSectionUser");
            modelBuilder.Entity<TextSectionUser>().HasKey(x => new { x.TextSectionId, x.UserId });

            modelBuilder.Entity<BillableItem>().ToTable("BillableItem");
            modelBuilder.Entity<BillableItem>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<BillableItem>().Property(x => x.Text).HasMaxLength(200);

            modelBuilder.Entity<Kiosk>().ToTable("Kiosk");
            modelBuilder.Entity<Kiosk>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<Kiosk>().HasIndex(x => new { x.HardwareSerialNo }).IsUnique();
            modelBuilder.Entity<Kiosk>().Property(x => x.Name).HasMaxLength(100);
            modelBuilder.Entity<Kiosk>().Property(x => x.HardwareSerialNo).HasMaxLength(100);

            modelBuilder.Entity<FeatureModule>().ToTable("FeatureModule");
            modelBuilder.Entity<FeatureModule>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<FeatureModule>().Property(x => x.Name).HasMaxLength(200);

            modelBuilder.Entity<FeatureModuleTenantEntry>().ToTable("FeatureModuleTenantEntry");
            modelBuilder.Entity<FeatureModuleTenantEntry>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<FeatureModuleTenantEntry>().HasIndex(x => x.FeatureModuleId);
            modelBuilder.Entity<FeatureModuleTenantEntry>().HasIndex(x => x.CreateDate);
            modelBuilder.Entity<FeatureModuleTenantEntry>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<FeatureModuleTenantEntry>().HasIndex(x => x.StartDate);
            modelBuilder.Entity<FeatureModuleTenantEntry>().HasIndex(x => x.EndDate);

            modelBuilder.Entity<ExternalPaymentTransaction>().ToTable("ExternalPaymentTransaction");
            modelBuilder.Entity<ExternalPaymentTransaction>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<ExternalPaymentTransaction>().Property(x => x.Status).HasMaxLength(50);
            modelBuilder.Entity<ExternalPaymentTransaction>().Property(x => x.TransactionId).HasMaxLength(50);
            modelBuilder.Entity<ExternalPaymentTransaction>().Property(x => x.TransactionType).HasMaxLength(50);

            // Pricing entity configurations (from PricingApi)
            modelBuilder.Entity<PriceProfile>().ToTable("PriceProfile");
            modelBuilder.Entity<PriceProfile>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<PriceProfile>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<PriceProfile>().Property(x => x.Name).HasMaxLength(50);
            modelBuilder.Entity<PriceProfile>().Property(x => x.Description).HasMaxLength(1000);

            modelBuilder.Entity<CostInterval>().ToTable("CostInterval");
            modelBuilder.Entity<CostInterval>()
                .HasDiscriminator<string>("CostType")
                .HasValue<CostIntervalDay>("CostIntervalDay")
                .HasValue<CostIntervalFixed>("CostIntervalFixed")
                .HasValue<CostIntervalDayMinMax>("CostIntervalDayMinMax")
                .HasValue<CostIntervalMonth>("CostIntervalMonth")
                .HasValue<CostIntervalYear>("CostIntervalYear")
                .HasValue<CostIntervalMonthSpecific>("CostIntervalMonthSpecific")
                .HasValue<CostIntervalDaySpecific>("CostIntervalDaySpecific")
                .HasValue<CostIntervalDaySpecificStartDay>("CostIntervalDaySpecificStartDay")
                .HasValue<CostFlexInterval>("CostFlexInterval");
            modelBuilder.Ignore<CostItemSpecificMonth>();
            modelBuilder.Ignore<CostItemSpecificDay>();
            modelBuilder.Entity<CostInterval>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<CostInterval>().Property(x => x.TenantId).IsRequired();
            modelBuilder.Entity<CostInterval>().Property(x => x.CostType).HasMaxLength(50);

            modelBuilder.Ignore<CostSpan>();

            // TallyWebIntegration entity configurations
            modelBuilder.Entity<TConMasterRadioTenantMap>().ToTable("TallyMasterRadioTenantMap");
            modelBuilder.Entity<TConMasterRadioTenantMap>().Property(x => x.Id);
            modelBuilder.Entity<TConMasterRadioTenantMap>().HasAlternateKey(x => new { x.TallyMasterRadioId, x.TenantId });

            modelBuilder.Entity<AccessGroup>().ToTable("TallyAccessGroup");
            modelBuilder.Entity<AccessGroup>().HasKey(x => x.Id);
            modelBuilder.Entity<AccessGroup>().Property(x => x.Name).HasMaxLength(100);
            modelBuilder.Entity<AccessGroup>().Ignore(x => x.SocketRadios);
            modelBuilder.Entity<AccessGroup>().Ignore(x => x.SwitchRadios);

            modelBuilder.Entity<RadioBilling>().ToTable("TallyRadioBilling");
            modelBuilder.Entity<RadioBilling>().HasKey(x => x.RadioAddress);
            modelBuilder.Entity<RadioBilling>().Property(x => x.RadioAddress).ValueGeneratedNever();

            Counter.AddCounterModelToContext(modelBuilder);

            ModifyDefaultCascadeBehavior(modelBuilder, DeleteBehavior.Restrict);
        }
    }
}
