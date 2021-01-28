using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cwiczenia3.dto.response
{
    public class RefreshTokenResponse
    {
        public string refreshToken { get; set; }
        public string studIndex { get; set; }

        public RefreshTokenResponse(string refreshToken, string studIndex)
        {
            this.refreshToken = refreshToken;
            this.studIndex = studIndex;
        }
    }
}
