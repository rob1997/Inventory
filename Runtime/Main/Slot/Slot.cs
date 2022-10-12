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
            //isEquipped = false
            UnEquipped,
            Equipping,
            
            //isEquipped = true
            Equipped,
            UnEquipping,
        }

        [SerializeField] protected T adapter;
        
        [field: SerializeField] public Transform EquipBone { get; set; }
        
        [field: SerializeField] public SlotState State { get; private set; }

        protected IGear Gear { get; private set; }

        public T Adapter => adapter;
        
        public InventoryController controller;
        
        protected void Switch()
        {
            Switch(Gear);
        }
        
        //switch to null for unEquip
        public void Switch(IGear gear)
        {
            Gear = gear;

            if (!CanSwitch()) return;

            switch (State)
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
            if (adapter == null || adapter.Item.Id != Gear.Id)
            {
                //destroy old
                if (adapter?.Obj != null) adapter.Obj.Destroy();
                
                GameObject obj = Object.Instantiate(Gear.Reference.Prefab, EquipBone);
                
                obj.transform.LocalReset();
                
                //has no adapter on object
                //assign adapter too
                if (!obj.TryGetComponent(out adapter)) Debug.LogError($"Item adapter not Found on {Gear.Title} Prefab");
                
                adapter.Initialize(Gear);

                RegisterEquippedEvent();
                
                RegisterUnEquippedEvent();
            }

            //re-equipping from adapter (same item)
            else
            {
                adapter.Obj.transform.LocalReset(EquipBone);

                //if it's start with item then we need to reassign events
                if (adapter.Equipped == null) RegisterEquippedEvent();
                
                if (adapter.UnEquipped == null) RegisterUnEquippedEvent();
            }
            
            State = SlotState.Equipping;
            
            controller.InvokeEquipInitialized();
            
            adapter.Equip(controller.GetCharacter());
        }

        private void RegisterEquippedEvent()
        {
            //assign adapter delegates
            adapter.Equipped = delegate
            {
                State = SlotState.Equipped;
                    
                Equipped();
            };
        }
        
        private void RegisterUnEquippedEvent()
        {
            //assign adapter delegates
            adapter.UnEquipped = delegate
            {
                State = SlotState.UnEquipped;
                    
                UnEquipped();
            };
        }
        
        protected virtual void Equipped()
        {
            controller.InvokeEquipped(adapter.Item);
            
            //if un-equipping or equipping a different item => un-equip current one
            if (Gear == null || Gear.Id != adapter.Item.Id)
            {
                UnEquip();
            }
        }

        protected void UnEquip()
        {
            State = SlotState.UnEquipping;
            
            controller.InvokeUnEquipInitialized();
            
            adapter.UnEquip();
        }

        protected virtual void UnEquipped()
        {
            controller.InvokeUnEquipped(adapter?.Item);
        }

        public virtual void StartWith(T startWithAdapter)
        {
            if (startWithAdapter == null || startWithAdapter.Obj == null) return;
            
            //destroy old
            if (adapter?.Obj != null) adapter.Obj.Destroy();

            adapter = startWithAdapter;
            
            Gear = null;
        }
        
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
