using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVAC
{
    class Program
    {
        static void Main(string[] args)
        {
            int failures = 0;
            while (failures <= 5)
            {
                var hubConnection = new HubConnection(Properties.Settings.Default.HubConnection);
                hubConnection.Headers.Add("X-AUTH-TOKEN", Properties.Settings.Default.Token);
                var proxy = hubConnection.CreateHubProxy(Properties.Settings.Default.HubProxy);

                proxy.On<Automation.Models.Operation>("UpdateOperation", operation =>
                {
                    Console.WriteLine("Received operation");
                });

                hubConnection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("Error opening connection {0} ", task.Exception.GetBaseException());
                        failures++;
                        System.Threading.Thread.Sleep(1000 * (failures ^ 2)); // exponential backoff
                    }
                    else
                    {
                        Console.WriteLine("Connected");
                    }
                }).Wait();
            }
        }
    }
}
