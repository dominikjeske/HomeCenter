using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class WirehomeRunner
    {
        private List<Runner> _Runners = new List<Runner>();

        public WirehomeRunner()
        {
            _Runners.Add(new DenonRunner(0));
        }

        public async Task Run()
        {
            Console.Clear();

            var bootstrapper = new WirehomeBootstrapper("componentConfiguration");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            await Task.Delay(1000).ConfigureAwait(false);

            Console.WriteLine("Select runner:");
            foreach(var runner in _Runners)
            {
                Console.WriteLine($"[{runner.Id}] {runner.GetType().Name}:");
            }

            var runnerId = ConsoleEx.ReadNumber();

            var run = _Runners.Single(r => r.Id == runnerId);
            run.Menu();
            
        }
    }

    

    public abstract class Runner
    {
        public int Id { get; private set; }

        public Runner(int runnerId)
        {
            Id = runnerId;
        }

        public abstract Task Run(int functionality);
        public abstract void MenuItems();

        public int Menu()
        {
            Console.Clear();
            ConsoleEx.WriteTitleLine($"Denon adapter:");
            ConsoleEx.WriteMenuLine("Chose number:");
            MenuItems();
            var runnerId = ConsoleEx.ReadNumber();
            return runnerId;
        }
    }

    public class DenonRunner : Runner
    {
        public DenonRunner(int runnerId) : base(runnerId)
        {
            
        }

        public override void MenuItems()
        {
            ConsoleEx.WriteMenuLine("[1]: VolumeUp");
        }


        public override Task Run(int functionality)
        {
            
            

            return Task.CompletedTask;
        }
    }
}