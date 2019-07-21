using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// Helper methods for dealing with project assets
	/// </summary>
	internal static class ProjectTools
	{
		// File Verbs
		private const string FILE_VERB_OPEN = "Open";

		// Project Assets
		private const string PDF_ASSET_GUID = "9cd3f8ee5111ed349b0756c5c07e176b";

		/// <summary>
		/// Opens the local documentation PDF if present.
		/// </summary>
		public static void OpenLocalHelpDocumentation()
		{
			var unityPdfAssetPath = AssetDatabase.GUIDToAssetPath(PDF_ASSET_GUID);
			if (string.IsNullOrEmpty(unityPdfAssetPath))
			{
				Debug.LogWarningFormat(WearableEditorConstants.LOCAL_PDF_NOT_FOUND, PDF_ASSET_GUID);
				return;
			}

			OpenFile(GetLocalDocumentationPdfFilePath());
		}

		/// <summary>
		/// Returns true if the local PDF documentation is present, otherwise false.
		/// </summary>
		/// <returns></returns>
		public static bool IsLocalPdfDocumentationPresent()
		{
			var unityPdfAssetPath = AssetDatabase.GUIDToAssetPath(PDF_ASSET_GUID);
			return AssetDatabase.LoadMainAssetAtPath(unityPdfAssetPath) != null;
		}

		/// <summary>
		/// Opens a file using the OS default program at <paramref name="filePath"/>.
		/// </summary>
		/// <param name="filePath"></param>
		public static void OpenFile(string filePath)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = filePath,
				Verb = FILE_VERB_OPEN
			});
		}

		/// <summary>
		/// Returns a path to the local documentation PDF
		/// </summary>
		/// <returns></returns>
		public static string GetLocalDocumentationPdfFilePath()
		{
			var unityPdfAssetPath = AssetDatabase.GUIDToAssetPath(PDF_ASSET_GUID);
			return Path.Combine(GetProjectPath(), unityPdfAssetPath);
		}

		/// <summary>
		/// Returns a path to the Unity project folder.
		/// </summary>
		/// <returns></returns>
		public static string GetProjectPath()
		{
			return Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		}
	}
}
