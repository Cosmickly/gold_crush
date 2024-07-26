using System;
using System.Collections.Generic;
using System.Linq;
using Players;
using Tiles;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private GameObject _aiPlayer;

    [SerializeField] private GameObject[] _playerModels;
    [SerializeField] private Material[] _playerColours;
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Parameters")]
    [SerializeField] public int NumOfHumans;
    [SerializeField] public int NumOfAIs;
    public int MaxLevel = 1;

    [SerializeField] private CameraController _cameraController;
    [SerializeField] private TilemapManager _tilemapManager;

    [Header("UI")] [SerializeField] private Scoreboard _scoreboard;

    [SerializeField] private FinalScreen _finalScreen;
    [SerializeField] private GameObject _finalScreenFirstSelected;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private GameObject _pauseScreen;
    [SerializeField] private GameObject _pauseFirstSelected;

    [Header("Random")]
    [SerializeField] public int RandomSeed;

    private readonly Dictionary<int, BasePlayer> _players = new();

    private AudioSource _audioSource;

    private IDisposable _menuSubscription;

    public int CurrentLevel { get; private set; } = 1;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        MaxLevel = PlayerPrefs.GetInt("LevelCount", 1);
        NumOfHumans = PlayerPrefs.GetInt("HumanCount", 1);
        NumOfAIs = PlayerPrefs.GetInt("AIcount", 0);

        for (var i = 0; i < NumOfHumans; i++) _players.Add(i, CreateHumanPlayer(i));

        for (int j = NumOfHumans; j < NumOfHumans + NumOfAIs; j++) _players.Add(j, CreateAIPlayer(j));

        _scoreboard.Players = _players.Values.ToList();
        _levelText.text = "Level " + CurrentLevel;

        if (RandomSeed != 0) Random.InitState(RandomSeed);

        ResetPlayers();
    }

    private void Start()
    {
        _finalScreen.gameObject.SetActive(false);
        _pauseScreen.SetActive(false);
        _audioSource.Play();
    }

    private void OnEnable()
    {
        _menuSubscription = InputSystem.onAnyButtonPress.Call(_ =>
        {
            if (EventSystem.current.currentSelectedGameObject == null) SelectFirstMenuItem();
        });
    }

    private void OnDisable()
    {
        _menuSubscription?.Dispose();
    }

    private void SelectFirstMenuItem()
    {
        if (_pauseScreen && _pauseScreen.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(_pauseFirstSelected);
        if (_finalScreen && _finalScreen.isActiveAndEnabled)
            EventSystem.current.SetSelectedGameObject(_finalScreenFirstSelected);
    }

    public void ReloadGameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    private BasePlayer CreateHumanPlayer(int i)
    {
        var player = PlayerInput.Instantiate(_playerPrefab, pairWithDevice: Keyboard.current);
        player.user.ActivateControlScheme("Player" + i);

        var playerController = PlayerSetup(player.gameObject, i);
        if (i == 0) _cameraController.SetTarget(player.transform);

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
        player.PlayerId = id;
        player.TilemapManager = _tilemapManager;
        var model = Instantiate(_playerModels[id], playerObject.transform.position, Quaternion.identity,
            player.ModelHolder);
        model.transform.localScale = new Vector3(2f, 2f, 2f);
        model.transform.localPosition = new Vector3(0, -1, 0);
        return player;
    }

    public void PlayerFell(BasePlayer player)
    {
        player.TogglePlayerEnabled(false);
        _tilemapManager.PlayerFell(player);

        bool allFell = _players.Values.All(p => p.Fell);

        if (allFell) NextLevel();
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
            var basePlayer = player.Value;
            var spawn = _spawnPoints[player.Key].position;
            basePlayer.transform.position = spawn;
            basePlayer.CurrentCell = _tilemapManager.GetCell(new Vector3(spawn.x, 0, spawn.z));
            basePlayer.Fell = false;
            basePlayer.TogglePlayerEnabled(true);
        }
    }

    private void GameOver()
    {
        // Debug.Log("Game Over!");
        _tilemapManager.DisableTilemapManager();
        _scoreboard.gameObject.SetActive(false);

        _finalScreen.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_finalScreenFirstSelected);
        _finalScreen.SetText(_players.Values.ToArray());
    }

    public void SetActivePauseScreen(bool toggle)
    {
        if (!_pauseScreen) return;
        _pauseScreen.SetActive(toggle);
        if (toggle) EventSystem.current.SetSelectedGameObject(_pauseFirstSelected);
    }
}