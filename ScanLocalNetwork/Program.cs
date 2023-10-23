using System.Net.NetworkInformation;

namespace ScanLocalNetwork
{
    class Program
    {
        static void Main()
        {
            // Получаем все сетевые интерфейсы на компьютере
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in interfaces)
            {
                // Проверяем, что интерфейс подключен и является локальной сетью
                if (networkInterface.OperationalStatus == OperationalStatus.Up &&
                    networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    Console.WriteLine("Имя интерфейса: " + networkInterface.Name);
                    Console.WriteLine("MAC-адрес: " + networkInterface.GetPhysicalAddress().ToString());

                    // Получаем IP-адреса интерфейса
                    IPInterfaceProperties ipProps = networkInterface.GetIPProperties();
                    foreach (var ipAddress in ipProps.UnicastAddresses)
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

            Console.ReadLine();
        }
    }
}