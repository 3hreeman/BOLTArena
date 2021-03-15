using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgFontSubObject : MonoBehaviour
{
    public CombatDmgFontObject dmgFont;

    public void OnAnimationEnd() {
        dmgFont.OnAnimationEnd();
    }
}