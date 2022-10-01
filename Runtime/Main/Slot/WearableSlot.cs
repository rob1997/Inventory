using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Slot
{
    public enum WearableSlotType
    {
        Head,
        UpperBody,
        LowerBody,
    }
    
    [Serializable]
    public class WearableSlot : Slot<IWearableAdapter>
    {
        public WearableSlot()
        {
            
        }
    }
}
