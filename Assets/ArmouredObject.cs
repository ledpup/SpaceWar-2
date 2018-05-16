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
        [SyncVar] public Vector3 QueuedPhysics = Vector3.zero;
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
                    RpcDestroyComponent(GetComponent<NetworkIdentity>().netId, componentName);
                    Destroy(componentTransform.gameObject);
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
        private void RpcDestroyComponent(NetworkInstanceId networkInstanceId, string component)
        {
            var localObject = ClientScene.FindLocalObject(networkInstanceId);
            var componentTransform = localObject.transform.Find(component);
            if (componentTransform != null)
            {
                Destroy(componentTransform.gameObject);
            }
        }

        [Command]
        public void CmdDestroy(GameObject button)
        {
            NetworkServer.Destroy(button);
        }

        [ClientRpc]
        private void RpcUpdateHud(Vector3 vector)
        {
            if (isLocalPlayer)
            {
                QueuedPhysics = vector;
                if (_playerHud != null)
                    _playerHud.ArmourText.text = "Armour " + Mathf.RoundToInt(_armour * 10).ToString();

            }
        }
    }
}
;