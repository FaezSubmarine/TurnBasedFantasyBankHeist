using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character : MonoBehaviour,IClickable
{
    [SerializeField]Renderer clickedIndicator;
    [SerializeField]Color hoveredColor, selectedColor;
    bool selected;
    Node currentNode;
    Pathfinding pathfinding;
    List<Node> path;
    bool currentlyMoving;
    [SerializeField] float moveSpeed;
    [SerializeField] int priorityNum =1;
    // Start is called before the first frame update
    int IClickable.getPriority()
    {
        return priorityNum;
    }
    bool IClickable.comparePriority(IClickable other)
    {
        if (other == null) return true;
        return priorityNum >= other.getPriority();
    }
    private void Awake()
    {
        path = new List<Node>();
        clickedIndicator = Instantiate(clickedIndicator, transform.position + new Vector3(0, 3), clickedIndicator.transform.rotation, transform).GetComponent<Renderer>();
        clickedIndicator.gameObject.SetActive(false);

        pathfinding = FindObjectOfType<Pathfinding>();

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }
    public void settingRend(bool setting)
    {
        foreach (SkinnedMeshRenderer eachRend in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            eachRend.enabled = setting;
        }
    }
    Node getCurrentNode()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position+new Vector3(0,2), Vector3.down, out hit, 10, 1 << 5))
        {
            return hit.transform.GetComponent<Node>();
        }
        return null;
    }
    //todo:now disable the confirmed indicator and turn off each node as the character covers it
    IEnumerator movingOnPath()
    {
        currentlyMoving = true;
        pathfinding._confirmedIndicator.gameObject.SetActive(false);
        for(int i = 0;i<path.Count-1;++i)
        {
            Vector3 currentPos = path[i].worldPosition;
            Vector3 nextPos = path[i + 1].worldPosition;
            float lerpNum = 0;
            while(lerpNum != 1)
            {
                lerpNum = Mathf.Min(lerpNum+Time.deltaTime*moveSpeed, 1);
                transform.position = Vector3.Lerp(currentPos, nextPos, lerpNum);
                yield return null;
            }
            path[i].unhighlighter(true);
        }
        path[path.Count - 1].unhighlighter(true);
        path.Clear();
        currentlyMoving = false;
    }
    void IClickable.leftClick()
    {
        selected = true;
        clickedIndicator.material.color = selectedColor;
    }
    void IClickable.cursorEnter()
    {
        if (!selected)
        {
            clickedIndicator.material.color = hoveredColor;
        }
        clickedIndicator.gameObject.SetActive(true);
    }
    void IClickable.cursorExit()
    {
        if(!selected)
            clickedIndicator.gameObject.SetActive(false);
    }
    bool IClickable.checkedPrevItem(GameObject subject)
    {
        if (!selected) return false;
        if (currentlyMoving) return true;
        Node node = subject.GetComponent<Node>();
        if (node == null) return false;
        if(node == currentNode)
        {
            StartCoroutine(movingOnPath());
            return true;
        }
        pathfinding._confirmedIndicator.gameObject.SetActive(true);
        pathfinding._confirmedIndicator.position = node.worldPosition + new Vector3(0, 3);
        if(node)
        {
            pathfinding.findPath(getCurrentNode(), node);
            //node.lightUpNeighbours();
            if (path.Count > 0)
            {
                foreach (Node prevNode in path)
                {
                    prevNode.unhighlighter(true);
                }
                path.Clear();
            }
            foreach (Node childNode in pathfinding.path)
            {
                childNode.highlighter(true);
            }
            path = pathfinding.path;
            currentNode = node;
            return true;
        }
        return false;
    }
    void IClickable.deselect()
    {
        if (currentlyMoving) return;
        selected = false;
        clickedIndicator.material.color = hoveredColor;
        clickedIndicator.gameObject.SetActive(false);
        currentNode = null;
        foreach(Node pathNode in path)
        {
            pathNode.unhighlighter(true);
        }
        path.Clear();
        currentNode = null;
        pathfinding._confirmedIndicator.gameObject.SetActive(false);
    }

    GameObject IClickable.getGameObject()
    {
        return gameObject;
    }
}
