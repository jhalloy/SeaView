using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UltEvents;
using NumSharp;

public class Visualizer : MonoBehaviour
{
    public UltEvent<bool> onLoadingStateChanged; // True if we're waiting on a request, false if not.

    public static Visualizer Instance;

    #region Fields

    [Header("Mesh & Material")]
    public Mesh arrowMesh;
    public float arrowScale = 1f;
    public Mesh landMesh;
    public float landScale = 1f;
    public Material arrowMaterial;
    public Material landMaterial;

    [Header("Grid Settings")]
    public float spacing = 1f;

    [Header("Colors / Magnitude Range")]
    public Gradient velocityGradient;
    // public Color landColor = Color.green;
    [HideInInspector] public float maxSpeed = float.MinValue; // used to reference the gradient for coloring (m/s)

    #endregion

    #region Properties

    private List<Matrix4x4[]> arrowBatches = new List<Matrix4x4[]>();
    private List<Matrix4x4[]> tempArrowBatches = new List<Matrix4x4[]>();
    private List<Matrix4x4[]> landBatches = new List<Matrix4x4[]>();
    private List<Matrix4x4[]> tempLandBatches = new List<Matrix4x4[]>();
    private const int batchSize = 1023; // Max allowed per DrawMeshInstanced call
    private List<MaterialPropertyBlock> propBlocks = new List<MaterialPropertyBlock>();

    private VisusClient visusClient => VisusClient.Instance;

    #endregion

    #region Mono

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        maxSpeed = float.MinValue;

        GenerateGrid();
    }

    void Update()
    {
        for (int i = 0; i < arrowBatches.Count; i++)
        {
            Graphics.DrawMeshInstanced(arrowMesh, 0, arrowMaterial, arrowBatches[i], arrowBatches[i].Length, propBlocks[i]);
        }
        foreach (var batch in landBatches)
        {
            Graphics.DrawMeshInstanced(landMesh, 0, landMaterial, batch);
        }
    }

    #endregion

    #region Methods

    // Gets the data from the API client and builds the grid.
    private async void GenerateGrid()
    {
        onLoadingStateChanged?.Invoke(true);

        List<Matrix4x4> arrowMatrices = new List<Matrix4x4>();
        List<float> speeds = new List<float>();
        List<Vector4> arrowColors = new List<Vector4>();
        List<Matrix4x4> landMatrices = new List<Matrix4x4>();

        var data = await visusClient.RequestVisusDataAsync(
            quality: -9, 
            time: 0, 
            z: new int[] { 0, 1 },
            x_range: new int[] { 0, 8640 },
            y_range: new int[] { 0, 6480 }
        );

        // Get the data
        await Task.Run(() => 
        {
            var u = new NDArray(data.u).reshape(data.shape);
            var v = new NDArray(data.v).reshape(data.shape);
            var w = new NDArray(data.w).reshape(data.shape);

            for (int x = 0; x < data.shape[2]; x++)
            {
                for (int y = 0; y < data.shape[1]; y++)
                {
                    for (int z = 0; z < data.shape[0]; z++)
                    {
                        // Debug.Log($"{x}, {y}, {z}");

                        Vector3 vel = new Vector3(u[z][y][x], v[z][y][x], w[z][y][x]);

                        Vector3 position = new Vector3(x * spacing, z * spacing, y * spacing);

                        if (vel.magnitude > 0)
                        {
                            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(vel.normalized, Vector3.up), Vector3.one * arrowScale);
                            arrowMatrices.Add(matrix);

                            float speed = vel.magnitude;
                            maxSpeed = Mathf.Max(speed, maxSpeed);
                            speeds.Add(speed);
                        }
                        else
                        {
                            Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * landScale);
                            landMatrices.Add(matrix);
                        }
                    }
                }
            }

            // Split into batches of 1023 (the max number of instances we can draw)
            tempArrowBatches.Clear();
            for (int i = 0; i < arrowMatrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, arrowMatrices.Count - i);
                tempArrowBatches.Add(arrowMatrices.GetRange(i, count).ToArray());
            }
            tempLandBatches.Clear();
            for (int i = 0; i < landMatrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, landMatrices.Count - i);
                tempLandBatches.Add(landMatrices.GetRange(i, count).ToArray());
            }
        });

        // Material blocks
        foreach (var speed in speeds)
        {
            float t = Mathf.Clamp01(Mathf.Min(speed, maxSpeed) / maxSpeed);
            Color color = velocityGradient.Evaluate(t);
            arrowColors.Add(color);
        }
        for (int i = 0; i < arrowMatrices.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, arrowMatrices.Count - i);
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetVectorArray("_Color", arrowColors.GetRange(i, count));
            propBlocks.Add(props);
        }

        arrowBatches = tempArrowBatches;
        landBatches = tempLandBatches;

        Debug.Log($"Max Speed from batch: {maxSpeed}");

        onLoadingStateChanged?.Invoke(false); 
    }

    #endregion
}
