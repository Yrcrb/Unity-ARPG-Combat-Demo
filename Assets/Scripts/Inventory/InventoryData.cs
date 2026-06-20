/// <summary>
/// 背包 M 层：纯数据。通过索引器读写，set 时自动广播 SlotChanged。
/// </summary>
public class InventoryData
{
    public static InventoryData Instance { get; internal set; }

    public const int DefaultSlotCount = 28;
    public readonly int slotCount;
    private readonly SlotData[] _slots;

    public InventoryData(int count = DefaultSlotCount)
    {
        slotCount = count;
        _slots = new SlotData[count];
    }

    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= slotCount) return "";
            return _slots[index].itemId;
        }
        set
        {
            if (index < 0 || index >= slotCount) return;
            _slots[index].itemId = value;
            EventBus.Instance.Invoke(E.SlotChanged, index);
        }
    }
}

public struct SlotData
{
    public string itemId;
}
