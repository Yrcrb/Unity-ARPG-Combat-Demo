using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 宝箱交互：角色进入范围 → 显示提示 → 按 E 键随机添加道具到背包。
/// 结构：宝箱本体挂此脚本 + Collider（非触发）；
/// 子物体 "InteractionZone" 挂 SphereCollider（IsTrigger=true）控制交互范围。
/// </summary>
public class ChestController : MonoBehaviour
{
    [Header("提示图标")]
    [SerializeField] private GameObject promptIcon;

    [Header("道具池（随机取其一）")]
    [SerializeField] private int[] itemPool = { 1, 2, 3, 4 };

    private PlayerInput _input;
    private bool _playerInRange;

    private void Awake()
    {
        if (promptIcon != null) promptIcon.SetActive(false);
        _input = SharedPlayerInput.Actions;
    }

    private void LateUpdate()
    {
        if (promptIcon != null && promptIcon.activeSelf)
            promptIcon.transform.LookAt(Camera.main.transform);
    }

    private void OnEnable()
    {
        _input.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        _input.Player.Interact.performed -= OnInteract;
    }

    // 子物体上的 Trigger 碰撞事件会向上传递到此脚本
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        Debug.Log("[Chest] 玩家进入交互范围");
        if (promptIcon != null) promptIcon.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        Debug.Log("[Chest] 玩家离开交互范围");
        if (promptIcon != null) promptIcon.SetActive(false);
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_playerInRange) return;
        int randomId = itemPool[Random.Range(0, itemPool.Length)];
        Debug.Log($"[Chest] 获得道具 id={randomId}");
        EventBus.Instance.Invoke(E.ItemPickup, randomId);
    }
}
