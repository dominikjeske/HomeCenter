using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class MqqtRunner : Runner
    {
        public MqqtRunner(string uid) : base(uid)
        {
            _tasks = new string[] { "Test" };
        }

        public override async Task RunTask(int taskId)
        {
            var mqttFactory = new MqttClientFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer("homeassistant.local")
                    .WithCredentials("dominik", "9dominik")
                    .Build();

                var result = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("samples/temperature/living_room")
                    .WithPayload("19.5")
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                await mqttClient.DisconnectAsync();

                Console.WriteLine("MQTT application message is published.");
            }
        }
    }
}