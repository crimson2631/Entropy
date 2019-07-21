using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor
{
	[CustomPropertyDrawer(typeof(WearableBluetoothProvider))]
	internal sealed class WearableBluetoothProviderDrawer : PropertyDrawer
	{
		private const string SIMULATE_HARDWARE_DEVICES_FIELD = "_simulateHardwareDevices";
		private const string DEBUG_LOGGING_FIELD = "_debugLogging";

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			EditorGUILayout.PropertyField(
				property.FindPropertyRelative(SIMULATE_HARDWARE_DEVICES_FIELD),
				WearableEditorConstants.EMPTY_LAYOUT_OPTIONS);

			EditorGUILayout.PropertyField(
				property.FindPropertyRelative(DEBUG_LOGGING_FIELD),
				WearableEditorConstants.EMPTY_LAYOUT_OPTIONS);

			EditorGUI.EndProperty();
		}
	}
}
