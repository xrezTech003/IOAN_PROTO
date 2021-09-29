using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

/**
<summary>
    CD : DraggableObject
    Have gameObject act differently based on holding status
</summary>
**/
public class DraggableObject /// <remarks>Not Derived from MonoBehaviour! NDH:: Confirmed Unnecessary, but maybe a good starting point for an update?</remarks>
{
    /// <summary>
    ///     ENUM : DragState
    ///     Values : FREE, LERP, ATTACHED, FREEING
    ///     <remarks>
    ///         not being dragged, grabbed and lerping to attach point, grabbed attached to controller, dropped but rotateing back to correct/standard orientation
    ///     </remarks>
    /// </summary>
    public enum DragState { FREE, LERP, ATTACHED, FREEING };

    #region PUBLIC_VAR
    public OVRManager ovrm;
    /// <summary>
    ///     VD : dragState
    ///     Current DragState
    /// </summary>
    public DragState dragState;

    /// <summary>
    ///     VD : holdingController
    ///     Object for holding Controller
    /// </summary>
    public GameObject holdingController;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///     VD : obj
    ///     Set at construction
    /// </summary>
    GameObject obj;

    /// <summary>
    ///     VD : lerper
    ///     set at construction
    /// </summary>
    Lerper lerper;
    #endregion

    #region PUBLIC_FUNC
    /**
    <summary>
        CO : DraggableObject(GameObject, float)
        <param name="obj"/>
        <param name="lerpDuration"/>
    </summary>
    **/
    public DraggableObject(GameObject obj, float lerpDuration)
    {
        this.obj = obj;
        lerper = new Lerper(lerpDuration);
    }

    /**
    <summary>
        FD : grabObject(GameObject)
        Sets v:holdingController to controller
        Sets v:dragState to FREE
        <param name="controller">VD: v:holdingControlller</param>
    </summary>
    **/
    public void grabObject(GameObject controller)
    {
        holdingController = controller;
        dragState = DragState.FREE;
    }

    /**
    <summary>
        FD : Update()
        If v:holdingController is null
            If v:dragState is freeing
                If update time is greater than 1 set v:dragState to free
                set v:obj rotation to lerper rotation
        Else
            If the hair trigger is up on controller
                Set v:holdingController to null
                if v:dragState is either lerp or attached
                    set v:dragState to freeing
                    set lerper start time and rotations
            Switching based on current v:dragState
                If free and if v:holdingController isn't null
                    Set v:dragState to lerp
                    Set lerper start time, points, and rotations
                If lerp
                    Set v:dragState attached if lerper update time is greater than 1
                    Set v:obj transform data to lerper data
                If attached 
                    Set v:obj transform data to attach vars                    
    </summary>
    **/
    public void update()
    {
        // if not being held
        if (holdingController == null)
        {
            // if the contoller is null you can only be in free or freeing
            // if you are in free there is nothing to do
            if (dragState == DragState.FREEING)
            {
                if (lerper.update(Time.time) >= 1) dragState = DragState.FREE;

                obj.transform.rotation = lerper.getRotation();
            }
        }
        else
        {
            Vector3 attachPoint = holdingController.transform.TransformPoint(new Vector3(0f, 0f, 0.1f));
            Quaternion attachRotation = holdingController.transform.rotation;
            SteamVR_Controller.Device controller = SteamVR_Controller.Input((int)holdingController.GetComponent<SteamVR_TrackedObject>().index);

            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger) || controller.GetHairTriggerUp())
            {
                // if dropped
                holdingController = null;
                if ((dragState == DragState.LERP) || (dragState == DragState.ATTACHED))
                {
                    // it really only should be in LERP or ATTACHED if dropped
                    dragState = DragState.FREEING;
                    lerper.setStartTime(Time.time);
                    lerper.setInterpRotations(obj.transform.rotation, Quaternion.identity);
                }
            }

            switch (dragState)
            {
                case DragState.FREE:
                    {
                        if (holdingController != null)
                        {
                            // just picked up
                            dragState = DragState.LERP;
                            lerper.setStartTime(Time.time);
                            lerper.setInterpPoints(obj.transform.position, attachPoint);
                            lerper.setInterpRotations(obj.transform.rotation, attachRotation);
                        }
                    }
                    break;
                case DragState.LERP:
                    // it is impossible to be here an not held
                    if (lerper.update(Time.time) >= 1) dragState = DragState.ATTACHED;
                    lerper.updateGoal(attachPoint, attachRotation);
                    obj.transform.position = lerper.getPoint();
                    obj.transform.rotation = lerper.getRotation();
                    break;
                case DragState.ATTACHED:
                    // it is impossible to be here an not held
                    obj.transform.position = attachPoint;
                    obj.transform.rotation = attachRotation;
                    break;
                case DragState.FREEING:
                    // can't be in freeing if holdingController is not null
                    Debug.Log("dragState can not be FREEING if holdingController is not null");
                    break;
            }

        }
    }
    #endregion
}
