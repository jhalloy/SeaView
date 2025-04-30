using UnityEngine;
using System.Collections.Generic;

public class Visualizer : MonoBehaviour
{
    #region Fields

    [Header("Mesh & Material")]
    public Mesh instanceMesh;
    public Material instanceMaterial;

    [Header("Grid Settings")]
    public int gridSizeX = 10;
    public int gridSizeY = 5;
    public int gridSizeZ = 10;
    public float spacing = 2f;

    [Header("Colors / Magnitude Range")]
    public Gradient gradient;
    public float maxSpeed = 10; // These are used to reference the gradient for coloring (m/s)
    public float minSpeed = 0;

    #endregion

    #region Properties

    private List<Matrix4x4[]> batches = new List<Matrix4x4[]>();
    private const int batchSize = 1023; // Max allowed per DrawMeshInstanced call

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

    void GenerateGrid()
    {
        List<Matrix4x4> allMatrices = new List<Matrix4x4>();

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 position = new Vector3(x * spacing, y * spacing, z * spacing);
                    Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                    allMatrices.Add(matrix);
                }
            }
        }

        // Split into batches of 1023
        for (int i = 0; i < allMatrices.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, allMatrices.Count - i);
            batches.Add(allMatrices.GetRange(i, count).ToArray());
        }
    }

    #endregion
}
