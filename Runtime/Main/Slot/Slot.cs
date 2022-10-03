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
        public Transform unEquipBone;
    
        public T adapter;

        [HideInInspector] public SlotState state;

        [SerializeReference] public Slot<T>[] dependencies;
        
        private IGear _gear;

        public void Switch(IGear gear)
        {
            _gear = gear;

            if (dependencies != null)
            {
                
            }
            
            switch (state)
            {
                case SlotState.UnEquipped:
                    
                    UnEquipped();
                    
                    break;
                
                case SlotState.Equipping:
                    break;
                
                case SlotState.Equipped:
                    
                    Equipped();
                    
                    break;
                
                case SlotState.UnEquipping:
                    break;
            }
        }

        private void Equip()
        {
            if (_gear == null)
            {
                return;
            }

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

        private void UnEquipped()
        {
            adapter.Obj.transform.SetParent(unEquipBone);
            
            if (_gear == null)
            {
                return;
            }
                    
            Equip();
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
