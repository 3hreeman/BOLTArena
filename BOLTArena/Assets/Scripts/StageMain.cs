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
    
}