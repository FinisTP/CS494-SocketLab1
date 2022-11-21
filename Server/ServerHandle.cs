using System;
using System.Collections;
using System.Collections.Generic;

namespace ServerSide {
    public class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            Console.WriteLine("{0} connected successfully" +
                " and is now player {1}: {2}", Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint, _fromClient, _username);
            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine("Player \"{0}\" (ID: {1}) has assumed the wrong client ID ({2})!", _username, _fromClient, _clientIdCheck);
            }
        }

        public static void UDPTestReceived(int _fromClient, Packet _packet)
        {
            string _msg = _packet.ReadString();
            Console.WriteLine("Received packet via UDP. Contains message: {0}", _msg);
        }
    }
}
