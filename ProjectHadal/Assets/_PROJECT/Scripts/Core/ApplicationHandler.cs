using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Hadal
{
    public static class ApplicationHandler
    {
		private static readonly string ApplicationPath = Application.dataPath.Replace("_Data", ".exe");
		
		public static void RestartApp()
		{
			#if UNITY_EDITOR
			Debug.LogWarning("In unity editor and cannot restart. Can only restart application in build.");
			#else
			Process.Start(ApplicationPath);
			Application.Quit();
			#endif
		}
    }
}