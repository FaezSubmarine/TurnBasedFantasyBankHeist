using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class Grid : MonoBehaviour {
	public LayerMask unwalkableMask;
	[SerializeField] Vector3 gridWorldSize;
	public float nodeRadius;
    Node[,] grid;

    float nodeDiameter;
	int gridSizeX,gridSizeZ;
    [SerializeField]float floorHeight;
    public float _floorHeight
    {
        get
        {
            return floorHeight;
        }
    }
    [SerializeField] Transform gridMat;
    public int numOfFloor { get; private set; }
    CameraControl cameraControl;
    List<Node> currentNeighbour = new List<Node>();
	void Awake() {
        cameraControl = FindObjectOfType<CameraControl>();
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.z/nodeDiameter);
        numOfFloor =(int) (gridWorldSize.y / floorHeight);
		CreateGrid();
	}
    public int maxSize()
    {
        return gridSizeX * numOfFloor * gridSizeZ* numOfFloor;
    }
    //todo: update grid based on environment moving?
    bool checkForWalkable(ref Vector3 worldPoint, float floorHeight)
    {
        Ray ray = new Ray(worldPoint, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, floorHeight, 1<<9);
        foreach (RaycastHit hit in hits)
        {
            worldPoint.y = hit.point.y + 0.05f;
            return true;
        }
        return false;
    }
	void CreateGrid() {
        int currentFloor = 0;
        grid = new Node[gridSizeX*numOfFloor, gridSizeZ*numOfFloor];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.z/2 - Vector3.up*gridWorldSize.y/2;
        for (float y = floorHeight; y <= gridWorldSize.y; y += floorHeight)
        {
            ++currentFloor;
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (z * nodeDiameter + nodeRadius) + Vector3.up * y;

                    if (!checkForWalkable(ref worldPoint, floorHeight))
                    {
                        continue;
                    }
                    bool walkable = !(Physics.CheckBox(worldPoint, new Vector3(nodeRadius / 4 - 0.1f, 0.03f, nodeRadius / 4 - 0.1f), Quaternion.identity, ~(1 << 5 | 1 << 8)));
                    
                    //todo: leave the node inside walkable to be red or delete entirely? Can walkable be edited at all for destructible terrain?
                     grid[x * currentFloor, z * currentFloor] = Instantiate(gridMat, worldPoint, gridMat.rotation, transform).GetComponent<Node>();
                     grid[x * currentFloor, z * currentFloor].setNode(worldPoint, x, z, currentFloor,walkable);
                }
            }
        }

	}

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0)
                    continue;

                for(int y = -1; y <= 1; ++y)
                {
                    int checkX = node.gridX + x;
                    int checkZ = node.gridZ + z;

                    if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                    {
                        if (node.floor + y <= 0) continue;
                        if (node.floor + y > numOfFloor) continue;
                        checkX *= node.floor+y;
                        checkZ *= node.floor+y;
                        if (grid[checkX, checkZ] == null) continue;
                        if (grid[checkX, checkZ].floor != node.floor + y) continue;
                        if (grid[checkX, checkZ] != null)
                        {
                            neighbours.Add(grid[checkX, checkZ]);
                        }
                    }
                }

            }
        }
        return neighbours;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, gridWorldSize.z));
    }
}