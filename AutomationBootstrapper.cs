using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

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

#if DEBUG
                if (token == null)
                {
                    return new User
                    {
                        UserName = "Test",
                        Claims = new string[] { "Garage", "Tag", "HVAC" }
                    };
                }
#endif

                if (!String.IsNullOrWhiteSpace(token))
                {
                    using (var sql = new SqlConnection())
                    {
                        var check = sql.CreateCommand();
                        check.CommandText = "SELECT User.UserName AS UserName, Claim.Name AS Claim FROM Token WHERE Token.Id = @token JOIN User on User.Id = Token.UserId JOIN Claim on Claim.UserId = User.Id";
                        check.Parameters.AddWithValue("token", token);

                        var user = new User();
                        using (var reader = check.ExecuteReader())
                        {
                            user.UserName = Convert.ToString(reader["UserName"]);
                            ((List<String>)user.Claims).Add(Convert.ToString(reader["Claim"]));
                        }

                        if (user.UserName != null)
                        {
                            return user;
                        }
                    }
                }

                return null;
            });

            StatelessAuthentication.Enable(pipelines, statelessAuthenticationConfiguration);
        }
    }
}