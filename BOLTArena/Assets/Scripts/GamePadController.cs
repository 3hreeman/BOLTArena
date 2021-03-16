using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamePadController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public GameObject padObj = null;
    public static GamePadController instance = null;
    private static string _resourcePath = "GamePad";
    
    public static void Instantiate() {
        var prefab = Resources.Load<GameObject>(_resourcePath);
        if (prefab == null) {
            Debug.Log(_resourcePath + " is not valid path.");
            return;
        }

        GameObject go = GameObject.Instantiate(prefab);
    }
    
    [SerializeField] 
    private RectTransform stick;
    private RectTransform rectTransform;

    [SerializeField, Range(10f, 150f)] 
    private float stickRange;

    public Vector2 inputVector;
    public bool isInput;
    private void Awake() {
        Debug.Log("GamePadController :: Awake");
        instance = this;
        padObj = transform.parent.gameObject;
        rectTransform = GetComponent<RectTransform>();
        DontDestroyOnLoad(padObj);
    }

    public TextMeshProUGUI txtGameTime;
    private void Update() {
        if (BoltNetwork.IsConnected) {
            txtGameTime.gameObject.SetActive(true);
            txtGameTime.text = NetworkInfo.GetLeftTime().ToString();
        }
        else {
            txtGameTime.gameObject.SetActive(false);
        }
    }

    public Action actionForBtnA;
    public Action actionForBtnB;

    public void AttachToCanvas(Transform canvas) {
        padObj.transform.SetParent(canvas, false);
        stickRange = rectTransform.sizeDelta.x / 2;
    }
    public void RegisterBtnAction(Action act_a, Action act_b) {
        stickRange = rectTransform.sizeDelta.x / 2;
        actionForBtnA = act_a;
        actionForBtnB = act_b;
    }

    public void DoActionA() {
        if (actionForBtnA != null) {
            actionForBtnA();
        }
    }

    public void DoActionB() {
        if (actionForBtnB != null) {
            actionForBtnB();
        }
    }

    void ControlStick(PointerEventData eventData) {
        var inputDir = eventData.position - rectTransform.anchoredPosition;
        var clampDir = inputDir.magnitude < stickRange ? inputDir : inputDir.normalized * stickRange;
        stick.anchoredPosition = clampDir;

        inputVector = clampDir.normalized;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        ControlStick(eventData);
        isInput = true;
    }

    public void OnDrag(PointerEventData eventData) {
        ControlStick(eventData);
    }

    
    public void OnEndDrag(PointerEventData eventData) {
        stick.anchoredPosition = Vector2.zero;
        inputVector = Vector2.zero;
        isInput = false;
    }

}
