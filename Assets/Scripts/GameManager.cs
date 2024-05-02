using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Players;
using Tiles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;
    [SerializeField] [Range(0, 4)] private int _numOfHumans; 
    [SerializeField] [Range(0, 4)] private int _totalPlayers; 
    
    private Dictionary<int, BasePlayer> _players = new ();

    [SerializeField] private Material[] _playerColours;
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private TilemapManager _tilemapManager;
    [SerializeField] private Scoreboard _scoreboard;

    private void Awake()
    {
        for (int i = 0; i < _totalPlayers; i++)
        {
            _players.Add(i, i < _numOfHumans ? CreatePlayer(i) : CreateAIPlayer(i));
        }

        _scoreboard.Players = _players.Values.ToList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private BasePlayer CreatePlayer(int i)
    {
        var player = PlayerInput.Instantiate(_playerPrefab, pairWithDevice: Keyboard.current);
        player.user.ActivateControlScheme("Player" + i);

        var playerController = PlayerSetup(player.gameObject, i);
        if (i==0) _cameraController.SetTarget(player.transform);

        return playerController;
    }

    private BasePlayer CreateAIPlayer(int i)
    {
        var aiPlayer = Instantiate(_aiPlayer, transform.position, Quaternion.identity);
        return PlayerSetup(aiPlayer, i);
    }

    private BasePlayer PlayerSetup(GameObject playerObject, int id)
    {
        var player = playerObject.GetComponent<BasePlayer>();
        player.ID = id;
        player.TilemapManager = _tilemapManager;
        player.SetMaterial(_playerColours[id]);
        player.transform.position = _spawnPoints[id].position;
        
        return player;
    }
}
