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
        //
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("AddScore");
        protoRet.AddInt(player.data.score);
        player.Send(protoRet);
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
}