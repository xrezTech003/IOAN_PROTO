using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

/// <summary>
///		CD : SegmentedSphereCreator
///		Class for creating a new Segmented Sphere
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SegementedSphereCreator : MonoBehaviour
{
	#region PRIVATE_VAR
	/// <summary>
	///		VD : vert
	///		List of vertices
	/// </summary>
	List<Vector3> vert = new List<Vector3>();

	/// <summary>
	///		VD : uv
	///		list of UVs
	/// </summary>
	List<Vector2> uv = new List<Vector2>();

	/// <summary>
	///		VD : normals
	///		List of normals
	/// </summary>
	List<Vector3> normals = new List<Vector3>();

	/// <summary>
	///		VD : triangles
	///		List of triangles
	/// </summary>
	List<int>[] triangles = new List<int>[4];

	/// <summary>
	///		VD : sphereMesh
	///		For mesh of new sphere object
	/// </summary>
	Mesh sphereMesh;
	#endregion

	#region UNITY_FUNC
	/**
	<summary>
		FD : Awake()
		Create new Sphere and get a new sphere mesh
		Init v:triangles
		Add all sphereMesh data to v:vert, v:uv, and v:normals
		Call f:subDivide with all sets of 3 v:triangles
		Set gameObject mesh values to v:triangles, v:vertices, v:normals, and v:uv
		Set MeshRenderer materials to new materials
	</summary>
	**/
	void Awake()
	{
		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphereMesh = sphere.GetComponent<MeshFilter>().mesh;

		//		Debug.Log("sphereMesh "  + sphereMesh.subMeshCount);

		for (int i = 0; i < triangles.Length; i++) triangles[i] = new List<int>();

		for (int v = 0; v < sphereMesh.vertices.Length; v++)
		{
			vert.Add(sphereMesh.vertices[v]);
			uv.Add(sphereMesh.uv[v]);
			normals.Add(sphereMesh.normals[v]);
		}

		for (int t = 0; t < sphereMesh.triangles.Length; /* intentionally blank*/ )
		{
			int v1 = sphereMesh.triangles[t++];
			int v2 = sphereMesh.triangles[t++];
			int v3 = sphereMesh.triangles[t++];

			//creates triangles and subdives if needed
			subDivide(v1, v2, v3);
			//			Debug.Log("Spanning triangle: "+ v1 + ", " + v2 + ", " + v3);
		}

		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.subMeshCount = triangles.Length;

		mesh.vertices = vert.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uv.ToArray();

		for (int i = 0; i < triangles.Length; i++)
		{
			int[] triangleArray = triangles[i].ToArray();
			mesh.SetTriangles(triangleArray, i);
			//	Debug.Log("triangle cnd " + i + " = " + mesh.GetTriangles(i).Length );
		}

		//		Debug.Log("mesh "  + mesh.subMeshCount);

		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.materials = new Material[4];

		Destroy(sphere);

		// read in tringals.
		// if all in one seg add to seg
		// if spans seg do we need to add verts?
		// do we add it to both with valus greater than 1?
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : getSegment(Vector2)
		If uv x is <= .5
			If uv y <= .5: return 0
			Else return 2
		Else
			If uv y <= .5: return 1
			Else return 3
		<param name="uv">Input UV</param>
	</summary>
	**/
	public int getSegment(Vector2 uv)
	{
		if (uv.x <= .5)
		{
			if (uv.y <= .5) return 0;
			else return 2;
		}
		else
		{
			if (uv.y <= .5) return 1;
			else return 3;
		}
	}

	/**
	<summary>
		FD : isOnCusp(Vector2)
		Return approximation of uv x to .5
		<param name="uv">Input UV</param>
	</summary>
	**/
	public bool isOnCusp(Vector2 uv)
	{
		return Mathf.Approximately(uv.x, .5f) || Mathf.Approximately(uv.x, .5f);
	}

	/**
	<summary>
		FD : subDivide(int, int, int)
		Find uv values from param
		Subdivide uv values based on the return values of f:isDiffSide
		If f:isDiffSide never returns true, Add uv values to v:triangles
		<param name="v1"></param>
		<param name="v2"></param>
		<param name="v3"></param>
	</summary>
	**/
	public void subDivide(int v1, int v2, int v3)
	{
		//Debug.Log("subDivide: " + v1 +", " + v2 + ", " + v3);
		Vector2 uv1 = uv[v1];
		Vector2 uv2 = uv[v2];
		Vector3 uv3 = uv[v3];

		if (isDiffSide(uv1, uv2))
		{
			int newPoint = splitPoint(v1, v2);
			subDivide(v1, newPoint, v3);
			subDivide(newPoint, v2, v3);
		}
		else if (isDiffSide(uv2, uv3))
		{
			int newPoint = splitPoint(v2, v3);
			subDivide(v1, v2, newPoint);
			subDivide(v1, newPoint, v3);
		}
		else if (isDiffSide(uv3, uv1))
		{
			int newPoint = splitPoint(v3, v1);
			subDivide(v1, v2, newPoint);
			subDivide(newPoint, v2, v3);
		}
		else
		{
			// all on same side
			int seg = getSegment(uv1);
			if (isOnCusp(uv1)) // this might be .5 so check other values
			{
				if (isOnCusp(uv2)) seg = getSegment(uv3);
				else seg = getSegment(uv2);
			}

			triangles[seg].Add(v1);
			triangles[seg].Add(v2);
			triangles[seg].Add(v3);
		}
	}

	/**
	<summary>
		FD : isDIffUSide(Vector2, Vector2)
		Return compliment of uv approximation of x values and x value comparisons
		<param name="uv1"></param>
		<param name="uv2"></param>
	</summary>
	**/
	public bool isDiffUSide(Vector2 uv1, Vector2 uv2)
	{
		return (!(Mathf.Approximately(uv1.x, .5f) || Mathf.Approximately(uv2.x, .5f))) && 
				((uv1.x < .5 && uv2.x > .5) || (uv1.x > .5 && uv2.x < .5));
	}

	/**
	<summary>
		FD : isDiffVSide(Vector2, Vector2)
		Return compliment of uv approximation of y values and y value comparisons
		<param name="uv1"></param>
		<param name="uv2"></param>
	</summary>
	**/
	public bool isDiffVSide(Vector2 uv1, Vector2 uv2)
	{
		return (!(Mathf.Approximately(uv1.y, .5f) || Mathf.Approximately(uv2.y, .5f))) && 
				((uv1.y < .5 && uv2.y > .5) || (uv1.y > .5 && uv2.y < .5));
	}

	/**
	<summary>
		FD : isDiffSide(Vector2, Vector2)
		return call values of f:isDiffUSide or f:isDiffVSide
		<param name="uv1"></param>
		<param name="uv2"></param>
	</summary>
	**/
	public bool isDiffSide(Vector2 uv1, Vector2 uv2)
	{
		return isDiffUSide(uv1, uv2) || isDiffVSide(uv1, uv2);
	}

	/**
	<summary>
		FD : interpValueWrap(float, float)
		If b < a: return 1 - f:interpValueWrap
		Else return percentage of a over difference + 1
		<param name="a"></param>
		<param name="b"></param>
	</summary>
	**/
	public float interpValueWrap(float a, float b)
	{
		if (b < a) return 1.0f - interpValueWrap(a, b);
		else
		{
			//assume a < b
			float dist = a + 1.0f - b;
			return a / dist;
		}

	}

	/**
	<summary>
		FD : closerToOneHalf(float, float)
		Find if values are closer to one half
		<param name="a"></param>
		<param name="b"></param>
	</summary>
	**/
	public bool closerToOneHalf(float a, float b)
	{
		float distToHalfSqr = (a - 0.5f) * (a - 0.5f) + (b - 0.5f) * (b - 0.5f);

		float distToZero = 0;

		if (a <= .5) distToZero += a * a;
		else distToZero += (a - 1) * (a - 1);

		if (b <= .5) distToZero += b * b;
		else distToZero += (b - 1) * (b - 1);

		return distToHalfSqr < distToZero;
	}

	/**
	<summary>
		FD : isInBetween(float, float, float)
		Return if b is in between a and c
		<param name="a"></param>
		<param name="b"></param>
		<param name="c"></param>
	</summary>
	**/
	public bool isInBetween(float a, float b, float c)
	{
		if (a <= c) return (a <= b) && (b <= c);
		else return (a >= b) && (b >= c);
	}

	/**
	<summary>
		FD : splitPoint(int, int)
		Find UVs at v1 and v2
		If f:isDiffUSide is true: set interpVal to .5 - uv1 x over difference
		Else If f:isDiffVSide is true: set interpVal to .5 - uv1 y over difference
		Else return -1
		Calculate values for new vertices, uvs, and normals and set them to v:vert, v:uv, and v:normals
		<param name="v1"></param>
		<param name="v2"></param>
	</summary>
	**/
	public int splitPoint(int v1, int v2)
	{
		//creates point where uv == .5
		//adds point to vers, uv, normals
		//return index
		Vector2 uv1 = uv[v1];
		Vector2 uv2 = uv[v2];

		Debug.Log("splitPoint: " + getSegment(uv1) + "-" + getSegment(uv2));

		float interpVal = 0;
		//		Vector2 interpUV = new Vector2(0,0);
		if (isDiffUSide(uv1, uv2))
		{
			// this can be spanning .5 or 0
			//			Debug.Log("isDiffUSide:" + uv1.x + "---" + uv2.x);
			//			isDiffUSide:0.5490332---0.4987062
			//				if(closerToOneHalf(uv1.x, uv2.x)) {
			interpVal = (.5f - uv1.x) / (uv2.x - uv1.x);
			//					interpUV =   uv1 + interpVal * (uv2 - uv1);
			//				} else { this doesn't happend
			//					interpVal = interpValueWrap(uv1.x, uv2.x);
			//					interpUV =   uv1 + interpVal * (uv2 - uv1);
			//					interpUV.x = 0;
			//				}
			// may also need to split on y.
			// will be recalled by subDivide
		}
		else if (isDiffVSide(uv1, uv2))
		{
			//			if(closerToOneHalf(uv1.y, uv2.y)) {
			interpVal = (.5f - uv1.y) / (uv2.y - uv1.y);
			//				interpUV =   uv1 + interpVal * (uv2 - uv1);
			// interpUV.y should now equal .5
			//			} else {
			//				interpVal = interpValueWrap(uv1.y, uv2.y);
			//				interpUV =   uv1 + interpVal * (uv2 - uv1);
			//				interpUV.y = 0;
			//			}

		}
		else
		{
			Debug.LogWarning("splitPoint called with two verticies on the same side.");
			return -1;
		}

		Vector3 p1 = vert[v1];
		Vector3 p2 = vert[v2];
		Vector3 interpPoint = p1 + interpVal * (p2 - p1);
		Vector2 interpUV = uv1 + interpVal * (uv2 - uv1);
		//			interpUV.x = 1.0f- interpUV.x;
		//			interpUV.y = 1.0f- interpUV.y;
		Vector3 interpNorm = interpPoint.normalized;

		if (!isInBetween(uv1.x, interpUV.x, uv2.x)) Debug.LogWarning("new uv x values out of wack");

		if (!isInBetween(uv1.y, interpUV.y, uv2.y)) Debug.LogWarning("new uv y values out of wack");

		Debug.Log("newPoint:  " + interpPoint + " -- " + interpUV.x + ", " + interpUV.y + "\n" + uv1.x + ", " + uv1.y + "---" + uv2.x + ", " + uv2.y);

		vert.Add(interpPoint);
		uv.Add(interpUV);
		normals.Add(interpNorm);
		return vert.Count - 1;
	}
	#endregion

	#region COMMENTED_CODE
	/*
		public bool sameSide(Vector2 uv1, Vector2 uv2) {
			return sameUSide(uv1, uv2) && sameVSide(uv1, uv2);
		}


		public bool sameUSide(Vector2 uv1, Vector2 uv2) {
				return (uv1.x <= .5 && uv2.x <= .5) ||
								(uv1.x >= .5 && uv2.x >= .5);
		}
		public bool sameVSide(Vector2 uv1, Vector2 uv2) {
				return (uv1.y <= .5 && uv2.y <= .5) ||
								(uv1.y >= .5 && uv2.y >= .5);
		}
		*/

	//returns true if the two numbers are closer to .5 than 0
	//wraps around so 1 is == 0.
	//give the percentage of the way from x to y wrapping around 0/1

	// Update is called once per frame
	void Update()
	{
	}
	#endregion
}
