using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Inventory.Main.Item;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Inventory.Main.Slot
{
    [Serializable]
    public abstract class Slot<T> : ISerializationCallbackReceiver where T : class, IGearAdapter
    {
        public Transform bone;
    
        public T adapter;

        public bool IsEquipped => adapter != null && adapter?.Obj != null;
        
        public virtual bool Equip(IGear gear, out string message)
        {
            if (IsEquipped)
            {
                if (!UnEquip(out message))
                {
                    return false;
                }
            }
            
            GameObject obj = Object.Instantiate(gear.Reference.Prefab, bone);

            string title = gear.Reference.Title;
            
            if (obj.TryGetComponent(out adapter))
            {
                adapter.Initialize(gear);

                message = $"Item {title} Equipped";

                gear.Equip();
                
                return true;
            }

            obj.Destroy();

            message = $"ItemAdapter Component not found on Item {title} Prefab";
            
            return false;
        }

        public virtual bool UnEquip(out string message)
        {
            if (!IsEquipped)
            {
                message = "Can't UnEquip, Slot already Empty";
                
                return false;
            }

            IGear gear = (IGear) adapter.Item;

            gear.UnEquip();
            
            adapter.Obj.Destroy();
            
            adapter = default;

            message = $"Item {gear.Reference.Title} UnEquipped";

            return true;
        }

        //Serialize adapter value
#if UNITY_EDITOR
        
        [SerializeField] private Object _value;
        
        public void OnBeforeSerialize()
        {
            if (IsEquipped)
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
