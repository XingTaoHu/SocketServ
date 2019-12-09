using System;
 
public class HandlePlayerEvent
{
    //上线
    public void OnLogin(Player player)
    {
        Scene.instance.AddPlayer(player.id);
    }

    //下线
    public void OnLogout(Player player)
    {
        Scene.instance.DelPlayer(player.id);
        if (player.tempData.status == PlayerTempData.Status.Room)
        {
            Room room = player.tempData.room;
            RoomMgr.instance.LeaveRoom(player);
            if (room != null)
                room.Broadcast(room.GetRoomInfo());
        }
        if(player.tempData.status == PlayerTempData.Status.Fight)
        {
            Room room = player.tempData.room;
            room.ExitFight(player);
            RoomMgr.instance.LeaveRoom(player);
        }
    }
}