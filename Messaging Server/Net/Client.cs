using Messaging_Server.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Messaging_Server.Net
{
    internal class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;
        internal Client(TcpClient client)
        {
            ClientSocket =  client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            var opcode = _packetReader.ReadByte();

            if (opcode != 0)
                ClientSocket.Close();

                Username = _packetReader.ReadMessage();

                //Waiting a new message from the client
                Task.Run(() => NewMessage());

        }

        void NewMessage()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();

                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.ReadMessage();
                            ServerManagement.BroadcastMessage($"[{DateTime.Now}]: [{Username}]: {msg}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    System.Diagnostics.Debug.WriteLine($"[{UID}]: Client disconnected");
                    ServerManagement.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }

            }
        }
    }
}
