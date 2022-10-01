using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Character;
using Core.Input;
using Core.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Main.Item;
using Inventory.Main.Slot;

namespace Inventory.Main
{
    public class InventoryController : Controller
    {
        [SerializeField] private Beamer lookBeamer;
    
        [SerializeField] private float interactRadius = 1.5f;
        
        [SerializeField] [HideInInspector]
        private GenericDictionary<UsableSlotType, UsableSlot> usables = GenericDictionary<UsableSlotType, UsableSlot>
            .ToGenericDictionary(Utils.GetEnumValues<UsableSlotType>().ToDictionary(s => s, s => new UsableSlot()));
        
        [SerializeField] [HideInInspector]
        private GenericDictionary<WearableSlotType, WearableSlot> wearables = GenericDictionary<WearableSlotType, WearableSlot>
            .ToGenericDictionary(Utils.GetEnumValues<WearableSlotType>().ToDictionary(s => s, s => new WearableSlot()));
        
        [field: SerializeField] public Bag Bag { get; private set; }
        
        private BaseInputActions _input;
        
        private Transform _characterTransform;

        //height of character from the waist
        private float _waistLine;
        
        public GenericDictionary<UsableSlotType, UsableSlot> Usables => usables;
        
        public GenericDictionary<WearableSlotType, WearableSlot> Wearables => wearables;

#if UNITY_EDITOR
        //used for editor scripting / finding private properties
        public const string UsableName = nameof(usables);
        
        public const string WearableName = nameof(wearables);
#endif
        
        public override void Initialize(Character character)
        {
            base.Initialize(character);

            if (GameManager.Instance.GetManager(out InputManager inputManager))
            {
                _input = inputManager.InputActions;
            }

            _characterTransform = character.transform;
            
            if (character.TryGetComponent(out CharacterController controller))
            {
                _waistLine = controller.height / 2f;
            }

            else _waistLine = 1f;
        }

        private void Update()
        {
            TryToPick();
        }

        private void TryToPick()
        {
            RaycastHit[] hits = lookBeamer.Beam();
        
            if (hits != null && hits.Length > 0)
            {
                RaycastHit hit = hits[0];

                //check if item is out of range
                if (Vector3.Distance(hit.point, transform.position) > interactRadius) return;
            
                if (hit.collider.TryGetComponent(out IItemAdapter adapter))
                {
                    adapter.Focus();
                
                    if (_input.General.Interact.triggered)
                    {
                        bool added = Bag.Add(adapter.Item, out string message);
                    
                        adapter.Pick(added, message);
                    }
                }
            }
        }

        #region Equip Item
        
        public bool Equip(int index, out string message)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                message = $"Gear Index {index} Empty";

                return false;
            }
            
            if (gear.IsEquipped)
            {
                message = $"{gear.Reference.Title} already Equipped";
                
                return false;
            }

            switch (gear)
            {
                case IUsable usable:
                    return EquipUsable(usable, out message);
                
                case IWearable wearable:
                    return EquipWearable(wearable, out message);
                
                default:
                    message = $"Equip not Implemented for Gear type {gear.GetType()}";
                    return false;
            }
        }

        private bool EquipUsable(IUsable usable, out string message)
        {
            UsableSlotType slotType = usable.SlotType;

            UsableSlot slot = Usables[slotType];
            
            switch (slotType)
            {
                case UsableSlotType.TwoHand:

                    if (!Usables[UsableSlotType.LeftHand].UnEquip(out message)) return false;
                    
                    if (!Usables[UsableSlotType.RightHand].UnEquip(out message)) return false;
                    
                    break;
            }
            
            return slot.Equip(usable, out message);
        }
        
        private bool EquipWearable(IWearable wearable, out string message)
        {
            WearableSlotType slotType = wearable.SlotType;

            WearableSlot slot = Wearables[slotType];
            
            return slot.Equip(wearable, out message);
        }

        #endregion

        #region UnEquip Item
        
        public bool UnEquip(int index, out string message)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                message = $"Gear Index {index} Empty";

                return false;
            }
            
            return UnEquip(gear, out message);
        }

        public bool UnEquip(IGear gear, out string message)
        {
            if (!gear.IsEquipped)
            {
                message = $"{gear.Reference.Title} already UnEquipped";
                
                return false;
            }
            
            switch (gear)
            {
                case IUsable usable:
                    return UnEquipUsable(usable, out message);
                
                case IWearable wearable:
                    return UnEquipWearable(wearable, out message);
                
                default:
                    message = $"UnEquip not Implemented for Gear type {gear.GetType()}";
                    return false;
            }
        }
        
        private bool UnEquipUsable(IUsable usable, out string message)
        {
            UsableSlot slot = Usables[usable.SlotType];
            
            return slot.UnEquip(out message);
        }

        private bool UnEquipWearable(IWearable wearable, out string message)
        {
            WearableSlot slot = Wearables[wearable.SlotType];
            
            return slot.UnEquip(out message);
        }

        #endregion

        #region Drop Item

        public void DropGear(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Can't drop Item, Slot {index} Empty");
                
                return;
            }

            GameObject obj = Instantiate(gear.Reference.Prefab, GetDropPosition(), Quaternion.identity);

            if (obj.TryGetComponent(out IItemAdapter adapter))
            {
                adapter.Initialize(gear);
                
                Bag.RemoveGear(index);
            }

            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }
        
        public void DropSupplement(int index, int count)
        {
            ISupplement supplement = Bag.Supplements[index];

            if (supplement == null)
            {
                Debug.LogError($"Can't drop Item, Slot {index} Empty");
                
                return;
            }

            GameObject obj = Instantiate(supplement.Reference.Prefab, GetDropPosition(), Quaternion.identity);

            //make a copy for the dropped supplement
            ISupplement supplementCopy = supplement.Clone<ISupplement>();
            
            supplement.Remove(count);

            if (supplement.Count == 0)
            {
                Bag.RemoveSupplement(index);
            }
            
            supplementCopy.Resize(Mathf.Clamp(count, 0, supplementCopy.Count));
            
            if (obj.TryGetComponent(out IItemAdapter adapter))
            {
                adapter.Initialize(supplementCopy);
            }
            
            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }

        private Vector3 GetDropPosition()
        {
            Vector3 position = _characterTransform.position + _characterTransform.forward * interactRadius;

            position += _characterTransform.right * UnityEngine.Random.Range(- interactRadius, interactRadius);
            
            position.y = _waistLine;

            return position;
        }

        #endregion
    }
}
