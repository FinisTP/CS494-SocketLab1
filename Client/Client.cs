using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

namespace ClientSide {
    public class Client
    {
        public static int dataBufferSize = 4096;

        private Client() {}
        private static Client _instance;
        private static readonly object _lock = new object();
        public static Client instance {
            get {
                if (_instance == null) lock(_lock) if (_instance == null) _instance = new Client();
                return _instance;
            }
            private set {}
        }

        public string ip = "127.0.0.1";
        public int port = 26950;
        public int myId = 0;
        public TCP tcp;
        public UDP udp;
        private bool _isConnected = false;

        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        public void ConnectToServer()
        {
            InitializeClientData();

            tcp = new TCP();
            udp = new UDP();

            _isConnected = true;
            tcp.Connect();
        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;
            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };

                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult ar)
            {
                socket.EndConnect(ar);
                if (!socket.Connected)
                {
                    return;
                }
                stream = socket.GetStream();

                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }

            public void SendData(Packet _packet)
            {
                
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending data to server: {0}", e);
                }
            }

            private void ReceiveCallback(IAsyncResult ar)
            {
                try
                {
                    int _byteLength = stream.EndRead(ar);
                    if (_byteLength <= 0)
                    {
                        instance.Disconnect();
                        return;
                    }
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    // handle data
                    receivedData.Reset(HandleData(_data));

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                }
                catch (Exception _e)
                {
                    Console.WriteLine("Error receiving TCP data: {0}", _e);
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;
                receivedData.SetBytes(_data);
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packedId = _packet.ReadInt();
                            packetHandlers[_packedId](_packet);
                        }
                    });

                    _packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1) return true;
                return false;
            }

            private void Disconnect()
            {
                instance.Disconnect();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endpoint;

            public UDP()
            {
                endpoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    _packet.InsertInt(instance.myId);
                    if (socket != null)
                    {
                        socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error sending data to server via UDP: {0}", e);
                }
            
            }

            public void Connect(int _localPort)
            {
                socket = new UdpClient(_localPort);
                socket.Connect(endpoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet _packet = new Packet())
                {
                    SendData(_packet);
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    byte[] _data = socket.EndReceive(_result, ref endpoint);
                    socket.BeginReceive(ReceiveCallback, null);
                    if (_data.Length < 4)
                    {
                        instance.Disconnect();
                        return;
                    }

                    HandleData(_data);

                } catch
                {
                    Disconnect();
                }
            }

            private void HandleData(byte[] _data)
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetLength = _packet.ReadInt();
                    _data = _packet.ReadBytes(_packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet2 = new Packet(_data))
                        {
                            int _packetId = _packet2.ReadInt();
                            packetHandlers[_packetId](_packet2);
                        }
                    });
                }
            }

            private void Disconnect()
            {
                instance.Disconnect();
                endpoint = null;
                socket = null;
            }
        }

        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.welcome, ClientHandle.Welcome },
                {(int)ServerPackets.udpTest, ClientHandle.UDPTest },
                {(int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnect }
            };
            Console.WriteLine("Initialized packets");
        }

        private void Disconnect()
        {
            if (_isConnected)
            {
                Console.WriteLine("{0} has disconnected from server.", tcp.socket.Client.RemoteEndPoint);
                _isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
            }
        }

    }    
}
