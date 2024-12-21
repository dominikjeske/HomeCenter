using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var runner = new HomeCenterRunner();
            await runner.Run();
        }
    }
}