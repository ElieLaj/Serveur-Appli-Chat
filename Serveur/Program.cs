using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Serveur démarré...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Nouveau client connecté!");

            clients.Add(client);

            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

        try
        {
            while (true)
            {
                string message = reader.ReadLine();
                if (message == null) break;

                Console.WriteLine("Message reçu: " + message);

                BroadcastMessage(message, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur: " + ex.Message);
        }
        finally
        {
            // Ferme et supprime le client de la liste
            clients.Remove(client);
            client.Close();
        }
    }

    static void BroadcastMessage(string message, TcpClient senderClient)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message + Environment.NewLine);

        foreach (TcpClient client in clients)
        {
            if (client != senderClient)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}