using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using Automation.Models;
using System.Data.SqlClient;
using System.Configuration;

namespace Automation.Modules
{
    public class GarageModule : NancyModule
    {

        public GarageModule()
        {
            this.RequiresAuthentication();
            this.RequiresClaims(new[] { "Garage" });

            Get["/garage/status"] = _ =>
                {
                    using (var sql = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnection"]))
                    {
                        sql.Open();
                        var check = sql.CreateCommand();
                        check.CommandText = 
                            @"SELECT TOP 1 Event.Id, Event.Timestamp, EventDescription.Name FROM Event
                            JOIN EventSource ON EventSource.Id = Event.SourceId
                            JOIN EventDescription ON EventDescription.Id = Event.DescriptionId
                            WHERE EventSource.Name = 'Garage Door'
                            ORDER BY Event.Timestamp DESC";

                        using (var reader = check.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                return Negotiate
                                    .WithAllowedMediaRange("application/xml")
                                    .WithAllowedMediaRange("application/json")
                                    .WithModel(new GarageStatus
                                    {
                                        Id = reader.GetInt32(0),
                                        Timestamp = reader.GetDateTime(1),
                                        Open = reader.GetString(2) == "Open"
                                    });
                            }
                        }

                        return Negotiate
                            .WithAllowedMediaRange("application/xml")
                            .WithAllowedMediaRange("application/json")
                            .WithModel(new Error
                            {
                                Code = 500,
                                Message = "No Event found"
                            });
                    }

                };
        }
    }
}