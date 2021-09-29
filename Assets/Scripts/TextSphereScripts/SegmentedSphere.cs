using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: Builds a segmented sphere, based on the number of segments in putted into public v:subdivisions 
/// </summary>
public class SegmentedSphere : MonoBehaviour {

	/// <summary>
	/// VD: Used if camera is inside object with text
	/// </summary>
	[Tooltip("Used if camera is inside object with text")]
	public bool reverseSurfaceNormals = false;

	/// <summary>
	/// VD: Earnest public, essentially the definition
	/// </summary>
	public int subdivisions = 2;

	/// <summary>
	/// VD: Set to 1/subdivisons in Awake
	/// </summary>
	float segmentSize;

	/// <summary>
	/// FD: Quite literally does nothing
	/// </summary>
	/// <param name="rowCol">nothing, at all</param>
	public void getMin(int rowCol) {

	}

	/// <summary>
	/// FD: Mathematically adjusts the UV's to match the number of segments
	/// </summary>
	/// <param name="segment">VD: Passed from a lop, this function will be called segmentSized^2 times</param>
	/// <param name="minUV">The min to return</param>
	/// <param name="maxUV">The max to return</param>
	public void getMinMaxUV(int segment, ref Vector2 minUV, ref Vector2 maxUV) {
		Vector2Int rowCol = SegIDtoRowCol(segment);
		int row = rowCol.x;
		int col = rowCol.y;
		minUV.x = col * segmentSize;
		minUV.y = row * segmentSize;

		maxUV.x = (col+1) * segmentSize;
		maxUV.y = (row+1) * segmentSize;

	}
	
	/// <summary>
	/// FD: COnverts a segment ID to it's rows and columns, reverse of rowColToID
	/// </summary>
	/// <param name="seg">Id to translate</param>
	/// <returns>Vector 2 row and col</returns>
	Vector2Int SegIDtoRowCol(int seg) {
		int col = seg % subdivisions;
		int row = (int) ((seg-col) / subdivisions);
		return new Vector2Int(row, col);
	}

	/// <summary>
	/// FD: Creates a segment Id of sorts using a consistent calulation based on rows and columns
	/// </summary>
	/// <param name="row">Row</param>
	/// <param name="col">Column</param>
	/// <returns>ID</returns>
	int rowColToID(int row, int col) {
		return row*subdivisions + col;
	}

	/// <summary>
	/// FD: Does the math for getRows and getCols ::: uses segmentSize to determine if UV value is close enough to considered part of the next vector over
	/// </summary>
	/// <param name="uvValue"></param>
	/// <returns></returns>
	public List<int> getRowOrCols(float uvValue) {
		float rowf = uvValue / segmentSize;
		int row = (int) (rowf); //probaly don't need the round but lets be really correct

		List<int> result = new List<int>();
		result.Add(row);

		if(Mathf.Approximately(rowf, (float) row) && row != 0) {
			result.Add(row-1);
		}
		return result;
	}

	/// <summary>
	/// FD: horizontal proximity check for UV segment
	/// </summary>
	/// <param name="uv">The Uv to check</param>
	/// <returns>The row or rows that the (one, not list of)UV is in</returns>
	public List<int> getRows(Vector2 uv) {
		return getRowOrCols(uv.y);
	}
	
	/// <summary>
	/// FD: vertical proximity check for UV segment
	/// </summary>
	/// <param name="uv">The Uv to check</param>
	/// <returns>The column or columns that the (one, not list of)UV is in</returns>
	public List<int> getCols(Vector2 uv) {
		return getRowOrCols(uv.x);
	}

	/// <summary>
	/// FD: Creates a list of segments indexed by rowColToID
	/// </summary>
	/// <param name="uv">List of UVs</param>
	/// <returns>List of segments</returns>
	public List<int> getSegment(Vector2 uv) {
		List<int> rows = getRows(uv);
		List<int> cols =  getCols(uv);

		List<int> segs = new List<int>();

		foreach(int row in rows) {
			foreach(int col in cols) {
				segs.Add(rowColToID(row, col));
			}
		}
		///<remarks>	//uv values may be on a boarder and therefor in more than one segment :
		///:: e.g. if segmentSize is .5 then a uv value of (.5, .5) would be in all four segments</remarks>
		return segs;
	}

	/// <summary>
	/// FD: Checks to see if a UV (vertex) is in a segment
	/// </summary>
	/// <param name="uv">The UV to check</param>
	/// <param name="i">Index of the segment to check for</param>
	/// <returns>Is it in the segment?</returns>
	public bool isInSegment(Vector2 uv, int i) {

		return getSegment(uv).Contains(i);
	}

	/// <summary>
	/// FD: Check if any instances of shared verticies
	/// </summary>
	/// <param name="l1">First set of segments to check</param>
	/// <param name="l2">Second set of segmentss to check</param>
	/// <returns>Overlap?</returns>
	public bool hasOverlap(List<int> l1, List<int> l2) {
		foreach(int i in l1) {
			if(l2.Contains(i)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// 	FD: this assumes there is no getOverlap:
	/// 	::but there is at least one numbber in l1 that is +=1 from a number in l2
	/// </summary>
	/// <param name="l1">First set of UVs to check</param>
	/// <param name="l2">Second set of UVs t check</param>
	/// <returns>The lower index segment number</returns>
	public int getMinInSequence(List<int> l1, List<int> l2) {
		foreach(int i in l1) {
			if(l2.Contains(i+1)) {
				return i;
			}
		}
		foreach(int i in l1) {
			if(l2.Contains(i-1)) {
				return i-1;
			}
		}
		return -1; // no result

	}
	
	/// <summary>
	/// FD: Creates a list of shared verticies
	/// </summary>
	/// <param name="l1">First set of UVs to check</param>
	/// <param name="l2">Second set of UVs to check</param>
	/// <returns>Overlap</returns>
	//set intersection
	public List<int> getOverlap(List<int> l1, List<int> l2) {
		List<int> result = new List<int>();
		foreach(int i in l1) {
			if(l2.Contains(i)) {
				result.Add(i);
			}
		}
		return result;
	}

	/// <summary>
	/// Runs hasOverlap on all segments each UV exists in
	/// </summary>
	/// <param name="uv1">First set of UVs to check</param>
	/// <param name="uv2">Second set of UVs to check</param>
	/// <returns>Overlap?</returns>
	public bool areInSameSegment(Vector2 uv1, Vector2 uv2) {
		List<int> segments1 = getSegment(uv1);
		List<int> segments2 = getSegment(uv2);
		return hasOverlap(segments1, segments2);
	}

	/// <summary>
	/// FD: If the rows don't overlap find the minumum value of the two and return the value of the vertical average :
	/// :: else if the columns don't overlap find the minimum vallue of the cols and retunr the average horizonal value between the two :
	/// :: else return 0
	/// </summary>
	/// <param name="uv1">VD: UV point to check</param>
	/// <param name="uv2">VD: other UV point to check</param>
	/// <returns>average value between either uv1.y and uv2.y or between uv1.x and uv2.x, depending on what is relevant</returns>
	public float getSplitPercentage(Vector2 uv1, Vector2 uv2) {

		if(! hasOverlap(getRows(uv1), getRows(uv2))) { // differnt segments
			int minRow = getMinInSequence(getRows(uv1), getRows(uv2));
			//Debug.Log("minRowSeg" + ((minRow+1) * segmentSize));
			if(minRow >= 0) {
				return ( ((minRow+1) * segmentSize) - uv1.y) / (uv2.y - uv1.y);
			} else {
				Debug.LogWarning("getSplitPercentage got called on UV values non adjacent segments.");
				return 0f;
			}
		} else if(! hasOverlap(getCols(uv1), getCols(uv2))) {
			int minCol = getMinInSequence(getCols(uv1), getCols(uv2));
			//	Debug.Log("minColSeg" + ((minCol+1) * segmentSize));
			if(minCol >= 0) {
				return ( ((minCol+1) * segmentSize) - uv1.x) / (uv2.x - uv1.x);
			} else {
				Debug.LogWarning("getSplitPercentage got called on UV values non adjacent segments.");
				return 0f;
			}
		} else {
			Debug.LogWarning("getSplitPercentage got called on UV values in same segment.");
			return 0f;
		}
	}

	/// <summary>
	/// Creates a third vertex between two in ratio with the UVs (getSplitPercentage)
	/// </summary>
	/// <param name="mesh">VD: mesh reference</param>
	/// <param name="v1">VD: First vertex</param>
	/// <param name="v2">VD: Second vertex</param>
	/// <returns>The mesh with the new point</returns>
	public int splitPoint(ProtoMesh mesh, int v1, int v2) {
		Vector2 uv1 = mesh.uvs[v1];
		Vector2 uv2 = mesh.uvs[v2];

		if(areInSameSegment(uv1, uv2)) {
			Debug.LogWarning("splitPoint called with two verticies on the same side.");
			return -1;
		} else {
			float interpVal = getSplitPercentage(uv1, uv2);
			Vector3 p1 = mesh.vertices[v1];
			Vector3 p2 = mesh.vertices[v2];
			Vector3 interpPoint = p1 + interpVal * (p2 - p1);
			Vector2 interpUV =   uv1 + interpVal * (uv2 - uv1);
			Vector3 interpNorm = interpPoint.normalized;
			//		Debug.Log(uv1.x + ", " + uv1.y + "-" + uv2.x + ", " + uv2.y + " --- " + interpVal);
			//			return -1;
			return mesh.addVertex(interpPoint, interpUV, interpNorm);
		}
	}

	/// <summary>
	/// FD: Unused
	/// </summary>
	/// <param name="uv"FD: UV to convert></param>
	/// <returns>String version of the UV</returns>
	public string uvString(Vector2 uv) {
		//use this for more decimal points
		return "uv(" + uv.x + ", " +uv.y + ")";
	}

	/// <summary>
	/// FD: creates new triangles if spans segments ::: else does nothing
	/// </summary>
	/// <param name="protoMesh">Mesh to build the triangle into</param>
	/// <param name="v1">Triangle vertex</param>
	/// <param name="v2">Triangle vertex</param>
	/// <param name="v3">Triangle vertex</param>
	//creates new triangles if spans segments
	//else does nothing
	public void subDivide(ProtoMesh protoMesh, int v1, int v2, int v3) {
		Vector2 uv1 = protoMesh.uvs[v1];
		Vector2 uv2 = protoMesh.uvs[v2];
		Vector3 uv3 = protoMesh.uvs[v3];

		if(! areInSameSegment(uv1, uv2)) {
			int newPoint = splitPoint(protoMesh, v1, v2);
			//			Debug.Log(uvString(protoMesh.uvs[newPoint]) + " should be on same side as " + uvString(uv1) + ", " + uvString(uv2));
			subDivide(protoMesh,v1, newPoint, v3);
			subDivide(protoMesh, newPoint, v2, v3);
		} else if (! areInSameSegment(uv2, uv3)) {
			int newPoint = splitPoint(protoMesh, v2, v3);
			//		Debug.Log(uvString(protoMesh.uvs[newPoint]) + " should be on same side as " + uvString(uv2) + ", " + uvString(uv3));
			subDivide(protoMesh, v1, v2, newPoint);
			subDivide(protoMesh, newPoint, v3, v1);
		} else if (! areInSameSegment(uv3, uv1)) {
			int newPoint = splitPoint(protoMesh,v3, v1);
			//		Debug.Log(uvString(protoMesh.uvs[newPoint]) + " should be on same side as " + uvString(uv3) + ", " + uvString(uv1));
			subDivide(protoMesh, v1, v2, newPoint);
			subDivide(protoMesh, newPoint, v2, v3);
		} else {
			List<int> seg1 = getSegment(uv1);
			List<int> seg2 = getSegment(uv2);
			List<int> seg3 = getSegment(uv3);

			List<int> overlap = getOverlap(getOverlap(seg1, seg2), seg3);
			if(overlap.Count != 1) {
				Debug.LogWarning("Triangle segment is ambigous");
			}
			protoMesh.addTriangle(v1,v2,v3);
		}

	}

	/// <summary>
	/// FD: Root function to recreate UV accurate triangles on the segmented mesh
	/// </summary>
	/// <param name="sourceMesh">FD: properly named</param>
	/// <returns>A protomesh version of the mesh without gaps</returns>
	public ProtoMesh fillInGaps(Mesh sourceMesh) {
		ProtoMesh protoMesh = new ProtoMesh(sourceMesh);
		protoMesh.triangles.Clear ();
		for(int t = 0; t < sourceMesh.triangles.Length; /* intentionally blank*/ ) {
			int v1 = 	sourceMesh.triangles[t++];
			int v2 = 	sourceMesh.triangles[t++];
			int v3 = 	sourceMesh.triangles[t++];

			//creates triangles and subdives if needed
			subDivide(protoMesh, v1, v2, v3);
			//			Debug.Log("Spanning triangle: "+ v1 + ", " + v2 + ", " + v3);
		}
		return protoMesh;


	}

	/// <summary>
	/// FD: Clear verticies not within the UV range aligning with the inputed segment
	/// </summary>
	/// <param name="source">The mesh to reference the UVs from</param>
	/// <param name="segmentNumber">The segment to check the verticies of</param>
	/// <returns>The oringal segment, minus the unneeded verticies</returns>
	public ProtoMesh filterSegment(ProtoMesh source, int segmentNumber) {
		ProtoMesh segment = new ProtoMesh(source);

		int v = 0;
		while(v < segment.vertices.Count) {
			if(! isInSegment(segment.uvs[v], segmentNumber)) {
				segment.removeVertex(v);
			} else {
				//				Debug.Log(segment.uvs[v] + " is in "+ segmentNumber);
				v++;
			}
		}
		return segment;


	}

	/// <summary>
	/// FD: Name says it all, mathematically flips the normals to the inside to display the text on the inside
	/// </summary>
	/// <param name="mesh">The mesh (or protomesh) to flip the nomrals on</param>
	void reverseNormals(Mesh mesh)
	{
		Vector3[] normals = mesh.normals;
		for (int i = 0; i < normals.Length; i++)
			normals[i] = -normals[i];
		mesh.normals = normals;

		for (int m = 0; m < mesh.subMeshCount; m++)
		{
			int[] triangles = mesh.GetTriangles(m);
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int temp = triangles[i + 0];
				triangles[i + 0] = triangles[i + 1];
				triangles[i + 1] = temp;
			}
			mesh.SetTriangles(triangles, m);
		}
	}

    #region UNITY FUNCTIONS
    /// <summary>
    /// FD: Using c:protomesh to generate the sphere, call UV setting functions
    /// </summary>
    void Awake() {
		segmentSize = 1.0f/subdivisions;

		GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Mesh sphereMesh = sphere.GetComponent<MeshFilter>().mesh;

		ProtoMesh protoMesh = fillInGaps(sphereMesh);  //TODO: uncomment this line EGM

		ProtoMesh[] seg = new ProtoMesh[subdivisions*subdivisions];

		for(int i = 0; i < seg.Length; i++) {
			seg[i] = filterSegment(protoMesh, i);

			Vector2 minUV = new Vector2();
			Vector2 maxUV = new Vector2();
			getMinMaxUV(i, ref minUV, ref maxUV); //TODO: uncomment this line
			if(maxUV.x == 1 ) { 
				//in the source mesh UV never go to 1.
				//need to interpolate based on acutal values of uv.x
				Vector2 minUVAlt = new Vector2();
				Vector2 maxUVAlt = new Vector2();
				seg[i].calcMinMax(ref minUVAlt, ref maxUVAlt); //TODO: remove this line
				if(maxUV.x == 1) {
					maxUV.x = maxUVAlt.x;
				}
			}




			//Debug.Log("Segment " + i + ":  min(" + minUV.x +", " + minUV.y +") - max(" + maxUV.x +", " + maxUV.y + ")" );

			GameObject gObj = new GameObject("SphereSegment" + i);
			gObj.AddComponent<MeshFilter>();
			gObj.AddComponent<MeshRenderer>();
			gObj.GetComponent<MeshFilter>().mesh = seg[i].toMesh(minUV, maxUV);
			if(reverseSurfaceNormals) {
				reverseNormals(gObj.GetComponent<MeshFilter>().mesh);
			}
			gObj.transform.parent = transform;
			gObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		}
		Destroy(sphere);

	}

	/// <summary>
	/// FD: Empty Start
	/// </summary>
	// Use this for initialization
	void Start () {

	}
	/// <summary>
	/// FD: Empty Update
	/// </summary>
	// Update is called once per frame
	void Update () {

	}
	#endregion
}
