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
            public object ItemObject { get; set; }

            public GameItem(string name, int value, object itemObject = null)
            {
                Name = name;
                Value = value;
                ItemObject = itemObject;
            }
        }

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

        public static void TeleportItemToMe(GameItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ItemObject == null)
            {
                Hax2.Log1("Item selecionado ou ItemObject é nulo!");
                return;
            }

            try
            {
                GameObject player = DebugCheats.GetLocalPlayer();
                if (player == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                Vector3 playerPosition = player.transform.position + Vector3.up * 1.5f;

                Transform itemTransform = null;
                var itemObjectType = selectedItem.ItemObject.GetType();
                var transformProperty = itemObjectType.GetProperty("transform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (transformProperty != null)
                {
                    itemTransform = transformProperty.GetValue(selectedItem.ItemObject) as Transform;
                }
                else
                {
                    var itemMono = selectedItem.ItemObject as MonoBehaviour;
                    if (itemMono != null)
                    {
                        itemTransform = itemMono.transform;
                    }
                }

                if (itemTransform == null)
                {
                    Hax2.Log1($"Não foi possível obter o Transform do item '{selectedItem.Name}'!");
                    return;
                }

                itemTransform.position = playerPosition;
                Hax2.Log1($"Item '{selectedItem.Name}' teleportado localmente para {playerPosition}");

                if (PhotonNetwork.IsConnected)
                {
                    var photonView = itemTransform.GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        if (photonView.IsMine)
                        {
                            photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { playerPosition, itemTransform.rotation });
                            Hax2.Log1($"Item '{selectedItem.Name}' teleportado e sincronizado via Photon RPC (IsMine).");
                        }
                        else
                        {
                            photonView.RequestOwnership();
                            photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { playerPosition, itemTransform.rotation });
                            Hax2.Log1($"Item '{selectedItem.Name}' teleportado e sincronizado via Photon RPC (IsMine).");
                            photonView.RPC("RequestItemTeleportRPC", photonView.Owner, new object[] { playerPosition });
                            Hax2.Log1($"Pedido de teleporte enviado ao dono do item '{selectedItem.Name}' para {playerPosition}");
                        }
                    }
                    else
                    {
                        Hax2.Log1($"Item '{selectedItem.Name}' não tem PhotonView, teleporte apenas local.");
                    }
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar item '{selectedItem.Name}': {e.Message}");
            }
        }

        [PunRPC]
        private static void RequestItemTeleportRPC(Vector3 targetPosition)
        {
            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer != null)
            {
                foreach (var item in DebugCheats.valuableObjects)
                {
                    var transform = item.GetType().GetProperty("transform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(item) as Transform;
                    if (transform != null)
                    {
                        var photonView = transform.GetComponent<PhotonView>();
                        if (photonView != null && photonView.IsMine)
                        {
                            transform.position = targetPosition;
                            photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, transform.rotation });
                            Hax2.Log1($"Teleporte de item solicitado recebido e sincronizado para {targetPosition}");
                            break;
                        }
                    }
                }
            }
        }
    }
}