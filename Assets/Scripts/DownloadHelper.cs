using System.Runtime.InteropServices;
using UnityEngine;

public static class DownloadFileHelper
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadString(string content, string filename);
#endif

    public static void DownloadToFile(string content, string filename)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadString(content, filename);
#endif
    }
}
