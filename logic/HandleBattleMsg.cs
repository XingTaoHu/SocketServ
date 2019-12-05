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
}
