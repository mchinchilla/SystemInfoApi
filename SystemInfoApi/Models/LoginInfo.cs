using System;

namespace SystemInfoApi.Models
{
    [Serializable]
    public class LoginInfo
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
}