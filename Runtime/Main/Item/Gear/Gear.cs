using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Gear<T> : Item<T>, IGear where T : GearReference
    {
        
    }
}
