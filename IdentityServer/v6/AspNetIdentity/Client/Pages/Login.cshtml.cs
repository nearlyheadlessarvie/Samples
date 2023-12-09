using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Generic;

namespace Client.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet(string returnUrl = null, string loginHint = null, string idp = null, string prompt = null)
        {
            var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    { OpenIdConnectParameterNames.LoginHint, loginHint },
                    { OpenIdConnectParameterNames.IdentityProvider, idp },
                    { OpenIdConnectParameterNames.Prompt, prompt }
                }
            )
            {
                RedirectUri = returnUrl
            };

            return new ChallengeResult("oidc", props);
        }
    }
}
