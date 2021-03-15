using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour {
    public GameObject m_hpObj;
    public GameObject m_nameObj;
    public GameObject m_killBoardObj;
    
    public List<GameObject> medalObjList = new List<GameObject>();

    public TextMeshProUGUI m_textPlayerName;
    public Slider m_playerHp;
    public Image m_hpImg;
    
    public PlayerObject targetUnit;

    public void Start() {
        transform.SetParent(GameObject.FindWithTag("StageCanvas").transform);
    }

    public void init(string name, bool isController, bool isNpc) {
        m_textPlayerName.text = name;
        m_hpImg.color = isController ? Color.green : (isNpc ? Color.red : Color.blue);
        gameObject.SetActive(false);
    }

    private void Update() {
        transform.position = targetUnit.GetCanvasPosition();
        bool targetAliveState = targetUnit.IsAlive();
        m_hpObj.SetActive(targetAliveState);
        m_nameObj.SetActive(targetAliveState);
        m_killBoardObj.SetActive(targetAliveState);

        if (targetAliveState) {
            m_playerHp.value = targetUnit.GetCurHpRatio();
        }
    }
}
