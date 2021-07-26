using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [HideInInspector]
    public UIManager uiManager;

    public abstract void OnShow();
    public abstract void OnHide();
}
