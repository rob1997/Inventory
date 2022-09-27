using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Wearable<T> : Gear<T> where T : WearableReference
    {
    
    }
}
