using CitiesManager.Core.DTO;
using CitiesManager.Core.Identity;
using CitiesManager.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CitiesManager.Core.Services {
    public class JwtService : IJwtService {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration) {
            _configuration = configuration;
        }

        public AuthenticationResponse CreateJwtToken(ApplicationUser user) {
            //generate expiration date based on the expiration minutes
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
            //Created claims for the payload
            Claim[] claims = new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),//Subject || Sub : Indicates the user identity
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique Id for the token
                new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),//Issued at (date and time of token generation) 
                //optional
                new Claim(ClaimTypes.NameIdentifier,user.Email), //Unique name identifier of the user (email)
                new Claim(ClaimTypes.Name,user.PersonName) //Name of the user
            };
            //Created security key
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            //Made ready the hashing algorithm
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            //Created the tokenGenerator
            //1:domain address of the particular web api controller which generated the token
            //2:target audience by the token can be consumed or stored
            //siginingCredenials represents the algorithm that is HmacSha256 which generates the hash based on the payload and header along with the secret value
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: signingCredentials);
            //Created security token Handler responsible for generating the actual token 
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //method that actually generates the token based on the algorithm mentioned
            //it automatically picks up the header values payload 
            //String Token Added as part of the response
            string token = tokenHandler.WriteToken(tokenGenerator);
            return new AuthenticationResponse() { Token = token, Email=user.Email, PersonName=user.PersonName, Expiration=expiration};
        }
    }
}
