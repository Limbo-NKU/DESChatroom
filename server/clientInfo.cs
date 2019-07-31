using System;
using System.Net;
using System.Net.Sockets;

namespace myChatRoom
{
    public partial class chatServer{
        class clientInfo
        {
            TcpClient client;
            byte[] key;
            string userName;

            public clientInfo(TcpClient newClient){
                Client=newClient;
                key=new byte[8];
                Random random=new Random();
                random.NextBytes(key);
                userName=((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()+":"
                    +((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
            }
            clientInfo(){
                key=new byte[8];
                Random random=new Random();
                random.NextBytes(key);
            }

            public TcpClient Client { get => client; set => client = value; }
            public byte[] Key { get => key; set => key = value; }
            public string UserName { get => userName; set => userName = value; }
        }
    }
}