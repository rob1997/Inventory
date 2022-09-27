using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Inventory.Main.Item
{
    [Serializable]
    public abstract class Item<T> : IItem where T : ItemReference
    {
        [SerializeField] protected T reference;

        public ItemReference Reference => reference;

        public TItem Clone<TItem>() where TItem : IItem
        {
            //soft clone/copy...maintain references
            return (TItem) this.MemberwiseClone();
        }
    }
}