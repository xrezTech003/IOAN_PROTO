using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///		CD : ProtoMesh
///		Mesh Handler
/// </summary>
public class ProtoMesh 
{
	#region PUBLIC_VAR
	/// <summary>
	///		VD : vertices
	/// </summary>
	public List<Vector3> vertices = new List<Vector3>();

	/// <summary>
	///		VD : uvs
	/// </summary>
	public List<Vector2> uvs = new List<Vector2>();

	/// <summary>
	///		VD : normals
	/// </summary>
	public List<Vector3> normals = new List<Vector3>();

	/// <summary>
	///		VD : triangles
	/// </summary>
	public List<int> triangles = new List<int>();
	#endregion

	#region CONSTRUCTOR
	/**
	<summary>
		CD : ProtoMesh(Mesh)
		Set Add all vertices, uvs, normals, and triangles in mesh to v:vertices, v:uvs, v:normals, v:triangles
		<param name="mesh"></param>
	</summary>
	**/
	public ProtoMesh(Mesh mesh)
	{
		// is there really no addAll in c#?
		// start by copying all values from origional mesh
		for (int v = 0; v < mesh.vertices.Length; v++)
		{
			vertices.Add(mesh.vertices[v]);
			uvs.Add(mesh.uv[v]);
			normals.Add(mesh.normals[v]);
		}

		for (int t = 0; t < mesh.triangles.Length; t++)
		{
			triangles.Add(mesh.triangles[t]);
		}
	}

	/**
	<summary>
		CD : ProtoMesh(ProtoMesh)
		Copy old values into new ProtoMesh
		<param name="protoMesh"></param>
	</summary>
	**/
	public ProtoMesh(ProtoMesh protoMesh)
	{
		// need to verify that values get copied
		vertices = new List<Vector3>(protoMesh.vertices);
		uvs = new List<Vector2>(protoMesh.uvs);
		normals = new List<Vector3>(protoMesh.normals);
		triangles = new List<int>(protoMesh.triangles);
	}
	#endregion

	#region PUBLIC_FUNC
	/**
	<summary>
		FD : calcMinMax(ref Vector2, ref Vector2)
		Set minUV and maxUV to minimum and maximum value within range of [-2, 2]
		<param name="minUV"></param>
		<param name="maxUV"></param>
	</summary>
	**/
	public void calcMinMax(ref Vector2 minUV, ref Vector2 maxUV)
	{
		minUV.x = 2.0f;
		minUV.y = 2.0f;
		maxUV.x = -2.0f;
		maxUV.y = -2.0f;

		for (int i = 0; i < uvs.Count; i++)
		{
			Vector2 curUV = uvs[i];
			minUV.x = minUV.x < curUV.x ? minUV.x : curUV.x;
			minUV.y = minUV.y < curUV.y ? minUV.y : curUV.y;
			maxUV.x = maxUV.x > curUV.x ? maxUV.x : curUV.x;
			maxUV.y = maxUV.y > curUV.y ? maxUV.y : curUV.y;
		}
	}

	/**
	<summary>
		FD : fixBounds(ref Vector2)
		Keeps uv between [0, 1]
		<param name="uv"></param>
	</summary>
	**/
	public void fixBounds(ref Vector2 uv)
	{
		uv.x = uv.x < 0 ? 0 : uv.x;
		uv.y = uv.y < 0 ? 0 : uv.y;

		uv.x = uv.x > 1 ? 1 : uv.x;
		uv.y = uv.y > 1 ? 1 : uv.y;

		if (Mathf.Approximately(uv.x, 1.0f)) uv.x = 1;

		if (Mathf.Approximately(uv.y, 1.0f)) uv.y = 1;
	}

	/**
	<summary>
		FD : toMesh(Vector2, Vector2)
		Make new Mesh where vertices are a copy of v:vertices
		Make a new Vector2 of the range between minUV and max UV
		Make new array of newUVs size of v:uvs
		For each vector in v:uvs
			Add to newUVs percent range from v:uvs
			Call fixBounds with newUVS[i]
		Set new mesh uvs to newUVs
		Copy all triangles and normals into new mesh
		Return new Mesh
		<param name="minUV"></param>
		<param name="maxUV"></param>
	</summary>
	**/
	public Mesh toMesh(Vector2 minUV, Vector2 maxUV)
	{
		Mesh m = new Mesh();
		Vector3[] newVerts = new Vector3[vertices.Count];
		for (int i = 0; i < vertices.Count; i++) newVerts[i] = vertices[i];
		m.vertices = newVerts;

		//rescale uvs;

		//    Vector2 minUV = new Vector2();
		//    Vector2 maxUV = new Vector2();
		//    calcMinMaxUV(ref minUV, ref maxUV);

		//    Debug.Log("UV scale " + maxUV +" - " + minUV);

		Vector2 uvRange = maxUV - minUV;
		Vector2[] newUVs = new Vector2[uvs.Count];

		for (int i = 0; i < uvs.Count; i++)
		{
			newUVs[i] = (uvs[i] - minUV) / uvRange;

			//it looks like there are slight rounding errors in the uv calculations
			//that are obvios on the edges of the meshes
			//fix here.
			fixBounds(ref newUVs[i]);
			//     Debug.Log("      :" + uvs[i] +" -> " + newUVs[i]);
		}

		m.uv = newUVs;

		Vector3[] newNormals = new Vector3[normals.Count];
		for (int i = 0; i < normals.Count; i++) newNormals[i] = normals[i];
		m.normals = newNormals;

		int[] newTriangles = new int[triangles.Count];
		for (int i = 0; i < triangles.Count; i++) newTriangles[i] = triangles[i];
		m.triangles = newTriangles;

		return m;
	}

	/**
	<summary>
		FD : addVertex(Vector3, Vector2, Vector3)
		Add params to v:vertices, v:uvs, and v:normals
		Return index of new vertex
		<param name="vert"></param>
		<param name="uv"></param>
		<param name="norm"></param>
	</summary>
	**/
	public int addVertex(Vector3 vert, Vector2 uv, Vector3 norm)
	{
		vertices.Add(vert);
		uvs.Add(uv);
		normals.Add(norm);

		//return new index
		return vertices.Count - 1;
	}

	/**
	<summary>
		FD : addTriangle(int, int, int)
		Add params to v:triangles
		<param name="v0"></param>
		<param name="v1"></param>
		<param name="v2"></param>
	</summary>
	**/
	public void addTriangle(int v0, int v1, int v2)
	{
		triangles.Add(v0);
		triangles.Add(v1);
		triangles.Add(v2);
	}

	/**
	<summary>
		FD : removeVertex(int)
		Remove vertIndex from v:vertices, v:uvs, and v:normals
		Remove vertIndex from triangles
		<param name="vertIndex"></param>
	</summary>
	**/
	public void removeVertex(int vertIndex)
	{
		vertices.RemoveAt(vertIndex);
		uvs.RemoveAt(vertIndex);
		normals.RemoveAt(vertIndex);

		// remove triangles with given id
		// reduce index for all indices >vertIndex
		int curT = 0;
		while (curT < triangles.Count)
		{
			int v0 = triangles[curT];
			int v1 = triangles[curT + 1];
			int v2 = triangles[curT + 2];

			if ((v0 == vertIndex) || (v1 == vertIndex) || (v2 == vertIndex))
			{
				triangles.RemoveRange(curT, 3);
				// do not inc curT already removed points
			}
			else
			{
				if (v0 > vertIndex) triangles[curT] = v0 - 1;
				if (v1 > vertIndex)triangles[curT + 1] = v1 - 1;
				if (v2 > vertIndex)triangles[curT + 2] = v2 - 1;
				curT += 3;
			}
		}
	}
	#endregion

	#region COMMENTED_CODE
	/*
	public void calcMinMaxUV(ref Vector2 min, ref Vector2 max) {
	Vector2 minUV = new Vector2(123.0f,123.0f);
	Vector2 maxUV = new Vector2(-123.0f,-123.0f);
	foreach(Vector2 curUV in uvs) {
		minUV.x = minUV.x < curUV.x ? minUV.x : curUV.x;
		minUV.y = minUV.y < curUV.y ? minUV.y : curUV.y;
		maxUV.x = maxUV.x > curUV.x ? maxUV.x : curUV.x;
		maxUV.y = maxUV.y > curUV.y ? maxUV.y : curUV.y;
	}
	min.x = minUV.x;
	min.y = minUV.y;
	max.x = maxUV.x;
	max.y = maxUV.y;
	}
	*/
	#endregion
}
