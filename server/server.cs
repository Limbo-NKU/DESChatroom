using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using myChatroom;

namespace myChatRoom
{
    public partial class chatServer
    {

        public enum serverStatus { UN_INITIALIZED, LISTENING, OFFLINE, ERROR };
        int MAX_BUF_SIZE = 1024;
        TcpListener listener;
        List<clientInfo> clientList = new List<clientInfo> { };
        serverStatus status;
        int port = 4321;
        //IPAddress localAddr=IPAddress.Parse("0.0.0.0");
        public chatServer()
        {
            status = serverStatus.UN_INITIALIZED;
            listener = new TcpListener(IPAddress.Any, 4321);
        }
        ~chatServer()
        {
            if (status == serverStatus.LISTENING)
            {
                listener.Stop();
            }
        }
        public async void startListen()
        {
            listener.Start();
            status = serverStatus.LISTENING;
            System.Console.WriteLine("Loading server commands...");
            ServerCommands.LoadCmdDict();
            System.Console.WriteLine("Server started at {0}", port);
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                clientInfo info = new clientInfo(client);
                System.Console.WriteLine("New user {0} connected.",
                    info.UserName);
                System.Console.WriteLine("Key: {0}", Convert.ToBase64String(info.Key));
                clientList.Add(info);
                this.clientEvent(info);
            }
        }
        async void clientEvent(clientInfo info)
        {
            byte[] buf = new byte[1024];
            TcpClient client = info.Client;
            NetworkStream stream = client.GetStream();
            //发送密钥
            if (stream.CanWrite)
            {
                stream.Write(info.Key, 0, info.Key.Length);
            }
            while (true)
            {
                if (status != serverStatus.LISTENING)
                {
                    return;
                }
                else if (!client.Connected)
                {
                    // clientList.Remove(client);
                    stream.Close();
                    this.removeClient(info);
                    return;
                }
                string recv = string.Empty;
                try
                {
                    while (true)
                    {
                        if (!client.Connected)
                        {
                            stream.Close();
                            this.removeClient(info);
                            // clientList.Remove(client);
                            return;
                        }
                        if (stream.CanRead)
                        {
                            int count = await stream.ReadAsync(buf, 0, MAX_BUF_SIZE);
                            recv += System.Text.Encoding.ASCII.GetString(buf, 0, count);
                            if (count == 0)
                            {
                                stream.Close();
                                // clientList.Remove(client);
                                this.removeClient(info);
                                return;
                            }
                            else if (count < MAX_BUF_SIZE)
                            {
                                break;
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("Uncatched stream closed");
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e);
                    stream.Close();
                    client.Close();
                    return;
                }
                if (recv.Length <= 0)
                {
                    return;
                }
                //处理客户端命令
                if (recv == "SEND KEY")
                {
                    if (stream.CanWrite)
                    {
                        string sendKey = "KEY:" + Convert.ToBase64String(info.Key);
                        await stream.WriteAsync(System.Text.Encoding.ASCII.GetBytes(sendKey), 0, System.Text.Encoding.ASCII.GetByteCount(sendKey));
                    }
                    continue;
                }
                //非客户端命令，解密消息
                string message = string.Empty;
                for (int i = 0; i < recv.Length; i += 24)
                {
                    string part = recv.Substring(i, recv.Length - i >= 24 ? 24 : recv.Length - i);
                    DESCrypt crypt = new DESCrypt();
                    message += crypt.Decrypt(part, info.Key);
                    // System.Console.Write(part);
                }
                System.Console.WriteLine("New message from:{0}", info.UserName);
                System.Console.WriteLine("\tContent:{0}", message);
                string reply=String.Empty;
                if (!ServerCommands.CheckServerCommands(info, message,out reply))
                {
                    string messageToClient = string.Format("{0}:{1}", info.UserName, message);
                    foreach (var i in clientList)
                    {
                        if (i != info && i.Client.Connected)
                        {
                            this.sendMessageToClient(messageToClient, i);
                        }
                    }
                }
                else
                {
                    this.sendMessageToClient(reply, info);
                }
            }
        }
        async void sendMessageToClient(string message, clientInfo info)
        {
            byte[] buf;
            TcpClient client = info.Client;
            if (!client.Connected)
            {
                return;
            }
            NetworkStream stream = client.GetStream();
            string cipher = string.Empty;
            for (int i = 0; i < message.Length; i += 8)
            {
                DESCrypt crypt = new DESCrypt();
                cipher += crypt.Encrypt(message.Substring(i, message.Length - i >= 8 ? 8 : message.Length - i), info.Key);
            }
            buf = System.Text.Encoding.ASCII.GetBytes(cipher);
            if (stream.CanWrite)
            {
                await stream.WriteAsync(buf, 0, buf.Length);
            }
            // stream.Close();
        }
        void removeClient(clientInfo info)
        {
            System.Console.WriteLine("User {0} quited.", info.UserName);
            info.Client.Close();
            clientList.Remove(info);
        }
        public static void Main()
        {
            chatServer server = new chatServer();
            server.startListen();
            while (true)
            {
                string message=System.Console.ReadLine();
                string messageToClient = string.Format("Server:{0}",  message);
                foreach (var i in server.clientList)
                {
                        server.sendMessageToClient(messageToClient, i);
                }
            }
        }
    }
}