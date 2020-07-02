using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

    //todo: now consider height
    Node startingNode;
    public List<Node> path { get; private set; }

    public Transform seeker, target;
	
	Grid grid;
    [SerializeField] Transform confirmedIndicator;
    public Transform _confirmedIndicator
    {
        get
        {
            return confirmedIndicator;
        }
        private set
        {
            confirmedIndicator = value;
        }
    }

    void Awake() {
		grid = GetComponent<Grid>();
        confirmedIndicator = Instantiate(confirmedIndicator);
        confirmedIndicator.gameObject.SetActive(false);
    }

    void Update() {
		//FindPath(seeker.position,target.position);
	}
    public void findPath(Node startNode, Node endNode)
    {
        path = new List<Node>();
        Heap<Node> openSet = new Heap<Node>(grid.maxSize());
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                //this is where the rejection began

                if (Mathf.Abs(neighbour.worldPosition.y - currentNode.worldPosition.y) > 2 || closedSet.Contains(neighbour) || !neighbour.walkable)
                {
                    continue;
                }

                float newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(currentNode);
        path.Reverse();
        //foreach(Node node in path)
        //{
        //    node.highlighter(true);
        //}
    }
    float GetDistance(Node nodeA, Node nodeB)
    {
        return Vector3.Distance(nodeA.worldPosition, nodeB.worldPosition);
        //int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        //int dstY = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

        //if (dstX > dstY)
        //    return 14 * dstY + 10 * (dstX - dstY);
        //return 14 * dstX + 10 * (dstY - dstX);
    }


}
