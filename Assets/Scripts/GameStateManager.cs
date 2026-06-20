using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState 
{
    Player,
    BackpackMenu,
    StartMenu,
    PauseMenu,
    Dialogue
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState currentState;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void ChangeState(GameState newState)
    {
        OnExit();
        currentState = newState;
        OnEnter();
    }

    public void OnEnter()
    {
        switch (currentState)
        {
            case GameState.Player:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case GameState.BackpackMenu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.StartMenu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.PauseMenu:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case GameState.Dialogue:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }
    public void OnExit()
    {
        switch (currentState)
        {
            case GameState.Player:
                break;
            case GameState.BackpackMenu:
                break;
            case GameState.StartMenu:
                break;
            case GameState.PauseMenu:
                break;
            case GameState.Dialogue:
                break;
        }

    }
}

public static class SharedPlayerInput
{
    private static PlayerInput actions;

    public static PlayerInput Actions
    {
        get
        {
            if (actions == null)
            {
                actions = new PlayerInput();
                actions.Disable();
            }

            return actions;
        }
    }

    public static void EnableGameplay()
    {
        Actions.UI.Disable();
        Actions.Player.Enable();
    }

    public static void EnableUI()
    {
        Actions.Player.Disable();
        Actions.UI.Enable();
    }
}
