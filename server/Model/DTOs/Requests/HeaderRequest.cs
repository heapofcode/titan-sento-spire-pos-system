using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Model.DTOs.Requests
{
    public class HeaderRequest
    {
        public string search { get; set; }
        public int skip { get; set; }
        public int take { get; set; }
        public bool isreturn { get; set; }
        public bool iscancel { get; set; }
    }
}
