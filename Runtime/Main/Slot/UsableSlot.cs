using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Slot
{
    public enum UsableSlotType
    {
        LeftHand,
        RightHand,
        TwoHand,
    }
    
    [Serializable]
    public class UsableSlot : Slot<IUsableAdapter>
    {
        public UsableSlot()
        {
            
        }
    }
}
