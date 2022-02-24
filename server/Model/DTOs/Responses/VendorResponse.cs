using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Model.DTOs.Responses
{
    public class SyncFusionVendorResponse
    {
        public object result { get; set; }
        public int count { get; set; }
    }

    public class VendoreResponse:SyncFusionVendorResponse
    {
        public bool Success { get; set; }
        public List<string> Message { get; set; }
        public List<string> Error { get; set; }
        public bool isReturn { get; set; }
    }
}
