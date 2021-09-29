using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: Externalised class for calculating Lerps and Slerps for c:ActivatedStar and c:DraggableStar
/// </summary>
public class Lerper
{
	/// <summary>
	/// VD: Externally setable start position for calculating Lerp
	/// </summary>
	private Vector3 startPos;
	/// <summary>
	/// VD: Externally setable end position for calculating Lerp
	/// </summary>
	private Vector3 endPos;
	/// <summary>
	/// VD: Externally setable start rotation for calculating Slerp
	/// </summary>
	private Quaternion startRot;
	/// <summary>
	/// VD: Externally setable end rotation for calculating Slerp
	/// </summary>
	private Quaternion endRot;
	/// <summary>
	/// VD: Externally setable start time for calculating Lerp and Slerp
	/// </summary>
	private float startTime;
	//float endTime;
	/// <summary>
	/// VD: Externally setable end time for calculating Lerp and Slerp
	/// </summary>
	private float timeDiffInv;
	/// <summary>
	/// VD: Normalizer variable for keeping the Lerp range within 0-1
	/// </summary>
	private float t = 0;

	/// <summary>
	/// FD: Constructor function: assigns v:startTime and v:timeDiffInv for use in f:Update 
	/// </summary>
	/// <param name="duration">Length of time to Lerp for</param>
	public Lerper(float duration)
	{
		timeDiffInv = 1.0f / duration;
	}

	/// <summary>
	/// FD: Changes startTime without changing timeDiff
	/// </summary>
	/// <param name="startTime">Pass in the time you'd like to set Start time to</param>
	/// <remarks>Based on the warning in f:Lerper, this maybe took over part of Lerper's 'old' functionlity - hypothetically</remarks>
	public void setStartTime(float startTime)
	{
		this.startTime = startTime;
	}

	/// <summary>
	/// FD: Tells the Lerper where the controller is and what direction it's facing. Stores in endPos and endRot for calculation
	/// </summary>
	/// <param name="goalPos">Value you'd like to use as the endPos in Lerper</param>
	/// <param name="goalRot">Value you'd like to use as the endRot in Lerper</param>
	/// <remarks>For use whenthe star is still in motion</remarks>
	public void updateGoal(Vector3 goalPos, Quaternion goalRot)
	{
		endPos = goalPos;
		endRot = goalRot;
	}

	/// <summary>
	/// FD: Tells the lerper where the star is and where it should be, used to keep things in place. Stores in v:startPos and v:endPos
	/// </summary>
	/// <param name="startPos">Where is the star?</param>
	/// <param name="endPos">Where should it be?</param>
	public void setInterpPoints(Vector3 startPos, Vector3 endPos)
	{
		this.startPos = startPos;
		this.endPos = endPos;
	}

	/// <summary>
	/// FD: Tells the c:lerper what rotation the star has and what angle it should end up with, used to keep the star(datasheets) facing the player. Stores in v:startRot and v:endRot
	/// </summary>
	/// <param name="startRot">What's it's Quaternion?</param>
	/// <param name="endRot">What should it be?</param>
	public void setInterpRotations(Quaternion startRot, Quaternion endRot)
	{
		this.startRot = startRot;
		this.endRot = endRot;
	}

	/// <summary>
	/// FD: Calculates and normalizes c:Lerper_v:t for use in f:getRotation and f:getPoint
	/// </summary>
	/// <param name="curTime">Pass in the current time(typically time.time)</param>
	/// <returns>Normalized time since the current Lerp began</returns>
	public float update(float curTime)
	{
		t = (curTime - startTime) * timeDiffInv;
		return t;
	}

	/// <summary>
	/// FD: Slerps the stored v:startRot and v:endRot using v:t as a percentage
	/// </summary>
	/// <returns>Slerped value</returns>
	public Quaternion getRotation()
	{
		//Lerp clamps t no need to check
		return Quaternion.Slerp(startRot, endRot, t);
	}

	/// <summary>
	/// FD: Lerps the stored v:startPos and v:endPos using v:t as a percentage
	/// </summary>
	/// <returns>Lerped value</returns>
	public Vector3 getPoint()
	{
		//Lerp clamps t no need to check
		return Vector3.Lerp(startPos, endPos, t);
	}

	#region ///<remarks>GC: Three unused public booleans referencing the t variable </remarks>

	/// <remarks> IV: These three booleans are basically garbage, probably used for log debugging at one point or a failed idea, worse yet maybe this class was stolen and half implemented</remarks>

	/// <summary>FD: Unused</summary>
	public bool isStarted()
	{
		return t >= 0;
	}

	/// <summary>FD: Unused</summary>
	public bool isDone()
	{
		return t >= 1;
	}

	/// <summary>FD: Unused</summary>
	public bool isRunning()
	{
		return t >= 0 && t <= 1;
	}
	#endregion

}
