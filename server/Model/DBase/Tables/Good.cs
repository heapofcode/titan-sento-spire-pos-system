using System;
using System.Collections.Generic;

namespace app.Model.DBase
{
    public class Good
    {
        public Guid Id { get; set; } = new Guid();
        public string Barcode { get; set; }
        public string VendoreCode { get; set; }
        public string GoodsName { get; set; }
        public decimal Price { get; set; }
        public decimal Balance { get; set; }
        public List<TableDraft> TableDrafts { get; set; } = new List<TableDraft>();
    }
}