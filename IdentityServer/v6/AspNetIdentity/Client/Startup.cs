using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddAuthentication(options => 
            {
                options.DefaultScheme = "cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.ClientId = "client";
                    options.MapInboundClaims = false;
                    options.SaveTokens = true;

                    options.Events.OnRedirectToIdentityProvider = n =>
                    {

                        if (n.Properties.Items.TryGetValue(OpenIdConnectParameterNames.LoginHint, out var loginHint) && !string.IsNullOrEmpty(loginHint))
                        {
                            n.ProtocolMessage.LoginHint = loginHint;
                        }

                        if (n.Properties.Items.TryGetValue(OpenIdConnectParameterNames.Prompt, out var prompt) && !string.IsNullOrEmpty(prompt))
                        {
                            n.ProtocolMessage.Prompt = prompt;
                        }

                        if (n.Properties.Items.TryGetValue(OpenIdConnectParameterNames.IdentityProvider, out var idp) && !string.IsNullOrEmpty(idp))
                        {
                            n.ProtocolMessage.AcrValues += $" idp:{idp}";
                        }


                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToIdentityProviderForSignOut = async n =>
                    {
                        if (await n.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken) is string idToken)
                        {
                            n.ProtocolMessage.IdTokenHint = idToken;
                        }
                        else if (n.Properties.GetTokenValue(OpenIdConnectParameterNames.IdToken) is string idTokenParameter)
                        {
                            n.ProtocolMessage.IdTokenHint = idTokenParameter;
                        }

                        if (n.Properties.GetParameter<string>(OpenIdConnectParameterNames.State) is string state)
                        {
                            n.ProtocolMessage.State = state;
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
