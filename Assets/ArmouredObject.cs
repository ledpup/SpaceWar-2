using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class ArmouredObject : MonoBehaviour
    {
        public float Armour = 10;

        public Text ArmourText;

        public Vector3 QueuedPhysics = Vector3.zero;
        private PlayerHud _playerHud;

        void Start()
        {
            _playerHud = gameObject.GetComponent<PlayerHud>();

            UpdateHud(Vector3.zero);
            
        }

        public void TakeDamage(float amount, Vector3 vector, bool componentDamaged, string componentName)
        {
            if (Armour <= 0)
                return;

            if (componentDamaged)
            {
                var componentTransform = transform.Find(componentName);
                var component = componentTransform.GetComponent<IComponent>();
                var destroyed = component.TakeDamage(amount);
                if (destroyed)
                {
                    DestroyComponent(componentName);
                    Destroy(componentTransform.gameObject);
                }
            }
            else
            {
                Armour -= amount * 10;

                UpdateHud(vector);
            }
            

            if (Armour <= 0)
            {
                Destroy(gameObject);
                Armour = 0;
            }
        }

        private void DestroyComponent(string component)
        {
            var componentTransform = gameObject.transform.Find(component);
            if (componentTransform != null)
            {
                Destroy(componentTransform.gameObject);
            }
        }

        private void UpdateHud(Vector3 vector)
        {
            QueuedPhysics = vector;
            if (_playerHud != null)
                _playerHud.ArmourText.text = "Armour " + Mathf.RoundToInt(Armour).ToString();
        }
    }
}
;