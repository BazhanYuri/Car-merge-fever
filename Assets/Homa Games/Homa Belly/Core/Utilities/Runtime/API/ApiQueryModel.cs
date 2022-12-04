using System;
using System.Collections.Generic;
using System.Globalization;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Base class for all Homa Games API queries: editor and runtime.
    /// This class holds and configures mandatory or common parameters for Homa Games API
    /// </summary>
    public abstract class ApiQueryModel
    {
        /// <summary>
        /// Configuration version: V1
        /// </summary>
        private static readonly string CONFIGURATION_VERSION = "cv";
        
        /// <summary>
        /// SDK version (Homa Belly): 1.4.0
        /// </summary>
        private static readonly string HOMA_BELLY_SDK_VERSION = "sv";
        
        /// <summary>
        /// App version: 0.1
        /// </summary>
        private static readonly string APP_VERSION = "av";
        
        /// <summary>
        /// App identifier (bundle id): com.companyname.appname
        /// </summary>
        private static readonly string APP_IDENTIFIER = "ai";
        
        /// <summary>
        /// Homa Belly token: t0000001
        /// </summary>
        private static readonly string HOMA_BELLY_TOKEN = "ti";
        
        /// <summary>
        /// Legacy ua field. Used for NTesting scopes, cross promo platform targeting, etc...
        /// </summary>
        private static readonly string PARAM_UA = "ua";
        
        /// <summary>
        /// Manifest version identifier: 1234abcd
        /// </summary>
        private static readonly string MANIFEST_VERSION = "mvi";
        
        /// <summary>
        /// Device OS: Android OS 9 / API-28 (PKQ1.180917.001/V10.0.24.0.PDHMIXM)
        /// </summary>
        private static readonly string DEVICE_OS = "dos";
        
        /// <summary>
        /// Device type: Handled
        /// </summary>
        private static readonly string DEVICE_TYPE = "dtp";
        
        /// <summary>
        /// Device version: Xiaomi Mi A1
        /// </summary>
        private static readonly string DEVICE_VERSION = "dv";

        // TokenIdentifier and ManifestVersionId are statically stored and used across all events
        // TODO: For runtime analytic events, read these values from TrackingData
        public static string TokenIdentifier = "t0000001";
        public static string ManifestVersionId;

        private Dictionary<string, object> _asDictionary = new Dictionary<string, object>();
        
        public override string ToString()
        {
            return Json.Serialize(ToDictionary());
        }
        
        protected string Sanitize(float value)
        {
            string stringValue = "0";
            try
            {
                // This will prevent the System.OverflowException
                decimal decimalValue = 0;
                if (value < (double)decimal.MinValue)
                    decimalValue = decimal.MinValue;
                else if (value > (double)decimal.MaxValue)
                    decimalValue = decimal.MaxValue;
                else
                {
                    // // Convert to decimal to avoid having double scientific notations. This line is used in singular
                    decimalValue = Convert.ToDecimal(value,CultureInfo.InvariantCulture);
                }
                
                // Convert to string using InvariantCulture to avoid having issues with the decimal number separator
                stringValue = decimalValue.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] Error converting double to decimal value: {value} {e}");
            }
            
            return stringValue;
        }

        protected string Sanitize(string input)
        {
            // '=': causes server error 500
            return !string.IsNullOrWhiteSpace(input) ? input : "";
        }

        public virtual Dictionary<string, object> ToDictionary()
        {
            // Clear any possible previous values
            _asDictionary.Clear();
            
            _asDictionary.Add(CONFIGURATION_VERSION, HomaBellyConstants.API_VERSION);
            _asDictionary.Add(HOMA_BELLY_SDK_VERSION, HomaBellyConstants.PRODUCT_VERSION);
            _asDictionary.Add(APP_VERSION, SystemConstants.ApplicationVersion);
            _asDictionary.Add(APP_IDENTIFIER, SystemConstants.ApplicationIdentifier);
            _asDictionary.Add(DEVICE_OS, SystemConstants.OperatingSystem);
            _asDictionary.Add(DEVICE_TYPE, SystemConstants.DeviceType);
            _asDictionary.Add(DEVICE_VERSION, SystemConstants.DeviceModel);
            _asDictionary.Add(HOMA_BELLY_TOKEN, TokenIdentifier);
            _asDictionary.Add(PARAM_UA, SystemConstants.UserAgent);
            _asDictionary.Add(MANIFEST_VERSION, ManifestVersionId);

            return _asDictionary;
        }
    }
}