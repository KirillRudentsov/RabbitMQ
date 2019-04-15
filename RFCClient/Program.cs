using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using System.Threading;
using GuiTesterWebServiceManager;


namespace ProducerMQ
{
    class MQProducer
    {
        public IConnection connection;
        public IModel channel;
        public string replyQueueName;

        public MQProducer(string ip, string vhost, string username, string pass, int port)
        {
            var factory = new ConnectionFactory()
            {
                HostName = ip,
                VirtualHost = vhost,
                UserName = username,
                Password = pass,
                Port = port
            };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
        }

        public void Public(string message,string routingKey, string exchangeName)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: exchangeName,
                                 routingKey: routingKey,
                                 basicProperties: props,
                                 body: messageBytes);

        }

        public void ClearQueue(string queueName)
        {
            channel.QueuePurge(queueName);
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public void Close()
        {
            connection.Close();
        }

        ~MQProducer() { }
    }

    static class Config
    {
        public static string ip
        {
            get
            {
                return ConfigurationSettings.AppSettings["ip"];
            }
        }

        public static string vhost
        {
            get
            {
                return ConfigurationSettings.AppSettings["vhost"];
            }
        }

        public static string username
        {
            get
            {
               return ConfigurationSettings.AppSettings["userName"];
            }
        }

        public static string password
        {
            get
            {
                return ConfigurationSettings.AppSettings["password"];
            }
        }

        public static int port
        {
            get
            {
                return Convert.ToInt32(ConfigurationSettings.AppSettings["port"]);
            }
        }

        public static int Delay
        {
            get
            {
                return Convert.ToInt32(ConfigurationSettings.AppSettings["DelayBetweenRequestCommandId"]);
            }
        }

        public static string ConnSTR
        {
            get
            {
                return ConfigurationSettings.AppSettings["db_connection"];
            }
        }

        public static string GetAllRobots
        {
            get
            {
                return ConfigurationSettings.AppSettings["GetAllRobots"];
            }
        }

        public static string GetCommandIdList
        {
            get
            {
                return ConfigurationSettings.AppSettings["GetCommandIdList"];
            }
        }
    }

    class Program
    {
        public static int countRecord = 0;

        static void Main(string[] args)
        {
            Console.BufferHeight = 5000;

            while (0 == 0)
            {
                try
                {
                    var mqProduce = new MQProducer(Config.ip, Config.vhost,
                            Config.username, Config.password, Config.port);

                    pushQueueRobots(mqProduce);

                    mqProduce.Close();
                    mqProduce.Dispose();

                    Console.WriteLine("Pushed!");
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                finally { Thread.Sleep(Config.Delay); }
            }
            Console.ReadKey();
        }

        static void pushQueueRobots(MQProducer producer)
        {
            try
            {
                var RobotARR = Db.GetListData(Config.GetAllRobots).Split(',');
                foreach (var robot in RobotARR)
                {
                    var CommandARR = Db.GetListData(Config.GetCommandIdList.Replace("*", robot)).Split(',');
                    countRecord = CommandARR.Length;
                    var RobotName = GetRobotName(robot);
                    producer.ClearQueue(RobotName); // QueueName = RobotName

                    foreach (var com in CommandARR)
                    {
                        producer.Public(com, RobotName, "GetCommandId");
                        //Console.WriteLine("Published : " + com + " for robotId : " + robot);
                    }
                }
                //Console.WriteLine("\n");

            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }

        static string GetRobotName(string robotId)
        {
            var robotName = "";
            var nRobotId = 0;

            try
            {
                nRobotId = Convert.ToInt32(robotId);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }


            // Real Robot's
            /* if (nRobotId == 100)
                 robotName = "CDR";
             if (nRobotId == 200)
                 robotName = "SIGOS";
             if (nRobotId == 400)
                 robotName = "SAMS";
             if (Enumerable.Range(1, 99).Contains(nRobotId))
                 robotName = "XDRIVER";
             if (nRobotId == 300)
                 robotName = "PAYMENT";
            */

            // For test example
            if (nRobotId == 300)
                robotName = "SAMS";

            return robotName;
        }

    }


}
