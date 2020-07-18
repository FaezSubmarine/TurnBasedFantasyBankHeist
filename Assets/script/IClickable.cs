using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClickable
{
    int getPriority();
    bool comparePriority(IClickable other);
    GameObject getGameObject();
    void cursorEnter();
    void cursorExit();

    void leftClick();
    void deselect();
    bool checkedPrevItem(GameObject subject);
}
