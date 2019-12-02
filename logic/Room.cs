using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Room
{
    //状态
    public enum Status
    {
        Prepare = 1,
        Fight = 2,
    }

    public Status status = Status.Prepare;
    //玩家
    public int maxPlayers = 6;
    public Dictionary<string, Player> list = new Dictionary<string, Player>();

    /// <summary>
    /// 添加玩家
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool AddPlayer(Player player)
    {
        lock (list) {
            if (list.Count > maxPlayers)
                return false;
            PlayerTempData tempData = player.tempData;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.status = PlayerTempData.Status.Room;
            if (list.Count == 0)
                tempData.isOwner = true;
            string id = player.id;
            list.Add(id, player);
        }
        return true;
    }

    /// <summary>
    /// 分配队伍
    /// </summary>
    /// <returns></returns>
    public int SwitchTeam()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.tempData.team == 1) count1++;
            if (player.tempData.team == 2) count2++;
        }
        if (count1 <= count2)
            return 1;
        else
            return 2;
    }

    /// <summary>
    /// 删除玩家
    /// </summary>
    /// <param name="id"></param>
    public void DelPlayer(string id)
    {
        lock (list) {
            if (!list.ContainsKey(id))
                return;
            bool isOwner = list[id].tempData.isOwner;
            list[id].tempData.status = PlayerTempData.Status.None;
            list.Remove(id);
            if (isOwner)
                UpdateOwner();
        }
    }

    /// <summary>
    /// 更换房主
    /// </summary>
    public void UpdateOwner()
    {
        lock (list) {
            if (list.Count <= 0)
                return;
            foreach (Player player in list.Values) {
                player.tempData.isOwner = false;
            }

            Player p = list.Values.First();
            p.tempData.isOwner = true;
        }
    }

    /// <summary>
    /// 广播消息
    /// </summary>
    /// <param name="proto"></param>
    public void Broadcast(ProtocolBase proto)
    {
        foreach (Player player in list.Values)
        {
            player.Send(proto);
        }
    }

    /// <summary>
    /// 房间信息
    /// </summary>
    /// <returns></returns>
    public ProtocolBytes GetRoomInfo()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        //房间信息
        protocol.AddInt(list.Count);
        //每个玩家的信息
        foreach (Player p in list.Values)
        {
            protocol.AddString(p.id);
            protocol.AddInt(p.tempData.team);
            protocol.AddInt(p.data.win);
            protocol.AddInt(p.data.fail);
            int isOwner = p.tempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
        }
        return protocol;
    }

    /// <summary>
    /// 房间能否开战
    /// </summary>
    /// <returns></returns>
    public bool CanStart()
    {
        if (status != Status.Prepare)
            return false;
        int count1 = 0;
        int count2 = 0;
        foreach (Player player in list.Values)
        {
            if (player.tempData.team == 1) count1++;
            if (player.tempData.team == 2) count2++;
        }
        if (count1 < 1 || count2 < 1)
            return false;
        return true;
    }

}
