using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bhaptics.SDK2
{
    /// <summary>
    /// Apple-only (iOS/visionOS) connection UI. There is no bHaptics Player app on Apple platforms,
    /// so unlike <see cref="BhapticsUI"/> this panel drives BLE scan / pair directly through
    /// <see cref="BhapticsAppleDevices"/>: a Scan button (auto-stops after <see cref="scanDuration"/>
    /// seconds) plus a Connect / Disconnect button per discovered device.
    /// </summary>
    public class BhapticsAppleUI : MonoBehaviour
    {
        [SerializeField] private float intervalRefreshTime = 1f;
        [SerializeField] private float scanDuration = 30f;

        [Header("Scan UI")]
        [SerializeField] private Button scanButton;
        [SerializeField] private Text scanButtonText;
        [SerializeField] private Text statusText;

        [Header("Devices UI")]
        [SerializeField] private Transform devicesContainer;
        [SerializeField] private BhapticsAppleDeviceUI devicePrefab;

        private List<BhapticsAppleDeviceUI> rows = new List<BhapticsAppleDeviceUI>();
        private float scanRemaining;
        private int deviceCount;

        private void Start()
        {
            if (scanButton != null)
            {
                scanButton.onClick.AddListener(ToggleScan);
            }

            InvokeRepeating("Refresh", 0f, intervalRefreshTime);
        }

        private void OnDisable()
        {
            StopScanning();
        }

        private void OnDestroy()
        {
            CancelInvoke("Refresh");
        }

        private void Update()
        {
            if (scanRemaining <= 0f)
            {
                return;
            }

            scanRemaining -= Time.deltaTime;

            if (scanRemaining <= 0f)
            {
                StopScanning();
            }

            UpdateScanUi();
        }

        private void ToggleScan()
        {
            if (scanRemaining > 0f)
            {
                StopScanning();
            }
            else
            {
                BhapticsAppleDevices.Scan();
                scanRemaining = scanDuration;
            }

            UpdateScanUi();
        }

        private void StopScanning()
        {
            scanRemaining = 0f;

            if (BhapticsAppleDevices.IsScanning())
            {
                BhapticsAppleDevices.StopScan();
            }
        }

        private void Refresh()
        {
            var devices = BhapticsAppleDevices.GetDevices();
            deviceCount = devices.Count;

            while (rows.Count < devices.Count)
            {
                rows.Add(Instantiate(devicePrefab, devicesContainer));
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (i < devices.Count)
                {
                    rows[i].RefreshDevice(devices[i]);
                }
                else
                {
                    rows[i].gameObject.SetActive(false);
                }
            }

            UpdateScanUi();
        }

        private void UpdateScanUi()
        {
            if (scanButtonText != null)
            {
                scanButtonText.text = scanRemaining > 0f
                    ? string.Format("Stop ({0}s)", Mathf.CeilToInt(scanRemaining))
                    : "Scan";
            }

            if (statusText != null)
            {
                if (scanRemaining > 0f)
                {
                    statusText.text = "Scanning for nearby bHaptics devices...";
                }
                else
                {
                    statusText.text = deviceCount == 0 ? "No devices found. Press Scan to search." : "";
                }
            }
        }
    }
}
