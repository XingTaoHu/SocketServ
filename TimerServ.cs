using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SocketServ
{
    class TimerServ
    {
        //定时器（心跳检测）
        public static void TimerStart()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(Tick);
            timer.Start();
            Console.Read();
        }

        static void Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("每秒执行一次:{0}", e.SignalTime);
        }

    }
}
