using UnityEditor;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// Helper methods for handling Unity Editor settings.
	/// </summary>
	internal static class PlayerSettingsTools
	{
		/// <summary>
		/// Returns true if .Net35 is the scripting runtime, otherwise false.
		/// </summary>
		public static bool IsDotNet35()
		{
			return PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy;
		}

		/// <summary>
		/// Returns true if .Net4 or later is the scripting runtime, otherwise false.
		/// </summary>
		public static bool IsDotNet4OrLater()
		{
			return PlayerSettings.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest;
		}
	}
}
