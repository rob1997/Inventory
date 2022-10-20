using System;
using System.Collections;
using System.Collections.Generic;
using Core.Character;
using Core.Utils;
using Inventory.Main.Slot;
using Locomotion.Utils;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class UsableAdapter<TItem, TReference> : GearAdapter<TItem, TReference>, IUsableAdapter
        where TItem : Usable<TReference> where TReference : UsableReference
    {
        public abstract bool CanUse { get; protected set; }
        
        private Animator _animator;

        public virtual void Use()
        {
            if (!CanUse) return;
        }

        protected override void CharacterReady()
        {
            _animator = Character.Animator;
        }

        public override void Equip()
        {
            CanUse = false;
            
            _animator.OverrideClips(Reference.ClipOverride);

            switch (Reference.SlotType)
            {
                case UsableSlotType.LeftHand:
                    _animator.SetTrigger(UsableSlot.EquipLeftHandHash);
                    break;
                
                case UsableSlotType.RightHand:
                    _animator.SetTrigger(UsableSlot.EquipRightHandHash);
                    break;
                
                case UsableSlotType.TwoHand:
                    _animator.SetTrigger(UsableSlot.EquipTwoHandHash);
                    break;
            }
        }

        public override void UnEquip()
        {
            CanUse = false;
            
            switch (Reference.SlotType)
            {
                case UsableSlotType.LeftHand:
                    _animator.SetTrigger(UsableSlot.UnEquipLeftHandHash);
                    break;
                
                case UsableSlotType.RightHand:
                    _animator.SetTrigger(UsableSlot.UnEquipRightHandHash);
                    break;
                
                case UsableSlotType.TwoHand:
                    _animator.SetTrigger(UsableSlot.UnEquipTwoHandHash);
                    break;
            }
        }

        public override void EquippedCallback()
        {
            CanUse = true;
        }
    }
}
