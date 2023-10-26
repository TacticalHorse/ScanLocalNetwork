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
            string ipBase = "10.22.4.";
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
                        // Проверяем, что это IPv4-адрес
                        if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine("IP-адрес: " + ipAddress.Address.ToString());

                            // Проверяем, есть ли шлюз по умолчанию
                            var defaultGateway = ipProps.GatewayAddresses.FirstOrDefault();
                            if (defaultGateway != null)
                            {
                                Console.WriteLine("Шлюз по умолчанию: " + defaultGateway.Address.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Шлюз по умолчанию: Не задан");
                            }

                            Console.WriteLine("Маска подсети: " + ipAddress.IPv4Mask.ToString());
                        }
                    }

                    Console.WriteLine("=========================================");
                }
            }
            countdown.Signal();
        }
    }
}