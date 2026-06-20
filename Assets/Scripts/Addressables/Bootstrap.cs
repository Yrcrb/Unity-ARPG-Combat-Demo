using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Bootstrap : MonoBehaviour
{
    private static bool _loaded;

    private AsyncOperationHandle<AnimationConfigSO> _configHandle;
    private AsyncOperationHandle<ItemDatabaseSO> _itemDbHandle;

    private async void Start()
    {
        if (_loaded) return;
        _loaded = true;
        DontDestroyOnLoad(gameObject);
        await LoadAll();
    }

    private async UniTask LoadAll()
    {
        // 1. 加载全局 SO
        _configHandle = Addressables.LoadAssetAsync<AnimationConfigSO>(AddressKeys.AnimConfig);
        await _configHandle;
        var animConfig = _configHandle.Result;
        if (animConfig == null)
        {
            Debug.LogError("[Bootstrap] AnimConfig 加载失败！");
            return;
        }

        _itemDbHandle = Addressables.LoadAssetAsync<ItemDatabaseSO>(AddressKeys.ItemDatabase);
        await _itemDbHandle;
        var itemDb = _itemDbHandle.Result;

        // 2. 加载 UI
        var uiHandle = Addressables.InstantiateAsync(AddressKeys.UIPrefab);
        await uiHandle;

        // 3. 加载主场景
        await Addressables.LoadSceneAsync(AddressKeys.MainScene, UnityEngine.SceneManagement.LoadSceneMode.Single);

        // 4. 生成 Player
        var spawnPoint = FindWithTagSafe("PlayerSpawn");
        var pos = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;

        var playerHandle = Addressables.InstantiateAsync(AddressKeys.Player, pos, Quaternion.identity);
        await playerHandle;
        var player = playerHandle.Result;
        player.GetComponent<PlayerStatesManager>().animConfig = animConfig;

        var playerT = player.transform;
        var freeLook = FindObjectOfType<Cinemachine.CinemachineFreeLook>();
        if (freeLook != null)
        {
            freeLook.Follow = playerT;
            freeLook.LookAt = playerT;
        }

        UIManager.Instance?.SetCurrentSceneObjects(player, FindWithTagSafe("Background"));

        // 5. 生成敌人
        var enemySpawns = FindGameObjectsWithTagSafe("EnemySpawn");
        if (enemySpawns.Length > 0)
        {
            foreach (var spawn in enemySpawns)
            {
                if (spawn == null) continue;
                var enemyHandle = Addressables.InstantiateAsync(
                    AddressKeys.StoneMan, spawn.transform.position, Quaternion.identity);
                await enemyHandle;
                var enemy = enemyHandle.Result;
                var stateMgr = enemy.GetComponent<EnemysStateManager>();
                stateMgr.animConfig = animConfig;
                stateMgr.entityId = "StoneMan";
            }
        }

        // 6. 初始化背包系统
        InitInventory(itemDb);

        Debug.Log("[Bootstrap] 加载完成");
    }

    private void InitInventory(ItemDatabaseSO itemDb)
    {
        var ctrl = FindObjectOfType<InventoryController>();
        var panel = FindObjectOfType<InventoryPanel>();
        var detail = FindObjectOfType<DetailPanel>();

        if (ctrl != null)
        {
            ctrl.itemDatabase = itemDb;
            ctrl.Init(28);
        }
        if (panel != null)
        {
            panel.itemDatabase = itemDb;
            panel.Init(28);
        }
        if (detail != null)
            detail.itemDatabase = itemDb;
    }

    private static GameObject FindWithTagSafe(string tag)
    {
        try { return GameObject.FindWithTag(tag); }
        catch { return null; }
    }

    private static GameObject[] FindGameObjectsWithTagSafe(string tag)
    {
        try { return GameObject.FindGameObjectsWithTag(tag); }
        catch { return new GameObject[0]; }
    }
}
