using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IGear : IItem
    {
        bool IsEquipped { get; }

        void Equip();
        
        void UnEquip();
    }
}
