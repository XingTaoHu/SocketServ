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

}
