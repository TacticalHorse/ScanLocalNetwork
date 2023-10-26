using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ScanLocalNetwork
{
    class Program
    {
        static CountdownEvent countdown;
        static int upCount = 0;
        static object lockObj = new object();
        const bool resolveNames = true;

        static void Main(string[] args)
        {
            countdown = new CountdownEvent(1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string ipBase = "192.168.194.";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                countdown.AddCount();
                p.SendAsync(ip, 100, ip);
            }
            countdown.Signal();
            countdown.Wait();
            sw.Stop();
            TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
            Console.ReadLine();
        }

        static void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            string mac = string.Empty;
            try
            {
                mac = GetMacByIP(ip);
            }
            catch 
            { 
                mac = "?"; 
            }

            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                if (resolveNames)
                {
                    string name;
                    try
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                        name = hostEntry.HostName;
                    }
                    catch (SocketException ex)
                    {
                        name = "?";
                    }
                    Console.WriteLine("{0}[{3}] ({1}) is up: ({2} ms)", ip, name, e.Reply.RoundtripTime, mac);
                }
                else
                {
                    Console.WriteLine("{0}[{2}] is up: ({1} ms)", ip, e.Reply.RoundtripTime, mac);
                }
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
                Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            countdown.Signal();
        }
        public static string GetMacByIP(string ipAddress)
        {
            // grab all online interfaces
            var query = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up && // only grabbing what's online
                    n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(_ => new
                {
                    PhysicalAddress = _.GetPhysicalAddress(),
                    IPProperties = _.GetIPProperties(),
                });

            // grab the first interface that has a unicast address that matches your search string
            var mac = query
                .Where(q => q.IPProperties.UnicastAddresses
                    .Any(ua => ua.Address.ToString() == ipAddress))
                .FirstOrDefault()
                .PhysicalAddress;

            // return the mac address with formatting (eg "00-00-00-00-00-00")
            return String.Join("-", mac.GetAddressBytes().Select(b => b.ToString("X2")));
        }
    }
}