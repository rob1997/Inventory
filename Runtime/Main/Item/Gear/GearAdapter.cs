using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class GearAdapter<TItem, TReference> : ItemAdapter<TItem, TReference>, IGearAdapter
        where TItem : Gear<TReference> where TReference : GearReference
    {   
        public Equipped Equipped { get; set; }
        
        public UnEquipped UnEquipped { get; set; }
        
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

        public void Equip()
        {
            //Equipped.Invoke();
            StartCoroutine(WaitAndDo(1, Equipped.Invoke));
        }

        public void UnEquip()
        {
            //UnEquipped.Invoke();
            StartCoroutine(WaitAndDo(1, UnEquipped.Invoke));
        }

        public IEnumerator WaitAndDo(float wait, Action todo)
        {
            yield return new WaitForSeconds(wait);
            
            todo.Invoke();
        }
    }
}