using System.Collections.Generic;

namespace app.Configuration
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public bool IsLife { get; set; }
        public string Role { get; set; }
        public List<string> Error { get; set; }
    }

}
