﻿using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => "It works!";
        }
    }
}