using myChatRoom;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace myChatroom
{
    public delegate string ServerCommandProc(clientInfo user,string args);

    class ServerCommands
    {
        private static Dictionary<string, ServerCommandProc> cmdDict = new Dictionary<string, ServerCommandProc>();
        public static void LoadCmdDict()
        {
            cmdDict.Add("SETNAME", SetNameProc);
        }
        private static string SetNameProc(clientInfo user,string args)
        {
            if (args.Trim().Length == 0)
            {
                user.UserName = ((IPEndPoint)user.Client.Client.RemoteEndPoint).Address.ToString() + ":"
                + ((IPEndPoint)user.Client.Client.RemoteEndPoint).Port.ToString();
            }
            else
            {
                user.UserName = args;
            }
            return "NEWNAME:" + user.UserName;
        }
        public static bool CheckServerCommands(clientInfo user,string message,out string reply)
        {
            reply = String.Empty;
            string[] commands = message.Split(':');
            if (commands.Length < 2)
            {
                return false;
            }
            else
            {
                ServerCommandProc proc;
                cmdDict.TryGetValue(commands[0],out proc);
                if (proc != null)
                {
                    reply=proc.Invoke(user, commands[1]);
                    return true;
                }
                return false;
            }
        }
    }
}
