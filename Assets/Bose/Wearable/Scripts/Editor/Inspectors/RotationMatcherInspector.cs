using Bose.Wearable.Extensions;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomEditor(typeof(RotationMatcher))]
	public sealed class RotationMatcherInspector : UnityEditor.Editor
	{
		private const string RotationSourceField = "_rotationSource";
		private const string UpdateIntervalField = "_updateInterval";

		private const string DescriptionBox =
			"Automatically rotates the attached GameObject to match the orientation of the connected device. Either " +
			"rotation sensor may be used.";

		private const string RequirementWillBeCreatedInfo =
			"A WearableRequirement component will automatically be added to this GameObject at runtime. You may also " +
			"add one manually to provide more control over device configuration.";

		private const string RequirementRemovedOrDisabledWarning =
			"The WearableRequirement component on this GameObject has been removed or disabled. If no other " +
			"Requirements in your project enable the proper rotation sensor, the Rotation Matcher will not function.";

		private const string RequiredRotationSensorManuallyDisabledWarning =
			"The required rotation sensor has been manually disabled on the attached WearableRequirement component. " +
			"If no other Requirements in your project enable the proper rotation sensor, the Rotation Matcher will " +
			"not function.";

		private const string RequiredUpdateIntervalManuallyLoweredWarning =
			"The update interval on the attached WearableRequirement component has been manually changed to an " +
			"interval slower than that requested above. If no other Requirements in your project request a faster " +
			"rate, the Rotation Matcher will update at a lower rate than requested.";

		private const string RequirementSensorsWillBeAlteredInfo =
			"The attached WearableRequirement does not enable the rotation sensor required by the Rotation Matcher " +
			"component. It will be automatically altered at runtime to enable the proper sensor.";

		private const string RequirementUpdateIntervalWillBeAlteredInfo =
			"The attached WearableRequirement has a slower update interval than that requested above. The requirement " +
			" will be automatically altered at runtime to ensure the proper update interval is reached.";

		private const string CameraFoundWarning =
			"A camera was detected in the hierarchy of this GameObject. Using a RotationMatcher to drive the rotation " +
			"of a camera is not recommended.";

		private SerializedProperty _rotationSource;
		private SerializedProperty _updateInterval;

		private void OnEnable()
		{
			_rotationSource = serializedObject.FindProperty(RotationSourceField);
			_updateInterval = serializedObject.FindProperty(UpdateIntervalField);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			RotationMatcher matcher = target as RotationMatcher;

			CheckAndWarnForCameraUsage(matcher);

			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.PropertyField(_rotationSource);
			EditorGUILayout.PropertyField(_updateInterval);

			WearableRequirement requirement = matcher.GetComponent<WearableRequirement>();

			if (Application.isPlaying)
			{
				if (requirement == null || !requirement.enabled)
				{
					// Runtime, manually removed
					EditorGUILayout.HelpBox(RequirementRemovedOrDisabledWarning, MessageType.Warning);
				}
			}
			else
			{
				if (requirement == null)
				{
					// Editor, no user-provided
					EditorGUILayout.HelpBox(RequirementWillBeCreatedInfo, MessageType.Info);
				}
			}

			if (requirement != null && requirement.enabled)
			{
				// Editor, user provided OR runtime

				// Check sensor validity
				var source = (RotationMatcher.RotationSensorSource) _rotationSource.enumValueIndex;
				SensorId rotationSensorId =
					source == RotationMatcher.RotationSensorSource.SixDof ?
						SensorId.RotationSixDof :
						SensorId.RotationNineDof;
				if (!requirement.DeviceConfig.GetSensorConfig(rotationSensorId).isEnabled)
				{
					if (Application.isPlaying)
					{
						EditorGUILayout.HelpBox(RequiredRotationSensorManuallyDisabledWarning, MessageType.Warning);
					}
					else
					{
						EditorGUILayout.HelpBox(RequirementSensorsWillBeAlteredInfo, MessageType.Info);
					}
				}

				// Check rate validity
				var interval = (SensorUpdateInterval) _updateInterval.enumValueIndex;
				if (requirement.DeviceConfig.updateInterval.IsSlowerThan(interval))
				{
					if (Application.isPlaying)
					{
						EditorGUILayout.HelpBox(RequiredUpdateIntervalManuallyLoweredWarning, MessageType.Warning);
					}
					else
					{
						EditorGUILayout.HelpBox(RequirementUpdateIntervalWillBeAlteredInfo, MessageType.Info);
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void CheckAndWarnForCameraUsage(RotationMatcher matcher)
		{
			var camera = matcher.GetComponentInChildren<Camera>();

			if (camera != null)
			{
				EditorGUILayout.HelpBox(CameraFoundWarning, MessageType.Warning);
			}
		}
	}
}
