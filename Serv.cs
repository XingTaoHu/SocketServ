using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace SocketServ
{
    public class Serv
    {
        //监听嵌套字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;

        //获取连接池索引，返回负数表示获取失败
        public int NewIndex() {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        //开启服务器
        public void Start(string host, int port) { 
            //连接池
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            //Listen(参数指定队列中最多可容纳等待接受的连接数，0表示不限制)
            listenfd.Listen(maxConn);
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }

        /// <summary>
        /// Accept回调
        /// 这里处理3件事情
        /// 1.给新的连接分配conn
        /// 2.异步接收客户端数据
        /// 3.再次调用BeginAccept实现循环
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();

                if (index < 0)
                {
                    socket.Close();
                    Console.Write("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接[" + adr + "] conn 池ID:" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn
                        .BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }
        }

        /// <summary>
        /// Receive回调
        /// 这里处理3件事情
        /// 1.接收并处理消息，因为是多人聊天，服务端收到消息后，需要转发给所有人
        /// 2.如果收到客户端关闭连接的信号，则断开连接
        /// 3.继续调用BeginReceive接收下一个数据
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCb(IAsyncResult ar)
        {
            //获取BeginReceive传递的Conn对象
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                //获取接收的字节数
                int count = conn.socket.EndReceive(ar);
                //关闭信号
                if (count <= 0)
                {
                    Console.WriteLine("收到字节数过小[" + conn.GetAddress() + "]断开连接");
                    conn.Close();
                    return;
                }
                //数据处理
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                Console.WriteLine("收到[" + conn.GetAddress() + "] 数据：" + str);
                str = conn.GetAddress() + ":" + str;
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                //广播
                for (int i = 0; i < conns.Length; i++)
                {
                    if (conns[i] == null)
                        continue;
                    if (!conns[i].isUse)
                        continue;
                    Console.WriteLine("将消息转播给：" + conns[i].GetAddress());
                    conns[i].socket.Send(bytes);
                }
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception e)
            {
                Console.WriteLine("接收数据错误[" + e.Message + "]需要断开连接[" + conn.GetAddress() + "]");
                conn.Close();
            }
        }





    }
}
