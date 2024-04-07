using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;
    [SerializeField] [Range(0, 4)] private int _numOfPlayers; 

    [SerializeField] private Material[] _playerColours;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private TilemapManager _tilemapManager;

    private void Start()
    {
        for (int i = 0; i < _numOfPlayers; i++)
        {
            CreatePlayer(i);
        }
        
        CreateAIPlayer();
    }

    private void CreatePlayer(int i)
    {
        string control = "Player" + i;
        var player = PlayerInput.Instantiate(_playerPrefab, pairWithDevice: Keyboard.current);
        player.user.ActivateControlScheme(control);
        var playerController = player.GetComponent<BasePlayerController>();
        playerController.SetGround(_tilemapManager);
        playerController.SetColour(_playerColours[i]);
        if (i==0) _cameraController.SetTarget(player.transform);
    }

    private void CreateAIPlayer()
    {
        var aiPlayer = Instantiate(_aiPlayer, transform.position, Quaternion.identity);
        var aiController = aiPlayer.GetComponent<AIController>();
        aiController.SetGround(_tilemapManager);
        aiController.SetColour(_playerColours[3]);
    }
}
