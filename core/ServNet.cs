﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
using System.Reflection;

public class ServNet
{
    //监听套接字
    public Socket listenfd;
    //客户端连接
    public Conn[] conns;
    //最大连接数
    public int maxConn = 50;
    //指向MySQL连接的成员
    MySqlConnection sqlConn;
    //单例
    public static ServNet instance;

    //主定时器
    System.Timers.Timer timer = new System.Timers.Timer(1000);
    //心跳时间
    public long heartBeatTime = 180;

    //协议
    public ProtocolBase proto;

    //消息分类
    public HandleConnMsg handleConnMsg = new HandleConnMsg();
    public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
    public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

    public ServNet()
    {
        instance = this;
    }

    //获取连接池索引，返回负数表示获取失败
    public int NewIndex()
    {
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
    public void Start(string host, int port)
    {
        //数据库
        ConnectMysql();
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
        //Listen (参数指定队列中最多可容纳等待接受的连接数，0表示不限制)
        listenfd.Listen(maxConn);
        //Accept
        listenfd.BeginAccept(AcceptCb, null);
        Console.WriteLine("[服务器]启动成功");
        //定时器
        timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
        timer.AutoReset = false;
        timer.Enabled = true;
    }

    //主定时器
    public void HandleMainTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        //处理心跳
        HeartBeat();
        timer.Start();
    }
    //心跳
    public void HeartBeat()
    {
        //Console.WriteLine("[主定时器执行]");
        long timeNow = Sys.GetTimeStamp();
        for (int i = 0; i < conns.Length;  i++)
        {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            if(conn.lastTickTime < timeNow - heartBeatTime)
            {
                Console.WriteLine("[心跳引起断开连接]" + conn.GetAddress());
                lock (conn)
                    conn.Close();
            }
        }
    }

    //连接数据库
    public void ConnectMysql()
    {
        string connStr = "Database=game;Data Source=127.0.0.1;";
        connStr += "User Id=root;Password=123456;port=3306";
        sqlConn = new MySqlConnection(connStr);
        try{
            sqlConn.Open();
        }
        catch(Exception e)
        {
            Console.WriteLine("[ServNet] 数据库连接失败 " + e.Message);
        }
    }

    //Accept回调
    private void AcceptCb(IAsyncResult ar)
    {
        try{
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            if(index < 0)
            {
                socket.Close();
                Console.WriteLine("[警告]连接已满");
            }
            else
            {
                Conn conn = conns[index];
                conn.Init(socket);
                string adr = conn.GetAddress();
                Console.WriteLine("客户端连接[" + adr + "] conn 池ID：" + index);
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch(Exception e){
            Console.WriteLine("AcceptCb 失败:" + e.Message);
        }
    }

    //Receive回调 
    private void ReceiveCb(IAsyncResult ar)
    {
        //获取传递的Conn对象 
        Conn conn = (Conn)ar.AsyncState;
        //加锁，防止多线程操作同一对象
        lock(conn)
        {
            try{
                int count = conn.socket.EndReceive(ar);
                Console.WriteLine("收到字节数长度:" + count);
                //关闭信号
                if(count <= 0)
                {
                    Console.WriteLine("收到字节数过小[" + conn.GetAddress() + "]断开连接");
                    conn.Close();
                    return;
                }
                conn.buffCount += count;
                //判断缓存区数据是否可以处理
                ProcessData(conn);
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            catch(Exception e)
            {
                Console.WriteLine("接收数据错误[" + e.Message + "]需要断开连接[" + conn.GetAddress() + "]");
                conn.Close();
            }
        }
    }

    //缓存区数据处理
    private void ProcessData(Conn conn)
    {
        //小于长度字节
        if (conn.buffCount < sizeof(Int32))
            return;
        //消息长度
        Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));//将readBuff前4个字节复制到lenBytes
        conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);//获取接收数据长度
        if (conn.buffCount < conn.msgLength + sizeof(Int32))    //如果数据分包了，先不处理
            return;
        //处理消息
        ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
        HandleMsg(conn, protocol);
        //清除已经处理的消息
        int count = conn.buffCount - conn.msgLength - sizeof(Int32);
        Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
        conn.buffCount = count;
        if(conn.buffCount > 0)
        {
            ProcessData(conn);
        }
    }

    //处理消息
    private void HandleMsg(Conn conn, ProtocolBase protocolBase)
    {
        string name = protocolBase.GetName();
        string methodName = "Msg" + name;
        //连接协议分发
        if(conn.player == null || name == "HeatBeat" || name == "Logout")
        {
            MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
            if(mm == null)
            {
                string str = "[警告]HandleMsg没有处理连接方法:";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[] { conn, protocolBase };
            Console.WriteLine("[处理连接消息]" + conn.GetAddress() + ":" + name);
            mm.Invoke(handleConnMsg, obj);
        }
        //角色协议分发 
        else{
            MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
            if(mm == null)
            {
                string str = "[警告]HandleMsg没有处理玩家方法:";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[] { conn.player, protocolBase };
            Console.WriteLine("[处理玩家消息]" + conn.player.id + ":" + name);
            mm.Invoke(handlePlayerMsg, obj);
        }
    }

    //发送消息
    public void Send(Conn conn, ProtocolBase protocol)
    {
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendbuff = length.Concat(bytes).ToArray();
        Console.WriteLine("bytes length:" + bytes.Length + ", length length:" + length.Length + ", sendbuff length:" + sendbuff.Length);
        try{
            //这里是异步发送，可以使用类似粘包分包处理方法确保sendbuff的全部内容被发送出去
            conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
        }
        catch(Exception e)
        {
            Console.WriteLine("[发送消息]" + conn.GetAddress() + " : " + e.Message);
        }
    }

    //广播
    public void Broadcast(ProtocolBase protocol)
    {
        for (int i = 0; i < conns.Length; i++)
        {
            if (!conns[i].isUse)
                continue;
            if (conns[i].player == null)
                continue;
            Send(conns[i], protocol);
        }
    }

    //关闭
    public void Close()
    {
        for (int i = 0; i < conns.Length; i++)
        {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            lock(conn)
            {
                conn.Close();
            }
        }
    }


    //打印信息
    public void Print()
    {
        Console.WriteLine("===服务器登录信息===");
        for (int i = 0; i < conns.Length; i++)
        {
            if (conns[i] == null)
                continue;
            if (!conns[i].isUse)
                continue;
            string str = "连接[" + conns[i].GetAddress() + "] ";
            if (conns[i].player != null)
                str += "玩家id " + conns[i].player.id;
            Console.WriteLine(str);
        }
    }


}