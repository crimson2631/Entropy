using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	internal static class PreferencesEditorWindow
	{
		// General
		private const string PREFERENCES_TITLE = "Bose Wearable";
		private const string PREFERENCES_TITLE_PATH = "Preferences/Bose Wearable";

		// 2019 Search
		#pragma warning disable 0414
		private static readonly string[] BOSE_AR_SDK_KEYWORDS;
		#pragma warning restore 0414

		// Compiler Override
		private const string COMPILER_OPTIONS_HEADER = "Compiler Override Options";
		private const string AUTO_GENERATE_PREFERENCE_LABEL = "Auto-Generate Compiler Override File";
		private const string REGENERATE_COMPILER_FILE_BUTTON = "Regenerate Compiler Override File";

		static PreferencesEditorWindow()
		{
			BOSE_AR_SDK_KEYWORDS = new[]
			{
				"Bose",
				"AR",
				"Bose AR",
				"Wearable"
			};
		}

		#if UNITY_2019_1_OR_NEWER
		[SettingsProvider]
		private static SettingsProvider CreatePreferenceSettingsProvider()
		{
			return new SettingsProvider(PREFERENCES_TITLE_PATH, SettingsScope.User)
			{
				guiHandler = DrawGUI,
				keywords = BOSE_AR_SDK_KEYWORDS
			};
		}

		private static void DrawGUI(string value)
		{
			OnGUI();
		}
		#endif

		#if !UNITY_2019_1_OR_NEWER
		[PreferenceItem(PREFERENCES_TITLE)]
		#endif
		private static void OnGUI()
		{
			EditorGUILayout.LabelField(COMPILER_OPTIONS_HEADER, EditorStyles.boldLabel);

			bool autoGeneratePreference;
			bool newAutoGeneratePreference;
			using (var scope = new EditorGUILayout.HorizontalScope())
			{
				autoGeneratePreference = CompilerOverrideTools.IsCompilerOverrideFileAutoGenerationEnabled();
				EditorGUILayout.LabelField(AUTO_GENERATE_PREFERENCE_LABEL, GUILayout.MinWidth(220f));
				newAutoGeneratePreference = EditorGUILayout.Toggle(autoGeneratePreference);
			}

			if (autoGeneratePreference != newAutoGeneratePreference)
			{
				CompilerOverrideTools.SetAutoGenerateCompilerOverrideFilePreference(newAutoGeneratePreference);
			}

			if (GUILayout.Button(REGENERATE_COMPILER_FILE_BUTTON))
			{
				CompilerOverrideTools.RegenerateCompilerOverrideFile();
			}
		}
	}
}
