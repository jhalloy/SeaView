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
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public float meshScale = 1f;

    [Header("Grid Settings")]
    public float spacing = 1f;

    [Header("Colors / Magnitude Range")]
    public Gradient gradient;
    public float maxSpeed = 10; // These are used to reference the gradient for coloring (m/s)
    public float minSpeed = 0;

    #endregion

    #region Properties

    private List<Matrix4x4[]> batches = new List<Matrix4x4[]>();
    private const int batchSize = 1023; // Max allowed per DrawMeshInstanced call

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
        GenerateGrid();
    }

    void Update()
    {
        foreach (var batch in batches)
        {
            Graphics.DrawMeshInstanced(instanceMesh, 0, instanceMaterial, batch);
        }
    }

    #endregion

    #region Methods

    // Gets the data from the API client and builds the grid.
    private async void GenerateGrid()
    {
        onLoadingStateChanged?.Invoke(true);

        List<Matrix4x4> allMatrices = new List<Matrix4x4>();

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

                        Vector3 position = new Vector3(x * spacing, z * spacing, y * spacing);
                        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(new Vector3(u[z][y][x], v[z][y][x], w[z][y][x]).normalized, Vector3.up), Vector3.one * meshScale);
                        allMatrices.Add(matrix);
                    }
                }
            }

            // Split into batches of 1023 (the max number of instances we can draw)
            for (int i = 0; i < allMatrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, allMatrices.Count - i);
                batches.Add(allMatrices.GetRange(i, count).ToArray());
            }
        });

        onLoadingStateChanged?.Invoke(false); 
    }

    #endregion
}
