using app.Configuration;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using app.Model.DBase;

namespace app.Model
{
    public class InitAppDbContext
    {
        public async static void InitializeAppDb(AppDbContext _context, UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> _roleManager)
        {
            await _context.Database.EnsureCreatedAsync();

            if (!_context.UserRoles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
                await _roleManager.CreateAsync(new IdentityRole(Roles.User));
                await _roleManager.CreateAsync(new IdentityRole(Roles.Vendor));
            }

            if (!_context.Settings.Any())
            {
                var cashDevice = new CashDevice() { HOST = "http://192.168.3.121", Login = "service", Password = "751426", Port = "COM1", Speed = "115200" };
                var cardDevice = new CardDevice() { Ip = "192.168.3.221" };
                var coinDevice = new CoinDevice() { Port = "COM1", Bound = 9600 };
                var lcdDevice = new LCDDevice() { ComPort = "192.168.1.201" };
                var scaleDevice = new ScaleDevice() { ComPort = "192.168.1.201" };
                var userMenu = new UserMenu() { JsonMenu = "" +
                    "{\"route\":\"/\",\"icon\":\"fa fa-home\",\"title\":\"Главная\"}"};
                _context.CashDevices.Add(cashDevice);
                _context.CardDevices.Add(cardDevice);
                _context.CoinDevices.Add(coinDevice);
                _context.LCDDevices.Add(lcdDevice);
                _context.ScaleDevices.Add(scaleDevice);
                _context.UserMenus.Add(userMenu);
                _context.Settings.Add(new Setting()
                {
                    CashDeviceId = cashDevice.Id,
                    CardDeviceId = cardDevice.Id,
                    CoinDeviceId = coinDevice.Id,
                    LCDDeviceId = lcdDevice.Id,
                    ScaleDeviceId = scaleDevice.Id,
                    UserMenuId = userMenu.Id
                });
            }

            if (!_context.Users.Any())
            {
                var Admin = new ApplicationUser() { Email = "admin@mail.com", UserName = "Admin", FullName = "Ivanov Ivan Ivanovich" };
                var isCreated = await _userManager.CreateAsync(Admin, "12345");
                if (isCreated.Succeeded)
                {
                    await _userManager.AddToRoleAsync(Admin, Roles.Admin);
                    _context.UserSettings.Add(new UserSetting() {
                        UserId = Admin.Id,
                        SettingId = _context.Settings.OrderBy(a=>a.Id).FirstOrDefault().Id
                    });
                }
                
                var Vendor = new ApplicationUser() { Email = "vendor@mail.com", UserName = "Vendor", FullName = "Petrov Petr Petrovich" };
                isCreated = await _userManager.CreateAsync(Vendor, "12345");
                if (isCreated.Succeeded)
                    await _userManager.AddToRoleAsync(Vendor, Roles.Vendor);
                    _context.UserSettings.Add(new UserSetting()
                    {
                        UserId = Vendor.Id,
                        SettingId = _context.Settings.OrderBy(a => a.Id).FirstOrDefault().Id
                    });
            }

            if (!_context.Goods.Any())
            {
                _context.Goods.Add(new Good
                {
                    Barcode = "8901057524421",
                    VendoreCode = "2",
                    GoodsName = "балтика 0 ж/б  0,45",
                    Price = 1.5M,
                    Balance = 20
                });
                _context.Goods.Add(new Good
                {
                    Barcode = "9023800430106",
                    VendoreCode = "3",
                    GoodsName = "балтика 'кулер'ст/б 0,47",
                    Price = 3.27M,
                    Balance = 100
                });
            }


            await _context.SaveChangesAsync();
        }
    }
}