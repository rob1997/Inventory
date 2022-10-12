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
using Locomotion.Controllers;

namespace Inventory.Main
{
    public class InventoryController : Controller
    {
        //When UnEquip is Initialized
        #region UnEquipInitialized

        public delegate void UnEquipInitialized();

        public event UnEquipInitialized OnUnEquipInitialized;

        public void InvokeUnEquipInitialized()
        {
            OnUnEquipInitialized?.Invoke();
        }

        #endregion
        
        //When Equip is Initialized
        #region EquipInitialized

        public delegate void EquipInitialized();

        public event EquipInitialized OnEquipInitialized;

        public void InvokeEquipInitialized()
        {
            OnEquipInitialized?.Invoke();
        }

        #endregion
        
        //When Equip is Completed i.e. after animations, logic...
        #region Equipped

        public delegate void Equipped(IItem item);

        public event Equipped OnEquipped;

        public void InvokeEquipped(IItem item)
        {
            OnEquipped?.Invoke(item);
        }

        #endregion

        //When UnEquip is Completed i.e. after animations, logic...
        #region UnEquipped

        public delegate void UnEquipped(IItem item);

        public event UnEquipped OnUnEquipped;

        public void InvokeUnEquipped(IItem item)
        {
            OnUnEquipped?.Invoke(item);
        }

        #endregion

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

            try
            {
                _waistLine = character.CharacterController.height / 2f;
            }
            
            catch (Exception _)
            {
                _waistLine = 1f;
            }

            character.OnEventDispatched += (label, args) =>
            {
                switch (label)
                {
                    case nameof(Character.Equipped):
                        usables[(UsableSlotType) (int) args[0]].Adapter?.Equipped.Invoke();
                        break;
                    case nameof(Character.UnEquipped):
                        usables[(UsableSlotType) (int) args[0]].Adapter?.UnEquipped.Invoke();
                        break;
                }
            };

            //UnEquip OnSprint and Stop Sprint when Equip
            if (character.GetController(out MotionController motionController))
            {
                if (motionController.IsInitialized) RegisterUnEquipOnSprint(motionController);
                
                else motionController.OnInitialized += delegate { RegisterUnEquipOnSprint(motionController); };
            }
        }

        //UnEquip OnSprint and Stop Sprint when Equip
        private void RegisterUnEquipOnSprint(MotionController motionController)
        {
            motionController.OnSpeedRateChange += rate =>
            {
                if (rate == MotionController.SpeedRate.Sprint) UnEquipAll();
            };
            
            OnEquipInitialized += delegate
            {
                if (motionController.Rate == MotionController.SpeedRate.Sprint)
                {
                    //switch to Run from Sprint if Equip is initialized (two way)
                    motionController.ChangeSpeedRate(MotionController.SpeedRate.Run);
                }
            };
        }
        
        private void Update()
        {
            TryToPick();
            
#if UNITY_EDITOR
            DebugSwitchUsables(_input.General.Change.ReadValue<float>() * - 1f);
#endif
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

#if UNITY_EDITOR

        private int _inventoryIndex = - 1;
        
        /// <summary>
        /// switch between all Gears in inventory
        /// scroll to last/first item and keep scrolling to unEquip all
        /// </summary>
        /// <param name="direction"></param>
        private void DebugSwitchUsables(float direction)
        {
            int[] indexes = Bag != null ? Bag.Gears.FindIndexes(g => g is IUsable) : 
                Utils.GetEnumValues<UsableSlotType>().Cast<int>().ToArray();
            
            if (direction > 0)
            {
                _inventoryIndex += 1;

                int length = indexes.Length;
                
                if (_inventoryIndex >= length)
                {
                    _inventoryIndex = length;
                    
                    UnEquipAllUsables();
                    
                    return;
                }
                
                if (Bag != null) Equip(indexes[_inventoryIndex]);
                
                else EquipUsableSlot((UsableSlotType) indexes[_inventoryIndex]);
            }

            else if (direction < 0)
            {
                _inventoryIndex -= 1;

                if (_inventoryIndex < 0)
                {
                    _inventoryIndex = - 1;
                    
                    UnEquipAllUsables();
                    
                    return;
                }
                
                if (Bag != null) Equip(indexes[_inventoryIndex]);
                
                else EquipUsableSlot((UsableSlotType) indexes[_inventoryIndex]);
            }
        }
#endif
        
        #region Equip

        public void Equip(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Gear Empty at index {index}");
                
                return;
            }
            
            switch (gear)
            {
                case IUsable usable:
                    EquipUsable(usable);    
                    break;
                
                case IWearable wearable:
                    EquipWearable(wearable);
                    break;
            }
        }
        
        private void EquipUsable(IUsable usable)
        {
            UsableSlot slot = usables[usable.SlotType];
            
            slot.Switch(usable);
        }
        
        //used for Equipping characters that don't have bags like NPCs
        private void EquipUsableSlot(UsableSlotType usableSlotType)
        {
            UsableSlot slot = usables[usableSlotType];
            
            slot.Switch(slot.Adapter?.Gear);
        }
        
        private void EquipWearable(IWearable wearable)
        {
            WearableSlot slot = wearables[wearable.SlotType];
            
            slot.Switch(wearable);
        }

        #endregion

        #region UnEquip

        public void UnEquip(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Gear Empty at index {index}");
                
                return;
            }
            
            switch (gear)
            {
                case IUsable usable:
                    UnEquipUsable(usable);
                    break;
                
                case IWearable wearable:
                    UnEquipWearable(wearable);
                    break;
            }
        }
        
        private void UnEquipUsable(IUsable usable)
        {
            UsableSlot slot = usables[usable.SlotType];

            //make sure we're unEquipping the same item
            if (slot.Adapter?.Item?.Id != usable.Id)
            {
                Debug.LogError($"{usable.Title} not Equipped on {usable.SlotType}");
                
                return;
            }
            
            //unEquip
            slot.Switch(null);
        }
        
        private void UnEquipWearable(IWearable wearable)
        {
            WearableSlot slot = wearables[wearable.SlotType];

            //make sure we're unEquipping the same item
            if (slot.Adapter?.Item?.Id != wearable.Id)
            {
                Debug.LogError($"{wearable.Title} not Equipped on {wearable.SlotType}");
                
                return;
            }
            
            //unEquip
            slot.Switch(null);
        }

        public void UnEquipUsableSlot(UsableSlotType slotType)
        {
            UsableSlot slot = usables[slotType];
            
            slot.Switch(null);
        }
        
        public void UnEquipWearableSlot(WearableSlotType slotType)
        {
            WearableSlot slot = wearables[slotType];
            
            slot.Switch(null);
        }

        public void UnEquipAll()
        {
            UnEquipAllUsables();
            UnEquipAllWearables();
        }
        
        public void UnEquipAllUsables()
        {
            foreach (var slot in usables.Values)
            {
                slot.Switch(null);
            }
        }
        
        public void UnEquipAllWearables()
        {
            foreach (var slot in wearables.Values)
            {
                slot.Switch(null);
            }
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
                adapter.Initialize(gear, true);
                
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
                adapter.Initialize(supplementCopy, true);
            }
            
            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }

        private Vector3 GetDropPosition()
        {
            Vector3 position = _characterTransform.position + _characterTransform.forward;

            position += _characterTransform.right * UnityEngine.Random.Range(- 1f, 1f);
            
            position.y = _waistLine;

            return position;
        }

        #endregion
    }
}
