using Automation.Models;
using Microsoft.Azure;
using Nancy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Nancy.Security;

namespace Automation.Modules
{
    public class ChoreModule : NancyModule
    {
        public ChoreModule()
        {
            this.RequiresAuthentication();

            Get["/chore/{date:datetime}"] = _ =>
            {
                return Negotiate
                    .WithAllowedMediaRange("application/xml")
                    .WithAllowedMediaRange("application/json")
                    .WithModel(GetChoresForDate((DateTime)_.date));
            };
        }

        public List<ChoreGroup> GetChoresForDate(DateTime date)
        {
            var result = new List<ChoreGroup>();

            using (var choreGroupSql = new SqlConnection(CloudConfigurationManager.GetSetting("DatabaseConnection")))
            {
                choreGroupSql.Open();

                var queryChoreGroups = choreGroupSql.CreateCommand();
                queryChoreGroups.CommandText = @"
                    SELECT Id, Name, StartDate, EndDate, RecurrenceDatePart, RecurrenceCount FROM ChoreGroup
                    WHERE StartDate <= @datearg
                    AND (EndDate IS NULL OR (EndDate < @datearg))
                    AND (SELECT COUNT(1) FROM ChoreGroupUser CGU WHERE CGU.GroupId = ChoreGroup.Id) > 0";
                queryChoreGroups.Parameters.AddWithValue("datearg", date);

                using (var choreGroupsReader = queryChoreGroups.ExecuteReader())
                {
                    while (choreGroupsReader.Read())
                    {
                        var choreGroup = new ChoreGroup {
                            Name = choreGroupsReader.GetString(1),
                            StartDate = choreGroupsReader.GetDateTime(2),
                            EndDate = choreGroupsReader.IsDBNull(3) ? DateTime.MaxValue : choreGroupsReader.GetDateTime(3),
                            Chores = new List<Chore>()
                        };

                        using (var choreSql = new SqlConnection(CloudConfigurationManager.GetSetting("DatabaseConnection")))
                        {
                            choreSql.Open();

                            var choreDetail = choreSql.CreateCommand();
                            choreDetail.CommandText = @"
                                SELECT Name, Description FROM Chore
                                WHERE GroupId = @groupid
                                ORDER BY Name ASC";
                            choreDetail.Parameters.AddWithValue("groupid", choreGroupsReader.GetInt32(0));

                            var chores = new List<Chore>();
                            using (var choreDetailReader = choreDetail.ExecuteReader())
                            {
                                while (choreDetailReader.Read())
                                {
                                    chores.Add(
                                        new Chore
                                        {
                                            Name = choreDetailReader.GetString(0),
                                            Description = choreDetailReader.IsDBNull(1) ? null : choreDetailReader.GetString(1)
                                        });
                                }
                            }

                            var choreUsers = choreSql.CreateCommand();
                            choreUsers.CommandText = @"
                                SELECT [User].UserName, [User].GroupMeId FROM [User]
                                JOIN ChoreGroupUser CGU ON CGU.UserId = [User].Id
                                WHERE CGU.GroupId = @groupid
                                ORDER BY [User].UserName ASC";
                            choreUsers.Parameters.AddWithValue("groupid", choreGroupsReader.GetInt32(0));

                            var users = new List<User>();
                            using (var choreUserReader = choreUsers.ExecuteReader())
                            {
                                while (choreUserReader.Read())
                                {
                                    users.Add(
                                        new User
                                        {
                                            UserName = choreUserReader.GetString(0),
                                            GroupMeId = choreUserReader.IsDBNull(1) ? -1 : choreUserReader.GetInt32(1)
                                        });
                                }
                            }

                            var choreRecurrence = new ChoreRecurrence(choreGroup.StartDate, choreGroupsReader.GetString(4), choreGroupsReader.GetInt32(5));

                            var currentUserIndex = -1;
                            var lastRecurrence = choreGroup.StartDate;
                            foreach (var choreDate in choreRecurrence)
                            {
                                if (choreDate <= date)
                                {
                                    lastRecurrence = choreDate;
                                }
                                else
                                {
                                    // We've passed the date, meaning we've found the right period
                                    // Start assigning chores

                                    var i = currentUserIndex;
                                    foreach (var chore in chores)
                                    {
                                        choreGroup.Chores.Add(new Chore
                                        {
                                            User = users[i]
                                        });

                                        i++;
                                        if (i >= users.Count)
                                        {
                                            i = 0;
                                        }
                                    }

                                    choreGroup.CurrentRecurrenceStart = lastRecurrence;
                                    choreGroup.CurrentRecurrenceEnd = choreDate;
                                    break;
                                }

                                // Loop through the user index we use later to start assigning
                                currentUserIndex++;
                                if (currentUserIndex >= users.Count)
                                {
                                    currentUserIndex = 0;
                                }
                            }
                        }
                        result.Add(choreGroup);
                    }
                }
            }

            return result;
        }
        
        public class ChoreRecurrence : IEnumerable<DateTime>, IEnumerator<DateTime>
        {
            private DateTime startDate;
            private string recurrence;
            private int recurrenceCount;

            public ChoreRecurrence(DateTime startDate, string recurrence, int recurrenceCount)
            {
                this.startDate = startDate;
                this.recurrence = recurrence;
                this.recurrenceCount = recurrenceCount;
            }

            private DateTime currentDateTime = DateTime.MinValue;

            public bool MoveNext()
            {
                // IEnumerator specifies that we start before the first element in the collection
                if (currentDateTime == DateTime.MinValue)
                {
                    currentDateTime = startDate;
                }
                else
                {
                    switch (recurrence)
                    {
                        case "year":
                            currentDateTime = currentDateTime.AddYears(1 * recurrenceCount);
                            break;
                        case "quarter":
                            currentDateTime = currentDateTime.AddMonths(3 * recurrenceCount);
                            break;
                        case "month":
                            currentDateTime = currentDateTime.AddMonths(1 * recurrenceCount);
                            break;
                        case "dayofyear":
                            currentDateTime = currentDateTime.AddDays(1 * recurrenceCount);
                            break;
                        case "day":
                            currentDateTime = currentDateTime.AddDays(1 * recurrenceCount);
                            break;
                        case "week":
                            currentDateTime = currentDateTime.AddDays(7 * recurrenceCount);
                            break;
                        case "hour":
                            currentDateTime = currentDateTime.AddHours(1 * recurrenceCount);
                            break;
                        case "minute":
                            currentDateTime = currentDateTime.AddMinutes(1 * recurrenceCount);
                            break;
                        case "second":
                            currentDateTime = currentDateTime.AddSeconds(1 * recurrenceCount);
                            break;
                        case "millisecond":
                            currentDateTime = currentDateTime.AddMilliseconds(1 * recurrenceCount);
                            break;
                        case "microsecond":
                            throw new NotSupportedException("Sub-millisecond recurrence types are not supported");
                        case "nanosecond":
                            throw new NotSupportedException("Sub-millisecond recurrence types are not supported");
                        default:
                            throw new NotSupportedException("Recurrence type not recognized");
                    }
                }
                return true;
            }

            public DateTime Current
            {
                get
                {
                    return currentDateTime;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Reset()
            {
                currentDateTime = DateTime.MinValue;
            }

            public IEnumerator<DateTime> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Dispose()
            {

            }

        }
    }
}