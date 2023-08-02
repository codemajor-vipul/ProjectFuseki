using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FusekiAPI.RabbitConnection
{
    public class RabbitConnectionFactory
    {
        private IConfiguration _iconfiguration;
        public RabbitConnectionFactory(IConfiguration configuration)
        {
            _iconfiguration = configuration;
        }

        public IModel ConnectToRabbitMQ()
        {
            IConnection conn;
            IModel channel;
            ConnectionFactory factory = new ConnectionFactory();
            factory.HostName = _iconfiguration.GetValue<string>("RabbitMQ:HostName");
            factory.VirtualHost = _iconfiguration.GetValue<string>("RabbitMQ:VirtualHost");
            factory.Port = _iconfiguration.GetValue<int>("RabbitMQ:Port");
            factory.UserName = _iconfiguration.GetValue<string>("RabbitMQ:UserName");
            factory.Password = _iconfiguration.GetValue<string>("RabbitMQ:Password");
            conn = factory.CreateConnection();
            channel = conn.CreateModel();
            channel.ExchangeDeclare(
                "ex.Fuseki",
                "direct",
                true,
                false,
                null
                );
            channel.ExchangeDeclare(
               "ex.FusekiTransactions",
               "fanout",
               true,
               false,
               null
               );
            channel.QueueDeclare(
                "FusekiQueue",
                true,
                false,
                false,
                null);
            channel.QueueDeclare(
                "FusekiTransactionsStatus",
                true,
                false,
                false,
                null);
            channel.QueueBind(
                "FusekiQueue",
                "ex.Fuseki",
                "fuseki123",
                null);
            channel.QueueBind(
                "FusekiTransactionsStatus",
                "ex.FusekiTransactions",
                "",
                null);
            return channel;
        }
    }
}
