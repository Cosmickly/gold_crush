using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Players;
using Tiles;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;
    [SerializeField] private Material[] _playerColours;
    [SerializeField] private Transform[] _spawnPoints;
    private Dictionary<int, BasePlayer> _players = new ();
    
    [Header("Parameters")]
    [SerializeField] [Range(0, 4)] private int _numOfHumans; 
    [SerializeField] [Range(0, 4)] private int _totalPlayers; 
    [SerializeField] [Range(1, 100)] private int _maxLevel;
    private int _currentLevel = 1;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private TilemapManager _tilemapManager;
    
    [Header("UI")]
    [SerializeField] private Scoreboard _scoreboard;
    [SerializeField] private FinalScreen _finalScreen;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Random")] 
    [SerializeField] public int RandomSeed;

    private void Awake()
    {
        for (int i = 0; i < _totalPlayers; i++)
        {
            _players.Add(i, i < _numOfHumans ? CreateHumanPlayer(i) : CreateAIPlayer(i));
        }

        _scoreboard.Players = _players.Values.ToList();
        _levelText.text = "Level " + _currentLevel;

        if (RandomSeed != 0)
        {
            Random.InitState(RandomSeed);
        }
    }

    private void Start()
    {
        _finalScreen.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private BasePlayer CreateHumanPlayer(int i)
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

    public void PlayerFell(BasePlayer player)
    {
        player.TogglePlayerEnabled(false);
        _tilemapManager.PlayerFell(player);

        bool allFell = _players.Values.All(p => p.Fell);

        if (allFell)
        {
            NextLevel();
        }
    }

    private void NextLevel()
    {
        _currentLevel++;
        if (_currentLevel > _maxLevel)
        {
            GameOver();
            return;
        }
        _levelText.text = "Level " + _currentLevel;
        StartCoroutine(_tilemapManager.ResetLevel());
    }

    public void ResetPlayers()
    {
        foreach (var player in _players)
        {
            BasePlayer basePlayer = player.Value;
            basePlayer.transform.position = _spawnPoints[player.Key].position;
            basePlayer.Fell = false;
            basePlayer.TogglePlayerEnabled(true);
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        _tilemapManager.enabled = false;
        _finalScreen.gameObject.SetActive(true);
        _finalScreen.SetText(_players.Values.ToArray());
    }
}
