using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using Microsoft.CSharp.RuntimeBinder;
using System.Configuration;

namespace Automation.Modules
{
    public class EventModule : NancyModule
    {
        public EventModule() {
            this.RequiresAuthentication();

            Post["/tag/{id:int}/{action}"] = _ =>
            {
                this.RequiresClaims(new[] { "Tag" });
                try
                {
                    AddEvent(_.id, _.action, _.value);
                }
                catch (NullReferenceException)
                {
                    AddEvent(_.id, _.action);
                }
                return Negotiate.WithStatusCode(HttpStatusCode.NoContent);
            };
        }

        private void AddEvent(int sourceId, string descriptionName)
        {
            AddEvent(sourceId, descriptionName, Double.NaN);
        }

        private void AddEvent(int sourceId, string descriptionName, double value)
        {
            /*
            using (var sql = new SqlConnection(ConfigurationManager.AppSettings["DatabaseConnection"]))
            {
                sql.Open();
                var insert = sql.CreateCommand();
                insert.CommandText = 
                    @"INSERT INTO Event (SourceId, DescriptionId, Timestamp, Value)
                    VALUES (@sourceId, (SELECT Id from EventDescription WHERE UPPER(Name) = UPPER(@descriptionName)), CURRENT_TIMESTAMP, @value)";
                insert.Parameters.AddWithValue("sourceId", sourceId);
                insert.Parameters.AddWithValue("descriptionName", descriptionName);
                insert.Parameters.AddWithValue("value", Double.IsNaN(value) ? (object)DBNull.Value : value);

                insert.ExecuteNonQuery();
            }
            */
        }
    }
}