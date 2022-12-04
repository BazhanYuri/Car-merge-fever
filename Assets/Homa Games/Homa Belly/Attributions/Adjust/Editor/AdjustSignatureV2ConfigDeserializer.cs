using System.Collections.Generic;
using HomaGames.HomaBelly.Utilities;

namespace HomaGames.HomaBelly
{
    public class AdjustSignatureV2ConfigDeserializer : IModelDeserializer<AdjustSignatureV2ConfigDeserializer.ConfigModel>
    {
        public ConfigModel Deserialize(string json)
        {
	        // Return empty manifest if json string is not valid
	        if (string.IsNullOrEmpty(json))
	        {
		        return null;
	        }

	        ConfigModel model = null;
	        // Basic info
	        var dictionary = Json.Deserialize(json) as Dictionary<string, object>;
	        if (dictionary != null && dictionary.ContainsKey("res"))
	        {
				// Res dictionary
		        var resDictionary = dictionary["res"] as Dictionary<string, object>;

		        if (resDictionary.TryGetValue("o_adjust_sdk_signature",out var adjustSignatureConfig))
		        {
			        var adjustSignatureConfigDictionary = adjustSignatureConfig as Dictionary<string, object>;

			        adjustSignatureConfigDictionary.TryGetValue("s_android_app_token", out var android_app_token);
			        adjustSignatureConfigDictionary.TryGetValue("s_ios_app_token", out var ios_app_token);
			        adjustSignatureConfigDictionary.TryGetValue("s_android_signature_file_url", out var android_lib_url);
			        adjustSignatureConfigDictionary.TryGetValue("s_android_signature_file_hash", out var android_md5_sum);
			        adjustSignatureConfigDictionary.TryGetValue("s_ios_signature_file_url", out var ios_lib_url);
			        adjustSignatureConfigDictionary.TryGetValue("s_ios_signature_file_hash", out var ios_md5_sum);

			        model =
				        new ConfigModel((string) android_app_token,
					        (string) ios_app_token,
					        (string) android_lib_url,
					        (string) ios_lib_url,
					        (string)ios_md5_sum,
					        (string)android_md5_sum);
		        }
	        }

	        return model;
        }

        public ConfigModel LoadFromCache()
        {
	        return default;
        }
        
        public class ConfigModel
        {
	        public string AndroidMd5Sum { get; }

	        public string IosMd5Sum { get; }

	        public string IosLibUrl { get; }

	        public string AndroidLibUrl { get; }

	        public string IosAppToken { get; }

	        public string AndroidAppToken { get; }
        
	        public ConfigModel(string android_app_token,
		        string ios_app_token,
		        string android_lib_url,
		        string ios_lib_url,
		        string ios_md5_sum,
		        string android_md5_sum)
	        {
		        AndroidAppToken = android_app_token;
		        IosAppToken = ios_app_token;
		        AndroidLibUrl = android_lib_url;
		        IosLibUrl = ios_lib_url;
		        IosMd5Sum = ios_md5_sum;
		        AndroidMd5Sum = android_md5_sum;
	        }

	        public override string ToString()
	        {
		        return $"{nameof(AndroidMd5Sum)}: {AndroidMd5Sum}, {nameof(IosMd5Sum)}: {IosMd5Sum}, {nameof(IosLibUrl)}: {IosLibUrl}, {nameof(AndroidLibUrl)}: {AndroidLibUrl}, {nameof(IosAppToken)}: {IosAppToken}, {nameof(AndroidAppToken)}: {AndroidAppToken}";
	        }
        }
    }
}