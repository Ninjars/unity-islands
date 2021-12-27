using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class UIController : MonoBehaviour {
        public GameObject inGameCameraController;
        public MenuCameraController menuCameraController;
        public Text startGameButton;

        public void setSaveGameExists(bool exists) {
            // TODO
            // if (exists) {
            //     startGameButton.text = "Continue";
            // } else {
            //     startGameButton.text = "Start";
            // }
        }
        

        public void onMenuVisible() {
            // TODO
            // inGameCameraController.SetActive(false);
            // menuCameraController.enabled = true;
        }

        public void onMenuHidden() {
            // TODO
            // inGameCameraController.SetActive(true);
            // menuCameraController.enabled = false;
            // startGameButton.text = "Continue";
        }
    }
}
