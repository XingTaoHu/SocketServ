using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RoomMgr
{
    //单例
    public static RoomMgr instance;
    public RoomMgr()
    {
        instance = this;
    }

    //房间列表
    public List<Room> list = new List<Room>();

    /// <summary>
    /// 创建房间
    /// </summary>
    /// <param name="player"></param>
    public void CreateRoom(Player player) {
        Room room = new Room();
        lock (list) {
            list.Add(room);
            room.AddPlayer(player);
        }
    }

    /// <summary>
    /// 玩家离开
    /// </summary>
    /// <param name="player"></param>
    public void LeaveRoom(Player player) {
        PlayerTempData tempData = player.tempData;
        if (tempData.status == PlayerTempData.Status.None)
            return;
        Room room = tempData.room;
        lock (list) {
            room.DelPlayer(player.id);
            if (room.list.Count == 0)
                list.Remove(room);
        }
    }

    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <returns></returns>
    public ProtocolBytes GetRoomList()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        int count = list.Count;
        protocol.AddInt(count);
        for (int i = 0; i < count; i++) {
            Room room = list[i];
            protocol.AddInt(room.list.Count);
            protocol.AddInt((int)room.status);
        }
        return protocol;
    }

}