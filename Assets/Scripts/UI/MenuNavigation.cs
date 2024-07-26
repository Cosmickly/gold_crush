using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace UI
{
    public class MenuNavigation : MonoBehaviour
    {
        [Header("Screens")] [SerializeField] private GameObject _mainMenu;

        [SerializeField] private GameObject _settings;
        [SerializeField] private GameObject _tutorial;
        [SerializeField] private GameObject _controls;
        [SerializeField] private GameObject _select;
        [SerializeField] private GameObject _credits;


        [Header("First Selected")] [SerializeField]
        private GameObject _mainMenuFirstSelected;

        [SerializeField] private GameObject _settingsFirstSelected;
        [SerializeField] private GameObject _tutorialFirstSelected;
        [SerializeField] private GameObject _controlsFirstSelected;
        [SerializeField] private GameObject _selectFirstSelected;
        [SerializeField] private GameObject _creditsFirstSelected;

        private IDisposable _menuSubscription;


        private void Start()
        {
            OpenMainMenu();
        }

        private void OnEnable()
        {
            _menuSubscription = InputSystem.onAnyButtonPress.Call(_ =>
            {
                if (!EventSystem.current.currentSelectedGameObject) SelectFirstMenuItem();
            });
        }

        private void OnDisable()
        {
            _menuSubscription?.Dispose();
        }

        private void SelectFirstMenuItem()
        {
            if (_mainMenu && _mainMenu.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_mainMenuFirstSelected);
            if (_settings && _settings.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_settingsFirstSelected);
            if (_tutorial && _tutorial.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_tutorialFirstSelected);
            if (_controls && _controls.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_controlsFirstSelected);
            if (_select && _select.activeInHierarchy) EventSystem.current.SetSelectedGameObject(_selectFirstSelected);
            if (_credits && _credits.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(_creditsFirstSelected);
        }


        public void Quit()
        {
            Application.Quit();
        }

        private void CloseAllMenus()
        {
            _mainMenu.SetActive(false);
            _settings.SetActive(false);
            _tutorial.SetActive(false);
            _controls.SetActive(false);
            _select.SetActive(false);
            _credits.SetActive(false);
        }

        public void OpenMainMenu()
        {
            CloseAllMenus();
            _mainMenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_mainMenuFirstSelected);
        }

        public void OpenSettings()
        {
            CloseAllMenus();
            _settings.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_settingsFirstSelected);
        }

        public void OpenTutorial()
        {
            CloseAllMenus();
            _tutorial.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_tutorialFirstSelected);
        }

        public void OpenControls()
        {
            CloseAllMenus();
            _controls.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_controlsFirstSelected);
        }

        public void OpenSelect()
        {
            CloseAllMenus();
            _select.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_selectFirstSelected);
        }

        public void OpenCredits()
        {
            CloseAllMenus();
            _credits.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_creditsFirstSelected);
        }
    }
}