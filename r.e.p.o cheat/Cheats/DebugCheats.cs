using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // Necessário para ReceiverGroup e RaiseEventOptions
using ExitGames.Client.Photon; // Necessário para SendOptions
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
namespace r.e.p.o_cheat
{
    static class DebugCheats
    {
        private static int frameCounter = 0;
        public static List<Enemy> enemyList = new List<Enemy>();
        public static List<object> valuableObjects = new List<object>();
        private static List<object> playerList = new List<object>();
        private static Camera cachedCamera;
        private static float scaleX, scaleY;
        public static Texture2D texture2;
        private static float lastUpdateTime = 0f;
        private static float lastExtractionUpdateTime = 0f; // Separado para Extraction Points
        private const float updateInterval = 1f;
        private const float extractionUpdateInterval = 5f; // Atualizar a cada 5 segundos
        private static GameObject localPlayer;
        private static List<ExtractionPointData> extractionPointList = new List<ExtractionPointData>();

        public static bool drawEspBool = false;
        public static bool drawItemEspBool = false;
        public static bool drawPlayerEspBool = false;
        public static bool draw3DPlayerEspBool = false;
        public static bool drawExtractionPointEspBool = false;

        // Novas variáveis para controle de exibição
        public static bool showEnemyNames = true;
        public static bool showEnemyDistance = true;
        public static bool showItemNames = true;
        public static bool showItemValue = true;
        public static bool showItemDistance = false; // Padrão false para itens
        public static bool showExtractionNames = true;
        public static bool showExtractionDistance = true;
        public static bool showPlayerNames = true;
        public static bool showPlayerDistance = true;
        public static bool showPlayerHP = true;

        private static List<PlayerData> playerDataList = new List<PlayerData>();
        private static float lastPlayerUpdateTime = 0f;
        private static float playerUpdateInterval = 1f; // Atualizar a cada 1 segundo
        private static Dictionary<int, int> playerHealthCache = new Dictionary<int, int>(); // Cache de saúde por PhotonView ID
        private const float maxEspDistance = 100f; // Distância máxima para exibir ESP

        public class PlayerData
        {
            public object PlayerObject { get; }
            public PhotonView PhotonView { get; }
            public Transform Transform { get; }
            public bool IsAlive { get; set; }
            public string Name { get; set; }

            public PlayerData(object player)
            {
                PlayerObject = player;
                PhotonView = player.GetType().GetField("photonView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(player) as PhotonView;
                Transform = player.GetType().GetProperty("transform", BindingFlags.Public | BindingFlags.Instance)?.GetValue(player) as Transform;
                Name = (player as PlayerAvatar) != null ? (SemiFunc.PlayerGetName(player as PlayerAvatar) ?? "Unknown Player") : "Unknown Player";
                IsAlive = true; // Inicialmente assumimos vivo, atualizado depois
            }
        }
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
            UpdateExtractionPointList();
            UpdatePlayerDataList();
        }
        private static void UpdatePlayerDataList()
        {
            playerDataList.Clear();
            playerHealthCache.Clear();
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        var data = new PlayerData(player);
                        if (data.PhotonView != null && data.Transform != null)
                        {
                            playerDataList.Add(data);
                            int health = GetPlayerHealth(player);
                            playerHealthCache[data.PhotonView.ViewID] = health;
                        }
                    }
                }
            }
            lastPlayerUpdateTime = Time.time;
            Hax2.Log1($"Lista de dados de jogadores atualizada: {playerDataList.Count} jogadores.");
        }
        private static void UpdateExtractionPointList()
        {
            extractionPointList.Clear();
            var extractionPoints = UnityEngine.Object.FindObjectsOfType(Type.GetType("ExtractionPoint, Assembly-CSharp"));
            if (extractionPoints != null)
            {
                foreach (var ep in extractionPoints)
                {
                    var extractionPoint = ep as ExtractionPoint;
                    if (extractionPoint != null && extractionPoint.gameObject.activeInHierarchy)
                    {
                        var currentStateField = extractionPoint.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
                        string cachedState = "Unknown";
                        if (currentStateField != null)
                        {
                            var stateValue = currentStateField.GetValue(extractionPoint);
                            cachedState = stateValue?.ToString() ?? "Unknown";
                        }
                        Vector3 cachedPosition = extractionPoint.transform.position;
                        extractionPointList.Add(new ExtractionPointData(extractionPoint, cachedState, cachedPosition));
                        Hax2.Log1($"Extraction Point cacheado na posição: {cachedPosition}");
                    }
                }
                Hax2.Log1($"Lista de Extraction Points atualizada: {extractionPointList.Count} pontos encontrados.");
            }
        }

        public class ExtractionPointData
        {
            public ExtractionPoint ExtractionPoint { get; }
            public string CachedState { get; }
            public Vector3 CachedPosition { get; }

            public ExtractionPointData(ExtractionPoint ep, string state, Vector3 position)
            {
                ExtractionPoint = ep;
                CachedState = state;
                CachedPosition = position;
            }
        }
        private static void UpdateLists()
        {
            UpdateExtractionPointList();
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

            // Removido o preenchimento de valuableObjects aqui, pois o Hax2 gerencia isso

            playerList.Clear();
            var players = SemiFunc.PlayerGetList();
            if (players != null)
            {
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        playerList.Add(player);
                    }
                }
            }

            lastUpdateTime = Time.time;
            Hax2.Log1($"Listas atualizadas: {enemyList.Count} inimigos, {valuableObjects.Count} itens, {playerList.Count} jogadores.");
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


        // DebugCheats.cs

        public static GameObject GetLocalPlayer()
        {
            if (PhotonNetwork.IsConnected)
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
                                    Hax2.Log1("Local player found via Photon: " + (gameObjectProperty.GetValue(player) as GameObject).name);
                                    return gameObjectProperty.GetValue(player) as GameObject;
                                }
                                Hax2.Log1("Local player found via PhotonView: " + photonView.gameObject.name);
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
                            Hax2.Log1("Local player found via Photon fallback: " + photonView.gameObject.name);
                            return photonView.gameObject;
                        }
                    }
                }
            }
            else
            {
                var players = SemiFunc.PlayerGetList();
                if (players != null && players.Count > 0)
                {
                    var player = players[0];
                    var gameObjectProperty = player.GetType().GetProperty("gameObject", BindingFlags.Public | BindingFlags.Instance);
                    if (gameObjectProperty != null)
                    {
                        var localPlayer = gameObjectProperty.GetValue(player) as GameObject;
                        Hax2.Log1("Local player found in singleplayer via PlayerGetList: " + localPlayer.name);
                        return localPlayer;
                    }
                }

                var playerAvatarType = Type.GetType("PlayerAvatar, Assembly-CSharp");
                if (playerAvatarType != null)
                {
                    var playerAvatar = GameHelper.FindObjectOfType(playerAvatarType) as MonoBehaviour;
                    if (playerAvatar != null)
                    {
                        Hax2.Log1("Local player found in singleplayer via PlayerAvatar: " + playerAvatar.gameObject.name);
                        return playerAvatar.gameObject;
                    }
                }

                var playerByTag = GameObject.FindWithTag("Player");
                if (playerByTag != null)
                {
                    Hax2.Log1("Local player found in singleplayer via tag 'Player': " + playerByTag.name);
                    return playerByTag;
                }

                Hax2.Log1("Nenhum jogador local encontrado no singleplayer!");
                return null;
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
                                            enemyList.Add(enemyInstance);
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

        private static void CreateBoundsEdges(Bounds bounds, Color color)
        {
            Vector3[] vertices = new Vector3[8];
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            vertices[0] = new Vector3(min.x, min.y, min.z); // Canto inferior esquerdo frontal
            vertices[1] = new Vector3(max.x, min.y, min.z); // Canto inferior direito frontal
            vertices[2] = new Vector3(max.x, min.y, max.z); // Canto inferior direito traseiro
            vertices[3] = new Vector3(min.x, min.y, max.z); // Canto inferior esquerdo traseiro
            vertices[4] = new Vector3(min.x, max.y, min.z); // Canto superior esquerdo frontal
            vertices[5] = new Vector3(max.x, max.y, min.z); // Canto superior direito frontal
            vertices[6] = new Vector3(max.x, max.y, max.z); // Canto superior direito traseiro
            vertices[7] = new Vector3(min.x, max.y, max.z); // Canto superior esquerdo traseiro

            Vector2[] screenVertices = new Vector2[8];
            bool isVisible = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3 screenPos = cachedCamera.WorldToScreenPoint(vertices[i]);
                if (screenPos.z > 0) isVisible = true;
                screenVertices[i] = new Vector2(screenPos.x * scaleX, Screen.height - (screenPos.y * scaleY));
            }

            if (!isVisible) return;

            DrawLine(screenVertices[0], screenVertices[1], color);
            DrawLine(screenVertices[1], screenVertices[2], color);
            DrawLine(screenVertices[2], screenVertices[3], color);
            DrawLine(screenVertices[3], screenVertices[0], color);

            DrawLine(screenVertices[4], screenVertices[5], color);
            DrawLine(screenVertices[5], screenVertices[6], color);
            DrawLine(screenVertices[6], screenVertices[7], color);
            DrawLine(screenVertices[7], screenVertices[4], color);

            DrawLine(screenVertices[0], screenVertices[4], color);
            DrawLine(screenVertices[1], screenVertices[5], color);
            DrawLine(screenVertices[2], screenVertices[6], color);
            DrawLine(screenVertices[3], screenVertices[7], color);
        }

        private static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            if (texture2 == null) return;

            float distance = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

            GUI.color = color;
            Matrix4x4 originalMatrix = GUI.matrix; // Salvar estado da GUI
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, distance, 1f), texture2);
            GUI.matrix = originalMatrix; // Restaurar estado da GUI
            GUI.color = Color.white;
        }

        private static Bounds GetActiveColliderBounds(GameObject obj)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            List<Collider> activeColliders = new List<Collider>();

            foreach (Collider col in colliders)
            {
                if (col.enabled && col.gameObject.activeInHierarchy)
                    activeColliders.Add(col);
            }

            if (activeColliders.Count == 0)
            {
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        if (renderers[i].enabled && renderers[i].gameObject.activeInHierarchy)
                            bounds.Encapsulate(renderers[i].bounds);
                    }
                    return bounds;
                }
                return new Bounds(obj.transform.position, Vector3.one * 0.5f);
            }

            Bounds resultBounds = activeColliders[0].bounds;
            for (int i = 1; i < activeColliders.Count; i++)
            {
                resultBounds.Encapsulate(activeColliders[i].bounds);
            }

            resultBounds.Expand(0.1f);
            return resultBounds;
        }
        public static void DrawESP()
        {
            if (!drawEspBool && !drawItemEspBool && !drawExtractionPointEspBool && !drawPlayerEspBool && !draw3DPlayerEspBool) return;

            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdatePlayerDataList();
                if (drawEspBool || drawItemEspBool || drawExtractionPointEspBool || drawPlayerEspBool || draw3DPlayerEspBool)
                {
                    UpdateLists();
                }
                UpdateLocalPlayer();
            }

            frameCounter++;
            if (frameCounter % 2 != 0) return;

            if (cachedCamera == null || cachedCamera != Camera.main)
            {
                cachedCamera = Camera.main;
                if (cachedCamera == null)
                {
                    Hax2.Log1("Camera.main não encontrada!");
                    return;
                }
            }

            scaleX = (float)Screen.width / cachedCamera.pixelWidth;
            scaleY = (float)Screen.height / cachedCamera.pixelHeight;

            // Enemy ESP
            if (drawEspBool)
            {
                GUIStyle enemyStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
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

                        float labelWidth = 100f;
                        float labelX = x - labelWidth / 2f;

                        var enemyParent = enemyInstance.GetComponentInParent(Type.GetType("EnemyParent, Assembly-CSharp"));
                        string enemyName = "Enemy";
                        if (enemyParent != null)
                        {
                            var nameField = enemyParent.GetType().GetField("enemyName", BindingFlags.Public | BindingFlags.Instance);
                            enemyName = nameField?.GetValue(enemyParent) as string ?? "Enemy";
                        }

                        string distanceText = "";
                        if (showEnemyDistance && localPlayer != null)
                        {
                            float distance2 = Vector3.Distance(localPlayer.transform.position, enemyInstance.transform.position);
                            distanceText = $" [{distance2:F1}m]";
                        }

                        string fullText = (showEnemyNames ? enemyName : "") + (showEnemyDistance ? distanceText : "");
                        if (string.IsNullOrEmpty(fullText)) continue;

                        float labelHeight = enemyStyle.CalcHeight(new GUIContent(fullText), labelWidth);
                        float labelY = y - height - labelHeight;

                        GUI.Label(new Rect(labelX, labelY, labelWidth, labelHeight), fullText, enemyStyle);
                    }
                }
            }

            // Item ESP
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
                        if (itemName.EndsWith("(Clone)", StringComparison.OrdinalIgnoreCase))
                        {
                            itemName = itemName.Substring(0, itemName.Length - "(Clone)".Length).Trim();
                        }

                        var valueField = valuableObject.GetType().GetField("dollarValueCurrent", BindingFlags.Public | BindingFlags.Instance);
                        int itemValue = valueField != null ? Convert.ToInt32(valueField.GetValue(valuableObject)) : 0;

                        string distanceText = "";
                        if (showItemDistance && localPlayer != null)
                        {
                            float distance = Vector3.Distance(localPlayer.transform.position, itemPosition);
                            distanceText = $" [{distance:F1}m]";
                        }

                        float labelWidth = 150f;
                        float valueLabelHeight = valueStyle.CalcHeight(new GUIContent(itemValue.ToString() + "$"), labelWidth);
                        float nameLabelHeight = nameStyle.CalcHeight(new GUIContent(itemName + distanceText), labelWidth);
                        float totalHeight = nameLabelHeight + valueLabelHeight + 5f;
                        float labelX = x - labelWidth / 2f;
                        float labelY = y - totalHeight - 5f;

                        string nameText = (showItemNames ? itemName : "") + (showItemDistance ? distanceText : "");
                        if (!string.IsNullOrEmpty(nameText))
                        {
                            GUI.Label(new Rect(labelX, labelY, labelWidth, nameLabelHeight), nameText, nameStyle);
                        }
                        if (showItemValue)
                        {
                            GUI.Label(new Rect(labelX, labelY + nameLabelHeight + 2f, labelWidth, valueLabelHeight), itemValue.ToString() + "$", valueStyle);
                        }

                        Bounds bounds = GetActiveColliderBounds(transform.gameObject);
                        CreateBoundsEdges(bounds, Color.yellow);
                    }
                }
            }

            // Extraction Point ESP
            if (drawExtractionPointEspBool)
            {
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    border = new RectOffset(1, 1, 1, 1)
                };

                GUIStyle valueStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                foreach (var epData in extractionPointList)
                {
                    if (epData.ExtractionPoint == null || !epData.ExtractionPoint.gameObject.activeInHierarchy) continue;

                    Vector3 screenPos = cachedCamera.WorldToScreenPoint(epData.CachedPosition);

                    if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
                    {
                        float x = screenPos.x * scaleX;
                        float y = Screen.height - (screenPos.y * scaleY);

                        string pointName = showExtractionNames ? "Extraction Point" : "";
                        string stateText = showExtractionNames ? $" ({epData.CachedState})" : "";
                        string distanceText = "";
                        if (showExtractionDistance && localPlayer != null)
                        {
                            distanceText = $"{Vector3.Distance(localPlayer.transform.position, epData.CachedPosition):F1}m";
                        }

                        nameStyle.normal.textColor = epData.CachedState == "Active" ? Color.green : (epData.CachedState == "Idle" ? Color.red : Color.cyan);

                        float labelWidth = 150f;
                        float valueLabelHeight = valueStyle.CalcHeight(new GUIContent(distanceText), labelWidth);
                        float nameLabelHeight = nameStyle.CalcHeight(new GUIContent(pointName + stateText), labelWidth);
                        float totalHeight = nameLabelHeight + (showExtractionDistance ? valueLabelHeight + 5f : 0f);
                        float labelX = x - labelWidth / 2f;
                        float labelY = y - totalHeight - 5f;

                        if (showExtractionNames)
                        {
                            GUI.Label(new Rect(labelX, labelY, labelWidth, nameLabelHeight), pointName + stateText, nameStyle);
                        }
                        if (showExtractionDistance && !string.IsNullOrEmpty(distanceText))
                        {
                            GUI.Label(new Rect(labelX, labelY + nameLabelHeight + 2f, labelWidth, valueLabelHeight), distanceText, valueStyle);
                        }
                    }
                }
            }

            // Player ESPif (drawPlayerEspBool || draw3DPlayerEspBool)
            {
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    border = new RectOffset(1, 1, 1, 1)
                };

                GUIStyle healthStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.green },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                GUIStyle distanceStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.yellow },
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                foreach (var playerData in playerDataList)
                {
                    if (playerData.PhotonView == null || playerData.PhotonView.IsMine || !playerData.Transform.gameObject.activeInHierarchy) continue;

                    Vector3 playerPos = playerData.Transform.position;
                    float distanceToPlayer = localPlayer != null ? Vector3.Distance(localPlayer.transform.position, playerPos) : float.MaxValue;
                    if (distanceToPlayer > maxEspDistance) continue;

                    Vector3 footPosition = playerPos;
                    float playerHeightEstimate = 2f;
                    Vector3 headPosition = playerPos + Vector3.up * playerHeightEstimate;

                    Vector3 screenFootPos = cachedCamera.WorldToScreenPoint(footPosition);
                    Vector3 screenHeadPos = cachedCamera.WorldToScreenPoint(headPosition);
                    bool isInFront = screenFootPos.z > 0 && screenHeadPos.z > 0;

                    float footX, footY, headY, width, height;
                    if (isInFront)
                    {
                        footX = screenFootPos.x * scaleX;
                        footY = Screen.height - (screenFootPos.y * scaleY);
                        headY = Screen.height - (screenHeadPos.y * scaleY);
                    }
                    else
                    {
                        // Projeta para a borda da tela se estiver atrás
                        Vector3 directionToPlayer = (playerPos - cachedCamera.transform.position).normalized;
                        Vector3 projectedPos = cachedCamera.transform.position + directionToPlayer * 5f; // Distância arbitrária para borda
                        Vector3 screenProjectedPos = cachedCamera.WorldToScreenPoint(projectedPos);
                        footX = Mathf.Clamp(screenProjectedPos.x * scaleX, 0, Screen.width);
                        footY = Mathf.Clamp(Screen.height - (screenProjectedPos.y * scaleY), 0, Screen.height);
                        headY = footY - 50f; // Aproximação para altura
                    }

                    height = Mathf.Abs(footY - headY);
                    float playerScale = playerData.Transform.localScale.y;
                    float baseWidth = playerScale * 200f;
                    width = (baseWidth / (distanceToPlayer + 1f)) * scaleX; // +1f para evitar divisão por zero
                    width = Mathf.Clamp(width, 30f, height * 1.2f);
                    height = Mathf.Clamp(height, 40f, 400f);

                    float x = footX;
                    float y = footY;

                    if (draw3DPlayerEspBool)
                    {
                        Bounds bounds = GetActiveColliderBounds(playerData.Transform.gameObject);
                        CreateBoundsEdges(bounds, Color.red);
                    }
                    else if (drawPlayerEspBool)
                    {
                        Box(x, y, width, height, texture2, 2f);
                    }


                    int health = playerHealthCache.ContainsKey(playerData.PhotonView.ViewID) ? playerHealthCache[playerData.PhotonView.ViewID] : 100;
                    string healthText = $"HP: {health}";
                    string distanceText = showPlayerDistance && localPlayer != null ? $"{distanceToPlayer:F1}m" : "";


                    float labelWidth = 150f;
                    float nameHeight = showPlayerNames ? nameStyle.CalcHeight(new GUIContent(playerData.Name), labelWidth) : 0f;
                    float healthHeight = healthStyle.CalcHeight(new GUIContent(healthText), labelWidth);
                    float distanceHeight = showPlayerDistance ? distanceStyle.CalcHeight(new GUIContent(distanceText), labelWidth) : 0f;

                    float totalHeight = nameHeight + healthHeight + (showPlayerDistance ? distanceHeight + 4f : 0f);
                    float labelX = x - labelWidth / 2f;
                    float labelY = y - height - totalHeight - 10f;

                    if (showPlayerNames)
                    {
                        GUI.Label(new Rect(labelX, labelY, labelWidth, nameHeight), playerData.Name, nameStyle);
                    }
                    if (showPlayerHP)
                    {
                        GUI.Label(new Rect(labelX, labelY + nameHeight + 2f, labelWidth, healthHeight), healthText, healthStyle);
                    }
                    if (showPlayerDistance && !string.IsNullOrEmpty(distanceText))
                    {
                        GUI.Label(new Rect(labelX, labelY + nameHeight + healthHeight + 4f, labelWidth, distanceHeight), distanceText, distanceStyle);
                    }
                }
            }
        }

        private static int GetPlayerHealth(object player)
        {
            try
            {
                var playerHealthField = player.GetType().GetField("playerHealth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerHealthField == null) return 100;

                var playerHealthInstance = playerHealthField.GetValue(player);
                if (playerHealthInstance == null) return 100;

                var healthField = playerHealthInstance.GetType().GetField("health", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (healthField == null) return 100;

                return (int)healthField.GetValue(playerHealthInstance);
            }
            catch (Exception e)
            {
                Hax2.Log1($"Erro ao obter vida do jogador: {e.Message}");
                return 100;
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