using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClickable
{
    GameObject getGameObject();
    void cursorEnter();
    void cursorExit();

    void leftClick();
    void deselect();
    bool checkedPrevItem(GameObject subject);
    
}
