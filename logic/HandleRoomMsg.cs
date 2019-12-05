using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class HandlePlayerMsg
{
    /// <summary>
    /// 获取玩家战绩查询
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgGetAchieve(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetAchieve");
        protocolRet.AddInt(player.data.win);
        protocolRet.AddInt(player.data.fail);
        player.Send(protocolRet);
    }

    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgGetRoomList(Player player, ProtocolBase protoBase)
    {
        player.Send(RoomMgr.instance.GetRoomList());
    }

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgCreateRoom(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        //条件检测
        if (player.tempData.status != PlayerTempData.Status.None)
        {
            Console.WriteLine("MsgCreateRoom Fail " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        RoomMgr.instance.CreateRoom(player);
        protocol.AddInt(0);
        player.Send(protocol);
    }

    /// <summary>
    /// 加入房间
    /// </summary>
    public void MsgEnterRoom(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        int start = 0;
        string protoName = protocol.GetString(start, ref start);
        int index = protocol.GetInt(start, ref start);
        Console.WriteLine("[MsgEnterRoom]" + player.id + " " + index);
        protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");
        if (index < 0 || index >= RoomMgr.instance.list.Count)
        {
            Console.WriteLine("[MsgEnterRoom] index err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        Room room = RoomMgr.instance.list[index];
        if (room.status != Room.Status.Prepare)
        {
            Console.WriteLine("[MsgEnterRoom] status err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        if (room.AddPlayer(player))
        {
            room.Broadcast(room.GetRoomInfo());
            protocol.AddInt(0);
            player.Send(protocol);
        }
        else
        {
            Console.WriteLine("[MsgEnterRoom] maxPlayer err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
        }
    }

    /// <summary>
    /// 获取房间信息
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgGetRoomInfo(Player player, ProtocolBase protoBase)
    {
        if (player.tempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("MsgGetRoomInfo status err " + player.id);
            return;
        }
        Room room = player.tempData.room;
        player.Send(room.GetRoomInfo());
    }

    /// <summary>
    /// 离开玩家
    /// </summary>
    /// <param name="player"></param>
    /// <param name="protoBase"></param>
    public void MsgLeaveRoom(Player player, ProtocolBase protoBase)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        if (player.tempData.status != PlayerTempData.Status.Room)
        {
            Console.WriteLine("MsgLeaveRoom status err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        protocol.AddInt(0);
        player.Send(protocol);
        Room room = player.tempData.room;
        RoomMgr.instance.LeaveRoom(player);
        if (room != null)
            room.Broadcast(room.GetRoomInfo());
    }

}
