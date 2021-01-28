using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia3.dto.request
{
    public class AuthorizationRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
