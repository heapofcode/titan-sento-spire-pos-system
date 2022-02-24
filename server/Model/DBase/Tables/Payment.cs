using System;

namespace app.Model.DBase
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; } //Сумма
        public decimal GetAmount { get; set; } //Принять
        public decimal CashPayment { get; set; } //Оплата нал
        public decimal CardPayment { get; set; } //Оплата карта
        public decimal CashBack { get; set; } //Сдача
    }

    public class TransientPayment
    {
        public Guid Id { get; set; } = new Guid();
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public decimal Amount { get; set; }
    }

    public class CurrencyNomination
    {
        public Guid Id { get; set; } = new Guid();
        public int ByteReceive { get; set; }
        public decimal Value { get; set; }
    }
}