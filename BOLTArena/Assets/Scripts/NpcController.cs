using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class NpcController : MonoBehaviour {
    private const float UPDATE_INTERVAL = .25f;
    public PlayerObject m_targetObject;
    public PlayerObject m_chaseObject;
    public float nextUpdateAt = 0;
    private void Start() {
        m_targetObject = GetComponent<PlayerObject>();
    }

    public float server_time;
    // Update is called once per frame
    void Update() {
        
        server_time = BoltNetwork.ServerTime;

        if (nextUpdateAt > server_time) {
            return;
        }
        nextUpdateAt = server_time + (m_chaseObject != null ? UPDATE_INTERVAL: UnityEngine.Random.Range(1, 5));
        // UpdateTarget();
        UpdateMove();
        UpdateAttack();
    }

    void UpdateTarget() {
        m_chaseObject = CombatManager.GetNearestPlayer(m_targetObject);
    }

    void UpdateMove() {
        Vector2 dir = Vector2.zero;
        if (m_targetObject.IsAlive()) {
            if (m_chaseObject != null) {
                dir = (m_chaseObject.transform.position - transform.position).normalized;
            }
            else {
                if (UnityEngine.Random.Range(0, 100) > 50) {
                    dir.x = UnityEngine.Random.Range(-1f, 1f);
                    dir.y = UnityEngine.Random.Range(-1f, 1f);
                }
                else {
                    dir.x = 0;
                    dir.y = 0;
                }
            }
        }
        m_targetObject.SetInputVector(dir);
    }

    void UpdateAttack() {
        if (m_chaseObject != null) {
            if (CombatManager.CheckTargetInRange(m_targetObject, m_chaseObject)) {
                m_targetObject.SetAttack(true);
            }
            else {
                m_targetObject.SetAttack(false);
            }
        }
        else {
            m_targetObject.SetAttack(false);
        }
    }
}
