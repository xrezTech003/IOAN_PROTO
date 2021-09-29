using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Tendril))]
//[RequireComponent(typeof(TrailRenderer))]

/// <summary>
/// CD: TendrilPathFollower. This class holds the trails the follow the path that is generated and interpolacted by c:Tendril.
/// </summary>
public class TendrilPathFollower : MonoBehaviour
{
	private Tendril tendril;
	private List<TrailRenderer> trailRenderers = new List<TrailRenderer>();

    public float baseSpeed;
    private float origSpeed;
    private float currSpeed;
	public float Speed
    {
        set
        {
            origSpeed = value;
            currSpeed = value;
        }
    }
    public Vector3 start;
    public Vector3 end;

	private Vector3 goalPoint;

	public float angularVel = 1;
	public float angle = 0;

    /// <summary>
    /// FD: Start(): This is used by getting the Tendril and actually using f:beginTendril() and f:getNextCurvePoint() to make the curves. The rest is just TendrilRenderer inits.
    /// </summary>
    private void Start()
    {
		tendril = GetComponent<Tendril>();
		TrailRenderer tr = GetComponent<TrailRenderer>();

		if(tr != null)
        {
			trailRenderers.Add(tr);
			tr.enabled = false;
		}

		foreach(Transform child in transform)
        {
			tr = child.GetComponent<TrailRenderer>();

			if(tr != null)
            {
				trailRenderers.Add(tr);
				tr.enabled = false;
			}
		}

		tendril.BeginTendril();
		goalPoint = tendril.GetNextCurvePoint();
		transform.position = goalPoint;

		foreach (TrailRenderer trailRenderer in trailRenderers)
            trailRenderer.enabled = true;
	}

    /// <summary>
    /// FD: Update(): This checks if we are still moving and if we are figure out current interpolation step and move accordingly.
    /// </summary>
	private void Update ()
    {
        if (!tendril.HasMoreCurvePoints()) return;

        //SPEED MOD
        //IncreaseSpeed();
        BellSpeed();

		float stepSize = currSpeed * Time.deltaTime;

		float distToGoal = Vector3.Distance(transform.position, goalPoint);

		while((distToGoal < stepSize) && tendril.HasMoreCurvePoints())
        {
			goalPoint = tendril.GetNextCurvePoint();
			distToGoal = Vector3.Distance(transform.position, goalPoint);
		}

		transform.LookAt(goalPoint);
		transform.position = Vector3.MoveTowards(transform.position, goalPoint, stepSize);
		transform.RotateAround(transform.position, transform.forward,angle);
		angle += angularVel* Time.deltaTime;
	}

    private void IncreaseSpeed()
    {
        float distRatio = Vector3.Distance(start, transform.position) / Vector3.Distance(start, end);

        currSpeed = origSpeed * (baseSpeed + distRatio);
    }

    private void BellSpeed()
    {
        float distRatio;

        if (Vector3.Distance(start, transform.position) <= Vector3.Distance(end, transform.position))
            distRatio = Vector3.Distance(start, transform.position) / Vector3.Distance(start, end);
        else 
            distRatio = Vector3.Distance(end, transform.position) / Vector3.Distance(start, end);

        currSpeed = origSpeed * (baseSpeed + distRatio);

        if (Vector3.Distance(transform.position, end) < 0.1f)
            currSpeed /= 1.5f;
    }
}
