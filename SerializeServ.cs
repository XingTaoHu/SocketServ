using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace SocketServ
{
    class SerializeServ
    {
        //序列化数据
        public static void SerializePlayer()
        {
            Player player = new Player();
            player.coin = 1;
            player.money = 10;
            player.name = "Rainer";
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("data.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, player);
            stream.Close();
        }

        //反序列化数据
        public static void DeserializePlayer()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("data.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            Player player = (Player)formatter.Deserialize(stream);
            stream.Close();
            Console.WriteLine("coin:{0}", player.coin);
            Console.WriteLine("money:{0}", player.money);
            Console.WriteLine("name:{0}", player.name);
            Console.Read();
        }
    }
}
