using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;

namespace Automation.Modules
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => "It works!";

            // Nancy does not like returning an empty model and an empty view (like only a status code)
            // So we give it a "model" if there's no claim specified using default routing
            Get["/checkauth/{claim?null}"] = _ =>
            {
                this.RequiresAuthentication();

                // This is dumb
                if (_.claim.Value != "null")
                {
                    this.RequiresClaims(new string[] { _.claim.Value});
                }

                return HttpStatusCode.NoContent;
            };
        }
    }
}