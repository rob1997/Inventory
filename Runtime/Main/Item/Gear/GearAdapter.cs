using System;
using System.Collections;
using System.Collections.Generic;
using Core.Character;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class GearAdapter<TItem, TReference> : ItemAdapter<TItem, TReference>, IGearAdapter
        where TItem : Gear<TReference> where TReference : GearReference
    {
        public Equipped Equipped { get; set; }
        
        public UnEquipped UnEquipped { get; set; }

        public IGear Gear => item;
        
        protected Character Character;
        
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

        public virtual void Equip(Character character)
        {
            Character = character;
        }

        public virtual void UnEquip()
        {
            
        }
    }
}