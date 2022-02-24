using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace app.Model.DBase
{

    public class ApplicationUser : IdentityUser
    { 
        public string FullName { get; set; }
    }

    public class UserMenu
    {
        public Guid Id { get; set; } = new Guid();
        public string JsonMenu { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }

    public class UserWorkShift
    {
        public Guid Id { get; set; } = new Guid();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int NumberOfWorkShift { get; set; }
        public DateTime StartWorkShift { get; set; }
        public DateTime EndWorkShift { get; set; }
        public decimal Amount { get; set; }
        public int DraftNumber { get; set; }
        public bool Status { get; set; }
    }
}
