using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// A set of helper methods for dealing with compiler override files in the Unity project.
	/// </summary>
	internal static class CompilerOverrideTools
	{
		private static readonly StringBuilder STRING_BUILDER;

		static CompilerOverrideTools()
		{
			STRING_BUILDER = new StringBuilder();
		}

		/// <summary>
		/// Returns true if the scripting runtime appropriate compiler override file exists, otherwise
		/// false.
		/// </summary>
		/// <returns></returns>
		public static bool DoesCompilerOverrideFileExist()
		{
			return File.Exists(GetCompilerOverrideFilePath());
		}

		/// <summary>
		/// Deletes any existing compiler override file if present and creates a new one appropriate for the currently
		/// set scripting runtime version.
		/// </summary>
		public static void RegenerateCompilerOverrideFile()
		{
			if (DoesCompilerOverrideFileExist() && !EditorUtility.DisplayDialog(
				    WearableEditorConstants.COMPILER_OVERRIDE_FILE_DELETION_WARNING_TITLE,
					string.Format(
						WearableEditorConstants.COMPILER_OVERRIDE_FILE_DELETION_WARNING_FORMAT,
						GetUnityCompilerOverrideFilePath()),
				    WearableEditorConstants.OK_BUTTON_LABEL,
				    WearableEditorConstants.CANCEL_BUTTON_LABEL))
			{
				return;
			}

			DeleteCompilerOverrideFile();
			CreateCompilerOverrideFile();
		}

		/// <summary>
		/// Deletes any compiler override file in the project for the current scripting runtime if present.
		/// Returns true if this file does not exist or was successfully deleted, otherwise false.
		/// </summary>
		/// <returns></returns>
		public static void DeleteCompilerOverrideFile()
		{
			if (AssetDatabase.DeleteAsset(GetUnityCompilerOverrideFilePath()))
			{
				Debug.LogFormat(
					WearableEditorConstants.DELETED_COMPILER_FILE_SUCCESS_FORMAT,
					GetCompilerOverrideFilename());
			}
		}

		/// <summary>
		/// If present, deletes any unused compiler override files based on the current scripting runtime
		/// version.
		/// </summary>
		public static void DeleteUnusedCompilerOverrideFile()
		{
			if (AssetDatabase.DeleteAsset(GetUnityIncompatibleCompilerOverrideFilePath()))
			{
				Debug.LogFormat(
					WearableEditorConstants.DELETED_UNUSED_COMPILER_FILE_SUCCESS_FORMAT,
					GetIncompatibleCompilerOverrideFilename());
			}
		}

		/// <summary>
		/// Creates a compiler override file if none exists. If one does exist, an assertion is thrown.
		/// </summary>
		public static void CreateCompilerOverrideFile()
		{
			Assert.IsFalse(DoesCompilerOverrideFileExist());

			var compilerOverrideFilepath = GetCompilerOverrideFilePath();
			File.WriteAllText(compilerOverrideFilepath, CreateCompilerOverrideFileContent());
			AssetDatabase.ImportAsset(GetUnityCompilerOverrideFilePath());

			Debug.LogFormat(
				WearableEditorConstants.CREATED_COMPILER_FILE_SUCCESS_FORMAT,
				GetCompilerOverrideFilename());
		}

		/// <summary>
		/// Returns a Unity Project Asset path for the appropriate compiler override file name.
		/// </summary>
		/// <returns></returns>
		public static string GetUnityCompilerOverrideFilePath()
		{
			return Path.Combine(WearableEditorConstants.ASSETS_FOLDER_NAME, GetCompilerOverrideFilename());
		}

		/// <summary>
		/// Returns a Unity Project Asset path for the appropriate incompatible compiler override file name.
		/// </summary>
		/// <returns></returns>
		public static string GetUnityIncompatibleCompilerOverrideFilePath()
		{
			return Path.Combine(WearableEditorConstants.ASSETS_FOLDER_NAME, GetIncompatibleCompilerOverrideFilename());
		}

		/// <summary>
		/// Returns a full path to the compiler override file in the Unity Project.
		/// </summary>
		/// <returns></returns>
		public static string GetCompilerOverrideFilePath()
		{
			return Path.Combine(Path.GetFullPath(Application.dataPath), GetCompilerOverrideFilename());
		}

		/// <summary>
		/// Returns a compiler override filename with extension for the appropriate scripting runtime version.
		/// </summary>
		/// <returns></returns>
		public static string GetCompilerOverrideFilename()
		{
			if (PlayerSettingsTools.IsDotNet35())
			{
				return WearableEditorConstants.MCS_FILENAME;
			}
			else if (PlayerSettingsTools.IsDotNet4OrLater())
			{
				return WearableEditorConstants.CSC_FILENAME;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns a filename with extension for the compiler override file incompatible with the current
		/// scripting runtime version.
		/// </summary>
		/// <returns></returns>
		public static string GetIncompatibleCompilerOverrideFilename()
		{
			if (PlayerSettingsTools.IsDotNet35())
			{
				return WearableEditorConstants.CSC_FILENAME;
			}
			else if (PlayerSettingsTools.IsDotNet4OrLater())
			{
				return WearableEditorConstants.MCS_FILENAME;
			}

			return string.Empty;
		}

		/// <summary>
		/// Returns true if user preference has been set to auto-generate the appropriate compiler override
		/// file, otherwise returns false.
		/// </summary>
		/// <returns></returns>
		public static bool IsCompilerOverrideFileAutoGenerationEnabled()
		{
			if (EditorPrefs.HasKey(WearableEditorConstants.USE_COMPILER_OVERRIDE_FILE_KEY))
			{
				return EditorPrefs.GetBool(WearableEditorConstants.USE_COMPILER_OVERRIDE_FILE_KEY);
			}

			EditorPrefs.SetBool(
				WearableEditorConstants.USE_COMPILER_OVERRIDE_FILE_KEY,
				WearableEditorConstants.DEFAULT_AUTO_GENERATE_COMPILER_FILE_PREF);

			return true;
		}

		/// <summary>
		/// Sets the user-preference to auto-generate the compiler override file or not.
		/// </summary>
		/// <param name="isEnabled"></param>
		public static void SetAutoGenerateCompilerOverrideFilePreference(bool isEnabled)
		{
			EditorPrefs.SetBool(WearableEditorConstants.USE_COMPILER_OVERRIDE_FILE_KEY, isEnabled);
		}

		/// <summary>
		/// Creates a multi-line string containing content for a C# compiler override file.
		/// </summary>
		/// <returns></returns>
		public static string CreateCompilerOverrideFileContent()
		{
			STRING_BUILDER.Length = 0;
			for (var i = 0; i < WearableEditorConstants.COMPILER_OVERRIDE_ARGUMENTS.Length; i++)
			{
				STRING_BUILDER.AppendLine(WearableEditorConstants.COMPILER_OVERRIDE_ARGUMENTS[i]);
			}

			return STRING_BUILDER.ToString();
		}
	}
}
