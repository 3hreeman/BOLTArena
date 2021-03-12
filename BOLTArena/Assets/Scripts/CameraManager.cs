using System;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    private static CameraManager instance = null;
    public PlayerObject m_targetObject = null;

    private void Awake() {
        Debug.Log("-----------------------------------CameraManager :: Awake");
        instance = this;
    }

    private void Start() {
        Debug.Log("-----------------------------------CameraManager :: Start");
    }

    public static void SetTarget(PlayerObject target) {
        instance.m_targetObject = target;
    }

    public float MOVE_SPD = 3;
    
    private void Update() {
        if (m_targetObject != null) {
            var pos = Vector3.Lerp(transform.position, m_targetObject.transform.position, MOVE_SPD * Time.deltaTime);
            pos.z = -10;
            transform.position = pos;
        }
    }
}
