using System.Net.Sockets;
using System.Net;

namespace KUKA.RSI {
    public static class ExpSensor {
        public static void ReInitPort(this UdpClient? client, int port) {
            bool isnew = false;
            IPEndPoint? localEndPoint = client?.Client.LocalEndPoint as IPEndPoint;
            if (localEndPoint != null && localEndPoint.Port == port)
                return;
            if (client != null && (localEndPoint != null || client.Client.Connected)) {
                client.Client.Close();
                //client.Client = new Socket(SocketType.Dgram , ProtocolType.Udp);
                isnew = true;
                client = null;
            }
            if (isnew || client == null || client.Client == null) {
                client = new UdpClient(port);
                return;
            }
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
        }

        public static void ReInitPort(this Socket clientS, int port) {
            IPEndPoint? localEndPoint = clientS.LocalEndPoint as IPEndPoint;
            if (localEndPoint != null && localEndPoint.Port == port)
                return;
            if (localEndPoint != null || clientS.Connected) {
                clientS.Close();
                clientS = new Socket(SocketType.Dgram, ProtocolType.Udp);
            }
            clientS.Bind(new IPEndPoint(IPAddress.Any, port));
        }
    }
}
