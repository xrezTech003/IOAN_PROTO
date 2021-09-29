using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// This class generates a phudo physics animation for tapped active starts.  It gets initiation messages from TapActivatedStar
/// </summary>
public class ActiveStarPhysics : NetworkBehaviour
{
    private delegate void UpdateEvent();
    private UpdateEvent update;

    #region PUBLIC_VAR
    [HelpBox(
        "Imagine the star is attached to a spring:\n" +
        "Length - extended length of spring\n" +
        "Mass   - mass of star\n" +
        "B      - damping constant for spring",
    messageType = HelpBoxMessageType.Info)]

    /// <summary>
    /// Maximum displacement for star from origin
    /// </summary>
    public float length = 2.5f;

    /// <summary>
    /// Mass of the star
    /// </summary>
    public float mass = 2.0f;

    /// <summary>
    /// Damping Modifyer for friction
    /// </summary>
    [Tooltip(
        "-1 < b < 0 => overdamped w/ oscillation\n" +
        "b <= -1    => underdamped\n" +
        "b >=  0    => overdamped w/o oscillations")]
    public float b = -0.625f;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    /// Activated Star Script
    /// </summary>
    private activatedStarScript activatedStar;

    /// <summary>
    /// Rigidbody Script
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Position of the anchor
    /// </summary>
    private Vector3 anchorPosition;

    /// <summary>
    /// Current Velocity
    /// </summary>
    private Vector3 velocity;

    /// <summary>
    /// Current Acceleration
    /// </summary>
    private Vector3 acceleration;

    /// <summary>
    /// Diameter of spring in meters
    /// </summary>
    private readonly float crossSectionDiameter = 0.25f;

    /// <summary>
    /// Young's modulus constant for x
    /// </summary>
    private readonly float youngsModulus = 32f; //Bismuth

    /// <summary>
    /// Spring Constant for Star
    /// </summary>
    private float k;
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// Initialize private values
    /// </summary>
    private void Start()
    {
        activatedStar = GetComponent<activatedStarScript>();
        rb = GetComponent<Rigidbody>();

        anchorPosition = transform.position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;

        k = (youngsModulus * (Mathf.PI * crossSectionDiameter)) / length;
        b = (b + 1) * Mathf.Sqrt(4 * mass * k);

        update = AnchorStar;
    }

    /// <summary>
    /// This zeroes out any rigid body physics 
    /// </summary>
    private void FixedUpdate()
    {
        rb.velocity = new Vector3(0, 0, 0);
        rb.angularVelocity = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Does Physics
    /// </summary>
    private void Update()
    {
        if (!isClient) return; //Only Calculate in clients

        update?.Invoke();
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// This is called to initiate the physics animation. Sends the event to the server 
    /// </summary>
    /// <param name="fullVelocity">velocity of the tap</param>
    public void Initiate(Vector3 fullVelocity)
    {
        if (isClient) Cmd_initiate(fullVelocity);
        else Rpc_initiate(fullVelocity);
    }

    /// <summary>
    /// Called to set the anchor of star
    /// </summary>
    /// <param name="b"></param>
    public void SetAnchor(bool b)
    {
        if (isClient) CmdAnchor(b);
        else RpcAnchor(b);
    }
    #endregion

    #region PRIVATE_FUNC
    void DampenedOscillations()
    {
        //Simple Kinematics
        float t = Time.deltaTime; //Get time interval
        transform.position += velocity * t + (Mathf.Pow(t, 2.0f) / 2.0f) * (acceleration); //dx = vt + (a(dt)^2)/(2.0)
        velocity += acceleration * t; //dv = a(dt)

        //Accleration Control
        Vector3 dist = anchorPosition - transform.position; //dx
        acceleration = (k * dist - b * velocity) / (mass); //ma = k(dx) - bv
    }

    void AnchorStar()
    {
        anchorPosition = transform.position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    IEnumerator AddThrust(Vector3 dir)
    {
        yield return new WaitForEndOfFrame();

        float start = (float)NetworkTime.time;
        while((float)NetworkTime.time - start < 0.1f)
        {
            acceleration += dir * 1.5f;
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Send the animation to all of the clients 
    /// </summary>
    /// <param name="fullVelocity">velocity of the tap</param>
    [Command]
    private void Cmd_initiate(Vector3 fullVelocity)
    {
        Rpc_initiate(fullVelocity);
    }

    /// <summary>
    /// Activates on the clients 
    /// </summary>
    /// <param name="fullVelocity">velocity of the tap</param>
    [ClientRpc]
    private void Rpc_initiate(Vector3 fullVelocity)
    {
        velocity = (fullVelocity + mass * velocity) / mass;
        StartCoroutine(AddThrust(fullVelocity.normalized));
    }

    /// <summary>
    /// Send Anchor to Clients
    /// </summary>
    /// <param name="b"></param>
    [Command]
    private void CmdAnchor(bool b)
    {
        RpcAnchor(b);
    }

    /// <summary>
    /// Set Anchor Value
    /// </summary>
    /// <param name="b"></param>
    [ClientRpc]
    private void RpcAnchor(bool b)
    {
        if (b) update = AnchorStar;
        else update = DampenedOscillations;
    }
    #endregion
}
