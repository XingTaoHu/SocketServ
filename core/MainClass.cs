using System;

class MainClass
{
    public static void Main(string[] args)
    {
        //DataMgr dataMgr = new DataMgr();
        ////注册
        //bool ret = dataMgr.Register("hxt", "123456");
        //if (ret)
        //    Console.WriteLine("注册成功");
        //else
        //    Console.WriteLine("注册失败");
        ////创建玩家
        //ret = dataMgr.CreatePlayer("hxt");
        //if (ret)
        //    Console.WriteLine("创建玩家成功");
        //else
        //    Console.WriteLine("创建玩家失败");
        ////获取玩家数据
        //PlayerData pd = dataMgr.GetPlayerData("hxt");
        //if (pd != null)
        //    Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        //else
        //    Console.WriteLine("获取玩家数据失败 ");
        ////更改玩家数据
        //pd.score += 10;
        ////保存数据
        //Player p = new Player();
        //p.id = "hxt";
        //p.data = pd;
        //ret = dataMgr.SavePlayer(p);
        //if (ret)
        //    Console.WriteLine("更改保存玩家数据成功");
        //else
        //    Console.WriteLine("更改保存玩家数据失败");
        ////重新读取
        //pd = dataMgr.GetPlayerData("hxt");
        //if (pd != null)
        //    Console.WriteLine("获取玩家成功 分数是 " + pd.score);
        //else
        //Console.WriteLine("重新获取玩家数据失败");

        DataMgr dataMgr = new DataMgr();
        ServNet servNet = new ServNet();
        Scene scene = new Scene();
        servNet.proto = new ProtocolBytes();
        servNet.Start("127.0.0.1", 1234);

        while(true)
        {
            string str = Console.ReadLine();
            switch(str)
            {
                case "quit":
                    servNet.Close();
                    return;
                case "print":
                    servNet.Print();
                    break;
            }
        }
    }
}