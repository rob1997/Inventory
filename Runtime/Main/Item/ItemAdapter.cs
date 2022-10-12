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

        private TReference _reference;

        protected TReference Reference
        {
            get
            {
                if (_reference != null) return _reference;

                _reference = (TReference) item?.Reference;
                
                return _reference;
            }
        }

        public IItem Item => item;

        public GameObject Obj => this != null ? gameObject : null;

        public void Initialize(IItem iItem, bool dropped = false)
        {
            item = (TItem) iItem;

            if (dropped)
            {
                Collider[] colliders = GetComponents<Collider>();

                foreach (var c in colliders) c.enabled = true;

                if (TryGetComponent(out Rigidbody rBody)) rBody.isKinematic = false;
            }
        }

        public void Focus()
        {
            Debug.Log($"Item {item.Title} in Focus");
        }

        public abstract void Pick(bool added, string message);
    }
}
