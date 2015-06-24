using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using Automation.Models;

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
                        .WithAllowedMediaRange("application/xml")
                        .WithAllowedMediaRange("application/json")
                        .WithModel(new GarageStatus
                        {
                            Open = false
                        });
                };
        }
    }
}