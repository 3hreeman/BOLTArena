using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScoreBoard : MonoBehaviour {
   public List<GameObject> slotObjList;
   public GameObject rankingNameObj;
   public GameObject slotObj;
   public TextMeshProUGUI btnShowTxt;
   public List<ScoreSlot> slotList = new List<ScoreSlot>();

   public bool IsShow = false;
   private void Start() {
      foreach (var obj in slotObjList) {
         slotList.Add(new ScoreSlot(obj));
      }
      ShowBoard();
   }

   public void ToggleBoard() {
      IsShow = !IsShow;
      ShowBoard();
   }
   void ShowBoard() {
      rankingNameObj.SetActive(IsShow);
      slotObj.SetActive(IsShow);
      btnShowTxt.text = IsShow ? "<" : ">";
   }

   public void Update() {
      if (IsShow == false) {
         return;
      }
      var scoreList = CombatManager.GetScoreBaseSortedPlayerList();
      for (int i = 0; i < slotList.Count; i++) {
         if (scoreList.Count-1 < i) {
            slotList[i].SetData(null);
         }
         else {
            slotList[i].SetData(scoreList[i]);
         }
      }
   }
}

public class ScoreSlot {
   public GameObject slot_obj;
   public TextMeshProUGUI txt_name;
   public TextMeshProUGUI txt_score;

   public ScoreSlot(GameObject go) {
      slot_obj = go;
      txt_name = go.transform.Find("txt_name").GetComponent<TextMeshProUGUI>();
      txt_score = go.transform.Find("txt_score").GetComponent<TextMeshProUGUI>();
   }

   public void SetData(PlayerObject player) {
      if (player == null) {
         txt_name.text = "";
         txt_score.text = "";
         return;
      }
      txt_name.text = player.playerName;
      txt_score.text = player.state.score.ToString();
   }
}
