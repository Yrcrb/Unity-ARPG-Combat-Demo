using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个背包格子 V 层，支持点击和拖拽。
/// 挂在格子预制体上。
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlotView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private int _index;
    private string _itemId;
    private InventoryData _data;
    private ItemDatabaseSO _db;
    private InventoryPanel _panel;
    private RectTransform _rt;
    private Canvas _canvas;
    private Vector3 _originalPos;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void Init(int index, InventoryData data, ItemDatabaseSO db, InventoryPanel panel)
    {
        _index = index;
        _data = data;
        _db = db;
        _panel = panel;
        _itemId = data[index];
        RefreshByDb(db, _itemId);
        EventBus.Instance.Add<int>(E.SlotChanged, OnSlotChanged);
    }

    public void SetScale(Vector3 scale) => _rt.localScale = scale;

    #region Display

    public void RefreshByDb(ItemDatabaseSO db, string itemId)
    {
        _itemId = itemId;
        if (iconImage == null) return;
        var entry = db?.GetItem(itemId);
        iconImage.sprite = entry?.icon;
        iconImage.enabled = entry != null;
    }

    private void OnSlotChanged(int index)
    {
        if (index != _index) return;
        _itemId = _data?[_index] ?? "";
        RefreshByDb(_db, _itemId);
    }

    #endregion

    #region Click

    public void OnPointerClick(PointerEventData eventData)
    {
        EventBus.Instance.Invoke(E.SlotSelected, _index, _itemId);
    }

    #endregion

    #region Drag

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_itemId)) return;
        _originalPos = _rt.position;
        _rt.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_itemId)) return;
        _rt.position = Input.mousePosition;

        _panel.ResetAllScales();
        var hit = FindHitSlot(eventData);
        if (hit != null && hit != this)
            hit.SetScale(Vector3.one * 1.1f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_itemId)) return;
        _panel.ResetAllScales();

        var hit = FindHitSlot(eventData);
        if (hit != null && hit != this)
            EventBus.Instance.Invoke(E.SlotSwap, _index, hit._index);
        _rt.position = _originalPos;
    }

    private SlotView FindHitSlot(PointerEventData eventData)
    {
        foreach (var slot in _panel.ActiveSlots.Values)
        {
            if (slot == this || slot.gameObject.activeSelf == false) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)slot.transform, eventData.position, _canvas?.worldCamera))
                return slot;
        }
        return null;
    }

    #endregion

    private void OnDisable()
    {
        EventBus.Instance.Remove<int>(E.SlotChanged, OnSlotChanged);
    }
}
