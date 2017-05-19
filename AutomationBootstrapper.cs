using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
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
                context.Request.Cookies["TOKEN"] = token;

                if (!String.IsNullOrWhiteSpace(token))
                {
                    using (var sql = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnection"]))
                    {
                        sql.Open();
                        var check = sql.CreateCommand();
                        check.CommandText = @"SELECT [User].UserName, [Claim].Name FROM [Token]
JOIN [User] on [User].Id = [Token].UserId
LEFT JOIN [UserClaim] on [UserClaim].UserId = [User].Id
LEFT JOIN [Claim] on [Claim].Id = [UserClaim].ClaimId
WHERE [Token].Value = @token";
                        check.Parameters.AddWithValue("token", token);

                        var user = new User();
                        using (var reader = check.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                user.UserName = reader.GetString(0);

                                // Don't add a NULL claim if user doesn't have any claims
                                if (!reader.IsDBNull(1))
                                { 
                                    ((List<String>)user.Claims).Add(reader.GetString(1));
                                }
                            }
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