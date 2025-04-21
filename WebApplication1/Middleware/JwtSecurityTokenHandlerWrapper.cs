using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

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

        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(_rsa),
                RoleClaimType = "role",
                NameClaimType = "email"
            };

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, validationParameters, out var securityToken);
                _logger.LogInformation("Token validated successfully");
                return principal;
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Token validation failed");
                throw;
            }
        }
    }
}