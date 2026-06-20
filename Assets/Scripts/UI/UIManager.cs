using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private PlayerInput inputAction;
    private enum UIState { Start, Settings, OriginalSettings, KeysSet, OriginalKeys, Normal, Bag }
    private GameObject Player;
    private GameObject BackGround;
    public GameStateManager gameState;
    public UnityEvent<int> onE;

    [Header("页面")]
    [SerializeField] private GameObject StartCanvas;
    [SerializeField] private GameObject SetCanvas;
    [SerializeField] private GameObject OriginalSetCanvas;
    [SerializeField] private GameObject NormalCanvas;
    [SerializeField] private GameObject KeysCanvas;
    [SerializeField] private GameObject OriginalKeysCanvas;
    [SerializeField] private GameObject BagCanvas;
    private UIState currentState;
    private UIState previousState;
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 如果已经有一个实例存在，销毁这个新的
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (!isInitialized)
        {
            Initialize();
            isInitialized = true;
        }
    }
    void Initialize()//初始化
    {
        if (gameState == null) gameState = GameStateManager.Instance;
        inputAction = SharedPlayerInput.Actions;
        ChangeState(UIState.Start);
    }
    private void OnEnable()
    {
        if (inputAction == null)
        {
            return;
        }

        inputAction.Player.UISetting.performed += OnSettingsPerformed;
        inputAction.Player.Bag.performed += OnBagPerformed;
        inputAction.UI.Cancel.performed += OnUICancelPerformed;
    }
    void OnDisable()
    {
        if (inputAction == null)
        {
            return;
        }

        inputAction.Player.UISetting.performed -= OnSettingsPerformed;
        inputAction.Player.Bag.performed -= OnBagPerformed;
        inputAction.UI.Cancel.performed -= OnUICancelPerformed;
    }

    private void ChangeState(UIState newState)
    {
        if (currentState != newState)
        {
            previousState = currentState;
        }

        if (currentState == UIState.Settings || currentState == UIState.KeysSet || currentState == UIState.Bag)
        {
            Time.timeScale = 1f;
            if (Player != null) Player.SetActive(false);
            if (BackGround != null) BackGround.SetActive(false);
        }

        // 隐藏所有Canvas
        StartCanvas.SetActive(false);
        SetCanvas.SetActive(false);
        NormalCanvas.SetActive(false);
        KeysCanvas.SetActive(false);
        OriginalSetCanvas.SetActive(false);
        OriginalKeysCanvas.SetActive(false);
        if (BagCanvas != null) BagCanvas.SetActive(false);

        switch (newState)
        {
            case UIState.Start:
                StartCanvas.SetActive(true);
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                gameState.ChangeState(GameState.StartMenu);
                EnableUIInput();
                Debug.Log("禁用输入");
                break;
            case UIState.Settings:
                SetCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                gameState.ChangeState(GameState.PauseMenu);
                Time.timeScale = 0f;
                EnableUIInput();
                break;
            case UIState.KeysSet:
                KeysCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                gameState.ChangeState(GameState.PauseMenu);
                Time.timeScale = 0f;
                EnableUIInput();
                break;
            case UIState.OriginalSettings:
                OriginalSetCanvas.SetActive(true);
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                EnableUIInput();
                break;
            case UIState.OriginalKeys:
                OriginalKeysCanvas.SetActive(true);
                if (Player != null) Player.SetActive(false);
                if (BackGround != null) BackGround.SetActive(false);
                EnableUIInput();
                break;
            case UIState.Normal:
                NormalCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                gameState.ChangeState(GameState.Player);
                EnableGameplayInput();
                OnBeginButton();
                break;
            case UIState.Bag:
                if (BagCanvas == null)
                    Debug.LogError("[UIManager] BagCanvas 未赋值！请在 Inspector 拖入背包面板");
                else
                    BagCanvas.SetActive(true);
                if (Player != null) Player.SetActive(true);
                if (BackGround != null) BackGround.SetActive(true);
                gameState.ChangeState(GameState.PauseMenu);
                Time.timeScale = 0f;
                inputAction.UI.Enable();
                break;
        }

        currentState = newState;
    }

    private void OnBeginButton()
    {
        /*
        NormalCanvas.SetActive(true);
        Player.SetActive(true);
        BackGround.SetActive(true);
        */
        //SceceLoadManager.LoadScene(GlobalValues.SceneData.StartScene);

    }

    public void OnSettingsButton() => ChangeState(UIState.Settings);
    public void OnStartButton() => ChangeState(UIState.Start);
    public void OnKeysButton() => ChangeState(UIState.KeysSet);
    public void OnNormalButton() => ChangeState(UIState.Normal);
    public void OnOriginalSettings() => ChangeState(UIState.OriginalSettings);
    public void OnOriginalKeysButton() => ChangeState(UIState.OriginalKeys);

    private void EnableGameplayInput()// 启用游戏输入
    {
        SharedPlayerInput.EnableGameplay();
    }
    private void EnableUIInput()
    {
        SharedPlayerInput.EnableUI();
    }
    public void SetCurrentSceneObjects(GameObject scenePlayer, GameObject sceneBackground)
    {
        if (scenePlayer != null)
        {
            Player = scenePlayer;
        }
        if (sceneBackground != null)
        {
            BackGround = sceneBackground;
        }
    }
    // 清除场景对象
    public void ClearSceneObjects()
    {
        Player = null;
        BackGround = null;
    }

    private void OnSettingsPerformed(InputAction.CallbackContext ctx)
    {
        if (currentState == UIState.Normal)
        {
            OnSettingsButton();
        }
    }

    private void OnBagPerformed(InputAction.CallbackContext ctx)
    {
        if (currentState == UIState.Normal)
            ChangeState(UIState.Bag);
        else if (currentState == UIState.Bag)
            ChangeState(UIState.Normal);
    }

    private void OnUICancelPerformed(InputAction.CallbackContext ctx)
    {
        if (currentState == UIState.Start || currentState == UIState.Normal)
        {
            return;
        }

        if (currentState == UIState.Bag)
        {
            ChangeState(UIState.Normal);
            return;
        }

        ChangeState(previousState);
    }
}
