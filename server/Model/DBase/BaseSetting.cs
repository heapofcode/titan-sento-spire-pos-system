using System;
using System.Collections.Generic;

namespace app.Model.DBase
{
    public class UserSetting
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid SettingId { get; set; }
        public Setting Setting { get; set; }
    }

    public class Setting
    {
        public Guid Id { get; set; } = new Guid();
        public Guid CashDeviceId { get; set; }
        public CashDevice CashDevice { get; set; }
        public Guid CardDeviceId { get; set; }
        public CardDevice CardDevice { get; set; }
        public Guid CoinDeviceId { get; set; }
        public CoinDevice CoinDevice { get; set; }
        public Guid LCDDeviceId { get; set; }
        public LCDDevice LCDDevice { get; set; }
        public Guid ScaleDeviceId { get; set; }
        public ScaleDevice ScaleDevice { get; set; }
        public Guid UserMenuId { get; set; }
        public UserMenu UserMenu { get; set; }
        public List<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
    }

    public class CoinDevice
    {
        public Guid Id { get; set; } = new Guid();
        public string Port { get; set; }
        public int Bound { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }

    public class CashDevice
    {
        public Guid Id { get; set; } = new Guid();
        public string HOST { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string Speed { get; set; }
        public DateTime LoadInfoDate { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }

    public class CardDevice
    {
        public Guid Id { get; set; } = new Guid();
        public string Ip { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }

    public class LCDDevice
    {
        public Guid Id { get; set; } = new Guid();
        public string ComPort { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }

    public class ScaleDevice
    {
        public Guid Id { get; set; } = new Guid();
        public string ComPort { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }
}