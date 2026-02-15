using System;
using System.Collections.Generic;
using GamePlay.Playable;
using Unity.Netcode;
using UnityEngine;
using Unity.Multiplayer.Playmode;

public class Game : MonoBehaviour
{
    [SerializeField]
    private NetworkObject _playerPrefab;

    [SerializeField]
    private Transform[] _spawnPoints;

    private Dictionary<ulong, NetworkObject> _players = new Dictionary<ulong, NetworkObject>();

    private void Start()
    {
#if UNITY_EDITOR
        if (CurrentPlayer.IsMainEditor)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            return;
        }
#endif

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        SpawnPlayer(0);
    }

    private void OnClientConnect(ulong clientId)
    {
        Debug.Log("Client Connected: " + clientId);
        SpawnPlayer(clientId);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        _players.Remove(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        var spawned = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(_playerPrefab, clientId,
            position: GetSpawnPoint(clientId), rotation: Quaternion.identity);
        _players[clientId] = spawned;
    }

    private Vector3 GetSpawnPoint(ulong clientId)
    {
        int index = Convert.ToInt32(clientId);
        return _spawnPoints[index].position;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        int currentConnectedClients = NetworkManager.Singleton.ConnectedClientsIds.Count;
        if (currentConnectedClients < _spawnPoints.Length)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null;
        }
        else
        {
            response.Approved = false;
            response.Reason = "Server is full!";
            Debug.Log("Denied a connection because the server is at capacity.");
        }

        response.Pending = false;
    }
}