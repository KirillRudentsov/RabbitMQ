using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Configuration;
using System.Threading;
using ConsumerMQ.ServiceReference1;
using System.ServiceModel;

namespace ConsumerMQ
{
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

        public static string ConnSTR
        {
            get
            {
                return ConfigurationSettings.AppSettings["db_connection"];
            }
        }

        public static string EndPointService
        {
            get
            {
                return ConfigurationSettings.AppSettings["EndPointService"];
            }
        }
    }

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

        public void Public(string message, string routingKey, string exchangeName)
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

        public string GetFromQueue(string QueueName, bool Ack)
        {
            var res = "";
            var ack = Ack;
            try
            {
                BasicGetResult basicRes = channel.BasicGet(QueueName, ack);

                if (basicRes != null)
                {
                    IBasicProperties props = basicRes.BasicProperties;
                    byte[] body = basicRes.Body;

                    //channel.BasicAck(basicRes.DeliveryTag, false);

                    res = Encoding.UTF8.GetString(body);

                    return res;
                }
                else { return "-1"; }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public void ClearQueue(string queueName)
        {
            channel.QueuePurge(queueName);
        }

        public void Close()
        {
            connection.Close();
        }

        ~MQProducer() { }
    }

    class Program
    {
        // ASMX SERVER

        static void Main(string[] args)
        {
            Console.WriteLine("Started Application");

            new Thread(() =>
            {
                var mqProcuce = new MQProducer(Config.ip, Config.vhost, Config.username,
                    Config.password, Config.port);

                int nRobotId = 200; // Input from asmx

                try
                {

                    DbServiceSoapClient cl = new DbServiceSoapClient(new BasicHttpBinding(),
                                       new EndpointAddress(Config.EndPointService));
                    var servRes = cl.GetCommandId(nRobotId);

                    Console.WriteLine("Response from asmx : " + servRes.CmdId);
                    Thread.Sleep(2000);
                    

                    var robotName = GetRobotName(nRobotId);
                    var commId = mqProcuce.GetFromQueue(robotName, true);
                    if (commId != "-1")
                    {
                        var nCommId = Convert.ToInt32(commId);
                        Db.start_exec_cmd(nCommId, nRobotId);

                        Console.WriteLine("Response commandId : " + nCommId);

                    }
                    else
                        Console.WriteLine("CommandId for " + robotName + " is not exist yet.");


                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }

            }).Start();

            Console.WriteLine("Press enter to exit");
            Console.ReadKey();
        }

        static string GetRobotName(int robotId)
        {
            var robotName = "";

            if (robotId == 100)
                robotName = "CDR";
            if (robotId == 200)
                robotName = "SIGOS";
            if (robotId == 300)
                robotName = "SAMS";
            if (Enumerable.Range(1, 99).Contains(robotId))
                robotName = "xDriver";

            return robotName;
        }
    }
}
