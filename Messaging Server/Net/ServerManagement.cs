using Messaging_Server.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging_Server.Net
{
    internal static class ServerManagement
    {

        public static event Action ConnectedEvent;
        public static event Action DisconnectedEvent;
        public static event Action ClientJoinEvent;
        public static event Action ClientLeftEvent;
        public static event Action NewMessageEvent;

        public static string IpAddress => "127.0.0.1";
        public static int Port => 7891;

        static List<Client> _users;
        static TcpListener _listener;
        static CancellationTokenSource ts;
        internal static async void StartServer()
        {
            ts = new();
            CancellationToken cancellationToken = ts.Token;

            try
            {
                _users = new List<Client>();
                _listener = new TcpListener(IPAddress.Parse(IpAddress), Port);
                _listener.Start();

                ConnectedEvent?.Invoke();

                await Task.Run(() =>
                {
                    while (true)
                    {
                        var client = new Client(_listener.AcceptTcpClient());
                        _users.Add(client);
                        //New user connected
                        BroadcastNewConnection();

                        System.Diagnostics.Debug.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {client.Username}");

                        BroadcastMessage($"[{client.Username}] has joined");

                        ClientJoinEvent?.Invoke();
                    }

                }, cancellationToken);

            }
            catch (SocketException ex) when (cancellationToken.IsCancellationRequested && 
                                                ex.SocketErrorCode == SocketError.Interrupted)
            {
                //Ignore ex because was caused after user stops TCPClient
            }
        }

        internal static void StopServer()
        {
            if (_listener != null)
            {
                _users.ToList().ForEach(c => BroadcastDisconnect(c.UID.ToString()));
                ts.Cancel();
                _listener.Stop();
                _users.Clear();
                DisconnectedEvent?.Invoke();
            }
        }

        static void BroadcastNewConnection()
        {
            //send to the user
            foreach (var user in _users)
            {
                //gather users online
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }


        internal static void BroadcastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
            NewMessageEvent?.Invoke();
        }
        internal static void BroadcastDisconnect(string uid)
        {

            var disconnectedUser = _users.FirstOrDefault(x => x.UID.ToString() == uid);
            _users.Remove(disconnectedUser);

            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }


            BroadcastMessage($"[{disconnectedUser.Username}] has left the chat");

            ClientLeftEvent?.Invoke();
        }
    }
}
