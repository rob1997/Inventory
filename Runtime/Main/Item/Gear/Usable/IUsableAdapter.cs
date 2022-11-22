using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IUsableAdapter : IGearAdapter
    {
        bool CanUse { get; }
        
        Transform Holster { get; }

        void Use();
    }
}
