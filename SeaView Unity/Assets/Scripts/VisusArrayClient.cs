using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MessagePack;
using System.Threading.Tasks;

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

public struct VisusArrays
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

public class VisusArrayClient : MonoBehaviour
{
    const string baseUrl = "http://localhost:5000/";

    async Task Start()
    {
        // Example call
        await(RequestVisusDataAsync(
            quality: 0,
            time: 0,
            z: new int[] { 0, 1 },
            x_range: new int[] { 0, 200 },
            y_range: new int[] { 0, 200 }
        ));
    }

    
    public async Task<VisusArrays> RequestVisusDataAsync(int quality, int time, int[] z, int[] x_range, int[] y_range)
    {
        string url = BuildQueryUrl(quality, time, z, x_range, y_range);
        using UnityWebRequest www = UnityWebRequest.Get(url);
        www.downloadHandler = new DownloadHandlerBuffer();

        var operation = www.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (www.result != UnityWebRequest.Result.Success)
            throw new Exception(www.error);

        byte[] rawData = www.downloadHandler.data;
        
        VisusPayload payload = MessagePackSerializer.Deserialize<VisusPayload>(rawData);
        float[] u = ByteArrayToFloatArray(payload.u_array);
        float[] v = ByteArrayToFloatArray(payload.v_array);
        float[] w = ByteArrayToFloatArray(payload.w_array);
        Debug.Log($"u[0] = {u[0]}");
        Debug.Log($"v[0] = {v[0]}");
        Debug.Log($"w[0] = {w[0]}");
        return new VisusArrays(payload.shape, u, v, w);
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
}
