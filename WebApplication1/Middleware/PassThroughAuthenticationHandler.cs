using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebApplication1.Middleware
{
    public class PassThroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public PassThroughAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder) : base(options, logger, encoder)
        {

        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name)));
            }
            return Task.FromResult(AuthenticateResult.NoResult());
        }
    }
}