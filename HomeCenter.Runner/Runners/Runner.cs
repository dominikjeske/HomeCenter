using HomeCenter.Model.Core;
using HomeCenter.Runner.ConsoleExtentions;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public abstract class Runner
    {
        protected string[] _tasks;
        protected Container Container;
        protected IMessageBroker MessageBroker;

        internal Runner(string uid)
        {
            Uid = uid;
        }

        public string Uid { get; }

        public abstract Task RunTask(int taskId);

        public virtual void RunnerReset()
        {
        }

        public void SetContainer(Container container)
        {
            Container = container;
            MessageBroker = container.GetInstance<IMessageBroker>();
        }

        public virtual async Task Run()
        {
            while (true)
            {
                var taskId = Menu();
                if (taskId == -1) continue;
                if (taskId == -2)
                {
                    RunnerReset();
                    break;
                }
                try
                {
                    await RunTask(taskId);
                }
                catch (Exception ee)
                {
                    ConsoleEx.WriteErrorLine(ee.ToString(), true);
                }
            }
        }

        public int Menu()
        {
            ConsoleEx.WriteTitleLine($"{GetType().Name}:");

            for (int i = 0; i < _tasks.Length; i++)
            {
                ConsoleEx.WriteMenuLine($"[{i}] {_tasks[i]}");
            }

            var runnerId = ConsoleEx.ReadNumber();
            if (runnerId == -2) return -2;

            if (runnerId >= _tasks.Length)
            {
                Console.WriteLine($"{runnerId} is outside of the scope");
                return -1;
            }

            return runnerId;
        }
    }
}