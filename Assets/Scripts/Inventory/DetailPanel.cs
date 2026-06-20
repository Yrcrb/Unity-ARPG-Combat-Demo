using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailPanel : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button discardButton;

    public ItemDatabaseSO itemDatabase;

    private int _currentIndex = -1;
    private Transform _panel;

    private void Awake()
    {
        _panel = transform;
        _panel.gameObject.SetActive(false);
        discardButton?.onClick.AddListener(OnDiscardClicked);
        EventBus.Instance.Add<int, string>(E.SlotSelected, OnSlotSelected);
    }

    private void OnSlotSelected(int index, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Hide();
            return;
        }

        _currentIndex = index;
        var entry = itemDatabase?.GetItem(itemId);
        if (entry == null)
        {
            Hide();
            return;
        }

        iconImage.sprite = entry.icon;
        iconImage.enabled = true;
        descText.text = entry.description;
        _panel.gameObject.SetActive(true);
        if (discardButton != null) discardButton.gameObject.SetActive(true);
        Debug.Log($"[DetailPanel] 显示道具 {entry.id}");
    }

    private void OnDiscardClicked()
    {
        if (_currentIndex < 0) return;
        EventBus.Instance.Invoke(E.ItemDiscard, _currentIndex);
        Hide();
    }

    private void Hide()
    {
        _currentIndex = -1;
        _panel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Remove<int, string>(E.SlotSelected, OnSlotSelected);
        discardButton?.onClick.RemoveListener(OnDiscardClicked);
    }
}
