using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;

namespace Automation
{
    public class GarageModule : NancyModule
    {

        public GarageModule()
        {
            this.RequiresAuthentication();
            this.RequiresClaims(new[] { "Garage" });

            Get["/garage/status"] = _ =>
                {
                    return Negotiate
                        .WithAllowedMediaRange("application/json")
                        .WithModel(new 
                        {
                            Open = 0
                        });
                };
        }
    }
}