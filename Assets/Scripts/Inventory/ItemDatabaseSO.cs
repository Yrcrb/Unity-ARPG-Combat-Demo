using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具静态数据注册表（SO 资产）。
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item Database", fileName = "ItemDatabase")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemEntry> items;

    public ItemEntry GetItem(string id)
    {
        if (items == null) return null;
        return items.Find(e => e.id == id);
    }
}

/// <summary>
/// 单条道具的静态信息。
/// </summary>
[System.Serializable]
public class ItemEntry
{
    [Tooltip("唯一标识")]
    public string id;

    [Tooltip("显示名称")]
    public string itemName;

    [Tooltip("类型标签（整理时用）")]
    public string type;

    [Tooltip("图标")]
    public Sprite icon;

    [TextArea, Tooltip("描述文字")]
    public string description;
}
