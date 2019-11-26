public class PlayerTempData
{
    public PlayerTempData()
    {
        status = Status.None;
    }

    //状态
    public enum Status
    { 
        None,
        Room,
        Fight,
    }

    public Status status;
    public Room room;
    public int team = 1;
    public bool isOwner = false;
}