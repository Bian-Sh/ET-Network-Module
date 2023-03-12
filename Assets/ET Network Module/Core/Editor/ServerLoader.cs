using System.Diagnostics;
using System.IO;
using UnityEditor;r
using UnityEngine;

public static class ServerLoader
{
    [MenuItem("Tools/Start Test Server", priority = 0)]
    static void StarServer()
    {
        Process pr = new Process();
        pr.StartInfo.WorkingDirectory = Path.Combine(Application.dataPath, "..", "Server/exe");
        pr.StartInfo.FileName = "Server.exe";
        pr.Start();
    }
}
