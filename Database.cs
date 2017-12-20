using Automation.Models;
using Automation.Properties;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Automation
{
    public static class Database
    {
        public static SQLiteConnection GetConnection()
        {
            var db = new SQLiteConnection(Settings.Default.DatabaseFile);

            db.CreateTable<User>();
            db.CreateTable<UserClaim>();
            db.CreateTable<Claim>();
            db.CreateTable<ClaimURL>();
            db.CreateTable<Token>();

            return db;
        }

        public static User GetUser(this SQLiteConnection db, string token)
        {
            try
            {
                var tokenobj = db.Get<Token>(token.ToLower());
            
                if (tokenobj == null)
                    return null;

                var userobj = db.GetWithChildren<User>(tokenobj.UserId, recursive: true);
       
                return userobj;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static List<ClaimURL> GetClaimURLs(this SQLiteConnection db, Claim claim)
        {
            return db.GetWithChildren<Claim>(claim.Id).ClaimURLs;
        }
    }
}