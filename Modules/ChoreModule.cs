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

        public static List<ChoreGroup> GetChoresForDate(DateTime date)
        {
            var result = new List<ChoreGroup>();

            using (var choreGroupSql = new SqlConnection(CloudConfigurationManager.GetSetting("DatabaseConnection")))
            {
                choreGroupSql.Open();

                var queryChoreGroups = choreGroupSql.CreateCommand();
                queryChoreGroups.CommandText = @"
                    SELECT Id, Name, StartDate, EndDate, RecurrenceDatePart, RecurrenceCount, SkipDatePart, SkipCount FROM ChoreGroup
                    WHERE StartDate <= @datearg
                    AND (EndDate IS NULL OR (EndDate > @datearg))
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
                            RecurrenceDatePart = choreGroupsReader.GetString(4),
                            RecurrenceCount = choreGroupsReader.GetInt32(5),
                            SkipDatePart = choreGroupsReader.IsDBNull(6) ? null : choreGroupsReader.GetString(6),
                            SkipCount = choreGroupsReader.IsDBNull(7) ? 0 : choreGroupsReader.GetInt32(7),
                            Chores = new List<Chore>()
                        };

                        var choreGroupId = choreGroupsReader.GetInt32(0);

                        using (var choreSql = new SqlConnection(CloudConfigurationManager.GetSetting("DatabaseConnection")))
                        {
                            choreSql.Open();

                            var choreDetail = choreSql.CreateCommand();
                            choreDetail.CommandText = @"
                                SELECT Name, Description FROM Chore
                                WHERE GroupId = @groupid
                                ORDER BY Name ASC";
                            choreDetail.Parameters.AddWithValue("groupid", choreGroupId);

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
                            choreUsers.Parameters.AddWithValue("groupid", choreGroupId);

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

                            var choreRecurrence = new ChoreRecurrence(choreGroup.StartDate, choreGroup.RecurrenceDatePart, choreGroup.RecurrenceCount, choreGroup.SkipDatePart, choreGroup.SkipCount);

                            var currentUserIndex = 0;
                            foreach (var chorePeriod in choreRecurrence)
                            {
                                // We're in the chore period
                                if (chorePeriod.Item1 <= date && date < chorePeriod.Item2)
                                {
                                    // We've passed the date, meaning we've found the right period
                                    // Start assigning chores

                                    var i = currentUserIndex;
                                    foreach (var chore in chores)
                                    {
                                        chore.User = users[i];
                                        choreGroup.Chores.Add(chore);

                                        i++;
                                        if (i >= users.Count)
                                        {
                                            i = 0;
                                        }
                                    }

                                    choreGroup.CurrentRecurrenceStart = chorePeriod.Item1;
                                    choreGroup.CurrentRecurrenceEnd = chorePeriod.Item2;
                                    break;
                                }

                                // We're past the chore period, must have skipped this date
                                if (date < chorePeriod.Item1)
                                {
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
        
        public class ChoreRecurrence : IEnumerable<Tuple<DateTime, DateTime>>, IEnumerator<Tuple<DateTime, DateTime>>
        {
            private DateTime startDate;
            private string recurrence;
            private int recurrenceCount;
            private string skip;
            private int skipCount;

            public ChoreRecurrence(DateTime startDate, string recurrence, int recurrenceCount, string skip, int? skipCount)
            {
                this.startDate = startDate;
                this.recurrence = recurrence;
                this.recurrenceCount = recurrenceCount;
                this.skip = skip;
                this.skipCount = skipCount.HasValue ? skipCount.Value : 0; // We don't use -1 because it's a valid argument to SqlDateAdd
            }

            private DateTime startDateTime = DateTime.MinValue;
            private DateTime endDateTime = DateTime.MinValue;

            public bool MoveNext()
            {
                // IEnumerator specifies that we start before the first element in the collection
                if (startDateTime == DateTime.MinValue)
                {
                    startDateTime = startDate;
                    endDateTime = SqlDateAdd(startDateTime, recurrence, recurrenceCount);
                }
                else
                {
                    startDateTime = endDateTime;
                    if (!String.IsNullOrEmpty(skip))
                    {
                        startDateTime = SqlDateAdd(startDateTime, skip, skipCount);
                    }
                    endDateTime = SqlDateAdd(startDateTime, recurrence, recurrenceCount);
                }
                return true;
            }

            public static DateTime SqlDateAdd(DateTime currentDateTime, string datePart, int count)
            {
                switch (datePart.ToLower())
                {
                    case "year":
                        return currentDateTime.AddYears(1 * count);
                    case "quarter":
                        return currentDateTime.AddMonths(3 * count);
                    case "month":
                        return currentDateTime.AddMonths(1 * count);
                    case "dayofyear":
                        return currentDateTime.AddDays(1 * count);
                    case "day":
                        return currentDateTime.AddDays(1 * count);
                    case "week":
                        return currentDateTime.AddDays(7 * count);
                    case "hour":
                        return currentDateTime.AddHours(1 * count);
                    case "minute":
                        return currentDateTime.AddMinutes(1 * count);
                    case "second":
                        return currentDateTime.AddSeconds(1 * count);
                    case "millisecond":
                        return currentDateTime.AddMilliseconds(1 * count);
                    case "microsecond":
                        throw new NotSupportedException("Sub-millisecond recurrence types are not supported");
                    case "nanosecond":
                        throw new NotSupportedException("Sub-millisecond recurrence types are not supported");
                    default:
                        throw new NotSupportedException("Recurrence type not recognized");
                }
            } 

            public Tuple<DateTime, DateTime> Current
            {
                get
                {
                    return new Tuple<DateTime, DateTime>(startDateTime, endDateTime);
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
                startDateTime = DateTime.MinValue;
                endDateTime = DateTime.MinValue; ;
            }

            public IEnumerator<Tuple<DateTime, DateTime>> GetEnumerator()
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