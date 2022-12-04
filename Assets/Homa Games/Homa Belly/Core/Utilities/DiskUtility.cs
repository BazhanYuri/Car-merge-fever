using System;
using UnityEngine;

#if UNITY_IOS || UNITY_IPHONE
using System.Runtime.InteropServices;
#endif

namespace HomaGames.HomaBelly
{
	/// <summary>
    /// Utility class for disk information:
    /// - Available free space (in B or KB)
    /// </summary>
    public class DiskUtility
    {
#if UNITY_IOS || UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern int getAvailableBytes();
#endif

		/// <summary>
		/// Obtain available free Bytes in disk
		/// </summary>
		public static int GetAvailableBytes(string path = "")
		{
#if UNITY_ANDROID
			if (Application.platform == RuntimePlatform.Android)
			{
				return AndroidDiskUtility.GetAvailableBytes(path);
			}
			else
			{
				return -1;
			}

#elif UNITY_IOS || UNITY_IPHONE
			if (Application.platform == RuntimePlatform.IPhonePlayer) {
				return getAvailableBytes();
			}
			else
			{
				return -1;
			}
#endif
			throw new NotSupportedException();
		}

		/// <summary>
        /// Obtain available free KB in disk
        /// </summary>
        /// <returns></returns>
		public static int GetAvailableKiloBytes(string path = "")
        {
			int availableBytes = GetAvailableBytes(path);
			return availableBytes <= 0 ? availableBytes : availableBytes / 1024;
        }
	}
}
