using System;
using System.Threading.Tasks;

namespace SonyBraviaRegisterTool
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var request = new SonyRegisterRequest();
                var http = new HttpMessagingService(new EventAggregator());

                Console.WriteLine("Sony Bravia TV registration tool.");
                Console.WriteLine("");
                Console.WriteLine("Write address of Sony Bravia TV (format like: 192.168.0.107)");

                var address = Console.ReadLine();
                request.Address = address?.Length == 0 ? "192.168.0.107" : address;

                await http.SendPostRequest(new MessageEnvelope<SonyRegisterRequest>(request)).ConfigureAwait(false);

                Console.WriteLine("Enter PIN from TV:");

                request.PIN = Console.ReadLine();

                await http.SendPostRequest(new MessageEnvelope<SonyRegisterRequest>(request)).ConfigureAwait(false);

                var key = request.ReadAuthKey();

                Console.WriteLine("Device was registered successfully. Applicaction hash:");
                Console.WriteLine(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
