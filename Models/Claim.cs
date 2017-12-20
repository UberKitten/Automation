﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation.Models
{
    public class Claim
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Name { get; set; }
    }
}