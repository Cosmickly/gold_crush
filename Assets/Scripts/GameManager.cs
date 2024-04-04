using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int numOfPlayers;

    [SerializeField] private Material[] playerColours;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private TilemapManager tilemapManager;

    private void Start()
    {
        for (int i = 0; i < numOfPlayers; i++)
        {
            CreatePlayer(i);
        }
    }

    private void CreatePlayer(int i)
    {
        string control = "Player" + i;
        var player = PlayerInput.Instantiate(playerPrefab, pairWithDevice: Keyboard.current);
        player.user.ActivateControlScheme(control);
        var playerController = player.GetComponent<PlayerController>();
        playerController.SetGround(tilemapManager);
        playerController.SetColour(playerColours[i]);
        if (i==0) cameraController.SetTarget(player.transform);
    }
}
