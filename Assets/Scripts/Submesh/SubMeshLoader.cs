using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// CD: Exists on a section of the Star Map ::: 
/// CD: Each piece makes it's own mesh filter and mesh render by extracting the data off of the mesh's in bakedmeshes3 folder ::: 
/// CD: Works hand in hand with DataLoad and Initializer to accomplish this task on a piece by piece basis
/// </summary>
public class SubMeshLoader : MonoBehaviour
{
    #region PUBLIC_VAR
    /// <summary>
    /// VD: boolean for when mesh data has been loaded in
    /// </summary>
    public bool ready = false;

    /// <summary>
    /// VD: Coordinates for the mesh
    /// </summary>
    public Vector2 coord;

    public Vector2 pos;
    #endregion

    #region PRIVATE_VAR
    /// <summary>
    ///VD: reference tag within c:SubmeshLoader for g:Initialise, will find object tagged Init in f:Update  
    /// </summary>
    private GameObject myInit;

    /// <summary>
    /// VD: reference tag within c:SubMeshLoader for g:Config, will find object called Config in scene multiple times in c:subMeshLoader
    /// </summary>
    private GameObject configRef;

    /// <summary>
    ///     Cache for meshes
    /// </summary>
    private static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

    /// <summary>
    ///     Cache for mesh colliders
    /// </summary>
    private static Dictionary<string, Mesh> meshColliders = new Dictionary<string, Mesh>();
    #endregion

    #region UNITY_FUNC
    /// <summary>
    /// On initialize, find and store g:Config as v:configRef
    /// </summary>
    void Start()
    {
        configRef = GameObject.Find("Config");
        myInit = GameObject.FindGameObjectWithTag("init");
    }

    /// <summary>
    /// Assigns o:self the material set in o:initialiser :::
    /// Function will check to see if the o:initialiser exists and assigns it if not
    /// </summary>
    void Update()
    {
        if (myInit)
        {
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            if (mr)
            {
                mr.SetPropertyBlock(myInit.GetComponent<shadeSwapper>().mMat);///<remarks>GS: I would like to test what changing this does to the loaded stars at some point, doesn't line up with what happens in f:ReadMeshIn</remarks>
			}
            //		matBlockSet = true;
        }
        else
        {
            myInit = GameObject.FindGameObjectWithTag("init");
            if (!myInit)
            {
                Debug.Log("error - init object not found!");
            }
        }
    }
    #endregion

    #region PUBLIC_FUNC
    /// <summary>
    /// FD: Coroutine start for ReadMeshIn (Useful for adding wait times)
    /// </summary>
    /// <param name="curStarCount">IV: VD: Unused</param>
    /// <param name="clothHeight">IV: VD: Unused</param>
    /// <param name="IDX">VD: x "Coordinate" of mesh-grid piece currently being loaded in (3/2020 Grid is (4-8) X (1-5)) </param>
    /// <param name="IDY">VD: y "Coordinate" of mesh-grid piece currently being loaded in (3/2020 Grid is (4-8) X (1-5)) </param>
    public void ReadMeshInStarter()
    {
        if (!configRef)
            configRef = GameObject.Find("Config");

        StartCoroutine(ReadMeshIn((int)coord.x, (int)coord.y));
    }
    #endregion

    #region PRIVATE_FUNC
    /// <summary>
    /// <list type="number"><listheader>FD: Primary Sar Map Mesh Loader </listheader>
    /// <item>Having been fed a specif mesh in the X_Y naming convention, reads the matching bakedmesh file and holds it as a string </item>
    /// <item>Reads the string once to determine how many vertices, normals, UVs, Colours, faces, and triangles are in the .obj file</item>
    /// <item>Creates an array for each element counted with length = that element's count</item>
    /// <item>Rereads the bakedmeshX_Y, makes another string and reds through to populate the newly made arrays with the values it counted on first pass</item>
    /// <item>Cretes an index list to match, then creates components meshRender and MeshFilter on o:self using data from arrays collected in the StringReadrs</item></list>
    /// </summary>
    /// <param name="inX">X-coordinate of mesh within 11 x 5 baked mesh grid</param>
    /// <param name="inY">Y-coordinate of mesh within 11 x 5 baked mesh grid</param>
    /// 
    private IEnumerator ReadMeshIn(int inX, int inY)
    {
        if (myInit == null)
            myInit = GameObject.FindGameObjectWithTag("init");

        Initializer init = myInit.GetComponent<Initializer>();
        shadeSwapper shade = myInit.GetComponent<shadeSwapper>();
        ConfigData configData = Config.Instance.Data;

        //Cache key
        string key = inX + "-" + inY;

        //If mesh isn't cache, load it in first
        if (!meshes.ContainsKey(key))
        {
            StartCoroutine(LoadMesh(inX, inY, key));
            yield return new WaitUntil(() => ready);
        }

        //Get Centroid Position
        SFScales scales = Config.Instance.GetScales();
        Vector2 modScales = new Vector2(scales.xScale / 11.0f, scales.zScale / 5.0f);
        pos = new Vector2()
        {
            x = ((inX - 1.0f) * modScales.x) - 5.0f * modScales.x,
            y = ((inY - 1.0f) * modScales.y) - 2.0f * modScales.y
        };

        //Progress loading bar
        Config.Instance.TickLoadingProgress();

        //Attach mesh to gameobject
        gameObject.AddComponent<MeshFilter>();
        gameObject.GetComponent<MeshFilter>().mesh = meshes[key];

        gameObject.AddComponent<MeshCollider>();
        gameObject.GetComponent<MeshCollider>().sharedMesh = meshColliders[key];

        gameObject.GetComponent<MeshFilter>().mesh.SetIndices(Enumerable.Range(0, gameObject.GetComponent<MeshFilter>().mesh.vertices.Count()).ToList(), MeshTopology.Points, 0);
        gameObject.GetComponent<MeshFilter>().mesh.bounds = new Bounds(transform.position, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));

        //Set High or Low quality shader
        MeshRenderer mRenderer = gameObject.AddComponent<MeshRenderer>();

        if (myInit)
        {
            if (inX >= configData.meshHQRangeX.x && inX <= configData.meshHQRangeX.y &&
                inY >= configData.meshHQRangeZ.x && inY <= configData.meshHQRangeZ.y)
            {
                mRenderer.sharedMaterial = shade.hqMat;
            }
            else if (inX >= configData.meshMQRangeX.x && inX <= configData.meshMQRangeX.y &&
                     inY >= configData.meshMQRangeZ.x && inY <= configData.meshMQRangeZ.y)
            {
                mRenderer.sharedMaterial = shade.mqMat;
                gameObject.layer = 13;
            }
            else
            {
                mRenderer.sharedMaterial = shade.lowMat;
                gameObject.layer = 13;
            }
        }
        else Debug.Log("error - init object not found!");

        mRenderer.receiveShadows = false;
        mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        gameObject.transform.position = Vector3.zero;

        init.myStarCount += gameObject.GetComponent<MeshFilter>().mesh.vertexCount;
        init.totalMeshesLoaded += 1;
    }

    /// <summary>
    /// FD: Loads in data using the LoadJob for the new byte meshes
    /// </summary>
    /// <param name="inX">X - coord</param>
    /// <param name="inY">Y - coord</param>
    /// <param name="key">"hash" key for mesh cache</param>
    /// <returns></returns>
    private IEnumerator LoadMesh(int inX, int inY, string key)
    {
        ready = false;

        string filename = "mesh_data/grid" + key + ".b";

        uint numVertices;
        ushort vertSize;
        uint numTriangles;
        ushort triSize;

        //Read in header data from file
        using (var mmf = MemoryMappedFile.CreateFromFile(filename, FileMode.Open))
        {
            using (var file = mmf.CreateViewAccessor(0x0, 0x0))
            {
                numVertices = file.ReadUInt32(0x0);
                vertSize = file.ReadUInt16(0x4);
                numTriangles = file.ReadUInt32(0x6);
                triSize = file.ReadUInt16(0xA);
            }
        }

        //NativeArray buffers for multithreading
        NativeArray<Vector3> vertices = new NativeArray<Vector3>((int)numVertices, Allocator.TempJob);
        NativeArray<Vector3> normals = new NativeArray<Vector3>((int)numVertices, Allocator.TempJob);
        NativeArray<Color> colors = new NativeArray<Color>((int)numVertices, Allocator.TempJob);
        NativeArray<Vector4> uvs = new NativeArray<Vector4>((int)numVertices, Allocator.TempJob);
        NativeArray<Vector4> uvBs = new NativeArray<Vector4>((int)numVertices, Allocator.TempJob);
        NativeArray<int> triangles = new NativeArray<int>((int)numTriangles * 3, Allocator.TempJob);

        //Create Loading Thread
        LoadJob job = new LoadJob()
        {
            x = inX,
            y = inY,
            numVert = numVertices,
            vSize = vertSize,
            numTri = numTriangles,
            tSize = triSize,

            limits = Config.Instance.GetLimits(),
            scales = Config.Instance.GetScales(),

            verts = vertices,
            nrms = normals,
            cols = colors,
            uvs = uvs,
            uvBs = uvBs,
            tris = triangles
        };

        //Wait till thread is completed
        JobHandle handle = job.Schedule();
        yield return new WaitUntil(() => handle.IsCompleted);
        handle.Complete();

        //Create new mesh
        Mesh mesh = new Mesh()
        {
            vertices = vertices.ToArray(),
            colors = colors.ToArray(),
            normals = normals.ToArray(),
            name = key
        };

        mesh.SetUVs(0, uvs.ToList());
        mesh.SetUVs(1, uvBs.ToList());
        mesh.SetTriangles(triangles.ToArray(), 0);

        //Create new mesh collider
        Mesh colliderMesh = new Mesh()
        {
            vertices = vertices.ToArray(),
            colors = colors.ToArray(),
            normals = normals.ToArray()
        };

        colliderMesh.SetUVs(0, uvs.ToList());
        colliderMesh.SetUVs(1, uvBs.ToList());
        colliderMesh.SetTriangles(triangles.ToArray(), 0);

        //Cache mesh and mesh collider
        meshes[key] = mesh;
        meshColliders[key] = colliderMesh;

        vertices.Dispose();
        normals.Dispose();
        colors.Dispose();
        uvs.Dispose();
        uvBs.Dispose();
        triangles.Dispose();

        ready = true;
    }

    /// <summary>
    /// FD: Dev function for debugging mesh data at a certain point
    /// </summary>
    /// <param name="mesh">mesh</param>
    /// <param name="i">point</param>
    private void PrintPointData(Mesh mesh, int i)
    {
        string output = "";
        output += mesh.vertices[i] + " ";
        output += mesh.colors[i] + " ";
        output += mesh.normals[i] + " ";
        output += mesh.uv[i] + " ";
        output += mesh.uv2[i];

        Debug.Log(output);
    }
    #endregion
}

/// <summary>
/// SD: this is the job used to load in the mesh data from the byte file
/// </summary>
public struct LoadJob : IJob
{
    //Header Data
    public int x, y;
    public uint numVert;
    public ushort vSize;
    public uint numTri;
    public ushort tSize;

    //Config data
    public SFLimitsStruct limits;
    public SFScales scales;

    //Mesh Data
    public NativeArray<Vector3> verts;
    public NativeArray<Vector3> nrms;
    public NativeArray<Color> cols;
    public NativeArray<Vector4> uvs;
    public NativeArray<Vector4> uvBs;
    public NativeArray<int> tris;

    public void Execute()
    {
        string filename = "mesh_data/grid" + x + "-" + y + ".b";

        //Open File and load all mesh data
        using (var mmf = MemoryMappedFile.CreateFromFile(filename, FileMode.Open))
        {
            using (var file = mmf.CreateViewAccessor(0x10, numVert * vSize))
            {
                //For each "star"
                for (int i = 0; i < numVert; i++)
                {
                    int offset = i * vSize;

                    //Star Data
                    uint starID = file.ReadUInt32(offset + 0);
                    float ra = file.ReadSingle(offset + 4);
                    float dec = file.ReadSingle(offset + 8);
                    double mag = file.ReadDouble(offset + 12);
                    double std = file.ReadDouble(offset + 20);
                    ushort obs = file.ReadUInt16(offset + 28);
                    ushort variability = file.ReadUInt16(offset + 30);
                    ushort catalog = file.ReadUInt16(offset + 32);
                    double astrocolor = file.ReadDouble(offset + 34);
                    double period = file.ReadDouble(offset + 42);
                    double snr = file.ReadDouble(offset + 50);
                    double rms = file.ReadDouble(offset + 58);
                    ushort type = file.ReadUInt16(offset + 66);
                    double pmra = file.ReadDouble(offset + 68);
                    float parallax = file.ReadSingle(offset + 76);

                    //Load Position
                    verts[i] = new Vector3()
                    {
                        x = scales.xScale * (Normalize(ra, limits.ra.x, limits.ra.y) - scales.xOffset),
                        y = scales.yScale * CalculateHeight(parallax),
                        z = scales.zScale * (Normalize(dec, limits.dec.x, limits.dec.y) - scales.zOffset)
                    };

                    //Load Colors
                    cols[i] = new Color()
                    {
                        r = Normalize((float)mag, limits.mag.x, limits.mag.y),
                        g = Normalize((float)std, limits.std.x, limits.std.y),
                        b = Normalize(obs, limits.obs.x, limits.obs.y),
                        a = Normalize(variability, limits.var_flag.x, limits.var_flag.y)
                    };

                    //Load UVs
                    uvs[i] = new Vector4()
                    {
                        x = Normalize(catalog, limits.catalog.x, limits.catalog.y),
                        y = Normalize((float)astrocolor, limits.color.x, limits.color.y),
                        z = Normalize((float)period, limits.period.x, limits.period.y),
                        w = Normalize((float)snr, limits.snr.x, limits.snr.y)
                    };

                    //Load UVBs
                    uvBs[i] = new Vector4()
                    {
                        x = Normalize((float)rms, limits.rms.x, limits.rms.y),
                        y = type,
                        z = Normalize((float)pmra, limits.pmra.x, limits.pmra.y),
                        w = 1.0f
                    };

                    //Load star id as the normal
                    string id = ((int)starID).ToString();
                    nrms[i] = new Vector3()
                    {
                        x = Convert.ToSingle(id.Substring(0, 4)),
                        y = Convert.ToSingle(id.Substring(4, 4)),
                        z = 0.5f
                    };
                }
            }

            //Load trianlge data
            using (var file = mmf.CreateViewAccessor(0x10 + numVert * vSize, numTri * tSize))
            {
                for (int i = 0; i < numTri; i++)
                {
                    int offset = i * tSize;

                    tris[i * 3 + 1] = (int)file.ReadUInt32(offset + 0x0);
                    tris[i * 3 + 0] = (int)file.ReadUInt32(offset + 0x4);
                    tris[i * 3 + 2] = (int)file.ReadUInt32(offset + 0x8);
                }
            }
        }
    }

    public float CalculateHeight(float parallax)
    {
        float h = Normalize(parallax, limits.parallax.x, limits.parallax.y);
        h -= scales.yOffset;

        h = Mathf.Atan(1.5f * h);

        return h;
    }

    //Normalize to range
    public float Normalize(float val, float max, float min)
    {
        return (val - min) / (max - min);
    }

    //Normalize to Power
    public float NormalizePower(float val, float max, float min, float mean, float power)
    {
        mean = (mean - min) / (max - min);
        val = (val - min) / (max - min);
        val = val - mean;

        if (val != 0f) val = (Mathf.Pow(Mathf.Abs(val), power) * (Mathf.Abs(val) / val)) + mean;
        return val;
    }
}