using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class HandlePlayerMsg
{
    /// <summary>
    /// 开始战斗
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgStartFight(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        if (player.tempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("[MsgStartFight] status err:" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        if (!player.tempData.isOwner)
        {
            Console.WriteLine("[MsgStartFight] owner err:" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        Room room = player.tempData.room;
        if (!room.CanStart())
        {
            Console.WriteLine("[MsgStartFight] CanStart err:" + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        protocol.AddInt(0);
        player.Send(protocol);
        room.StartFight();
    }

    /// <summary>
    /// 同步坦克单元
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgUpdateUnitInfo(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        int start = 0;
        string protoName = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float posZ = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        float gunRot = protocol.GetFloat(start, ref start);
        float gunRoll = protocol.GetFloat(start, ref start);
        if (player.tempData.status != PlayerTempData.Status.Fight)
            return;
        Room room = player.tempData.room;
        player.tempData.posX = posX;
        player.tempData.posY = posY;
        player.tempData.posZ = posZ;
        player.tempData.lastUpdateTime = Sys.GetTimeStamp();
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("UpdateUnitInfo");
        protoRet.AddString(player.id);
        protoRet.AddFloat(posX);
        protoRet.AddFloat(posY);
        protoRet.AddFloat(posZ);
        protoRet.AddFloat(rotX);
        protoRet.AddFloat(rotY);
        protoRet.AddFloat(rotZ);
        protoRet.AddFloat(gunRot);
        protoRet.AddFloat(gunRoll);
        room.Broadcast(protoRet);
    }
}
