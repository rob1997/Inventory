using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Character;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Slot
{
    public enum UsableSlotType
    {
        LeftHand,
        RightHand,
        TwoHand,
    }
    
    [Serializable]
    public class UsableSlot : Slot<IUsableAdapter>
    {
        //slots that need to be unEquipped for this to be equipped,
        //twoHanded is a dependency for both right/left hand,
        //right and left hand are dependencies for twoHanded
        //beware of circular dependency
        [field: SerializeField] public UsableSlotType[] Dependencies { get; private set; }
        
        [field: SerializeField] public Transform UnEquipBone { get; set; }

        protected override bool CanSwitch()
        {
            bool dependent = CheckDependency();

            //if dependent wait until dependencies are unequipped
            return !dependent;
        }

        private bool CheckDependency()
        {
            bool dependent = false;
            
            if (Dependencies != null)
            {   
                foreach (var dependency in Dependencies)
                {
                    UsableSlot slot = controller.Usables[dependency];
                    
                    if (slot.State != SlotState.UnEquipped)
                    {
                        slot.Switch(null);

                        dependent = true;
                    }
                }
            }
            
            //if dependent wait until dependencies are unequipped
            return dependent;
        }
        
        protected override void UnEquipped()
        {
            base.UnEquipped();
            
            //make sure it's not an already empty slot
            if (adapter?.Obj != null) adapter.Obj.transform.SetParent(UnEquipBone);
            
            if (Gear == null)
            {
                if (Dependencies != null)
                {
                    foreach (var dependency in Dependencies)
                    {
                        controller.Usables[dependency].Switch();
                    }
                }
                
                return;
            }
                    
            Equip();
        }
        
        public void AddDependency(UsableSlotType slotType)
        {
            if (Dependencies == null) Dependencies = new UsableSlotType[]{};

            Dependencies = Dependencies.Append(slotType).ToArray();
        }
        
        public void RemoveDependency(UsableSlotType slotType)
        {
            if (Dependencies == null)
            {
                Dependencies = new UsableSlotType[]{};
                return;
            }

            Dependencies = Dependencies.Where(d => d != slotType).ToArray();
        }
    }
}
