using Messaging_Client.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Messaging_Client.Net
{
    internal class Server
    {
        TcpClient _client;
        public int Port => 7891;
        public string IpAddress => "127.0.0.1";
        public bool IsConnected => _client.Connected;
        public PacketReader PacketReader { get; set; }

        public event Action ConnectedEvent;
        public event Action DisconnectedEvent;
        public event Action NewMessageEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!IsConnected)
            {
                try
                {
                    _client.Connect(IpAddress, Port);
                    PacketReader = new PacketReader(_client.GetStream());

                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var connectPacket = new PacketBuilder();
                        connectPacket.WriteOpCode(0);
                        connectPacket.WriteMessage(username);
                        _client.Client.Send(connectPacket.GetPacketBytes());

                    }

                    ReadPackets();
                }
                catch (Exception ex)
                {
                    //Catch if server is offline
                    System.Diagnostics.Debug.WriteLine(ex);
                }


            }
        }
        public void SendMessageToServer(string message)
        {
            if (IsConnected)
            {
                var messagePacket = new PacketBuilder();
                messagePacket.WriteOpCode(5);
                messagePacket.WriteMessage(message);
                _client.Client.Send(messagePacket.GetPacketBytes());
            }

        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1: //when new user enter to the chat room
                            ConnectedEvent?.Invoke();
                            break;
                        case 5: //when a user send a message
                            NewMessageEvent?.Invoke();
                            break;
                        case 10: //when a user disconnect
                            DisconnectedEvent?.Invoke();
                            break;
                        default:
                            break;
                    }
                }

            });
        }

    }
}


