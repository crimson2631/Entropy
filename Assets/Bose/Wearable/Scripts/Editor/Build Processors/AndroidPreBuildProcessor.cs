#if UNITY_ANDROID

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	internal sealed class AndroidPreBuildProcessor
		#if UNITY_2018_1_OR_NEWER
		: IPreprocessBuildWithReport
		#else
        : IPreprocessBuild
        #endif
	{
		public int callbackOrder
		{
			get { return WearableEditorConstants.ANDROID_PRE_BUILD_PROCESSOR_ORDER; }
		}

		#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
		{
			Process();
		}
		#else
		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			Process();
		}
        #endif

		private void Process()
		{
			// Make sure the target Android version is at or above the minimum.
			int minSdkVersion = (int)PlayerSettings.Android.minSdkVersion;
			if (minSdkVersion < WearableEditorConstants.MINIMUM_SUPPORTED_ANDROID_VERSION)
			{
				PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)WearableEditorConstants.MINIMUM_SUPPORTED_ANDROID_VERSION;

				var msg = string.Format(
					WearableEditorConstants.ANDROID_VERSION_ALTERATION_WARNING_WITH_MESSAGE,
					WearableEditorConstants.MINIMUM_SUPPORTED_ANDROID_VERSION,
					minSdkVersion
				);
				Debug.LogWarning(msg);
			}
		}
	}
}

#endif
