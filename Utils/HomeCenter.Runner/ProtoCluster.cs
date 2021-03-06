﻿
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public static class ProtoCluster
    {
        public static Task Start()
        {
            Console.WriteLine("Start listening...");

            var context = new RootContext();
            
            var props = Props.FromProducer(() => new A());
            context.SpawnNamed(props, "chatserver");

            Console.ReadLine();

            return Task.CompletedTask;
        }
    }

    public class A : IActor
    {
        
        public virtual Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
            }
            else
            {
            }

            return Task.CompletedTask;
        }
    }
}