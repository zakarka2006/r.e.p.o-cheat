using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Reflection;

namespace r.e.p.o_cheat
{
    public static class Teleport
    {
        public static void TeleportPlayerToMe(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                Hax2.Log1("Invalid player index!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                Hax2.Log1("Selected player is null!");
                return;
            }

            GameObject localPlayer = DebugCheats.GetLocalPlayer();
            if (localPlayer == null)
            {
                Hax2.Log1("Local player not found!");
                return;
            }

            // Ensure transform is valid
            if (!(selectedPlayer is MonoBehaviour playerMono) || playerMono.transform == null)
            {
                Hax2.Log1("Invalid player transform!");
                return;
            }

            Vector3 targetPosition = localPlayer.transform.position + Vector3.up * 1.5f;
            playerMono.transform.position = targetPosition;

            // Ensure PhotonView exists before attempting RPC
            PhotonView photonView = playerMono.GetComponent<PhotonView>();
            if (photonView != null && PhotonNetwork.IsConnected)
            {
                photonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, playerMono.transform.rotation });
                Hax2.Log1($"Sent RPC 'SpawnRPC' to sync teleport.");
            }
        }
        public static void TeleportMeToPlayer(int selectedPlayerIndex, List<object> playerList, List<string> playerNames)
        {
            if (selectedPlayerIndex < 0 || selectedPlayerIndex >= playerList.Count)
            {
                Hax2.Log1("Índice de jogador inválido!");
                return;
            }
            var selectedPlayer = playerList[selectedPlayerIndex];
            if (selectedPlayer == null)
            {
                Hax2.Log1("Jogador selecionado é nulo!");
                return;
            }

            try
            {
                GameObject localPlayer = DebugCheats.GetLocalPlayer();
                if (localPlayer == null)
                {
                    Hax2.Log1("Jogador local não encontrado!");
                    return;
                }

                var localPhotonViewField = localPlayer.GetComponent<PhotonView>();
                if (localPhotonViewField == null)
                {
                    Hax2.Log1("PhotonViewField não encontrado no jogador local!");
                    return;
                }
                var localPhotonView = localPhotonViewField;
                if (localPhotonView == null)
                {
                    Hax2.Log1("PhotonView local não é válido!");
                    return;
                }

                var playerMono = selectedPlayer as MonoBehaviour;
                if (playerMono == null)
                {
                    Hax2.Log1("selectedPlayer não é um MonoBehaviour!");
                    return;
                }

                var targetTransform = playerMono.transform;
                if (targetTransform == null)
                {
                    Hax2.Log1("Transform do jogador selecionado é nulo!");
                    return;
                }

                Vector3 targetPosition = targetTransform.position + Vector3.up * 1.5f;
                localPlayer.transform.position = targetPosition;
                Hax2.Log1($"Você foi teleportado localmente para {playerNames[selectedPlayerIndex]} em {targetPosition}");

                if (PhotonNetwork.IsConnected && localPhotonView != null)
                {
                    localPhotonView.RPC("SpawnRPC", RpcTarget.AllBuffered, new object[] { targetPosition, localPlayer.transform.rotation });
                    Hax2.Log1($"RPC 'SpawnRPC' enviado para todos com posição: {targetPosition}");
                }
                else
                {
                    Hax2.Log1("Não conectado ao Photon, teleporte apenas local.");
                }
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao teleportar você até {playerNames[selectedPlayerIndex]}: {e.Message}");
            }
        }
    }
}