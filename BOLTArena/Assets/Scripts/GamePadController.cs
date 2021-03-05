using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamePadController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

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
        rectTransform = GetComponent<RectTransform>();
        stickRange = rectTransform.sizeDelta.x / 2;
        DontDestroyOnLoad(this);
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
