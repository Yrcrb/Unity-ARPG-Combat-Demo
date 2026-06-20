using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包面板 V 层：全量扫描 M 数据初始化，之后订阅 SlotChanged 增量刷新。
/// </summary>
public class InventoryPanel : MonoBehaviour
{
    [Header("布局")]
    [SerializeField] private RectTransform firstSlotAnchor;
    [SerializeField] private int cols = 5;
    [SerializeField] private float horizontalSpacing = 80f;
    [SerializeField] private float verticalSpacing = 80f;

    [Header("池")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int poolSize = 28;

    public ItemDatabaseSO itemDatabase;

    public IReadOnlyDictionary<int, SlotView> ActiveSlots => _activeSlots;

    private readonly Dictionary<int, SlotView> _activeSlots = new();
    private ObjectPool<SlotView> _pool;

    private void Start()
    {
        if (itemDatabase == null)
            itemDatabase = FindObjectOfType<InventoryController>()?.itemDatabase;

        if (InventoryData.Instance != null)
            Init(poolSize);
    }

    public void Init(int maxSlots)
    {
        _pool = new ObjectPool<SlotView>(
            () =>
            {
                var go = Instantiate(slotPrefab, transform);
                go.SetActive(false);
                return go.GetComponent<SlotView>();
            },
            sv => sv.gameObject.SetActive(true),
            sv => sv.gameObject.SetActive(false),
            maxSlots + 5, maxSlots);

        // 全量扫描 M → 创建已有道具的格子
        var data = InventoryData.Instance;
        for (int i = 0; i < data.slotCount; i++)
        {
            if (!string.IsNullOrEmpty(data[i]))
                CreateOrRefreshSlot(i, data[i]);
        }

        // 此后增量走事件
        EventBus.Instance.Add<int>(E.SlotChanged, OnSlotChanged);

        Debug.Log($"[InventoryPanel] 初始化完成，池 {maxSlots} 格，已激活 {_activeSlots.Count} 格");
    }

    private void OnSlotChanged(int index)
    {
        if (InventoryData.Instance == null) return;
        CreateOrRefreshSlot(index, InventoryData.Instance[index]);
    }

    public void CreateOrRefreshSlot(int index, string itemId, ItemDatabaseSO db = null)
    {
        var d = db != null ? db : itemDatabase;
        if (_activeSlots.TryGetValue(index, out var sv))
        {
            if (string.IsNullOrEmpty(itemId))
            {
                _pool.Release(sv);
                _activeSlots.Remove(index);
            }
            else
            {
                sv.RefreshByDb(d, itemId);
            }
        }
        else if (!string.IsNullOrEmpty(itemId))
        {
            sv = _pool.Get();
            if (sv == null) return;

            var rt = (RectTransform)sv.transform;
            int row = index / cols;
            int col = index % cols;
            rt.anchoredPosition = firstSlotAnchor.anchoredPosition
                + new Vector2(col * horizontalSpacing, -row * verticalSpacing);

            sv.Init(index, InventoryData.Instance, d, this);
            sv.RefreshByDb(d, itemId);
            sv.gameObject.SetActive(true);
            _activeSlots[index] = sv;
        }
    }

    public void ResetAllScales()
    {
        foreach (var s in _activeSlots.Values)
            s.SetScale(Vector3.one);
    }

    private void ClearAll()
    {
        foreach (var s in _activeSlots.Values)
            _pool?.Release(s);
        _activeSlots.Clear();
    }

    private void OnDestroy()
    {
        EventBus.Instance.Remove<int>(E.SlotChanged, OnSlotChanged);
    }
}
