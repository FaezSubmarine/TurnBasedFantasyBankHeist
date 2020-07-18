using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Node : MonoBehaviour, IHeapItem<Node>,IClickable {
	public Vector3 worldPosition;
	public int gridX;
	public int gridZ;
    public int floor;
	public float gCost;
	public float hCost;
	public Node parent;
    public bool walkable = true;
    [SerializeField] int priorityNum = 0;
    [SerializeField] Color unselectedColor, hoveredColor,unwalkableColor;
    public Renderer rend { get; private set; }

    List<Node> currentNeighbour = new List<Node>();

    bool persistentHighlight;
    int IClickable.getPriority()
    {
        return priorityNum;
    }
    public void setRendSettings(bool setting)
    {
        if (setting)
        {
            rend.enabled = true;
            if (rend.material.color == hoveredColor && !persistentHighlight)
            {
                rend.material.color = unselectedColor;
            }
        }
        else
        {
            rend.enabled = false;
        }
    }
    bool IClickable.comparePriority(IClickable other)
    {
        if (other == null) return true;
        //TODO: highlighting nodes at different height
        if (other is Node)
        {
            return transform.position.y > other.getGameObject().transform.position.y;
        }
        return priorityNum>= other.getPriority();
    }
    void Awake()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = unselectedColor;

    }
    public void setNode(Vector3 _worldPos, int _gridX, int _gridY,int floor, bool walkable) {
		worldPosition = _worldPos;
		gridX = _gridX;
		gridZ = _gridY;
        this.floor = floor;
        this.walkable = walkable;
        if (!walkable)
        {
            rend.material.color = unwalkableColor;
        }
	}

	public float fCost {
		get {
			return gCost + hCost;
		}
	}

    public int HeapIndex { get; set; }
    //todo:maybe make the material do emission?
    public void highlighter(bool persistentHighlight = false)
    {
        if(this.persistentHighlight && rend.material.color == hoveredColor)
        {
            return;
        }
        if(!walkable)
        {
            return;
        }
        if (!rend.enabled)
        {
            return;
        }
        this.persistentHighlight = persistentHighlight;
        rend.material.color = hoveredColor;

    }
    public void unhighlighter(bool persistentHighlight = false)
    {
        if(this.persistentHighlight != persistentHighlight)
        {
            return;
        }
        if (!walkable)
        {
            return;
        }
        //if (!rend.enabled)
        //{
        //    return;
        //}
        persistentHighlight = false;

        rend.material.color = unselectedColor;
    }
    void IClickable.cursorEnter()
    {
        highlighter();
    }
    void IClickable.cursorExit()
    {
        unhighlighter();
    }
    bool IClickable.checkedPrevItem(GameObject subject)
    {
        return false;
    }
    void IClickable.deselect()
    {
        foreach(Node node in currentNeighbour)
        {
            node.unhighlighter(true);
        }
    }
    void IClickable.leftClick()
    {
        lightUpNeighbours();
    }
    public void lightUpNeighbours()
    {
        if (currentNeighbour != null)
        {
            foreach (Node node in currentNeighbour)
            {
                node.unhighlighter(true);
            }
            currentNeighbour = null;
        }
        currentNeighbour = FindObjectOfType<Grid>().GetNeighbours(this);
    }

    GameObject IClickable.getGameObject()
    {
        return gameObject;
    }
    public int CompareTo(Node nodeToCompare) {
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0) {
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
