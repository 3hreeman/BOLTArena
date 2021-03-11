using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMain : MonoBehaviour {
    private void Awake() {
        Debug.Log("Stage Main :: Awake");
        if (StartMain.IsInit == false) {
            SceneManager.LoadScene("Start");
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(2)) {
            Player.ServerPlayer.playerObject.TakeDmg(10);
        }
    }
}