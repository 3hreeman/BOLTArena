using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ScreenInfo {
    public static int referenceWidth = 720;
    public static int referenceHeight = 1280;
    public static Vector2 screenSize = new Vector2(720, 1280);
    
    public static void SetScreenSize() {
        screenSize = new Vector2(Screen.width, Screen.height);
        Debug.Log("Set Screen Size "+screenSize.ToString());
    }
}
