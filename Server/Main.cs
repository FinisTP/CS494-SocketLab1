using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerSide {

    public class SocketListener {
        public static string HOST_NAME = "localhost";
        public static int PORT = 6999;
        public static void Main(String[] args)
        {
            StartServer2();
            System.Windows.Forms.Application.Run();
        }

        public static void StartServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(HOST_NAME);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, PORT);

            try {
                Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Associate the socket with the server endpoint
                listener.Bind(localEndPoint);

                // Listen to 10 requests at a time
                listener.Listen(10);

                while (true) {
                    Console.WriteLine("Waiting connection ... ");
                    Socket clientSocket = listener.Accept();

                    byte[] bytes = null;
                    string data = null;

                    while (true) {
                        bytes = new Byte[1024];
                        int receivedPacketLength = clientSocket.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, receivedPacketLength); 
                        if (data.IndexOf("<EOF>") > -1) break;
                    }

                    Console.WriteLine("Received ", data, " from ", clientSocket.AddressFamily.ToString());
                    byte[] message = Encoding.ASCII.GetBytes("Server confirmation of message " + data);

                    clientSocket.Send(message);
                    
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
            }
            
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public static void StartServer2() {
            Server.Start(50, 26950);
        }
    }
}
