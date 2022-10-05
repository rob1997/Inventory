using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ItemAdapter<TItem, TReference> : MonoBehaviour, IItemAdapter 
        where TItem : Item<TReference> where TReference : ItemReference
    {
        [SerializeField] protected TItem item;

        public IItem Item => item;

        public GameObject Obj => this != null ? gameObject : null;

        public void Initialize(IItem iItem)
        {
            item = (TItem) iItem;
        }

        public void Focus()
        {
            Debug.Log($"Item {item.Title} in Focus");
        }

        public abstract void Pick(bool added, string message);
    }
}
