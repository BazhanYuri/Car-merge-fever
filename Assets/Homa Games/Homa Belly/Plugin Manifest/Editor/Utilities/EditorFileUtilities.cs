using System.IO;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
	public static class EditorFileUtilities
	{
		public static string ReadAllText(string filePath)
		{
			if (filePath.Contains("://") || filePath.Contains(":///"))
			{
				UnityWebRequest www = UnityWebRequest.Get(filePath);
				www.SendWebRequest();
                // Wait until async operation has finished
				while(!www.isDone)
                {
					continue;
                }
				return www.downloadHandler.text;
			}
			else
			{
				return File.ReadAllText(filePath);
			}
		}

		public static void CreateIntermediateDirectoriesIfNecessary(string filePath)
        {
			string parentPath = Directory.GetParent(filePath)?.ToString();
			if (!string.IsNullOrEmpty(parentPath) && !Directory.Exists(parentPath))
			{
				Directory.CreateDirectory(parentPath);
			}
		}
        
        /// <summary>
        /// Remove empty directories recursively starting at the specified path.
        /// This method can take a while depending on the depth of the folders.
        /// This method take care of .meta files created for folders.
        /// </summary>
        public static void RemoveEmptyDirectoriesRecursively(string startLocation, bool removeStartLocationTooIfEmpty)
        {
	        foreach (var directory in Directory.GetDirectories(startLocation))
	        {
		        RemoveEmptyDirectoriesRecursively(directory,false);
		        RemoveDirectoryIfEmpty(directory);
	        }

	        if (removeStartLocationTooIfEmpty)
	        {
		        RemoveDirectoryIfEmpty(startLocation);
	        }

	        void RemoveDirectoryIfEmpty(string directory)
	        {
		        string[] files = Directory.GetFiles(directory);
		        var fileCount = files.Length;
		        var directoryCount = Directory.GetDirectories(directory).Length;

		        if (fileCount == 0 &&
		            directoryCount == 0)
		        {
			        // Take care of the .meta file in the parent folder when removing a folder
			        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
			        string metaFilePath = $"{directoryInfo.Parent}/{directoryInfo.Name}.meta";
			        File.Delete(metaFilePath);
			        Directory.Delete(directory, false);
		        }
	        }
        }
	}
}
