using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Configuration
{
    public class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Vendor = "Vendor";
    }

    public class DraftStatuses
    {
        public const string Cancel = "Cancel";
        public const string Returned = "Returned";
        public const string In = "In";
        public const string Out = "Out";
    }
}
