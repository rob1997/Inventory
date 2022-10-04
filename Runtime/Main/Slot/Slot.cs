using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Inventory.Main.Item;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Inventory.Main.Slot
{
    [Serializable]
    public abstract class Slot<T> : ISerializationCallbackReceiver where T : class, IGearAdapter
    {
        public enum SlotState
        {
            //false
            UnEquipped,
            Equipping,
            
            //true
            Equipped,
            UnEquipping,
        }
        
        [FormerlySerializedAs("bone")] public Transform equipBone;
    
        public T adapter;

        [HideInInspector] public SlotState state;
        
        private IGear _gear;

        protected IGear Gear => _gear;

        public InventoryController controller;
        
        protected void Switch()
        {
            Switch(_gear);
        }
        
        //switch to null for unEquip
        public void Switch(IGear gear)
        {
            _gear = gear;

            if (_gear != null && !CanSwitch()) return;

            switch (state)
            {
                case SlotState.UnEquipped:
                    //already unequipped (avoid circular dependency on usable slots)
                    if (gear == null) return;
                    UnEquipped();
                    break;
                
                case SlotState.Equipped:
                    //already equipped with the same item (avoid circular dependency on usable slots)
                    if (gear != null && gear.Id == adapter?.Item?.Id) return;
                    Equipped();
                    break;
            }
        }

        protected abstract bool CanSwitch();
        
        protected void Equip()
        {
            //no item/adapter in slot
            //or
            //new item is being equipped
            if (adapter == null || adapter.Item.Id != _gear.Id)
            {
                //destroy old
                if (adapter?.Obj != null) adapter.Obj.Destroy();
                
                GameObject obj = Object.Instantiate(_gear.Reference.Prefab, equipBone);

                obj.TryGetComponent(out adapter);
                
                adapter.Initialize(_gear);

                adapter.Equipped = delegate
                {
                    state = SlotState.Equipped;
                    
                    Equipped();
                };
                
                adapter.UnEquipped = delegate
                {
                    state = SlotState.UnEquipped;
                    
                    UnEquipped();
                };
            }

            else
            {
                adapter.Obj.transform.SetParent(equipBone);
            }

            state = SlotState.Equipping;
            
            adapter.Equip();
        }
        
        private void Equipped()
        {
            if (_gear == null || _gear.Id != adapter.Item.Id)
            {
                UnEquip();
            }
        }

        private void UnEquip()
        {
            state = SlotState.UnEquipping;
            
            adapter.UnEquip();
        }

        protected abstract void UnEquipped();

        //Serialize adapter value
#if UNITY_EDITOR
        
        [SerializeField] private Object _value;
        
        public void OnBeforeSerialize()
        {
            if (adapter?.Obj != null)
            {
                _value = adapter as Object;
            }
        }
        
        public void OnAfterDeserialize()
        {
            adapter = _value as T;
        }
        
#endif
        
    }
}
