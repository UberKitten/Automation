using Automation.Models;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Automation
{
    public class HVAC : Hub
    {

        public void SendOperation()
        {
            Clients.All.UpdateOperation(new Operation
            {
                Downstairs = new OperationZone
                {
                    Compressor = new OperationItem
                    {
                        Active = true,
                        Timeout = new TimeSpan(0, 15, 0)
                    },
                    Fan = new OperationItem
                    {
                        Active = true,
                        Timeout = new TimeSpan(0, 10, 0)
                    },
                    Heat = new OperationItem
                    {
                        Active = false,
                        Timeout = TimeSpan.Zero
                    }
                },
                Upstairs = new OperationZone
                {
                    Compressor = new OperationItem
                    {
                        Active = true,
                        Timeout = new TimeSpan(0, 5, 0)
                    },
                    Fan = new OperationItem
                    {
                        Active = true,
                        Timeout = new TimeSpan(0, 7, 0)
                    },
                    Heat = new OperationItem
                    {
                        Active = false,
                        Timeout = TimeSpan.Zero
                    }
                }
            });
        }
    }
}