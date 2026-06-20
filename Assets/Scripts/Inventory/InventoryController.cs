using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包 C 层：持有 M 层数据，负责所有业务逻辑（Add/Remove/Swap/Sort）。
/// 不持有 V 层引用，不参与通知链。
/// </summary>
public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    public ItemDatabaseSO itemDatabase;
    public InventoryData data { get; private set; }

    [Header("整理按钮（背包界面上）")]
    [SerializeField] private UnityEngine.UI.Button sortButton;

    private bool _inited;

    private void Awake()
    {
        Instance = this;
        if (itemDatabase == null)
            Debug.LogError("[InventoryCtrl] ItemDatabaseSO 未赋值！");
        if (sortButton != null)
            sortButton.onClick.AddListener(Sort);
        if (!_inited) Init();
    }

    public void Init(int slotCount = 28)
    {
        _inited = true;
        data = new InventoryData(slotCount);
        InventoryData.Instance = data;
        EventBus.Instance.Remove<int>(E.ItemDiscard, OnDiscard);
        EventBus.Instance.Remove<int, int>(E.SlotSwap, OnSwap);
        EventBus.Instance.Remove<int>(E.ItemPickup, OnItemPickup);
        EventBus.Instance.Add<int>(E.ItemDiscard, OnDiscard);
        EventBus.Instance.Add<int, int>(E.SlotSwap, OnSwap);
        EventBus.Instance.Add<int>(E.ItemPickup, OnItemPickup);
    }

    public bool Add(string itemId)
    {
        if (data == null) return false;
        int idx = FindEmpty();
        if (idx < 0) return false;
        data[idx] = itemId;
        return true;
    }

    public void Remove(int index)
    {
        if (data == null) return;
        int n = data.slotCount;
        if (index < 0 || index >= n) return;

        for (int i = index; i < n - 1; i++)
            data[i] = data[i + 1];
        data[n - 1] = "";
    }

    public void Swap(int a, int b)
    {
        if (data == null || a == b) return;
        string temp = data[a];
        data[a] = data[b];
        data[b] = temp;
    }

    public void Sort()
    {
        if (data == null || itemDatabase == null) return;
        int n = data.slotCount;

        var nonEmpty = new List<(int origIdx, string id)>();
        for (int i = 0; i < n; i++)
        {
            if (!string.IsNullOrEmpty(data[i]))
                nonEmpty.Add((i, data[i]));
        }

        nonEmpty.Sort((a, b) =>
        {
            var ea = itemDatabase.GetItem(a.id);
            var eb = itemDatabase.GetItem(b.id);
            int c = string.Compare(ea?.type ?? "", eb?.type ?? "");
            if (c != 0) return c;
            return string.Compare(a.id, b.id);
        });

        for (int i = 0; i < n; i++)
        {
            string newId = i < nonEmpty.Count ? nonEmpty[i].id : "";
            if (data[i] != newId)
                data[i] = newId;
        }
    }

    private int FindEmpty()
    {
        for (int i = 0; i < data.slotCount; i++)
            if (string.IsNullOrEmpty(data[i]))
                return i;
        return -1;
    }

    private void OnItemPickup(int id)
    {
        if (!Add(id.ToString()))
            Debug.Log("[InventoryCtrl] 背包已满！");
    }

    private void OnDiscard(int index) => Remove(index);
    private void OnSwap(int a, int b) => Swap(a, b);

    private void OnDestroy()
    {
        EventBus.Instance.Remove<int>(E.ItemDiscard, OnDiscard);
        EventBus.Instance.Remove<int, int>(E.SlotSwap, OnSwap);
        EventBus.Instance.Remove<int>(E.ItemPickup, OnItemPickup);
        if (sortButton != null) sortButton.onClick.RemoveListener(Sort);
    }
}
