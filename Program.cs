using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketServ
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            OpenSocket();
        }

        static void OpenSocket()
        { 
            //Socket 
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
            listenfd.Bind(ipEp);
            //Listen(参数指定队列中最多可容纳等待接受的连接数，0表示不限制)
            listenfd.Listen(0);
            Console.WriteLine("[服务器]启动成功");
            while (true) { 
                //Accept
                Socket connfd = listenfd.Accept();
                Console.WriteLine("[服务器]Accept");
                //Recv
                byte[] readBuff = new byte[1024];
                int count = connfd.Receive(readBuff);
                string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
                Console.WriteLine("[服务器接收]" + str);
                //send
                byte[] bytes = System.Text.Encoding.Default.GetBytes("serv echo " + str);
                connfd.Send(bytes);
            }
        }

    }
}
