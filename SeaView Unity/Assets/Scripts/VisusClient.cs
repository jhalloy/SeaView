using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MessagePack;
using System.Threading.Tasks;
using MyBox;

[MessagePackObject]
public class VisusPayload
{
    [Key("shape")]
    public int[] shape { get; set; }

    [Key("dtype")]
    public string dtype { get; set; }

    [Key("u_array")]
    public byte[] u_array { get; set; }

    [Key("v_array")]
    public byte[] v_array { get; set; }

    [Key("w_array")]
    public byte[] w_array { get; set; }
}

public class VisusArrays
{
    public VisusArrays(int[] shape, float[] u, float[] v, float[] w) {
        this.shape = shape;
        this.u = u;
        this.v = v;
        this.w = w;
    }
    public int[] shape;
    public float[] u;
    public float[] v;
    public float[] w;
}

public class VisusClient : MonoBehaviour
{
    public static VisusClient Instance { get; private set;}

    const string baseUrl = "http://localhost:5000/";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public async Task<VisusArrays> RequestVisusDataAsync(int quality, int time, int[] z, int[] x_range, int[] y_range)
    {
        string url = BuildQueryUrl(quality, time, z, x_range, y_range);
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        TaskCompletionSource<VisusArrays> tcs = new TaskCompletionSource<VisusArrays>();

        request.SendWebRequest().completed += (asyncOperation) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to get data from flask server.");
                tcs.SetResult(null);
            }
            else
            {
                byte[] rawData = request.downloadHandler.data;

                VisusPayload payload = MessagePackSerializer.Deserialize<VisusPayload>(rawData);

                float[] u = ByteArrayToFloatArray(payload.u_array);
                float[] v = ByteArrayToFloatArray(payload.v_array);
                float[] w = ByteArrayToFloatArray(payload.w_array);

                Debug.Log($"DATA REQUEST\nshape: ({payload.shape[0]}, {payload.shape[1]}, {payload.shape[2]})\nu[0]: {u[0]}\nv[0]: {v[0]}\nw[0]: {w[0]}");

                tcs.SetResult(new VisusArrays(payload.shape, u, v, w));
            }
        };

        return await tcs.Task;

        // var operation = request.SendWebRequest();

        // while (!operation.isDone)
        //     await Task.Yield();

        // if (request.result != UnityWebRequest.Result.Success)
        //     throw new Exception(request.error);

        // byte[] rawData = request.downloadHandler.data;
        
        // VisusPayload payload = MessagePackSerializer.Deserialize<VisusPayload>(rawData);
        // float[] u = ByteArrayToFloatArray(payload.u_array);
        // float[] v = ByteArrayToFloatArray(payload.v_array);
        // float[] w = ByteArrayToFloatArray(payload.w_array);

        // // Debug prints
        // Debug.Log($"u[0] = {u[0]}");
        // Debug.Log($"v[0] = {v[0]}");
        // Debug.Log($"w[0] = {w[0]}");

        // return new VisusArrays(payload.shape, u, v, w);
    }

    //public VisusArrays RequestVisusData(int quality, int time, int[] z, int[] x_range, int[] y_range) {
    //    StartCoroutine(RequestVisusData_coroutine(quality, time, z, x_range, y_range));
    //}

    string BuildQueryUrl(int quality, int time, int[] z, int[] x_range, int[] y_range)
    {
        var sb = new StringBuilder($"{baseUrl}?quality={quality}&time={time}");
        AppendRange(sb, "z", z);
        AppendRange(sb, "x_range", x_range);
        AppendRange(sb, "y_range", y_range);
        return sb.ToString();
    }

    void AppendRange(StringBuilder sb, string name, int[] values)
    {
        foreach (int v in values)
            sb.Append($"&{name}={v}");
    }

    float[] ByteArrayToFloatArray(byte[] bytes)
    {
        int len = bytes.Length / sizeof(float);
        float[] floats = new float[len];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }

    [ButtonMethod]
    private async void TestRequest()
    {
        // Example call
        await RequestVisusDataAsync(
            quality: 0,
            time: 0,
            z: new int[] { 0, 1 },
            x_range: new int[] { 0, 200 },
            y_range: new int[] { 0, 200 }
        );
    }
}
