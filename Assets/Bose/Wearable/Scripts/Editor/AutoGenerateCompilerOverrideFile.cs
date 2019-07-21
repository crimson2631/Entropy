using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	internal static class AutoGenerateCompilerOverrideFile
	{
		/// <summary>
		/// On assembly load, if we're configured to auto-generate the compiler override files delete any
		/// legacy or unused files for the current scripting runtime version and generate if it does not
		/// exist the appropriate compiler override file for the current scripting runtime version.
		/// </summary>
		[InitializeOnLoadMethod]
		public static void TryGenerateCompilerOverrideFile()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			if (!CompilerOverrideTools.IsCompilerOverrideFileAutoGenerationEnabled())
			{
				return;
			}

			CompilerOverrideTools.DeleteUnusedCompilerOverrideFile();

			if (CompilerOverrideTools.DoesCompilerOverrideFileExist())
			{
				// If the compiler file already exists, check for all required arguments and warn if not present.
				var filePath = CompilerOverrideTools.GetCompilerOverrideFilePath();
				var fileContents = File.ReadAllText(filePath);
				for (var i = 0; i < WearableEditorConstants.COMPILER_OVERRIDE_ARGUMENTS.Length; i++)
				{
					var arg = WearableEditorConstants.COMPILER_OVERRIDE_ARGUMENTS[i];
					if (!fileContents.Contains(arg))
					{
						Debug.LogWarningFormat(
							WearableEditorConstants.COMPILER_ARGUMENT_NOT_FOUND_FORMAT,
							arg,
							filePath);
					}
				}

				return;
			}

			CompilerOverrideTools.CreateCompilerOverrideFile();
		}
	}
}
