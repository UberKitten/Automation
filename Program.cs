using Mono.Unix;
using Mono.Unix.Native;
using Microsoft.Owin.Hosting;
using System;

namespace Automation
{
    public class Program
    {
        static void Main(string[] args)
        {
            var uri = "http://+:5001";

            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("Nancy started on " + uri);

                // check if we're running on mono
                if (Type.GetType("Mono.Runtime") != null)
                {
                    // on mono, processes will usually run as daemons - this allows you to listen
                    // for termination signals (ctrl+c, shutdown, etc) and finalize correctly
                    UnixSignal.WaitAny(new[] {
                        new UnixSignal(Signum.SIGINT),
                        new UnixSignal(Signum.SIGTERM),
                        new UnixSignal(Signum.SIGQUIT),
                        new UnixSignal(Signum.SIGHUP)
                    });
                }
                else
                {
                    Console.ReadLine();
                }

                Console.WriteLine("Stopping Nancy");
            }
        }
    }
}