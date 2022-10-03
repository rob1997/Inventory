using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using UnityEngine;

namespace Inventory.Main.Item
{
    public delegate void Equipped();
    
    public delegate void UnEquipped();
    
    public interface IGearAdapter : IItemAdapter
    {
        Equipped Equipped { get; set; }
        
        UnEquipped UnEquipped { get; set; }
        
        void Equip();
        
        void UnEquip();
    }
}
