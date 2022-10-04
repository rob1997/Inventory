using System;
using System.Collections;
using System.Collections.Generic;
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
        //slots that need to be unEquipped for this one to be equipped,
        //twoHanded is a dependency for both right/left hand,
        //right and left hand are dependencies for twoHanded
        //beware of circular dependency, dependency is vertical (top to bottom or vice versa) and single path-ed and inherited
        public UsableSlotType[] dependencies;
        
        public Transform unEquipBone;

        protected override bool CanSwitch()
        {
            bool dependent = CheckDependency();

            //if dependent wait until dependencies are unequipped
            return !dependent;
        }

        private bool CheckDependency()
        {
            bool dependent = false;
            
            if (dependencies != null)
            {   
                foreach (var dependency in dependencies)
                {
                    UsableSlot slot = controller.Usables[dependency];
                    
                    if (slot.state != SlotState.UnEquipped || slot.Gear != null)
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
            //make sure it's not an already empty slot
            if (adapter?.Obj != null) adapter.Obj.transform.SetParent(unEquipBone);
            
            if (Gear == null)
            {
                if (dependencies != null)
                {
                    foreach (var dependency in dependencies)
                    {
                        controller.Usables[dependency].Switch();
                    }
                }
                
                return;
            }
                    
            Equip();
        }
    }
}
