using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IItem
    {
        ItemReference Reference { get; }

        T Clone<T>() where T : IItem;
    }
}
