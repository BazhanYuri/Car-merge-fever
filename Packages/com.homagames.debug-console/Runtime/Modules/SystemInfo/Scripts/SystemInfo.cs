using System;
using System.Collections;
using System.Collections.Generic;
using HomaGames.HomaConsole;
using TMPro;
using UnityEngine;

namespace HomaGames.HomaConsole
{
    [AddComponentMenu("")]
    public class SystemInfo : MonoBehaviour
    {
        public RectTransform contentRoot;
        public TMP_Text headerPrefab;
        public TMP_Text contentPrefab;

        private void Awake()
        {
            RefreshData();
        }

        private void RefreshData()
        {
            AddHeader("Device");
            AddItem("Model : " + UnityEngine.SystemInfo.deviceModel);
            AddItem("Name : " + UnityEngine.SystemInfo.deviceName);
            AddItem("Type : " + UnityEngine.SystemInfo.deviceType);
            AddItem("ID : " + UnityEngine.SystemInfo.deviceUniqueIdentifier);
            AddHeader("Graphics");
            AddItem("Name : " + UnityEngine.SystemInfo.graphicsDeviceName);
            AddItem("Type : " + UnityEngine.SystemInfo.graphicsDeviceType);
            AddItem("Vendor : " + UnityEngine.SystemInfo.graphicsDeviceVendor);
            AddItem("Version : " + UnityEngine.SystemInfo.graphicsDeviceVersion);
            AddItem("Memory : " + UnityEngine.SystemInfo.graphicsMemorySize);
            AddItem("Multithreaded : " + UnityEngine.SystemInfo.graphicsMultiThreaded);
            AddItem("Shader Level : " + UnityEngine.SystemInfo.graphicsShaderLevel);
        }

        public void AddHeader(string headerName)
        {
            var header = Instantiate(headerPrefab.gameObject, contentRoot);
            header.transform.localScale = Vector3.one;
            header.GetComponent<TMP_Text>().text = headerName;
        }

        public void AddItem(string content)
        {
            var item = Instantiate(contentPrefab.gameObject, contentRoot);
            item.transform.localScale = Vector3.one;
            item.GetComponent<TMP_Text>().text = content;
        }
    }
}
