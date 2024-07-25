using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _settings;
    [SerializeField] private GameObject _tutorial;
    [SerializeField] private GameObject _controls;
    [SerializeField] private GameObject _select;
    [SerializeField] private GameObject _credits;


    [Header("First Selected")]
    [SerializeField] private GameObject _mainMenuFirstSelected;
    [SerializeField] private GameObject _settingsFirstSelected;
    [SerializeField] private GameObject _tutorialFirstSelected;
    [SerializeField] private GameObject _controlsFirstSelected;
    [SerializeField] private GameObject _selectFirstSelected;
    [SerializeField] private GameObject _creditsFirstSelected;

    private void Start()
    {
        OpenMainMenu();
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
