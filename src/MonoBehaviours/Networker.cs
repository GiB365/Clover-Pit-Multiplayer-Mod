using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine.DedicatedServer;
using HarmonyLib;
using System.Linq;
using System;

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

    public void Host(string[] args)
    {
        if (args == null)
        {
            args = ["7777"];
        }

        if (args.Length > 1)
        {
            ConsolePrompt.Log("Usage: host [port]");
            return;
        }

        int port = int.Parse(args[0]);

        server.Start(port);

        ConsolePrompt.Log($"Hosting server on port {port}");

        serverListener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < max_players)
                request.AcceptIfKey(server_password);
            else
                request.Reject();
        };
        
        serverListener.PeerConnectedEvent += peer =>
        {
            ConsolePrompt.Log($"We got connection: {peer}");

            NetDataWriter writer = new();

            writer.Put("Hello client!");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        Join(["127.0.0.1", port.ToString()]);
    }

    public void Join(string[] args)
    {
        string ip, password;
        int port;

        if (args.Length == 0)
        {
            args = ["127.0.0.1", "7777", ""];
        }
        else if (args.Length < 2)
        {
            args = [args[0], "7777", ""];
        }
        else if (args.Length < 3)
        {
            args = [args[0], args[1], ""];
        }
        else
        {
            ConsolePrompt.Log("Usage: join [ip] [port] [password]");
            return;
        }

        ip = args[0];
        port = int.Parse(args[1]);
        password = args[2];

        ConsolePrompt.Log($"Trying to connect to {ip}:{port} with password: {password}");
        
        client.Start();
        client.Connect(ip, port, password);

        clientListener.NetworkReceiveEvent += (fromPeer, dataReader, delivetyMethod, channel) =>
        {
            ConsolePrompt.Log($"Recieved packet from peer Id: {fromPeer.Id}");
            ConsolePrompt.Log($"Packet info: {dataReader.GetString(100)}");

            dataReader.Recycle();
        };
    }

    public void Leave(string[] args)
    {
        if (args.Length != 0)
        {
            ConsolePrompt.Log("Usage: leave");
            return;
        }

        client.Stop();
        server.Stop();
    }
    
    private void Update()
    {
        client?.PollEvents();
        server?.PollEvents();
    }
}