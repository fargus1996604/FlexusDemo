using Unity.Netcode;
using UnityEngine;

namespace Test
{
    public class TestRpc : NetworkBehaviour
    {
        private void Start()
        {
//        var input = InputController.Instance.GetPlayerInputHandler();
            /*input.InteractPressed.AddListener(() =>
        {
            Debug.Log("InteractPressed");
            if (IsClient)
            {
                PingRpc(5);
            }
        });
        input.Enable();*/
        }

        [Rpc(SendTo.Server)]
        public void PingRpc(int pingCount)
        {
            // Server -> Clients because PongRpc sends to NotServer
            // Note: This will send to all clients.
            // Sending to the specific client that requested the pong will be discussed in the next section.
            Debug.Log($"Ping RPC: {pingCount}");
            PongRpc(pingCount, "PONG!");
        }

        [Rpc(SendTo.NotServer)]
        void PongRpc(int pingCount, string message)
        {
            Debug.Log($"Received pong from server for ping {pingCount} and message {message}");
        }
    }
}