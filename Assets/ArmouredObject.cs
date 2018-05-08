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

            RpcClientAfterCollision();
        }

        public void TakeDamage(float amount)
        {
            if (!isServer || _armour <= 0)
                return;

            _armour -= amount;
            RpcClientAfterCollision();

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
        private void RpcClientAfterCollision()
        {
            if (isLocalPlayer)
            {
                if (_playerHud != null)
                    _playerHud.ArmourText.text = "Armour " + Mathf.RoundToInt(_armour * 10).ToString();

            }
        }
    }
}
