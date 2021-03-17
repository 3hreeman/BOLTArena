using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class StageMain : MonoBehaviour {

    private void Awake() {
        if (ServerMain.HeadlessMode == true) {
            return;
        }
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

    private void OnGUI() {
        var btnSize = ScreenInfo.screenSize.x / 10;
        Rect tex = new Rect(ScreenInfo.screenSize.x-btnSize, 0, btnSize, btnSize);
        GUILayout.BeginArea(tex);
        if (GUILayout.Button("Shutdown", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true))) {
            StageEnd();
        }
        GUILayout.EndArea();
    }

    public static void StageEnd() {
        BoltNetwork.Shutdown();
        CombatManager.ResetAll();
        CombatDmgFontObject.ResetAll();
        if (BoltNetwork.IsServer && ServerMain.HeadlessMode) {
            SceneManager.LoadScene("ServerStart");
        }
        else {
            SceneManager.LoadScene("Start");
        }
    }
}