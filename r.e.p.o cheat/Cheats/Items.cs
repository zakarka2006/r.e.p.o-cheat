using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace r.e.p.o_cheat
{
    public static class ItemTeleport
    {
        public class GameItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public object ItemObject { get; set; } // Referência ao objeto do jogo

            public GameItem(string name, int value, object itemObject = null)
            {
                Name = name;
                Value = value;
                ItemObject = itemObject;
            }
        }

        // Obtém a lista de itens a partir do DebugCheats.valuableObjects
        public static List<GameItem> GetItemList()
        {
            List<GameItem> itemList = new List<GameItem>();

            foreach (var valuableObject in DebugCheats.valuableObjects)
            {
                if (valuableObject == null) continue;

                var transform = valuableObject.GetType().GetProperty("transform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(valuableObject) as Transform;
                if (transform == null || !transform.gameObject.activeInHierarchy) continue;

                string itemName;
                try
                {
                    itemName = valuableObject.GetType().GetProperty("name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(valuableObject) as string;
                    if (string.IsNullOrEmpty(itemName))
                    {
                        itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                    }
                }
                catch (Exception e)
                {
                    itemName = (valuableObject as UnityEngine.Object)?.name ?? "Unknown";
                    Hax2.Log1($"Erro ao acessar 'name' do item: {e.Message}. Usando nome do GameObject: {itemName}");
                }

                if (itemName.StartsWith("Valuable", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = itemName.Substring("Valuable".Length).Trim();
                }
                if (itemName.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
                {
                    itemName = itemName.Substring(0, itemName.Length - "(Clone)".Length).Trim();
                }

                var valueField = valuableObject.GetType().GetField("dollarValueCurrent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                int itemValue = valueField != null ? Convert.ToInt32(valueField.GetValue(valuableObject)) : 0;

                itemList.Add(new GameItem(itemName, itemValue, valuableObject));
            }

            if (itemList.Count == 0)
            {
                itemList.Add(new GameItem("No items found", 0));
            }

            return itemList;
        }

        // Teleporta o item selecionado até o jogador
        public static void TeleportItemToMe(GameItem selectedItem)
        {
            if (selectedItem == null)
            {
                Hax2.Log1("No item selected!");
                return;
            }

            if (selectedItem.ItemObject == null)
            {
                Hax2.Log1("Item object not available for teleport!");
                return;
            }

            try
            {
                // Encontrar o jogador local usando DebugCheats.GetLocalPlayer
                GameObject player = DebugCheats.GetLocalPlayer();
                if (player == null)
                {
                    Hax2.Log1("Local player not found!");
                    return;
                }

                Vector3 playerPosition = player.transform.position;

                // Teleportar o item
                var itemMono = selectedItem.ItemObject as MonoBehaviour;
                if (itemMono != null)
                {
                    itemMono.transform.position = playerPosition;
                    Hax2.Log1($"Item '{selectedItem.Name}' teleported to player at {playerPosition}");

                    // Sincronizar via Photon, se aplicável
                    if (PhotonNetwork.IsConnected)
                    {
                        var photonView = itemMono.GetComponent<PhotonView>();
                        if (photonView != null)
                        {
                            photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { playerPosition, itemMono.transform.rotation });
                            Hax2.Log1($"Item '{selectedItem.Name}' teleported via Photon RPC.");
                        }
                    }
                }
                else
                {
                    Hax2.Log1("Item is not a MonoBehaviour!");
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Error teleporting item '{selectedItem.Name}': {e.Message}");
            }
        }
    }
}