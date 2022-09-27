using System;
using System.Collections;
using System.Collections.Generic;
using Core.Character;
using Core.Input;
using Core.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using Inventory.Main.Item;

namespace Inventory.Main
{
    public class InventoryController : Controller
    {
        [SerializeField] private Beamer lookBeamer;
    
        [SerializeField] private Bag bag;
    
        [SerializeField] private float interactRadius = 1.5f;
        
        private BaseInputActions _input;
        
        private Transform _characterTransform;

        //height of character from the waist
        private float _waistLine;
        
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
                        bool added = bag.Add(adapter.Item, out string message);
                    
                        adapter.Pick(added, message);
                    }
                }
            }
        }

        private void DropGear(int index)
        {
            IGear gear = bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Can't drop Item, Slot {index} Empty");
                
                return;
            }

            GameObject obj = Instantiate(gear.Reference.Prefab, GetDropPosition(), Quaternion.identity);

            if (obj.TryGetComponent(out IItemAdapter adapter))
            {
                adapter.Initialize(gear);
                
                bag.RemoveGear(index);
            }

            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }
        
        private void DropSupplement(int index, int count)
        {
            ISupplement supplement = bag.Supplements[index];

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
                bag.RemoveSupplement(index);
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
    }
}
