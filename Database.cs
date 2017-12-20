using Automation.Models;
using SQLite;
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
            var db = new SQLiteConnection(ConfigurationManager.AppSettings["DatabaseFile"]);

            db.CreateTable<User>();
            db.CreateTable<UserClaim>();
            db.CreateTable<Claim>();
            db.CreateTable<ClaimURL>();
            db.CreateTable<Token>();

            return db;
        }
    }
}