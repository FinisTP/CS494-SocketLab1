using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;

namespace ClientSide {
    public class ClientHandle
    {
        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _myId = _packet.ReadInt();

            Console.WriteLine("Message from server: {0}", _msg);
            Client.instance.myId = _myId;

            ClientSend.WelcomeReceived();
            Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void UDPTest(Packet _packet)
        {
            string _msg = _packet.ReadString();
            Console.WriteLine("Received packet via UDP. Contains message: {0}", _msg);
            ClientSend.UDPTestReceived();
        }

        public static void PlayerDisconnect(Packet _packet)
        {
            int _id = _packet.ReadInt();
            Console.WriteLine("Player ID {0} has disconnected!", _id);
        }
    }
}
