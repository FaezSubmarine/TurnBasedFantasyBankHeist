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
    IClickable cursorItem;
    IClickable clickedItem;
    public Vector2 cursorPos { get; private set; }
    int numOfFloor;
    [SerializeField]int currentFloor;
    float floorHeight;

    [SerializeField]float decayTime;
    [SerializeField] float cappedSpeed;
    AccelerationFloat Xaccel, Zaccel;


    List<Transform> singleRend;

    RaycastHit[] oldHits;
    private void Awake()
    {
        cam = Camera.main;
        Xaccel = new AccelerationFloat(decayTime*Time.deltaTime,cappedSpeed);
        Zaccel = new AccelerationFloat(decayTime*Time.deltaTime,cappedSpeed);

        singleRend = new List<Transform>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Grid grid = FindObjectOfType<Grid>();
        numOfFloor = grid.numOfFloor;
        floorHeight = grid._floorHeight;
        if (currentFloor != 0)
        {
            transform.Translate(0, currentFloor * floorHeight, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Xaccel.addAcceleration(moveVec.x * moveSpeed);
        Zaccel.addAcceleration(moveVec.y * moveSpeed);
        transform.Translate(new Vector3(Xaccel.getFinalSpeed(),0, Zaccel.getFinalSpeed()));
        hovering();
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

        //todo: First, extract this bit out, then make it so its fading out and in
        if (oldHits != null && oldHits.SequenceEqual(hits) == false)
        {
            string oldHitsDebug = "oldHits: ";
            string newHitsDebug = "newHits ";
            foreach (RaycastHit oldbruh in oldHits)
            {
                oldHitsDebug += oldbruh.transform.name + " ";
            }
            foreach (RaycastHit newBruh in hits)
            {
                newHitsDebug += newBruh.transform.name + " ";
            }
            Debug.Log(oldHitsDebug + " " + newHitsDebug);
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
                    diffRend.enabled = true;
                }
            }
        }

        IClickable thisItem = null;
        foreach (RaycastHit oneHit in hits)
        {
            thisItem = oneHit.transform.GetComponent<IClickable>();
            if (thisItem != null)
            {
                if (cursorItem != null)
                {
                    cursorItem.cursorExit();
                    cursorItem = null;
                }
                thisItem = oneHit.transform.GetComponent<IClickable>();
                thisItem.cursorEnter();
                cursorItem = thisItem;
                continue;
            }
            else
            {
                thisItem = null;
            }
            Renderer rend = oneHit.transform.GetComponent<Renderer>();
            if (oneHit.transform.gameObject.layer != 9 && rend)
            {

                //singleRend.Add(rend.transform);
                if (rend.transform.childCount > 0)
                {
                    foreach(Transform child in rend.transform)
                    {
                        child.GetComponent<Renderer>().enabled = false;
                    }
                }
                rend.enabled = false;
            }
        }
        if (thisItem == null && cursorItem != null)
        {
            cursorItem.cursorExit();
            cursorItem = null;
        }
        //it does work but it flickers...
        //if (oldHits != null && oldHits.SequenceEqual(hits) == false)
        //{
        //    string oldHitsDebug = "oldHits: ";
        //    string newHitsDebug = "newHits ";
        //    foreach(RaycastHit oldbruh in oldHits)
        //    {
        //        oldHitsDebug += oldbruh.transform.name+" ";
        //    }
        //    foreach(RaycastHit newBruh in hits)
        //    {
        //        newHitsDebug += newBruh.transform.name+" ";
        //    }
        //    Debug.Log(oldHitsDebug + " " + newHitsDebug);
        //    IEnumerable<RaycastHit> diff = oldHits.Except(hits);

        //    foreach(RaycastHit diffHit in diff)
        //    {
        //        Renderer diffRend = diffHit.transform.GetComponent<Renderer>();
        //        if(diffHit.transform.gameObject.layer != 9 && diffRend)
        //        {
        //            if (diffRend.transform.childCount > 0)
        //            {
        //                foreach (Transform child in diffRend.transform)
        //                {
        //                    child.GetComponent<Renderer>().enabled = true;
        //                }
        //            }
        //            diffRend.enabled = true;
        //        }
        //    }
        //}
        oldHits = hits;
        return;



        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.GetComponent<IClickable>() != null)
            {
                if (cursorItem != null)
                {
                    cursorItem.cursorExit();
                    cursorItem = null;
                }
                cursorItem = hit.transform.GetComponent<IClickable>();
                cursorItem.cursorEnter();
            }
        }
        else
        {
            if (cursorItem != null)
            {
                cursorItem.cursorExit();
                cursorItem = null;
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
        if (clickedItem!= null && cursorItem != null && 
            clickedItem.checkedPrevItem(cursorItem.getGameObject()))
        {
            return;
        }
        if(clickedItem != null && cursorItem != clickedItem)
        {
            clickedItem.deselect();
        }
        if(cursorItem != null)
        {
            cursorItem.leftClick();
            clickedItem = cursorItem;
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
        if (currentFloor+deltaChange == Mathf.Clamp(currentFloor + deltaChange, 0, numOfFloor))
        {
            currentFloor += deltaChange;
            transform.Translate(0, deltaChange * floorHeight, 0);
        }
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