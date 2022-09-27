using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class GearAdapter<TItem, TReference> : ItemAdapter<TItem, TReference> 
        where TItem : Gear<TReference> where TReference : GearReference
    {
        public override void Pick(bool picked, string message)
        {
            if (picked)
            {
                Debug.Log(message);
            
                Destroy(gameObject);
            }

            else
            {
                Debug.LogWarning(message);
            }
        }
    }
}