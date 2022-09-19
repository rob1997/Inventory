using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Item;
using UnityEngine;

namespace Inventory.Main
{
    [Serializable]
    public class Pouch
    {
        private int _count = 0;

        private Consumable _consumable;

        public int Count => _count;
    
        public int Limit => Consumable.limit;
    
        public Consumable Consumable => _consumable;

        public bool IsEmpty => Consumable == null && Count <= 0;

        public void Add(int count) => _count += Mathf.Clamp(count, 0, Limit - _count);
    
        public void Remove(int count) => _count -= Mathf.Clamp(count, 0, _count);

        public void Resize(int count) => _count = Mathf.Clamp(count, 0, Limit);
    
        public void Fill(Consumable consumable, int count)
        {
            _consumable = consumable;
        
            _count = count;
        }
    
        public void Clear()
        {
            _consumable = null;

            _count = 0;
        }
    }
}
