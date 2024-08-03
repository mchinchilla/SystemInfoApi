using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SystemInfoApi.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly string _expDate;

        public JwtService( IConfiguration config )
        {
            _secret = config.GetSection( "JWT" ).GetSection( "secret" ).Value;
            _expDate = config.GetSection( "JWT" ).GetSection( "expirationInMinutes" ).Value;
        }

        public string GenerateSecurityToken( string user, string password )
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes( _secret );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity( new[]
                {
                    new Claim( ClaimTypes.NameIdentifier, user ),
                    new Claim( JwtRegisteredClaimNames.UniqueName, user ),
                    new Claim( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() )
                } ),
                Expires = DateTime.UtcNow.AddMinutes( double.Parse( _expDate ) ),
                SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature ),
                Audience = "localhost",
                Issuer = "localhost"
            };

            var token = tokenHandler.CreateToken( tokenDescriptor );

            return tokenHandler.WriteToken( token );
        }
    }
}