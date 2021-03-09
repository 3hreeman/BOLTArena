using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour {
    public float m_nextActionTime = 0;
    public GameObject m_chaseTarget = null;

    public PlayerObject m_targetObject;
    
    private void Start() {
        m_targetObject = GetComponent<PlayerObject>();
    }

    // Update is called once per frame
    void Update() {
        UpdateMove();
    }

    void UpdateTarget() {
        
    }

    void UpdateMove() {
        if (m_chaseTarget != null) {
            
        }
        else {
            
        }
    }
}
