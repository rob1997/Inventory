using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IItemAdapter
    {
        IItem Item { get; }
        
        GameObject Obj { get; }

        void Initialize(IItem item);
        
        void Focus();

        void Pick(bool picked, string message);
    }
}
