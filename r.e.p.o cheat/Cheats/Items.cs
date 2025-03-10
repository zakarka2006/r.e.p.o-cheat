using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Reflection;

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

        private static PhotonView punManagerPhotonView;

        private static void InitializePunManager()
        {
            if (punManagerPhotonView == null)
            {
                var punManagerType = Type.GetType("PunManager, Assembly-CSharp");
                var punManagerInstance = GameHelper.FindObjectOfType(punManagerType);
                if (punManagerInstance != null)
                {
                    punManagerPhotonView = (PhotonView)punManagerType.GetField("photonView", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(punManagerInstance);
                    if (punManagerPhotonView == null) { Hax2.Log1("PhotonView not found in PunManager."); }
                }
                else { Hax2.Log1("PunManager instance not found."); }
            }
        }

        public static List<GameItem> GetItemList()
        {
            List<GameItem> itemList = new List<GameItem>();

            foreach (var valuableObject in DebugCheats.valuableObjects)
            {
                if (valuableObject == null) continue;

                var transform = valuableObject.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as Transform;
                if (transform == null || !transform.gameObject.activeInHierarchy) continue;

                string itemName;
                try
                {
                    itemName = valuableObject.GetType().GetProperty("name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as string;
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

                var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
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

                Vector3 targetPosition = player.transform.position + Vector3.up * 1.5f;
                Hax2.Log1($"Target position for teleport: {targetPosition}");

                Transform itemTransform = null;
                var itemObjectType = selectedItem.ItemObject.GetType();
                var transformProperty = itemObjectType.GetProperty("transform", BindingFlags.Public | BindingFlags.Instance);
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

                PhotonView itemPhotonView = itemTransform.GetComponent<PhotonView>();
                if (itemPhotonView == null)
                {
                    Hax2.Log1($"Item '{selectedItem.Name}' não tem PhotonView, teleporte apenas local.");
                    itemTransform.position = targetPosition;
                    return;
                }

                if (itemTransform.GetComponent<ItemTeleportComponent>() == null)
                {
                    itemTransform.gameObject.AddComponent<ItemTeleportComponent>();
                    Hax2.Log1($"Adicionado ItemTeleportComponent ao item '{selectedItem.Name}'");
                }

                InitializePunManager();
                if (punManagerPhotonView == null)
                {
                    Hax2.Log1("PunManager PhotonView não inicializado, teleporte apenas local.");
                    itemTransform.position = targetPosition;
                    return;
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    PerformTeleport(itemTransform, itemPhotonView, targetPosition);
                }
                else
                {
                    if (!itemPhotonView.IsMine)
                    {
                        itemPhotonView.RequestOwnership();
                        Hax2.Log1($"Requested ownership of item '{selectedItem.Name}' (ViewID: {itemPhotonView.ViewID})");
                    }

                    int viewID = itemPhotonView.ViewID;
                    string steamID = SemiFunc.PlayerGetSteamID(SemiFunc.PlayerAvatarLocal());
                    if (string.IsNullOrEmpty(steamID))
                    {
                        Hax2.Log1("Não foi possível obter SteamID local, teleporte apenas local.");
                        itemTransform.position = targetPosition;
                        return;
                    }

                    punManagerPhotonView.RPC("RequestTeleportItemRPC", RpcTarget.MasterClient, viewID, targetPosition, steamID);
                    Hax2.Log1($"Solicitado teleporte do item '{selectedItem.Name}' (ViewID: {viewID}) para {targetPosition} ao Master Client");

                    itemTransform.position = targetPosition;
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar item '{selectedItem.Name}': {e.Message}");
            }
        }

        private static void PerformTeleport(Transform itemTransform, PhotonView itemPhotonView, Vector3 targetPosition)
        {
            var transformView = itemTransform.GetComponent<PhotonTransformView>();
            bool wasTransformViewActive = false;
            if (transformView != null && transformView.enabled)
            {
                wasTransformViewActive = true;
                transformView.enabled = false;
                Hax2.Log1($"PhotonTransformView desativado temporariamente no item '{itemTransform.name}'");
            }

            PhotonNetwork.RemoveBufferedRPCs(itemPhotonView.ViewID, "TeleportItemRPC");

            itemTransform.position = targetPosition;
            Rigidbody rb = itemTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            itemPhotonView.RPC("TeleportItemRPC", RpcTarget.All, targetPosition);
            Hax2.Log1($"Master Client teleportou item '{itemTransform.name}' (ViewID: {itemPhotonView.ViewID}) para {targetPosition}");

            if (wasTransformViewActive || rb != null)
            {
                itemTransform.gameObject.AddComponent<DelayedPhysicsReset>().Setup(rb, transformView);
            }
        }

        [PunRPC]
        private static void RequestTeleportItemRPC(int itemViewID, Vector3 targetPosition, string requesterSteamID)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonView itemPhotonView = PhotonView.Find(itemViewID);
                if (itemPhotonView != null)
                {
                    Transform itemTransform = itemPhotonView.transform;
                    PerformTeleport(itemTransform, itemPhotonView, targetPosition);
                    Hax2.Log1($"Master Client processou teleporte do item (ViewID: {itemViewID}) para {targetPosition} a pedido de SteamID: {requesterSteamID}");
                }
                else
                {
                    Hax2.Log1($"Item com ViewID {itemViewID} não encontrado no Master Client.");
                }
            }
        }
    }

    public class ItemTeleportComponent : MonoBehaviour, IPunOwnershipCallbacks
    {
        private PhotonView photonView;

        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Hax2.Log1($"PhotonView não encontrado no item '{gameObject.name}', adicionando um novo.");
                photonView = gameObject.AddComponent<PhotonView>();
            }
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDestroy()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        [PunRPC]
        private void TeleportItemRPC(Vector3 targetPosition)
        {
            var transformView = GetComponent<PhotonTransformView>();
            if (transformView != null && transformView.enabled)
            {
                transformView.enabled = false;
                Hax2.Log1($"PhotonTransformView desativado no item '{gameObject.name}' durante teleporte");
            }

            transform.position = targetPosition;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            Hax2.Log1($"Item '{gameObject.name}' sincronizado para {targetPosition} via RPC");
        }

        public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership requested for '{gameObject.name}' by player {requestingPlayer.ActorNumber}");
            }
        }

        public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership of '{gameObject.name}' transferred from {previousOwner?.ActorNumber} to {targetView.OwnerActorNr}");
            }
        }

        public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
        {
            if (targetView == photonView)
            {
                Hax2.Log1($"Ownership transfer failed for '{gameObject.name}' by player {senderOfFailedRequest.ActorNumber}");
            }
        }
    }

    public class DelayedPhysicsReset : MonoBehaviour
    {
        private Rigidbody rb;
        private PhotonTransformView transformView;
        private float delay = 1f;

        public void Setup(Rigidbody rigidbody, PhotonTransformView tView = null)
        {
            rb = rigidbody;
            transformView = tView;
            Invoke(nameof(ResetPhysics), delay);
        }

        private void ResetPhysics()
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                Hax2.Log1($"Physics reativada para '{gameObject.name}' após teleporte");
            }
            if (transformView != null)
            {
                transformView.enabled = true;
                Hax2.Log1($"PhotonTransformView reativado para '{gameObject.name}' após teleporte");
            }
            Destroy(this);
        }
    }
}