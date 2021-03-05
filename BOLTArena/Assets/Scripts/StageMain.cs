using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageMain : MonoBehaviour {
    public static Dictionary<BoltConnection, Player> PlayerDict;

    private void Awake() {
        Debug.Log("Stage Main :: Awake");
        if (StartMain.IsInit == false) {
            SceneManager.LoadScene("Start");
        }
    }

    private void Start() {
        PlayerDict = new Dictionary<BoltConnection, Player>();
    }

    public static void AddPlayer(BoltConnection conn) {
        if (PlayerDict.ContainsKey(conn)) {
            return;
        }

        Debug.Log("StageMain :: New Player Added - " + conn.ConnectionId);
        PlayerDict.Add(conn, conn.GetPlayer());
    }
}