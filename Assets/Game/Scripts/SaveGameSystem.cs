using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Game {
    [Serializable]
    public class SaveGame {
        public SheepData[] sheepData;

        [Serializable]
        public class SheepData {
            public int level;
            public int voice;

            public SheepData(int level, int voice) {
                this.level = level;
                this.voice = voice;
            }
        }
    }

    public static class SaveGameSystem {
        public static bool SaveGame(SaveGame saveGame, string name) {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(GetSavePath(name), FileMode.Create)) {
                try {
                    formatter.Serialize(stream, saveGame);
                } catch (Exception) {
                    return false;
                }
            }

            return true;
        }

        public static SaveGame LoadGame(string name) {
            if (!DoesSaveGameExist(name)) {
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(GetSavePath(name), FileMode.Open)) {
                try {
                    return formatter.Deserialize(stream) as SaveGame;
                } catch (Exception) {
                    return null;
                }
            }
        }

        public static bool DeleteSaveGame(string name) {
            try {
                File.Delete(GetSavePath(name));
            } catch (Exception) {
                return false;
            }

            return true;
        }

        public static bool DoesSaveGameExist(string name) {
            return File.Exists(GetSavePath(name));
        }

        private static string GetSavePath(string name) {
            return Path.Combine(Application.persistentDataPath, name + ".sav");
        }
    }
}
