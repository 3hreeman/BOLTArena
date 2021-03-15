using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StageMain : MonoBehaviour {
    private void Awake() {
        if (StartMain.IsInit == false) {
            SceneManager.LoadScene("Start");
        }
    }

    private void Update() {
        if (BoltNetwork.IsServer) {
            if (Input.GetMouseButtonDown(2)) {
                CombatManager.GenerateNpc(5);
            }            
        }
    }
}