using System;
using System.Net.Sockets;
using System.Threading;
using myChatRoom.DESOperation;

namespace myChatRoom
{
    public class chatClient
    {
        //属性
        public enum clientStatus { UN_CONNECTED, CONNECTED, ERROR, UN_INITIALIZED };
        const int MAX_BUF_SIZE = 1024;
        clientStatus status = clientStatus.UN_INITIALIZED;
        TcpClient client;
        NetworkStream stream;
        byte[] key;
        //方法
        chatClient()
        {
            status = clientStatus.UN_INITIALIZED;
        }
        ~chatClient()
        {
            if (status != clientStatus.UN_INITIALIZED)
            {
                stream.Close();
                client.Close();
            }
        }
        bool connectServer()
        {
            System.Console.WriteLine("Please input server's address [and port].");
            string address = System.Console.ReadLine();
            int port = 4321;
            if (address.Contains(":"))
            {
                port = int.Parse(address.Substring(address.IndexOf(':') + 1));
                address = address.Substring(0, address.IndexOf(':') - 1);
            }
            client = new TcpClient(address, port);
            // client.Connect(address,port);
            if (client.Connected)
            {
                stream = client.GetStream();
                status = clientStatus.CONNECTED;
                // Thread t=new Thread(new ThreadStart(this.receiveEvent));
                // t.Join();
                receiveEvent();
                return true;
            }
            status = clientStatus.UN_CONNECTED;
            return false;
        }
        async void receiveEvent()
        {
            byte[] buf = new byte[MAX_BUF_SIZE];
            //接收密钥
            while (true)
            {
                if (stream.CanRead)
                {
                    int count = stream.Read(buf, 0, MAX_BUF_SIZE);
                    string recv = System.Text.Encoding.ASCII.GetString(buf);
                    //密钥以明文形式发送
                    if (recv.Contains("KEY:"))
                    {
                        System.Console.WriteLine(recv);
                        key = Convert.FromBase64String(recv.Substring(4, 12));
                        // System.Console.WriteLine("Key: {0}",Convert.ToBase64String(key));
                        break;
                    }
                    else
                    {
                        stream.Write(System.Text.Encoding.ASCII.GetBytes("SEND KEY"), 0, System.Text.Encoding.ASCII.GetByteCount("SEND KEY"));
                    }
                }
            }
            //接收消息循环
            //在服务器端关闭时会报错退出
            while (true)
            {
                string message = string.Empty;
                // byte[] buf=new byte[MAX_BUF_SIZE];
                if (status != clientStatus.CONNECTED)
                {
                    return;
                }
                while (true)
                {
                    int count = await stream.ReadAsync(buf, 0, MAX_BUF_SIZE);
                    message += System.Text.Encoding.ASCII.GetString(buf, 0, count);
                    if (count < MAX_BUF_SIZE)
                    {
                        break;
                    }
                }
                // System.Console.WriteLine("Phys Recv: {0}",message);
                for (int i = 0; i < message.Length; i += 24)
                {
                    string part = message.Substring(i, message.Length - i >= 24 ? 24 : message.Length - i);
                    DESCrypt crypt = new DESCrypt();
                    part = crypt.Decrypt(part, key);
                    System.Console.Write(part);
                }
                System.Console.WriteLine();
                message.Remove(0);
            }
        }
        async void sendMessage(string message)
        {
            byte[] buf;
            if (status != clientStatus.CONNECTED)
            {
                return;
            }
            string cipher = string.Empty;
            for (int i = 0; i < message.Length; i += 8)
            {
                DESCrypt crypt = new DESCrypt();
                cipher += crypt.Encrypt(message.Substring(i, message.Length - i >= 8 ? 8 : message.Length - i), key);
            }
            buf = System.Text.Encoding.ASCII.GetBytes(cipher);
            if (stream.CanWrite)
            {
                await stream.WriteAsync(buf, 0, buf.Length);
                // System.Console.WriteLine("Phy Send: {0}",cipher);
            }
        }
        public static void Main()
        {
            chatClient client = new chatClient();
            if (!client.connectServer())
            {
                System.Console.WriteLine("Cannot connect to server,please check.");
                return;
            }
            else
            {
                System.Console.WriteLine("Connected to server.");
                string input = string.Empty;
                while (true)
                {
                    input = System.Console.ReadLine();
                    client.sendMessage(input);
                }
            }
        }
    }
}
