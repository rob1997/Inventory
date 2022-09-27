using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class UsableAdapter<TItem, TReference> : GearAdapter<TItem, TReference> 
        where TItem : Usable<TReference> where TReference : UsableReference
    {
        
    }
}