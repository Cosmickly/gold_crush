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
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;

    [SerializeField] private GameObject[] _playerModels;

    [SerializeField] private Material[] _playerColours;
    [SerializeField] private Transform[] _spawnPoints;
    public Dictionary<int, BasePlayer> _players = new ();

    [Header("Parameters")]
    private readonly int _maxPlayers = 4;

    [SerializeField] public int NumOfHumans;
    // {
    //     get => NumOfHumans;
    //     private set => NumOfHumans = Mathf.Clamp(value, 0, _maxPlayers); 
    // }

    [SerializeField] public int NumOfAis;
    // {
    //     get => NumOfAIs;
    //     private set => NumOfAIs = Mathf.Clamp(value, 0, _maxPlayers - NumOfHumans); 
    // }


    public int MaxLevel = 1;
    public int CurrentLevel { get; private set; } = 1;

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
        MaxLevel = PlayerPrefs.GetInt("LevelCount", 1);
        NumOfHumans = PlayerPrefs.GetInt("HumanCount", 1);
        NumOfAis = PlayerPrefs.GetInt("AIcount", 0);
        
        for (int i = 0; i < NumOfHumans; i++)
        {
            _players.Add(i, CreateHumanPlayer(i));
        }
        
        for (int j = NumOfHumans; j < NumOfHumans + NumOfAis; j++)
        {
            _players.Add(j, CreateAIPlayer(j));
        }

        _scoreboard.Players = _players.Values.ToList();
        _levelText.text = "Level " + CurrentLevel;

        if (RandomSeed != 0)
        {
            Random.InitState(RandomSeed);
        }
        
        ResetPlayers();
    }

    private void Start()
    {
        _finalScreen.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ReloadGameScene();
    }

    public void ReloadGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(sceneBuildIndex: 0);
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
        var model = Instantiate(_playerModels[id], playerObject.transform.position, Quaternion.identity, player.ModelHolder);
        model.transform.localScale = new Vector3(2f, 2f, 2f);
        model.transform.localPosition = new Vector3(0, -1, 0);
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
        CurrentLevel++;
        if (CurrentLevel > MaxLevel)
        {
            GameOver();
            return;
        }
        _levelText.text = "Level " + CurrentLevel;
        StartCoroutine(_tilemapManager.ResetLevel());
    }

    public void ResetPlayers()
    {
        foreach (var player in _players)
        {
            BasePlayer basePlayer = player.Value;
            var spawn = _spawnPoints[player.Key].position;
            basePlayer.transform.position = spawn;
            basePlayer.CurrentCell = _tilemapManager.GetCell(new Vector3(spawn.x, 0, spawn.z));
            basePlayer.Fell = false;
            basePlayer.TogglePlayerEnabled(true);
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        _tilemapManager.enabled = false;
        _scoreboard.gameObject.SetActive(false);
        _finalScreen.gameObject.SetActive(true);
        _finalScreen.SetText(_players.Values.ToArray());
    }
}
