using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace app.Model.DBase
{
    public class TransientDraft
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        public bool isReturn { get; set; }
        public int refDraftNumber { get; set; }
    }

    public class TableDraft
    {
        public Guid Id { get; set; } = new Guid();
        public Guid DraftId { get; set; }
        public Draft Draft { get; set; }
        public Guid GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Amount { get; set; }
        //public List<Draft> Drafts { get; set; } = new List<Draft>();
    }

    public class BillingDraft
    {
        public Guid Id { get; set; } = new Guid();
        public Guid DraftId { get; set; }
        public Draft Draft { get; set; }
        public decimal Amount { get; set; }
        public decimal CashPayment { get; set; }
        public decimal CardPayment { get; set; }
        public decimal CashBack { get; set; }
        public string UNOperation { get; set; }
        public string UNBankresponse { get; set; }
        public string CardNumber { get; set; }
    }

    public class Draft
    {
        public Guid Id { get; set; } = new Guid();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid UserWorkShiftId { get; set; }
        public UserWorkShift UserWorkShift { get; set; }
        public string DateTime { get; set; }
        public int DraftNumber { get; set; }
        public int? refDraftNumber { get; set; } 
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public List<TableDraft> TableDraft { get; set; }
        public BillingDraft BillingDraft { get; set; }
    }

    public class InOutDraft
    {
        public Guid Id { get; set; } = new Guid();
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid UserWorkShiftId { get; set; }
        public UserWorkShift UserWorkShift { get; set; }
        public string DateTime { get; set; }
        public int DraftNumber { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
    }
}
