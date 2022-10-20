using System;
using System.Collections;
using System.Collections.Generic;
using Core.Character;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ItemAdapter<TItem, TReference> : MonoBehaviour, IItemAdapter 
        where TItem : Item<TReference> where TReference : ItemReference
    {
        [SerializeField] protected TItem item;

        private TReference _reference;

        [field: SerializeField] [field: HideInInspector] 
        public Character Character { get; private set; }
        
        [field: SerializeField] [field: HideInInspector] 
        public bool Initialized { get; private set; }
        
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

        public void StartWith(IItem iItem, Character character)
        {
            item = (TItem) iItem;

            Character = character;
        }

        private void Start()
        {
            //dry initialize (works with start with)
            if (!Initialized && (item != null || Character != null))
            {
                Initialize(item, Character);
            }
        }

        public virtual void Initialize(IItem iItem, Character character)
        {
            if (Initialized)
            {
                return;
            }

            else
            {
                Initialized = true;
            }
            
            item = (TItem) iItem;

            Character = character;

            if (Character == null) Dropped();

            else
            {
                if (Character.IsReady) CharacterReady();

                else Character.OnReady += CharacterReady;
            }
        }

        protected abstract void CharacterReady();

        private void Dropped()
        {
            Collider[] colliders = GetComponents<Collider>();

            foreach (var c in colliders) c.enabled = true;

            if (TryGetComponent(out Rigidbody rBody)) rBody.isKinematic = false;
        }
        
        public void Focus()
        {
            Debug.Log($"Item {item.Title} in Focus");
        }

        public abstract void Pick(bool added, string message);
    }
}
