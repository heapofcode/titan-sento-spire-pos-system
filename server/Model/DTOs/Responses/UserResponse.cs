using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace app.Model.DTOs.Responses
{
    public class UserResponse
    {
        public bool Success { get; set; }
        public List<JsonMenu> JsonMenu { get; set; }
        public List<string> Message { get; set; }
        public object UserInfo { get; set; }
    }

    public class JsonMenu
    {
        public string route { get; set; }
        public string icon { get; set; }
        public string title { get; set; }
    }
}
