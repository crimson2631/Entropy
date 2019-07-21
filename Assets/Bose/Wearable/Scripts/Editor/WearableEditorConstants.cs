
using UnityEngine;

namespace Bose.Wearable.Editor
{
	/// <summary>
	/// An internal helper class to contain constants for editor usage.
	/// </summary>
	internal static class WearableEditorConstants
	{
		// Compiler Override
		public static readonly string[] COMPILER_OVERRIDE_ARGUMENTS;

		// Files and folders
		public const string ASSETS_FOLDER_NAME = "Assets";
		public const string CSC_FILENAME = "csc.rsp";
		public const string MCS_FILENAME = "mcs.rsp";

		// Logs
		public const string DELETED_COMPILER_FILE_SUCCESS_FORMAT
			= "[Bose Wearable] Deleted compiler override file: [{0}].";

		public const string DELETED_UNUSED_COMPILER_FILE_SUCCESS_FORMAT
			= "[Bose Wearable] Deleted unused compiler override file: [{0}].";

		public const string CREATED_COMPILER_FILE_SUCCESS_FORMAT
			= "[Bose Wearable] Created compiler override file: [{0}].";

		public const string COMPILER_ARGUMENT_NOT_FOUND_FORMAT
			= "[Bose Wearable] Required compiler argument {0} not found in [{1}], please add this manually.";

		public const string LOCAL_PDF_NOT_FOUND
			= "[Bose Wearable] Could not find the local PDF documentation with GUID [{0}] to open, please reimport this asset";

		// Editor Prefs
		public const string USE_COMPILER_OVERRIDE_FILE_KEY = "bose_use_compiler_override_key";

		public const bool DEFAULT_AUTO_GENERATE_COMPILER_FILE_PREF = true;

		// UI
		public const string OK_BUTTON_LABEL = "OK";
		public const string CANCEL_BUTTON_LABEL = "Cancel";

		public const string COMPILER_OVERRIDE_FILE_DELETION_WARNING_TITLE = "Compiler Override File Exists";

		public const string COMPILER_OVERRIDE_FILE_DELETION_WARNING_FORMAT
			= "Regenerating the compiler override will delete the existing file [{0}], is it " +
			  "OK to proceed?";

		// Inspector
		public const float SINGLE_LINE_HEIGHT = 20f;
		public static readonly GUILayoutOption[] EMPTY_LAYOUT_OPTIONS;

		public const string DEVICE_SPECIFIC_GESTURE_DISCOURAGED_WARNING =
			"Use of device-specific gestures is discouraged as it may not be supported on all Bose AR devices. " +
			"Consider using a device-agnostic gesture.";

		#region Platform Constants: iOS

		// Post Build Processor
		public const int XCODE_PRE_BUILD_PROCESSOR_ORDER = 0;
		public const int XCODE_POST_BUILD_PROCESSOR_ORDER = 0;

		// Xcode Workspace
		public const string XCODE_PROJECT_NAME = "Unity-iPhone.xcodeproj/project.pbxproj";

		// Framework path was broken in 2018.3.0->2018.3.3
		// From 2018.3.4 Release notes:
		// *  iOS: Fixed iOS Frameworks location is ignored when building Xcode project. (1108970)
		#if UNITY_2018_3_0 || UNITY_2018_3_1 || UNITY_2018_3_2 || UNITY_2018_3_3

		public const string XcodeProjectFrameworksPath = "Frameworks";

		#else

		public const string XCODE_PROJECT_FRAMEWORKS_PATH = "Frameworks/Plugins/iOS/BoseWearable";

		#endif

		// Messages
		public const string ARCHITECTURE_ALTERATION_WARNING_WITH_MESSAGE = "[Bose Wearable] iOS Architecture forced to 'ARM64'. " +
		                                                               "Was set to '{0}'.";
		public const string OS_VERSION_ALTERATION_WARNING_WITH_MESSAGE = "[Bose Wearable] iOS Minimum Version forced to '{0}'. Was set to '{1}'.";

		public const string OS_BLUETOOTH_ALTERATION_WARNING = "[Bose Wearable] Background Mode forced to allow connections to BLE devices.";

		// Info.plist properties
		public const string XCODE_INFO_PLIST_RELATIVE_PATH = "./Info.plist";
		public const string XCODE_INFO_PLIST_BLUETOOTH_KEY = "NSBluetoothPeripheralUsageDescription";
		public const string XCODE_INFO_PLIST_BLUETOOTH_MESSAGE = "This app uses Bluetooth to communicate with Bose AR devices.";
		public const string XCODE_INFO_PLIST_ALTERATION_WARNING_WITH_MESSAGE = "[Bose Wearable] Added missing property to Info.plist: {0}: {1}";

		// Xcode Build Properties and Values
		public const string XCODE_BUILD_PROPERTY_ENABLE_VALUE = "TRUE";
		public const string XCODE_BUILD_PROPERTY_DISABLE_VALUE = "FALSE";
		public const string XCODE_BUILD_PROPERTY_BIT_CODE_KEY = "ENABLE_BITCODE";
		public const string XCODE_BUILD_PROPERTY_MODULES_KEY = "CLANG_ENABLE_MODULES";
		public const string XCODE_BUILD_PROPERTY_SEARCH_PATHS_KEY = "LD_RUNPATH_SEARCH_PATHS";
		public const string XCODE_BUILD_PROPERTY_SEARCH_PATHS_VALUE = "@executable_path/Frameworks";
		public const string XCODE_BUILD_PROPERTY_EMBED_SWIFT_KEY = "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES";
		public const string XCODE_BUILD_PROPERTY_SWIFT_VERSION_KEY = "SWIFT_VERSION";
		public const string XCODE_BUILD_PROPERTY_SWIFT_VERSION_VALUE = "5.0";
		public const string XCODE_BUILD_PROPERTY_SWIFT_OPTIMIZATION_KEY = "SWIFT_OPTIMIZATION_LEVEL";
		public const string XCODE_BUILD_PROPERTY_SWIFT_OPTIMIZATION_VALUE = "-Onone";

		// Framework Path and Names
		public const string FRAMEWORK_FILE_FILTER = "*.framework";

		// Supported iOS Versions
		public const float MINIMUM_SUPPORTEDI_OS_VERSION = 11.4f;

		/// <summary>
		/// This corresponds to the location of the native plugins in the Unity project.
		/// </summary>
		public const string NATIVE_ARTIFACTS_PATH = "Plugins/iOS/BoseWearable/";

		#endregion

		#region Platform Constants: Android

		// Build Order
		public const int ANDROID_PRE_BUILD_PROCESSOR_ORDER = 0;

		// Build Tools Messages
		public const string BUILD_TOOLS_UNSUPPORTED_PLATFORM_WARNING = "[Bose Wearable] Trying to build for unsupported platform {0}.";

		// Messages
		public const string ANDROID_VERSION_ALTERATION_WARNING_WITH_MESSAGE = "[Bose Wearable] Android Minimum SDK Version forced to '{0}'. Was set to '{1}'.";

		// Supported Android SDK Versions
		public const int MINIMUM_SUPPORTED_ANDROID_VERSION = 22;

		#endregion

		static WearableEditorConstants()
		{
			COMPILER_OVERRIDE_ARGUMENTS = new[]
			{
				"-unsafe"
			};

			EMPTY_LAYOUT_OPTIONS = new GUILayoutOption[0];
		}
	}
}
