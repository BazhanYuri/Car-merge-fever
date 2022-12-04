using System;
using UnityEngine;

namespace HomaGames.HomaBelly
{
	/// <summary>
	/// Disk utility plugin for Android
	/// </summary>
	public class AndroidDiskUtility
	{
		static string StorageBlockSize = "getBlockSizeLong";
		static string StorageBlockAvailable = "getAvailableBlocksLong";

		private static AndroidJavaObject fileSystemStats;

		/// <summary>
		/// Obtain available free Bytes in disk
		/// </summary>
		public static int GetAvailableBytes(string path)
		{
			try
			{
				long availableBlocks = GetStatFs(path).Call<long>(StorageBlockAvailable);
				long blockSize = GetStatFs(path).Call<long>(StorageBlockSize);
				long availableBytes = availableBlocks * blockSize;
				return (int)availableBytes;
			}
			catch (Exception e)
			{
				HomaGamesLog.Warning($"[Disk Utility] Exception thrown while detecting available space: {e.Message}");
			}

			return -1;
		}

		static AndroidJavaObject GetStatFs(string path)
		{
			if (fileSystemStats == null)
			{
				fileSystemStats = new AndroidJavaObject("android.os.StatFs", path);
			}

			return fileSystemStats;
		}
	}
}