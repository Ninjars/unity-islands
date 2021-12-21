using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    [RequireComponent(typeof(InteractionController))]
    public class GameManager : MonoBehaviour {
        private string saveGameName = "sheepIsle";
        public SheepAgent sheepPrefab;
        public int initialSheepCount = 3;
        public float spawnRadius = 30;
        public GameObject inGameCameraController;
        public MenuCameraController menuCameraController;

        public Text startGameButton;

        public AmbienceController ambientSoundController;

        private InteractionController interactionController;
        private bool isSpring;

        void Start() {
            interactionController = GetComponent<InteractionController>();
            // GameEventMessage.AddListener((GameEventMessage message) => onGameMessage(message.EventName));
            if (SaveGameSystem.DoesSaveGameExist(saveGameName)) {
                var success = loadIsland();
                if (!success) {
                    Debug.Log("failed to load saved island");
                    spawnInitialSheep();
                    startGameButton.text = "Start";
                } else {
                    SheepAgent[] allSheep = FindObjectsOfType<SheepAgent>();
                    if (allSheep.Length == initialSheepCount) {
                        startGameButton.text = "Start";
                    } else {
                        startGameButton.text = "Continue";
                    }
                }

            } else {
                spawnInitialSheep();
                startGameButton.text = "Start";
            }
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

        private void spawnInitialSheep() {
            for (int i = 0; i < initialSheepCount; i++) {
                spawnSheep(0, -1);
            }
        }

        private void spawnSheep(int foodLevel, int voice) {
            var position = Game.Utils.RandomNavSphere(transform.position, spawnRadius, -1);
            var sheep = GameObject.Instantiate(sheepPrefab, position, UnityEngine.Random.rotation);
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
            spawnInitialSheep();
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
            inGameCameraController.SetActive(false);
            menuCameraController.enabled = true;
            interactionController.enabled = false;
        }

        public void onMenuHidden() {
            inGameCameraController.SetActive(true);
            menuCameraController.enabled = false;
            interactionController.enabled = true;
            startGameButton.text = "Continue";
        }

        #region save functions
        private bool loadIsland() {
            var saveGame = SaveGameSystem.LoadGame(saveGameName);
            if (saveGame == null) {
                return false;
            }

            try {
                foreach (var value in saveGame.sheepData) {
                    spawnSheep(value.level, value.voice);
                }
            } catch (NullReferenceException e) {
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
