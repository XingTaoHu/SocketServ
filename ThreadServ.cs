using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SocketServ
{
    class ThreadServ
    {
        //线程互斥
        static string str = "";
        public static void ThreadStart()
        {
            Thread t1 = new Thread(Add1);
            t1.Start();
            Thread t2 = new Thread(Add2);
            t2.Start();
            Thread.Sleep(1000);
            Console.WriteLine(str);
            Console.Read();
        }
        static void Add1()
        {
            lock (str)
            {
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(20);
                    str += "A";
                }
            }
        }
        static void Add2()
        {
            lock (str)
            {
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(20);
                    str += "B";
                }
            }
        }
    }
}
