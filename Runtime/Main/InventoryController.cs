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

            if (Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                Equip((IUsable) Bag.Gears[0]);
            }
            
            else if (Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                Equip((IUsable) Bag.Gears[1]);
            }
            
            else if (Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                Equip((IUsable) Bag.Gears[2]);
            }
            
            else if (Keyboard.current.numpad4Key.wasPressedThisFrame)
            {
                UnEquip((IUsable) Bag.Gears[0]);
            }
            
            else if (Keyboard.current.numpad5Key.wasPressedThisFrame)
            {
                UnEquip((IUsable) Bag.Gears[1]);
            }
            
            else if (Keyboard.current.numpad6Key.wasPressedThisFrame)
            {
                UnEquip((IUsable) Bag.Gears[2]);
            }
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
        
        public void Equip(IUsable usable)
        {
            UsableSlot slot = usables[usable.SlotType];
            
            slot.Switch(usable);
        }
        
        public void UnEquip(IUsable usable)
        {
            UsableSlot slot = usables[usable.SlotType];

            //make sure we're unEquipping the same item
            if (slot.adapter?.Item?.Id == usable.Id)
            {
                slot.Switch(null);
            }
        }
        
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
