using System;
using UnityEngine;
using Photon.Pun;
using System.Reflection;
using System.Collections.Generic;

namespace r.e.p.o_cheat
{
    static class DebugCheats
    {
        private static int frameCounter = 0;
        public static bool drawEspBool = false; 
        public static bool drawItemEspBool = false;
        private static List<Enemy> enemyList = new List<Enemy>();
        private static List<object> valuableObjects = new List<object>(); 
        private static Camera cachedCamera;
        private static float scaleX, scaleY;
        public static Texture2D texture2;
        private static float lastUpdateTime = 0f;
        private const float updateInterval = 0.5f; 
        private static GameObject localPlayer; 


        static DebugCheats()
        {
            cachedCamera = Camera.main;
            if (cachedCamera != null)
            {
                scaleX = (float)Screen.width / cachedCamera.pixelWidth;
                scaleY = (float)Screen.height / cachedCamera.pixelHeight;
            }
            UpdateLists();
            UpdateLocalPlayer(); 
        }
        private static void UpdateLists()
        {
            enemyList.Clear();
            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                var enemyDirectorInstance = enemyDirectorType.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (enemyDirectorInstance != null)
                {
                    var enemiesSpawnedField = enemyDirectorType.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
                    if (enemiesSpawnedField != null)
                    {
                        var enemies = enemiesSpawnedField.GetValue(enemyDirectorInstance) as IEnumerable<object>;
                        if (enemies != null)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy != null)
                                {
                                    var enemyInstanceField = enemy.GetType().GetField("enemyInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("childEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (enemyInstanceField != null)
                                    {
                                        var enemyInstance = enemyInstanceField.GetValue(enemy) as Enemy;
                                        if (enemyInstance != null && enemyInstance.gameObject != null && enemyInstance.gameObject.activeInHierarchy)
                                        {
                                            enemyList.Add(enemyInstance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            valuableObjects.Clear();
            var valuableArray = UnityEngine.Object.FindObjectsOfType(Type.GetType("ValuableObject, Assembly-CSharp"));
            if (valuableArray != null)
            {
                valuableObjects.AddRange(valuableArray);
            }

            lastUpdateTime = Time.time;
            Hax2.Log1($"Listas atualizadas: {enemyList.Count} inimigos, {valuableObjects.Count} itens.");
        }

        private static void UpdateLocalPlayer()
        {
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (photonViewField != null)
                    {
                        var photonView = photonViewField.GetValue(player) as PhotonView;
                        if (photonView != null && photonView.IsMine)
                        {
                            var gameObjectProperty = player.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                            localPlayer = gameObjectProperty != null ? (gameObjectProperty.GetValue(player) as GameObject) : photonView.gameObject;
                            Hax2.Log1("Jogador local atualizado: " + localPlayer.name);
                            return;
                        }
                    }
                }
            }
            Hax2.Log1("Nenhum jogador local encontrado para atualizar.");
        }
        public static void SpawnItem()
        {
            if (!PhotonNetwork.IsConnected)
            {
                Hax2.Log1("Photon not connected. SpawnItem only works in multiplayer.");
                return;
            }

            // Se for o Master Client, spawnar diretamente
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnItemLocally();
            }
            else
            {
                // Se não for o Master Client, usar o PhotonView do jogador local para enviar RPC
                GameObject localPlayer = GetLocalPlayer();
                if (localPlayer != null)
                {
                    PhotonView photonView = localPlayer.GetComponent<PhotonView>();
                    if (photonView != null)
                    {
                        photonView.RPC("SpawnItemRPC", RpcTarget.MasterClient);
                        Hax2.Log1("RPC sent to Master Client to spawn item.");
                    }
                    else
                    {
                        Hax2.Log1("No PhotonView found on local player to send RPC.");
                    }
                }
                else
                {
                    Hax2.Log1("Local player not found to send RPC.");
                }
            }
        }

        private static void SpawnItemLocally()
        {
            var debugAxelType = Type.GetType("DebugAxel, Assembly-CSharp");
            if (debugAxelType != null)
            {
                var debugAxelInstance = GameHelper.FindObjectOfType(debugAxelType);
                if (debugAxelInstance != null)
                {
                    var spawnObjectMethod = debugAxelType.GetMethod("SpawnObject", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (spawnObjectMethod != null)
                    {
                        GameObject player = GetLocalPlayer();
                        Vector3 spawnPosition;

                        if (player != null)
                        {
                            spawnPosition = player.transform.position;
                            Hax2.Log1("Spawning item at player's feet: " + spawnPosition.ToString());
                        }
                        else
                        {
                            spawnPosition = new Vector3(0f, 1f, 0f);
                            Hax2.Log1("Player not found, using default spawn position.");
                        }

                        GameObject itemToSpawn = AssetManager.instance.surplusValuableSmall;
                        string path = "Valuables/";

                        // Instanciar o item via Photon para sincronização
                        GameObject spawnedItem = PhotonNetwork.Instantiate(path + itemToSpawn.name, spawnPosition, Quaternion.identity);
                        Hax2.Log1("Item spawned successfully via Photon: " + spawnedItem.name);

                        // Se o SpawnObject ainda precisar ser chamado no DebugAxel, invoque-o
                        spawnObjectMethod.Invoke(debugAxelInstance, new object[] { itemToSpawn, spawnPosition, path });
                    }
                    else
                    {
                        Hax2.Log1("SpawnObject method not found in DebugAxel.");
                    }
                }
                else
                {
                    Hax2.Log1("DebugAxel instance not found in the scene.");
                }
            }
            else
            {
                Hax2.Log1("DebugAxel type not found.");
            }
        }

        // Método RPC (deve estar em uma classe com PhotonView, como o jogador)
        [PunRPC]
        private static void SpawnItemRPC()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnItemLocally();
            }
        }
        private static GameObject GetLocalPlayer()
        {
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    var photonViewField = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (photonViewField != null)
                    {
                        var photonView = photonViewField.GetValue(player) as PhotonView;
                        if (photonView != null && photonView.IsMine) 
                        {
                            var gameObjectProperty = player.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                            if (gameObjectProperty != null)
                            {
                                return gameObjectProperty.GetValue(player) as GameObject;
                            }
                            return photonView.gameObject;
                        }
                    }
                }
            }

            if (PhotonNetwork.LocalPlayer != null)
            {
                foreach (var photonView in UnityEngine.Object.FindObjectsOfType<PhotonView>())
                {
                    if (photonView.Owner == PhotonNetwork.LocalPlayer && photonView.IsMine)
                    {
                        return photonView.gameObject;
                    }
                }
            }

            Hax2.Log1("Nenhum jogador local encontrado!");
            return null; 
        }

        public static void UpdateEnemyList()
        {
            enemyList.Clear();
            Hax2.Log1("Atualizando lista de inimigos");

            var enemyDirectorType = Type.GetType("EnemyDirector, Assembly-CSharp");
            if (enemyDirectorType != null)
            {
                var enemyDirectorInstance = enemyDirectorType.GetField("instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (enemyDirectorInstance != null)
                {
                    var enemiesSpawnedField = enemyDirectorType.GetField("enemiesSpawned", BindingFlags.Public | BindingFlags.Instance);
                    if (enemiesSpawnedField != null)
                    {
                        var enemies = enemiesSpawnedField.GetValue(enemyDirectorInstance) as IEnumerable<object>;
                        if (enemies != null)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy != null)
                                {
                                    var enemyInstanceField = enemy.GetType().GetField("enemyInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("Enemy", BindingFlags.NonPublic | BindingFlags.Instance)
                                                          ?? enemy.GetType().GetField("childEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (enemyInstanceField != null)
                                    {
                                        var enemyInstance = enemyInstanceField.GetValue(enemy) as Enemy;
                                        if (enemyInstance != null && enemyInstance.gameObject != null && enemyInstance.gameObject.activeInHierarchy)
                                        {
                                            enemyList.Add(enemyInstance); // Armazenar diretamente o Enemy
                                        }
                                    }
                                }
                            }
                            Hax2.Log1($"Inimigos encontrados: {enemyList.Count}");
                        }
                        else
                        {
                            Hax2.Log1("Nenhum inimigo encontrado em enemiesSpawned");
                        }
                    }
                    else
                    {
                        Hax2.Log1("Campo 'enemiesSpawned' não encontrado");
                    }
                }
                else
                {
                    Hax2.Log1("Instância de EnemyDirector é nula");
                }
            }
            else
            {
                Hax2.Log1("EnemyDirector não encontrado");
            }
        }

        public static void RectFilled(float x, float y, float width, float height, Texture2D text)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), text);
        }

        public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        public static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 2f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }

        public static void DrawESP()
        {
            if (!drawEspBool && !drawItemEspBool) return;

            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdateLists();
                UpdateLocalPlayer();
            }

            frameCounter++;
            if (frameCounter % 2 != 0) return;

            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
                if (cachedCamera == null)
                {
                    Hax2.Log1("Camera.main não encontrada!");
                    return;
                }
                scaleX = (float)Screen.width / cachedCamera.pixelWidth;
                scaleY = (float)Screen.height / cachedCamera.pixelHeight;
            }

            // ESP de inimigos com altura dinâmica para nomes longos
            if (drawEspBool)
            {
                // Estilo para o nome do inimigo
                GUIStyle enemyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true, // Permitir quebra de linha
                    fontSize = 12,
                    fontStyle = FontStyle.Bold // Opcional, para consistência
                };

                foreach (var enemyInstance in enemyList)
                {
                    if (enemyInstance == null || !enemyInstance.gameObject.activeInHierarchy || enemyInstance.CenterTransform == null) continue;

                    Vector3 footPosition = enemyInstance.transform.position;
                    float enemyHeightEstimate = 2f;
                    Vector3 headPosition = enemyInstance.transform.position + Vector3.up * enemyHeightEstimate;

                    Vector3 screenFootPos = cachedCamera.WorldToScreenPoint(footPosition);
                    Vector3 screenHeadPos = cachedCamera.WorldToScreenPoint(headPosition);

                    if (screenFootPos.z > 0 && screenHeadPos.z > 0)
                    {
                        float footX = screenFootPos.x * scaleX;
                        float footY = Screen.height - (screenFootPos.y * scaleY);
                        float headY = Screen.height - (screenHeadPos.y * scaleY);

                        float height = Mathf.Abs(footY - headY);
                        float enemyScale = enemyInstance.transform.localScale.y;
                        float baseWidth = enemyScale * 200f;
                        float distance = screenFootPos.z;
                        float width = (baseWidth / distance) * scaleX;

                        width = Mathf.Clamp(width, 30f, height * 1.2f);
                        height = Mathf.Clamp(height, 40f, 400f);

                        float x = footX;
                        float y = footY;

                        Box(x, y, width, height, texture2, 1f);

                        float labelWidth = 100f; // Largura fixa da label
                        float labelX = x - labelWidth / 2f; // Centro da caixa

                        var enemyParent = enemyInstance.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                        string enemyName = "Enemy";
                        if (enemyParent != null)
                        {
                            var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                            enemyName = nameField?.GetValue(enemyParent) as string ?? "Enemy";
                        }

                        string distanceText = "";
                        if (localPlayer != null)
                        {
                            float distance2 = Vector3.Distance(localPlayer.transform.position, enemyInstance.transform.position);
                            distanceText = $" [{distance2:F1}m]";
                        }
                        string fullText = enemyName + distanceText;

                        // Calcular altura dinâmica baseada no texto
                        float labelHeight = enemyStyle.CalcHeight(new GUIContent(fullText), labelWidth);
                        float labelY = y - height - labelHeight; // Posicionar acima da caixa com altura dinâmica

                        GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), fullText, enemyStyle);
                    }
                }
            }

            // ESP de itens (mantido como na última versão)
            if (drawItemEspBool)
            {
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    border = new RectOffset(1, 1, 1, 1)
                };

                GUIStyle valueStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                foreach (var valuableObject in valuableObjects)
                {
                    if (valuableObject == null) continue;

                    var transform = valuableObject.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(valuableObject) as Transform;
                    if (transform == null || !transform.gameObject.activeInHierarchy) continue;

                    Vector3 itemPosition = transform.position;
                    Vector3 screenPos = cachedCamera.WorldToScreenPoint(itemPosition);

                    if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                    {
                        float x = screenPos.x * scaleX;
                        float y = Screen.height - (screenPos.y * scaleY);

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
                        if (itemName.EndsWith(" (Clone)", StringComparison.OrdinalIgnoreCase))
                        {
                            itemName = itemName.Substring(0, itemName.Length - " (Clone)".Length).Trim();
                        }

                        var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                        int itemValue = valueField != null ? Convert.ToInt32(valueField.GetValue(valuableObject)) : 0;

                        float labelWidth = 150f;
                        float valueLabelHeight = valueStyle.CalcHeight(new GUIContent(itemValue.ToString() + "$"), labelWidth);
                        float nameLabelHeight = nameStyle.CalcHeight(new GUIContent(itemName), labelWidth);
                        float totalHeight = nameLabelHeight + valueLabelHeight + 5f;
                        float labelX = x - labelWidth / 2f;
                        float labelY = y - totalHeight - 5f;

                        GUI.Label(new Rect(labelX, labelY, labelWidth, nameLabelHeight), itemName, nameStyle);
                        GUI.Label(new Rect(labelX, labelY + nameLabelHeight + 2f, labelWidth, valueLabelHeight), itemValue.ToString() + "$", valueStyle);
                    }
                }
            }
        }
        public static void KillAllEnemies()
        {
            Hax2.Log1("Tentando matar todos os inimigos");

            foreach (var enemyInstance in enemyList)
            {
                if (enemyInstance == null) continue;

                try
                {
                    var healthField = enemyInstance.GetType().GetField("Health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (healthField != null)
                    {
                        var healthComponent = healthField.GetValue(enemyInstance);
                        if (healthComponent != null)
                        {
                            var healthType = healthComponent.GetType();
                            var hurtMethod = healthType.GetMethod("Hurt", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (hurtMethod != null)
                            {
                                hurtMethod.Invoke(healthComponent, new object[] { 9999, Vector3.zero });
                                Hax2.Log1($"Inimigo ferido com 9999 de dano via Hurt");
                            }
                            else
                                Hax2.Log1("Método 'Hurt' não encontrado em EnemyHealth");
                        }
                        else
                            Hax2.Log1("Componente EnemyHealth é nulo");
                    }
                    else
                        Hax2.Log1("Campo 'Health' não encontrado em Enemy");
                }
                catch (Exception e)
                {
                    Hax2.Log1($"Erro ao matar inimigo: {e.Message}");
                }
            }
            UpdateEnemyList();
        }
    }
}