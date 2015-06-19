using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class GarageModule : NancyModule
    {
        public GarageModule()
        {
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