using System;
using UnityEngine;

namespace Bose.Wearable
{
	[Serializable]
	internal sealed partial class WearableBluetoothProvider : WearableProviderBase
	{
		#region Provider Unique
		/// <summary>
		/// Represents a session with an Wearable Device.
		/// </summary>
		private enum SessionStatus
		{
			Closed,
			Opening,
			Open
		}

		private ConnectionStatus _currentConnectionStatus;

		/// <summary>
		/// The RSSI threshold below which devices will be filtered out.
		/// </summary>
		public int RSSIFilterThreshold
		{
			get
			{
				return _RSSIFilterThreshold == 0
					? WearableConstants.DEFAULT_RSSI_THRESHOLD
					: _RSSIFilterThreshold;
			}
		}

		/// <summary>
		/// Sets the Received Signal Strength Indication (RSSI) filter level; devices underneath the rssiThreshold filter
		/// threshold will not be made available to connect to. A valid value for <paramref name="rssiThreshold"/> is
		/// set between -70 and -30; anything outside of that range will be clamped to the nearest allowed value.
		/// </summary>
		/// <param name="rssiThreshold"></param>
		public void SetRssiFilter(int rssiThreshold)
		{
			_RSSIFilterThreshold = Mathf.Clamp(rssiThreshold, WearableConstants.MINIMUM_RSSI_VALUE, WearableConstants.MAXIMUM_RSSI_VALUE);
		}

		/// <summary>
		/// Indicates whether the SDK has been initialized to simulate available and connected devices.
		/// </summary>
		public bool SimulateHardwareDevices
		{
			get { return _simulateHardwareDevices; }
		}

		[Tooltip(WearableConstants.SIMULATE_HARDWARE_DEVICE_TOOLTIP), SerializeField]
		private bool _simulateHardwareDevices;

		#endregion

		#region Provider API

		internal override void SetDebugLogging(LogLevel logLevel)
		{
			SetDebugLoggingInternal(logLevel);
		}

		internal override void SearchForDevices(
			AppIntentProfile appIntentProfile,
			Action<Device[]> onDevicesUpdated,
			bool autoReconnect,
			float autoReconnectTimeout)
		{
			StopSearchingForDevices();

			base.SearchForDevices(appIntentProfile, onDevicesUpdated, autoReconnect, autoReconnectTimeout);

			if (onDevicesUpdated == null)
			{
				return;
			}

			#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
			StartSearch(appIntentProfile, RSSIFilterThreshold);
			_performDeviceSearch = true;
			_nextDeviceSearchTime = Time.unscaledTime + WearableConstants.DEVICE_SEARCH_UPDATE_INTERVAL_IN_SECONDS;
			OnConnectionStatusChanged(autoReconnect ? ConnectionStatus.AutoReconnect : ConnectionStatus.Searching);
			#else
			OnReceivedSearchDevices(WearableConstants.EMPTY_DEVICE_LIST);
			#endif
		}

		internal override void StopSearchingForDevices()
		{
			base.StopSearchingForDevices();

			if (_performDeviceSearch)
			{
				_performDeviceSearch = false;
				_nextDeviceSearchTime = float.PositiveInfinity;

				StopSearch();
			}
		}

		internal override void CancelDeviceConnection()
		{
			CancelDeviceConnectionInternal();

			StopDeviceConnection();

			OnConnectionStatusChanged(ConnectionStatus.Cancelled, _deviceToConnect);
		}

		internal override void ConnectToDevice(Device device)
		{
			StopSearchingForDevices();
			DisconnectFromDevice();

			_performDeviceConnection = true;
			_deviceToConnect = device;
			_nextDeviceConnectTime = Time.unscaledTime + WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
			_pollForConfigStatus = false;

			// Must grab product specific controls once to determine support for this device.
			_supportsActiveNoiseReduction = true;
			_supportsControllableNoiseCancellation = true;

			OpenSession(_deviceToConnect.uid);

			OnConnectionStatusChanged(ConnectionStatus.Connecting, _deviceToConnect);
		}

		internal override void DisconnectFromDevice()
		{
			StopDeviceConnection();
			StopDeviceMonitor();

			_config.DisableAllSensors();
			_config.DisableAllGestures();

			if (_connectedDevice == null)
			{
				return;
			}

			OnConnectionStatusChanged(ConnectionStatus.Disconnected, _connectedDevice);

			_connectedDevice = null;

			CloseSession();
		}

		public override DeviceConnectionInfo GetDeviceConnectionInfo()
		{
			return GetDeviceConnectionInfoInternal();
		}

		internal override FirmwareUpdateInformation GetFirmwareUpdateInformation()
		{
			return GetFirmwareUpdateInformationInternal();
		}

		internal override void SelectFirmwareUpdateOption(int index)
		{
			SelectFirmwareUpdateOptionInternal(index);
		}

		internal override void SetDeviceConfiguration(WearableDeviceConfig config)
		{
			SetDeviceConfigurationInternal(config);
			_pollForConfigStatus = true;
		}

		internal override WearableDeviceConfig GetCachedDeviceConfiguration()
		{
			return _connectedDevice.HasValue ? _config : WearableConstants.DISABLED_DEVICE_CONFIG;
		}

		protected override void RequestDeviceConfigurationInternal()
		{
			OnReceivedDeviceConfiguration(GetDeviceConfigurationInternal());
		}

		protected override void RequestIntentProfileValidationInternal(AppIntentProfile appIntentProfile)
		{
			bool profileIsValid = IsAppIntentProfileValid(appIntentProfile);

			OnReceivedIntentValidationResponse(profileIsValid);
		}

		protected override void SetActiveNoiseReductionModeInternal(ActiveNoiseReductionMode mode)
		{
			_settingProductSpecificControls = true;
			_nextProductControlQueryTime += WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
			SetActiveNoiseReductionModeProvider(mode);
		}

		protected override void SetControllableNoiseCancellationLevelInternal(int level, bool enabled)
		{
			_settingProductSpecificControls = true;
			_nextProductControlQueryTime += WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
			SetControllableNoiseCancellationLevelProvider(level, enabled);
		}

		internal override DynamicDeviceInfo GetDynamicDeviceInfo()
		{
			if (Time.unscaledTime > _nextDynamicDeviceInfoTime)
			{
				if (_supportsActiveNoiseReduction && _pollForANRInfo)
				{
					UpdateActiveNoiseReductionInformation();
				}
				else if (_supportsControllableNoiseCancellation)
				{
					UpdateControllableNoiseCancellationInformation();
				}

				_pollForANRInfo = !_pollForANRInfo;
				_nextDynamicDeviceInfoTime += WearableConstants.DEVICE_PROVIDER_DYNAMIC_INFO_UPDATE_INTERVAL_IN_SECONDS;
			}

			return GetDynamicDeviceInfoInternal();
		}

		internal override void OnInitializeProvider()
		{
			if (_initialized)
			{
				return;
			}

			WearableDeviceInitialize(_simulateHardwareDevices);

			base.OnInitializeProvider();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			DisconnectFromDevice();

			StopDeviceConnection();
			StopDeviceMonitor();
			StopSearchingForDevices();
		}

		/// <summary>
		/// When enabled, resume monitoring the device session if necessary.
		/// </summary>
		internal override void OnEnableProvider()
		{
			if (_enabled)
			{
				return;
			}

			base.OnEnableProvider();

			if (_connectedDevice != null)
			{
				StartDeviceMonitor();
			}
		}

		/// <summary>
		/// When disabled, stop actively searching for, connecting to, and monitoring devices.
		/// </summary>
		internal override void OnDisableProvider()
		{
			if (!_enabled)
			{
				return;
			}

			base.OnDisableProvider();

			StopSearchingForDevices();
			StopDeviceMonitor();
			StopDeviceConnection();
		}

		internal override void OnUpdate()
		{
			// Request the latest updates for this frame
			if (_connectedDevice != null)
			{
				GetLatestSensorUpdates();
			}

			// Check if it's time to query discovered devices
			if (_performDeviceSearch && Time.unscaledTime >= _nextDeviceSearchTime)
			{
				_nextDeviceSearchTime += WearableConstants.DEVICE_SEARCH_UPDATE_INTERVAL_IN_SECONDS;
				Device[] devices = GetDiscoveredDevices();
				OnReceivedSearchDevices(devices);
			}

			// Check if it's time to query the connection routine
			if (_performDeviceConnection && Time.unscaledTime >= _nextDeviceConnectTime)
			{
				_nextDeviceConnectTime += WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
				PerformDeviceConnection();
			}

			// Check if it's time to query the device monitor
			if (_pollDeviceMonitor && Time.unscaledTime >= _nextDeviceMonitorTime)
			{
				// NB: The monitor uses the same time interval
				_nextDeviceMonitorTime += WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
				MonitorDeviceSession();
			}

			// Check if we need to query if product controls are being set.
			if (_settingProductSpecificControls && Time.unscaledTime >= _nextProductControlQueryTime)
			{
				_nextProductControlQueryTime += WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
				CheckProductControlSetStatus();
			}

			// Allow the provider base to do its own update work
			base.OnUpdate();
		}

		#endregion

		#region Private

		// Config status
		private WearableDeviceConfig _config;

		// Device search
		private bool _performDeviceSearch;
		private float _nextDeviceSearchTime;

		// Device connection
		private bool _performDeviceConnection;
		private Device _deviceToConnect;
		private float _nextDeviceConnectTime;

		// Device monitoring
		private bool _pollDeviceMonitor;
		private float _nextDeviceMonitorTime;
		private bool _pollForConfigStatus;

		// Product control request
		private float _nextDynamicDeviceInfoTime;
		private float _nextProductControlQueryTime;
		private bool _pollForANRInfo;
		private bool _settingProductSpecificControls;
		private bool _supportsActiveNoiseReduction;
		private bool _supportsControllableNoiseCancellation;


		private int _RSSIFilterThreshold;


		internal WearableBluetoothProvider()
		{
			_config = new WearableDeviceConfig();
		}

		/// <summary>
		/// Used internally by WearableControl to get the latest buffer of SensorFrame updates from
		/// the Wearable Device; the newest frame in that batch is set as the CurrentSensorFrame.
		/// </summary>
		private void GetLatestSensorUpdates()
		{
			_currentSensorFrames.Clear();

			GetLatestSensorUpdatesInternal();

			if (_currentSensorFrames.Count > 0)
			{
				_lastSensorFrame = _currentSensorFrames[_currentSensorFrames.Count - 1];

				OnSensorsUpdated(_lastSensorFrame);
			}

			_currentGestureData.Clear();

			GetLatestGestureUpdatesInternal();

			if (_currentGestureData.Count > 0)
			{
				for (int currentGestureIndex = 0; currentGestureIndex < _currentGestureData.Count; ++currentGestureIndex)
				{
					OnGestureDetected(_currentGestureData[currentGestureIndex].gestureId);
				}
			}

			if (_pollForConfigStatus)
			{
				ConfigStatus sensor = GetSensorConfigStatusInternal();
				ConfigStatus gesture = GetGestureConfigStatusInternal();

				if (!(sensor  == ConfigStatus.Pending || sensor  == ConfigStatus.Idle) &&
				    !(gesture == ConfigStatus.Pending || gesture == ConfigStatus.Idle))
				{
					_pollForConfigStatus = false;
				}

				if (sensor == ConfigStatus.Failure || gesture == ConfigStatus.Failure)
				{
					OnConfigurationFailed(sensor, gesture);
				}

				if (sensor == ConfigStatus.Success && gesture == ConfigStatus.Success)
				{
					OnConfigurationSucceeded();
				}
			}

			_config = GetDeviceConfigurationInternal();
		}

		/// <summary>
		/// Used internally to get the latest list of discovered devices from
		/// the native SDK.
		/// </summary>
		private Device[] GetDiscoveredDevices()
		{
			return GetDiscoveredDevicesInternal();
		}

		/// <summary>
		/// Attempts to create a session to a specified device and then checks for the session status perpetually until
		/// a SessionStatus of either Open or Closed is returned, equating to either successful or failed.
		/// </summary>
		private void PerformDeviceConnection()
		{
			if (Application.isEditor)
			{
				return;
			}

			string errorMessage = string.Empty;

			ConnectionStatus sessionStatus = GetConnectionStatus(ref errorMessage);
			switch (sessionStatus)
			{
				// Receiving a session status of Closed while attempting to open a session indicates an error occured.
				case ConnectionStatus.Failed:
					if (string.IsNullOrEmpty(errorMessage))
					{
						Debug.LogWarning(WearableConstants.DEVICE_CONNECTION_FAILED);
					}
					else
					{
						Debug.LogWarningFormat(WearableConstants.DEVICE_CONNECTION_FAILED_WITH_MESSAGE, errorMessage);
					}

					StopDeviceConnection();

					OnConnectionStatusChanged(ConnectionStatus.Failed, _deviceToConnect);

					break;

				case ConnectionStatus.Searching:
				case ConnectionStatus.Connecting:
					// Device is still connecting, just wait
					break;

				case ConnectionStatus.SecurePairingRequired:
					if (_currentConnectionStatus != ConnectionStatus.SecurePairingRequired)
					{
						OnConnectionStatusChanged(ConnectionStatus.SecurePairingRequired, _deviceToConnect);
					}
					break;

				case ConnectionStatus.FirmwareUpdateAvailable:
					if (_currentConnectionStatus != ConnectionStatus.FirmwareUpdateAvailable)
					{
						OnConnectionStatusChanged(ConnectionStatus.FirmwareUpdateAvailable, _deviceToConnect);
					}
					break;

				case ConnectionStatus.FirmwareUpdateRequired:
					if (_currentConnectionStatus != ConnectionStatus.FirmwareUpdateRequired)
					{
						OnConnectionStatusChanged(ConnectionStatus.FirmwareUpdateRequired, _deviceToConnect);
					}
					break;

				case ConnectionStatus.Connected:
					Debug.Log(WearableConstants.DEVICE_CONNECTION_OPENED);

					// ProductId and VariantId are only accessible after a connection has been opened. Update the values for the _connectDevice.
					GetDeviceInfo(ref _deviceToConnect);

					// Make sure productId and variantId values are defined.
					if (!Enum.IsDefined(typeof(ProductId), _deviceToConnect.productId))
					{
						_deviceToConnect.productId = ProductId.Undefined;
					}

					DynamicDeviceInfo dynamicDeviceInfo = GetDynamicDeviceInfoInternal();
					_supportsActiveNoiseReduction = (ActiveNoiseReductionMode)dynamicDeviceInfo.availableActiveNoiseReductionModes != ActiveNoiseReductionMode.Invalid;
					_supportsControllableNoiseCancellation = dynamicDeviceInfo.totalControllableNoiseCancellationLevels > 0;

					_deviceToConnect.SetDynamicInfo(dynamicDeviceInfo);
					_connectedDevice = _deviceToConnect;

					OnConnectionStatusChanged(ConnectionStatus.Connected, _connectedDevice);

					StartDeviceMonitor();

					StopDeviceConnection();

					break;
				default:
					throw new ArgumentOutOfRangeException("sessionStatus", sessionStatus, null);
			}

			_currentConnectionStatus = sessionStatus;
		}

		/// <summary>
		/// Enables the device monitor
		/// </summary>
		private void StartDeviceMonitor()
		{
			_pollDeviceMonitor = true;

			// NB The device monitor runs on the same time interval as the connection routine
			_nextDeviceMonitorTime = Time.unscaledTime + WearableConstants.DEVICE_CONNECT_UPDATE_INTERVAL_IN_SECONDS;
		}

		/// <summary>
		/// Halts the device monitor
		/// </summary>
		private void StopDeviceMonitor()
		{
			_pollDeviceMonitor = false;
			_nextDeviceMonitorTime = float.PositiveInfinity;
		}

		/// <summary>
		/// Monitors the current device SessionStatus until a non-Open session status is returned. Once this has occured,
		/// the device has become disconnected and should render all state as such.
		/// </summary>
		private void MonitorDeviceSession()
		{
			if (Application.isEditor)
			{
				return;
			}

			string errorMessage = string.Empty;

			SessionStatus sessionStatus = (SessionStatus)GetSessionStatus(ref errorMessage);
			if (sessionStatus != SessionStatus.Open)
			{
				if (string.IsNullOrEmpty(errorMessage))
				{
					Debug.Log(WearableConstants.DEVICE_CONNECTION_MONITOR_WARNING);
				}
				else
				{
					Debug.LogFormat(WearableConstants.DEVICE_CONNECTION_MONITOR_WARNING_WITH_MESSAGE, errorMessage);
				}

				if (_connectedDevice != null)
				{
					OnConnectionStatusChanged(Wearable.ConnectionStatus.Disconnected, _connectedDevice);
				}

				_config.DisableAllSensors();
				_config.DisableAllGestures();

				StopDeviceMonitor();

				_connectedDevice = null;
			}
		}

		private void CheckProductControlSetStatus()
		{
			if (GetDeviceProductSpecificControlSetFinished())
			{
				_settingProductSpecificControls = false;
				OnAnrCncWriteComplete();
			}
		}

		/// <summary>
		/// Halts the device connection routine
		/// </summary>
		private void StopDeviceConnection()
		{
			_performDeviceConnection = false;
			_nextDeviceConnectTime = float.PositiveInfinity;
		}

		internal override void SetAppFocusChanged(bool hasFocus)
		{
			SetAppFocusChangedInternal(hasFocus);
		}

		#endregion
	}
}
