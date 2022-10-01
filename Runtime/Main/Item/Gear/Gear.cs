using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    [Serializable]
    public abstract class Gear<T> : Item<T>, IGear where T : GearReference
    {
        [HideInInspector] [SerializeField] private bool isEquipped;

        public bool IsEquipped => isEquipped;
        
        public virtual void Equip()
        {
            //already equipped
            if (isEquipped) Debug.LogWarning($"Item {reference.Title} already Equipped");
            
            isEquipped = true;
        }

        public void UnEquip()
        {
            //already unequipped
            if (!isEquipped) Debug.LogWarning($"Item {reference.Title} already UnEquipped");
            
            isEquipped = false;
        }
    }
}
