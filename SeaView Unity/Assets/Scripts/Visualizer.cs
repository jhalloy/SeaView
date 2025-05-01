using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Visualizer : MonoBehaviour
{
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
        List<Matrix4x4> allMatrices = new List<Matrix4x4>();

        // Get the data
        await Task.Run(async () => 
        {
            var data = await visusClient.RequestVisusDataAsync(
                quality: 0, 
                time: 0, 
                z: new int[] { 0, 1 },
                x_range: new int[] { 0, 200 },
                y_range: new int[] { 0, 200 }
            );

            float[] u = data.u;
            float[] v = data.v;
            float[] w = data.w;

            for (int x = 0; x < u.Length; x++)
            {
                for (int y = 0; y < v.Length; y++)
                {
                    for (int z = 0; z < w.Length; z++)
                    {
                        Debug.Log($"{x}, {y}, {z}");
                        Vector3 position = new Vector3(x * spacing, y * spacing, z * spacing);
                        Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.LookRotation(new Vector3(u[x], v[y], w[z]).normalized, Vector3.up), Vector3.one * meshScale);
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
    }

    #endregion
}
