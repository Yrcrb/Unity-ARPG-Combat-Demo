using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public GameState currentState;
    void Start()
    {
        ChangeState(GameState.StartMenu);
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
