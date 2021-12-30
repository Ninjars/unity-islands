using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game {
    interface WorldProvider {
        WorldData getWorldData();
    }

    [RequireComponent(typeof(InteractionController), typeof(UIController), typeof(WorldManager))]
    public class GameManager : MonoBehaviour {
        private string saveGameName = "SheepIslandsSave";

        [Header("World Generation Attributes")]
        [Tooltip("Used to form the world seed. Will use a random seed if no name is provided.")]
        public string worldName;
        [Tooltip("Square maximum dimension of the world")]
        public float size = 100;
        [Tooltip("Scaling value for island UVs")]
        [Range(0, 1)]
        public float uvScale = 0.25f;
        [Tooltip("Number of points used to generate the world graph")]
        public int pointCount = 250;
        [Tooltip("The lowest percentile of the generated world will be clipped")]
        [Range(0, 1)]
        public float clipPercentile = 0.5f;
        [Tooltip("Used to generate the world nav meshes")]
        public List<GameObject> validNavAgents;
        public Material topSideMaterial;
        public Material undersideSideMaterial;

        [Header("World Population Attributes")]
        public SheepAgent sheepPrefab;
        public int initialSheepCount = 3;
        public AmbienceController ambientSoundController;

        private InteractionController interactionController;
        private UIController uiController;
        private WorldManager worldManager;

        void Start() {
            interactionController = GetComponent<InteractionController>();
            uiController = GetComponent<UIController>();
            worldManager = GetComponent<WorldManager>();

            bool saveLoaded = false;
            if (SaveGameSystem.DoesSaveGameExist(saveGameName)) {
                Debug.Log("Loading save");
                saveLoaded = loadIsland();
                if (!saveLoaded) {
                    Debug.Log("failed to load saved island");
                }
            }

            if (!saveLoaded) {
                Debug.Log("Creating new island");
                initialiseWorld();
            }
            uiController.setSaveGameExists(saveLoaded);

            // TODO: port ui controls over from sheep isle
            // GameEventMessage.AddListener((GameEventMessage message) => onGameMessage(message.EventName));
        }

        private void onGameMessage(string message) {
            switch (message) {
                case "EXIT": {
                        onExitEvent();
                        break;
                    }
                case "SAVE": {
                        onSaveEvent();
                        break;
                    }
                case "RESET": {
                        onResetEvent();
                        break;
                    }
                case "MENU-VISIBLE": {
                        onMenuVisible();
                        break;
                    }
                case "MENU-HIDDEN": {
                        onMenuHidden();
                        break;
                    }
            }
        }

        private void Update() {
            if (Input.GetKey("escape")) {
                // GameEventManager.ProcessGameEvent(new GameEventMessage("MENU"));
            }
        }

        private void initialiseWorld() {
            int seed = worldName.Length == 0 ? Mathf.FloorToInt(UnityEngine.Random.value * int.MaxValue) : worldName.GetHashCode();

            WorldData world = worldManager.generateWorld(
                new WorldGenerator.WorldConfig(
                    worldName,
                    seed,
                    topSideMaterial,
                    undersideSideMaterial,
                    size,
                    uvScale,
                    pointCount,
                    clipPercentile,
                    validNavAgents
                )
            );

            Utils.RandomProvider gameRandom = new Utils.SeededRandomProvider(world.randomSeedValue);
            spawnInitialSheep(world, gameRandom);
        }

        private void spawnInitialSheep(WorldData world, Utils.RandomProvider random) {
            Debug.Log($"spawning {initialSheepCount} sheep");
            IslandData island = world.islands[0];
            Vector3 spawnOrigin = island.topsideBounds.center;
            float maxRadius = island.topsideBounds.max.magnitude * 0.75f;
            for (int i = 0; i < initialSheepCount; i++) {
                int attempts = 0;
                Vector3 position = Vector3.positiveInfinity;
                while (attempts < 5 && position.x == float.PositiveInfinity)  {
                    float spawnRadius = random.getFloat(maxRadius);
                    position = Utils.RandomUtils.RandomNavSphere(random, spawnOrigin, spawnRadius, -1);
                    attempts++;
                }
                if (position.x != float.PositiveInfinity) {
                    spawnSheep(random, position, 0, -1);
                }
            }
        }

        private void spawnSheep(Utils.RandomProvider random, Vector3 position, int foodLevel, int voice) {
            Debug.Log($"spawn position: {position}");
            var sheep = GameObject.Instantiate(sheepPrefab, position, UnityEngine.Random.rotation);
            sheep.random = random;
            sheep.foodEaten = foodLevel;
            if (voice >= 0) {
                sheep.setVoice(voice);
            }
        }

        public void onResetEvent() {
            SheepAgent[] allSheep = FindObjectsOfType<SheepAgent>();
            foreach (var sheep in allSheep) {
                GameObject.Destroy(sheep.gameObject);
            }
            Food[] allFood = FindObjectsOfType<Food>();
            foreach (var food in allFood) {
                GameObject.Destroy(food.gameObject);
            }
            
            initialiseWorld();
            onSaveEvent();
        }

        public void onSaveEvent() {
            var success = saveIsland();
            if (!success) {
                Debug.LogError("failed to save game!");
            } else {
                Debug.Log("updated save");
            }
        }

        public void onExitEvent() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void onMenuVisible() {
            uiController.onMenuVisible();
            interactionController.enabled = false;
        }

        public void onMenuHidden() {
            uiController.onMenuHidden();
            interactionController.enabled = true;
        }

        #region save functions
        private bool loadIsland() {
            // TODO: restore entire world from save
            var saveGame = SaveGameSystem.LoadGame(saveGameName);
            if (saveGame == null) {
                return false;
            }

            try {
                foreach (var value in saveGame.sheepData) {
                    // spawnSheep(value.level, value.voice);
                }
            } catch (NullReferenceException) {
                Debug.Log("invalid save file");
                SaveGameSystem.DeleteSaveGame(saveGameName);
                return false;
            }

            return true;
        }

        private bool saveIsland() {
            SheepAgent[] allSheep = FindObjectsOfType<SheepAgent>();
            SaveGame.SheepData[] sheepLevels = allSheep.Select(sheep => new SaveGame.SheepData(sheep.foodEaten, sheep.getVoice())).ToArray();
            SaveGame saveGame = new SaveGame();
            saveGame.sheepData = sheepLevels;
            return SaveGameSystem.SaveGame(saveGame, saveGameName);
        }

        #endregion
    }
}
