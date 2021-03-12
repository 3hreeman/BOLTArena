using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StageMain : MonoBehaviour {
    private void Awake() {
        Debug.Log("Stage Main :: Awake");
        if (StartMain.IsInit == false) {
            SceneManager.LoadScene("Start");
        }
    }

    private void Update() {
        if (BoltNetwork.IsServer) {
            if (Input.GetMouseButtonDown(2)) {
                var data = new PlayerData() {m_playerName = "NPC", m_resKey = "Enemy"};
                var npc_player = new Player(data);
                CombatManager.GeneratePlayerObject(npc_player, data, true);
            }            
        }
    }
}