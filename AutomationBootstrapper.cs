using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Automation
{
    public class AutomationBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(Nancy.TinyIoc.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            var statelessAuthenticationConfiguration = new StatelessAuthenticationConfiguration(context =>
            {
                string token = context.Request.Headers["X-AUTH-TOKEN"].FirstOrDefault();
                if (token == null && context.Request.Query.token.HasValue)
                {
                    token = context.Request.Query.token.Value;
                }
                if (token == null && context.Request.Cookies.ContainsKey("TOKEN"))
                {
                    token = context.Request.Cookies["TOKEN"];
                }
                context.Request.Cookies["TOKEN"] = token;

                if (!String.IsNullOrWhiteSpace(token))
                {
                    var db = Database.GetConnection();
                    return db.GetUser(token);
                }

                return null;
            });

            StatelessAuthentication.Enable(pipelines, statelessAuthenticationConfiguration);
        }
    }
}