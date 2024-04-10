using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;
    [SerializeField] [Range(0, 4)] private int _numOfPlayers; 
    [SerializeField] [Range(0, 4)] private int _totalPlayers; 
    
    private Dictionary<int, BasePlayerController> _players = new ();

    [SerializeField] private Material[] _playerColours;
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private TilemapManager _tilemapManager;

    private void Start()
    {
        for (int i = 0; i < _totalPlayers; i++)
        {
            _players.Add(i, i < _numOfPlayers ? CreatePlayer(i) : CreateAIPlayer(i));
        }
    }

    private BasePlayerController CreatePlayer(int i)
    {
        var player = PlayerInput.Instantiate(_playerPrefab, pairWithDevice: Keyboard.current);
        player.user.ActivateControlScheme("Player" + i);

        var playerController = PlayerSetup(player.gameObject, i);
        if (i==0) _cameraController.SetTarget(player.transform);

        return playerController;
    }

    private BasePlayerController CreateAIPlayer(int i)
    {
        var aiPlayer = Instantiate(_aiPlayer, transform.position, Quaternion.identity);
        return PlayerSetup(aiPlayer, i);
    }

    private BasePlayerController PlayerSetup(GameObject player, int id)
    {
        var controller = player.GetComponent<BasePlayerController>();
        controller.SetGround(_tilemapManager);
        controller.SetMaterial(_playerColours[id]);
        controller.transform.position = _spawnPoints[id].position;

        return controller;
    }
}
