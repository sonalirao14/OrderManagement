using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace WebApplication1.Middleware
{
    public class JwtSecurityTokenHandlerWrapper
    {
        private readonly RSA _rsa;
        private readonly ILogger<JwtSecurityTokenHandlerWrapper> _logger;

        public JwtSecurityTokenHandlerWrapper(ILogger<JwtSecurityTokenHandlerWrapper> logger)
        {
            _logger = logger;
            var publicKeyPath = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "public.pem");
            //_logger.LogInformation("Reading public key from: {PublicKeyPath}", publicKeyPath); was just checking public key path was correct or not
            var publicKeyText = File.ReadAllText(publicKeyPath);
            _rsa = RSA.Create();
            try
            {
                _rsa.ImportFromPem(publicKeyText.ToCharArray());
                _logger.LogInformation("Public key imported successfully.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to import public key from {PublicKeyPath}", publicKeyPath);
                throw;
            }
        }

        public async Task<ClaimsPrincipal> ValidateJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(_rsa),
                //RoleClaimType = "userRole",
                //NameClaimType = "email",
                //RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/roles"
            };
            var pqr =  await GetCLaims(token);

            //var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                //var jwtToken = handler.ReadJwtToken("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiI2ODA4YTQyNzZiZTgyMDdiZjFjMWQ1ZjciLCJyb2xlIjoiYWRtaW4iLCJlbWFpbCI6ImRlbW9hZG1pbjNAZ21haWwuY29tIiwiaWF0IjoxNzQ1NDAwNzIyLCJleHAiOjE3NDU0MDQzMjJ9.Z8dYWoZjJ_raHTVW4er3QiQH7FKfB241GIF5XXGLDhUq6FEWUuZ7dvf818NJVW-Cl1dqSycftmX-dETt5cORRdroMPjBFj16DTlSgvW99-U-jzM3Ts8vbX8FFetwQdoK9tz2DNtsLpEErpvW1II4BEDGNrUIhr5UBVJ2K1zK2pfcapqfI1GvoGJ8dpzvgOZBnCY5NFIvTmdAyQaz_GKITK0aQlewL1eJ14oo56ZjGiyx3_m79lsTXrW9tPhnEf4cEz4iW3a8uFKZIfjugljp93OSOgsIR3f85ylMQXGCZCHD9NrKOCg_SCw4HkHMyy0dP4_8uZnOBrOSbbglFE87lw");
                //_logger.LogInformation("token", token);
                _logger.LogInformation("Raw token payload: {Payload}",
                    System.Text.Json.JsonSerializer.Serialize(jwtToken.Payload));
                foreach (var claim in jwtToken.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                var principal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                _logger.LogInformation("Token validated successfully. Claims: {Claims}",
                    string.Join(", ", principal.Claims.Select(c => $"{c.Type}:{c.Value}")));
                return principal;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Token validation failed. Token: {Token}", token);
                throw;
            }
    }
        private async Task<IEnumerable<Claim>> GetCLaims(string token)
        {
            if (token == null || token == "")
            {
                return null;
            }

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            return await Task.FromResult(jwtToken.Claims);
        }
    }
}