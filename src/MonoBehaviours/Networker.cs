using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace CloverPit_MultiplayerMod.MonoBehaviours;

public class Networker : MonoBehaviour
{
    public static Networker instance;

    private NetManager client, server;
    private EventBasedNetListener clientListener, serverListener;

    private int max_players = 2;
    private string server_password = "";

    private void Awake()
    {
        if (instance)
        {
            Destroy(this);
            return;
        }

        instance = this;

        clientListener = new();
        serverListener = new();

        client = new(clientListener);
        server = new(serverListener);
    }

    public void Host(int port = 7777)
    {
        server.Start(port);

        Plugin.Logger.LogDebug("Hosting server on port " + port.ToString());

        serverListener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < max_players)
                request.AcceptIfKey(server_password);
            else
                request.Reject();
        };
        
        serverListener.PeerConnectedEvent += peer =>
        {
            Plugin.Logger.LogDebug("We got connection: " + peer);  // Show peer IP

            NetDataWriter writer = new();

            writer.Put("Hello client!");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        Join("127.0.0.1", port);
    }

    public void Join(string ip, int port, string password = "")
    {
        client.Start();
        client.Connect(ip, port, password);

        clientListener.NetworkReceiveEvent += (fromPeer, dataReader, delivetyMethod, channel) =>
        {
            Plugin.Logger.LogDebug("Recieved packet from peer Id: " + fromPeer.Id.ToString());
            Plugin.Logger.LogDebug("Packet info: " + dataReader.GetString(100));

            dataReader.Recycle();
        };
    }
    
    public void Leave()
    {
        client.Stop();
        server.Stop();
    }
}