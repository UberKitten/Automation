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
                    /*using (var sql = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnection"]))
                    {
                        sql.Open();
                        var check = sql.CreateCommand();
                        check.CommandText = @"SELECT [User].UserName, [User].GroupMeId, [Claim].Name, [ClaimURL].[URL] FROM [Token]
JOIN [User] on [User].Id = Token.UserId
LEFT JOIN [UserClaim] on [UserClaim].UserId = [User].Id
LEFT JOIN [Claim] on [Claim].Id = [UserClaim].ClaimId
LEFT JOIN [ClaimURL] on [ClaimURL].ClaimId = [Claim].Id
WHERE [Token].Value = @token";
                        check.Parameters.AddWithValue("token", token);

                        var user = new User();
                        var claims = new HashSet<string>();
                        var urls = new HashSet<string>();

                        using (var reader = check.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                user.UserName = reader.GetString(0);

                                if (!reader.IsDBNull(1))
                                { 
                                    user.GroupMeId = reader.GetInt32(1);
                                }

                                if (!reader.IsDBNull(2))
                                { 
                                    claims.Add(reader.GetString(2));
                                }

                                if (!reader.IsDBNull(3))
                                {
                                    urls.Add(reader.GetString(3));
                                }
                            }
                        }
                        user.Claims = claims;
                        user.ClaimURLs = urls;

                        if (user.UserName != null)
                        {
                            return user;
                        }
                    }*/
                }

                return null;
            });

            StatelessAuthentication.Enable(pipelines, statelessAuthenticationConfiguration);
        }
    }
}