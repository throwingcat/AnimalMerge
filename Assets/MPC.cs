using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class MPC
{
    [MenuItem("Window/MessagePack/My Generator")]
    // MessagePack Code 생성.
    public static async void MessagePackCodeGen()
    {
        Debug.Log("MessagePack Code Generate Start");
        string log = await InvokeProcessStartAsync();
        Debug.Log(log);

        AssetDatabase.Refresh();
    }

    // MessagePack CodeGen Task 생성.
    private static Task<string> InvokeProcessStartAsync()
    {
        var psi = new System.Diagnostics.ProcessStartInfo()
        {
            CreateNoWindow = true,
            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = "mpc",
            Arguments = "-i ./Scripts/ -o ./Scripts/MessagePackGenerated.cs",
            WorkingDirectory = Application.dataPath
        };

        System.Diagnostics.Process p;
        try
        {
            p = System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            return Task.FromException<string>(ex);
        }

        var tcs = new TaskCompletionSource<string>();
        p.EnableRaisingEvents = true;
        p.Exited += (object sender, EventArgs e) =>
        {
            var data = p.StandardOutput.ReadToEnd();
            p.Dispose();
            p = null;

            tcs.TrySetResult(data);
        };

        return tcs.Task;
    }
}
