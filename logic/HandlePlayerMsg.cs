using System;
 
public partial class HandlePlayerMsg{

    //增加分数
    //协议参数
    public void MsgAddScore(Player player, ProtocolBase protoBase)
    {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        player.data.score += 1;
        Console.WriteLine("MsgAddScore " + player.id + " " + player.data.score);
        //增加分数
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("AddScore");
        protoRet.AddString(player.id);
        protoRet.AddInt(player.data.score);
        ServNet.instance.Broadcast(protoRet);
    }

    //获取分数
    //协议参数
    //返回协议:int分数
    public void MsgGetScore(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        protocol.AddInt(player.data.score);
        player.Send(protocol);
        Console.WriteLine("MsgGetScore " + player.id + " " + player.data.score);
    }

    //获取玩家列表
    public void MsgGetList(Player player, ProtocolBase protoBase)
    {
        Scene.instance.SendPlayerList(player);
    }

    //更新信息
    public void MsgUpdateInfo(Player player, ProtocolBase protoBase)
    {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float x = protocol.GetFloat(start, ref start);
        float y = protocol.GetFloat(start, ref start);
        float z = protocol.GetFloat(start, ref start);
        int score = player.data.score;
        Scene.instance.UpdateInfo(player.id, x, y, z, score);
        //广播 
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("UpdateInfo");
        protoRet.AddString(player.id);
        protoRet.AddFloat(x);
        protoRet.AddFloat(y);
        protoRet.AddFloat(z);
        protoRet.AddInt(score);
        ServNet.instance.Broadcast(protoRet);
    }

}