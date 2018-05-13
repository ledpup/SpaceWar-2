using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Assets
{
    public class ArmouredObject : NetworkBehaviour
    {
        public float Armour = 1;

        public Text ArmourText;

        [SyncVar] float _armour;
        private PlayerHud _playerHud;

        void Start()
        {
            _armour = Armour;
            _playerHud = gameObject.GetComponent<PlayerHud>();

            RpcUpdateHud(Vector3.zero);
        }

        public void TakeDamage(float amount, Vector3 vector, bool componentDamaged, string componentName)
        {
            if (!isServer || _armour <= 0)
                return;

            if (componentDamaged)
            {
                var componentTransform = transform.Find(componentName);
                var component = componentTransform.GetComponent<IComponent>();
                var destroyed = component.TakeDamage(amount);
                if (destroyed)
                {
                    componentTransform.parent = null;
                    NetworkServer.Destroy(componentTransform.gameObject);

                    //RpcDestroySubcomponent(GetComponent<NetworkIdentity>().netId, componentName);
                }
            }
            else
            {
                _armour -= amount;
                RpcUpdateHud(vector);
            }
            

            if (_armour <= 0)
            {
                RpcDied();
                _armour = 0;
                //Invoke("BackToLobby", 3f);    
            }
        }

        [ClientRpc]
        private void RpcDied()
        {
            Destroy(gameObject);
        }

        [ClientRpc]
        private void RpcDestroySubcomponent(NetworkInstanceId networkInstanceId, string component)
        {
            var localObject = ClientScene.FindLocalObject(networkInstanceId);
            var componentTransform = localObject.transform.Find(component);
            if (componentTransform != null)
                Destroy(componentTransform.gameObject);

        }

        [ClientRpc]
        private void RpcUpdateHud(Vector3 vector)
        {
            var rigidbody = transform.GetComponent<Rigidbody>();

            rigidbody.AddForce(vector);
            if (isLocalPlayer)
            {
                if (_playerHud != null)
                    _playerHud.ArmourText.text = "Armour " + Mathf.RoundToInt(_armour * 10).ToString();

            }
        }
    }
}
;