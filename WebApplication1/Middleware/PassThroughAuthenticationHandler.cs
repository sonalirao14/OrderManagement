using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebApplication1.Middleware
{
    public class PassThroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public PassThroughAuthenticationHandler(
             IOptionsMonitor<AuthenticationSchemeOptions> options,
             ILoggerFactory logger,
             UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation("PassThroughAuthenticationHandler triggered");

            if (Context.User?.Identity?.IsAuthenticated == true && Context.User.Claims.Any())
            {
                Logger.LogInformation("User claims found: {Claims}",
                    string.Join(", ", Context.User.Claims.Select(c => $"{c.Type}:{c.Value}")));
              

                var claims = Context.User.Claims.ToList();

                // Check for role claim explicitly
                var roleClaim = Context.User.FindFirst("role")?.Value;
                if (!string.IsNullOrEmpty(roleClaim))
                {
                    if (!claims.Any(c => c.Type == ClaimTypes.Role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleClaim));
                        Logger.LogInformation("Added ClaimTypes.Role: {Role}", roleClaim);
                    }
                }
                else
                {
                    Logger.LogWarning("No 'role' claim found in JWT claims.");
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name, "email", ClaimTypes.Role);
                var principal = new ClaimsPrincipal(identity);

                Logger.LogInformation("New ClaimsIdentity created. IsAuthenticated: {IsAuth}", identity.IsAuthenticated);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
            }

            Logger.LogWarning("No authenticated user or claims found in context. Authentication failed.");
            return Task.FromResult(AuthenticateResult.Fail("Unauthenticated"));
        }
    }
}