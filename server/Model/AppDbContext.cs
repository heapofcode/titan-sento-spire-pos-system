using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using app.Model.DBase;
using app.Model.Devices;

namespace app.Model
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserSetting>()
                .HasKey(a => new { a.UserId, a.SettingId });

            base.OnModelCreating(builder);
        }

        public DbSet<CashDeviceError> CashDeviceErrors { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<CashDevice> CashDevices { get; set; }
        public DbSet<CardDevice> CardDevices { get; set; }
        public DbSet<CoinDevice> CoinDevices { get; set; }
        public DbSet<CurrencyNomination> CurrencyNominations { get; set; }
        public DbSet<TransientPayment> TransientPayments { get; set; }
        public DbSet<LCDDevice> LCDDevices { get; set; }
        public DbSet<ScaleDevice> ScaleDevices { get; set; }
        public DbSet<UserMenu> UserMenus { get; set; }
        public DbSet<UserWorkShift> UserWorkShifts { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Good> Goods { get; set; }
        public DbSet<Draft> Drafts { get; set; }
        public DbSet<TransientDraft> TransientDrafts { get; set; }
        public DbSet<TableDraft> TableDrafts { get; set; }
        public DbSet<BillingDraft> BillingDrafts { get; set; }
        public DbSet<InOutDraft> InOutDrafts { get; set; }
    }
}
