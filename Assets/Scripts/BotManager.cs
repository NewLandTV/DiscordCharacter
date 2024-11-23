using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [SerializeField]
    private Character character;
    private Socket client;
    private Queue<string> receiveBuffer = new Queue<string>();

    private void Awake()
    {
        // Setup client and connect.
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        client.Connect("127.0.0.1", 1677);

        // Start receive thread.
        ThreadPool.QueueUserWorkItem((_) =>
        {
            while (true)
            {
                byte[] buffer = new byte[1024];

                client.Receive(buffer, 0);

                receiveBuffer.Enqueue(Encoding.UTF8.GetString(buffer));

                Thread.Sleep(1);
            }
        });
    }

    private void Update()
    {
        if (receiveBuffer.Count > 0)
        {
            switch (ushort.Parse(receiveBuffer.Dequeue()))
            {
                // Move left, right, up, down
                case 0:
                    character.MoveTo(Vector3.left);

                    break;
                case 1:
                    character.MoveTo(Vector3.right);

                    break;
                case 2:
                    character.MoveTo(Vector3.up);

                    break;
                case 3:
                    character.MoveTo(Vector3.down);

                    break;
            }
        }
    }
}
