using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientSide {
    public class SocketClient {
        public static string HOST_NAME = "localhost";
        public static int PORT = 6999;
        public static void Main(string[] args) {
            StartClient2();
            System.Windows.Forms.Application.Run();
        }

        public static void StartClient() {
            try {
                IPHostEntry ipHost = Dns.GetHostEntry(HOST_NAME);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint remoteEndpoint = new IPEndPoint(ipAddr, 6999);

                Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try {
                    sender.Connect(remoteEndpoint);
                    Console.WriteLine("Socket connected to -> {0}", sender.RemoteEndPoint.ToString());
                    byte[] messageSent = Encoding.ASCII.GetBytes("Sending out a test message <EOF>");
                    int byteSent = sender.Send(messageSent);

                    byte[] messageReceived = new byte[1024];
                    int byteRcv = sender.Receive(messageReceived);
                    Console.WriteLine("Message from server -> {0}", Encoding.ASCII.GetString(messageReceived, 0, byteRcv));

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                } catch(ArgumentNullException exception) {
                    Console.WriteLine("Null argument " + exception.ToString());
                } catch(SocketException exception) {
                    Console.WriteLine("Socket exception " + exception.ToString());
                } catch(Exception exception) {
                    Console.WriteLine("Exception " + exception.ToString());
                }
            } catch (Exception exception) {
                Console.WriteLine(exception.ToString());
            }
        }

        public static void StartClient2() {
            Client.instance.ConnectToServer();
        }
    }
}