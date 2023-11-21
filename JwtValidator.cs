using ContactManager.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Web;

namespace ContactManager
{
    public class JwtValidator
    {
        private string issuer;
        private string audience;
        private ICollection<SymmetricSecurityKey> signingKeys;
        private ContactsManagerContext db = new ContactsManagerContext();

        public bool ValidateToken (string token, out JwtSecurityToken jwt)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateLifetime = true
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                jwt = (JwtSecurityToken)validatedToken;

                string username = jwt.Claims.FirstOrDefault(c => c.Type == "sub").Value;
                string GivenName = jwt.Claims.FirstOrDefault(c => c.Type == "GivenName").Value;
                string Surname = jwt.Claims.FirstOrDefault(c => c.Type == "Surname").Value;

                tUser user = db.Users.FirstOrDefault(u => u.Username == username && u.FirstName == GivenName && u.LastName == Surname);

                if (user == null)
                {
                    throw new SecurityTokenValidationException("User is not in the database");
                }
                

                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                // Log the reason why the token is not valid
                jwt = null;
                return false;
            }
        }

        public JwtValidator(string iss, string aud, string secretKey)
        {
            issuer = iss;
            audience = aud;

            signingKeys = new List<SymmetricSecurityKey>
            {
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        }
    }
}