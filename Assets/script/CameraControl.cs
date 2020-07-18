using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
public class CameraControl : MonoBehaviour
{
    //todo: Add elevation, give acceleration to the moving, make the object disappear if got in the way
    float addedRotation = 0;
    [SerializeField]float zoomSpeed = 30;
    Coroutine movingCam;
    Vector2 moveVec;
    [SerializeField]float moveSpeed;
    [SerializeField] float rotateSpeed = 10;
    public Camera cam { get; private set; }
    IClickable oldCursorItem;
    IClickable clickedItem;
    public Vector2 cursorPos { get; private set; }
    Vector2 oldCursorPos;
    [SerializeField] float maxDeltaChange = 0.01f;
    //int numOfFloor;
    [SerializeField]int currentFloor;
    int prevFloor = 0;
    //float floorHeight;
    //todo: disable nodes on other floors
    [SerializeField]float decayTime;
    [SerializeField] float cappedSpeed;
    AccelerationFloat Xaccel, Zaccel;


    List<Transform> singleRend;

    RaycastHit[] oldHits;
    RaycastHit[] oldFloorHits;

    Grid grid;
    private void Awake()
    {
        cam = Camera.main;
        Xaccel = new AccelerationFloat(decayTime*Time.deltaTime,cappedSpeed);
        Zaccel = new AccelerationFloat(decayTime*Time.deltaTime,cappedSpeed);

        singleRend = new List<Transform>();
        grid = FindObjectOfType<Grid>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (currentFloor != 0)
        {
            transform.Translate(0, currentFloor * grid._floorHeight, 0);
        }
        changeCurrentFloorFade();
    }
    //TODO: the camera is attach to the character
    private void changeCurrentFloorFade()
    {
        for(int i = currentFloor; i < grid.numOfFloor; ++i)
        {
            foreach (Node floorNode in grid.floorNodeDict[i])
            {
                floorNode.setRendSettings(false);
            }
        }
        foreach (Node floorNode in grid.floorNodeDict[currentFloor])
        {
            floorNode.setRendSettings(true);
        }
        prevFloor = currentFloor;
    }

    // Update is called once per frame
    void Update()
    {
        Xaccel.addAcceleration(moveVec.x * moveSpeed);
        Zaccel.addAcceleration(moveVec.y * moveSpeed);
        transform.Translate(new Vector3(Xaccel.getFinalSpeed(),0, Zaccel.getFinalSpeed()));
        if (Vector2.SqrMagnitude(cursorPos - oldCursorPos) >maxDeltaChange*maxDeltaChange)
        {
            hovering();
        }
        oldCursorPos = cursorPos;
    }

    public void zoomAction(InputAction.CallbackContext context)
    {
        float contextFloat = -Mathf.Clamp(context.ReadValue<float>(), -1, 1);
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView+ contextFloat * zoomSpeed, 10, 120);
    }
    public void rotateCam(InputAction.CallbackContext context)
    {
        if (movingCam == null)
        {
            movingCam = StartCoroutine(rotateAction(transform.rotation.eulerAngles, -90 * context.ReadValue<float>()));
        }
    }
    void hovering()
    {
        Ray ray = cam.ScreenPointToRay(cursorPos);
        Debug.DrawRay(ray.origin, ray.direction*300, Color.blue);
        //todo:50 is a magic number, but how to measure the correct distance?
        RaycastHit[] hits = Physics.RaycastAll(ray, 300);
        
        //OBJECT FADE BACK INTO EXISTENCE
        if (oldHits != null && oldHits.SequenceEqual(hits) == false)
        {
            fadeObjectsIn(hits);
        }
        //CLICKABLE STUFF
        IClickable newCursorItem = null;
        foreach (RaycastHit oneHit in hits)
        {
            IClickable thisItem = oneHit.transform.GetComponent<IClickable>();
            if (thisItem != null && thisItem.comparePriority(newCursorItem))
            {
                newCursorItem = thisItem;
            }

            Renderer rend = oneHit.transform.GetComponent<Renderer>();
            if (thisItem == null && oneHit.transform.gameObject.layer != 9 && rend)
            {
                fadeObjectsOut(rend);
            }
            else if (thisItem == null && oneHit.transform.gameObject.layer == 9)
            {
                fadeForFloor(oneHit);
            }
        }
        if( newCursorItem != null && oldCursorItem != newCursorItem) {
            if (oldCursorItem != null)
            {
                oldCursorItem.cursorExit();
            }
            newCursorItem.cursorEnter();
            oldCursorItem = newCursorItem;
        }
        if (newCursorItem == null && oldCursorItem != null)
        {
            oldCursorItem.cursorExit();
            oldCursorItem = null;
        }
        oldHits = hits;
    }

    //todo: CHANGE IT SO THAT IF THE CAMERA IS THE ONE THAT MAKES IT FADE
    private void fadeForFloor(RaycastHit oneHit)
    {
        Vector3 testPoint = oneHit.point;
        testPoint.y = (currentFloor + 0.5f) * grid._floorHeight;
        Ray floorRayUp = new Ray(testPoint, Vector3.up);
        //Debug.DrawRay(floorRayUp.origin, floorRayUp.direction,Color.blue);
        RaycastHit[] testForFloorUp = Physics.RaycastAll(floorRayUp, 100, 1 << 9 | 1<<8);

        if (oldFloorHits != null && oldFloorHits.SequenceEqual(testForFloorUp) == false)
        {
            IEnumerable<RaycastHit> diffFloor = oldFloorHits.Except(testForFloorUp);
            foreach (RaycastHit oneDiffFloor in diffFloor)
            {
                switch (oneDiffFloor.transform.gameObject.layer)
                {
                    case 9:
                        oneDiffFloor.transform.GetComponent<Renderer>().enabled = true;
                        break;
                    case 8:
                        oneDiffFloor.transform.GetComponent<character>().settingRend(true);
                        break;
                }
            }
        }
        foreach (RaycastHit floorHit in testForFloorUp)
        {
            switch (floorHit.transform.gameObject.layer)
            {
                case 9:
                    floorHit.transform.GetComponent<Renderer>().enabled = false;
                    break;
                case 8:
                    floorHit.transform.GetComponent<character>().settingRend(false);
                    break;
            }
        }
        oldFloorHits = testForFloorUp;
    }

    private void fadeObjectsOut(Renderer rend)
    {
        if (rend.transform.childCount > 0)
        {
            foreach (Transform child in rend.transform)
            {
                child.GetComponent<Renderer>().enabled = false;
            }
        }
        rend.enabled = false;
    }

    private void fadeObjectsIn(RaycastHit[] hits)
    {
        IEnumerable<RaycastHit> diff = oldHits.Except(hits);

        foreach (RaycastHit diffHit in diff)
        {
            Renderer diffRend = diffHit.transform.GetComponent<Renderer>();
            if (diffHit.transform.gameObject.layer != 9 && diffRend)
            {
                if (diffRend.transform.childCount > 0)
                {
                    foreach (Transform child in diffRend.transform)
                    {
                        child.GetComponent<Renderer>().enabled = true;
                    }
                }
                if (diffRend.GetComponent<IClickable>() == null)
                    diffRend.enabled = true;
            }
        }
    }

    public void moveCam(InputAction.CallbackContext context)
    {
        moveVec = context.ReadValue<Vector2>();

    }
    public void settingCursorPos(InputAction.CallbackContext context)
    {
        cursorPos = context.ReadValue<Vector2>();
    }
    public void leftClicking(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Started)
        {
            return;
        }
        if (clickedItem!= null && oldCursorItem != null && 
            clickedItem.checkedPrevItem(oldCursorItem.getGameObject()))
        {
            return;
        }
        if(clickedItem != null && oldCursorItem != clickedItem)
        {
            clickedItem.deselect();
        }
        if(oldCursorItem != null)
        {
            oldCursorItem.leftClick();
            clickedItem = oldCursorItem;
        }
    }
    float getSinResult(float x)
    {
        float PI = Mathf.PI;
        return (Mathf.Sin(PI * x - (PI / 2)) + 1) / 2;
    }
    public void rightClicking(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Started || clickedItem == null)
        {
            return;
        }
        clickedItem.deselect();
    }
    public void elevating(InputAction.CallbackContext context)
    {
        if(context.phase != InputActionPhase.Started)
        {
            return;
        }
        int deltaChange = (int)context.ReadValue<float>();
        //Debug.Log("delta change " + deltaChange);
        if (currentFloor+deltaChange == Mathf.Clamp(currentFloor + deltaChange, 0, grid.numOfFloor))
        {
            currentFloor += deltaChange;
            transform.Translate(0, deltaChange * grid._floorHeight, 0);
        }
        changeCurrentFloorFade();
    }
    IEnumerator rotateAction(Vector3 ogRot, float yAdd)
    {
        Quaternion origin = Quaternion.Euler(ogRot);
        Quaternion target = Quaternion.Euler(ogRot + new Vector3(0, yAdd, 0));
        float t = 0;
        while(t != 1)
        {
            t = Mathf.Min(t + Time.deltaTime*rotateSpeed, 1);
            transform.rotation = Quaternion.Slerp(origin, target, getSinResult(t));
            yield return null;
        }
        movingCam = null;
    }
}
//todo: fuck it, leave it alone for now
public class AccelerationFloat
{
    float decayTime;
    float finalSpeed;
    float cappedSpeed;
    float deltaSpeed;
    float nonZeroDeltaSpeed;
    float lastSpeed;
    public AccelerationFloat()
    {
    }
    public AccelerationFloat(float decayTime,float cappedSpeed)
    {
        //todo: get decceleration from time and remaining speed
        this.decayTime = decayTime;
        this.cappedSpeed = cappedSpeed;
    }
    float lerp = 0;
    float getDeccel()
    {
        return nonZeroDeltaSpeed* decayTime;
    }
    public float getFinalSpeed()
    {
        //if (added)
        //{
        //    added = false;
        //    return finalSpeed;
        //}
        if (deltaSpeed != 0)
        {
            lerp = 0;
            lastSpeed = finalSpeed;

            return finalSpeed;
        }
        return 0;
        //lerp = Mathf.Clamp(lerp+ Mathf.Abs(lastSpeed)*decayTime*Time.deltaTime,0,1);
        //finalSpeed = Mathf.Lerp(lastSpeed, 0, lerp);
        //if (finalSpeed > Mathf.Epsilon)
        //{
        //    finalSpeed = Mathf.Min(finalSpeed + getDeccel(), 0);
        //}else if (finalSpeed < Mathf.Epsilon)
        //{
        //    finalSpeed = Mathf.Max(finalSpeed + getDeccel(), 0);

        //}
        //Debug.Log(finalSpeed);
        //return finalSpeed;
    }
    public void addAcceleration(float deltaSpeed)
    {
        if (deltaSpeed != 0)
        {
            nonZeroDeltaSpeed = deltaSpeed;
        }
        this.deltaSpeed = deltaSpeed;
        finalSpeed = Mathf.Clamp(finalSpeed+deltaSpeed,-cappedSpeed,cappedSpeed);
    }
}