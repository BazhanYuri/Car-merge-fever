using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.Geryon;
#pragma warning disable CS1998

namespace HomaGames.HomaBelly.Utilities
{
    // [30/03/2022] This class has been kept for backward compatibility, but
    // must be deleted in a near future. Packages that have known references 
    // to this class are:
    // - Singular
    // - HelpShift
    public static class GeryonUtils
    {
        /// <summary>
        /// Try to obtain Geryon NTesting ID with reflection. If not found,
        /// returns an empty string.
        ///
        /// Upon Geryon v3.0.0+, it is initialized asynchronously. Hence, this method
        /// asynchronously awaits for its initialization (2 seconds) and then tries to
        /// obtian the NTESTING_ID
        /// </summary>
        /// <returns>The Geryon NTESTING_ID if found, or an empty string if not</returns>
        public static string GetGeryonTestingId()
        {
            return Geryon.Config.NTESTING_ID;
        }

        /// <summary>
        /// Try to obtain Geryon dynamic variable
        /// </summary>
        /// <param name="propertyName">The property name of the variable. All in caps and without type prefix: ie. IDFA_CONSENT_POPUP_DELAY_S</param>
        /// <returns></returns>
        public static string GetGeryonDynamicVariableValue(string propertyName)
        {
            return DynamicVariable<string>.Get(propertyName, string.Empty);
        }

        /// <summary>
        /// Obtains NTesting OverrideId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingOverrideId()
        {
            return Geryon.Config.OverrideId;
        }

        /// <summary>
        /// Obtains NTesting VariantId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingVariantId()
        {
            return Geryon.Config.VariantId;
        }

        /// <summary>
        /// Obtains NTesting ScopeId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingScopeId()
        {
            return Geryon.Config.ScopeId;
        }

        /// <summary>
        /// Obtains by reflection the external token value for the given property name:
        /// - ExternalToken0
        /// - ExternalToken1
        /// </summary>
        /// <param name="externalTokenPropertyName"></param>
        /// <param name="externalToken"></param>
        public static async Task<string> GetNTestingExternalToken(string externalTokenPropertyName)
        {
            switch (externalTokenPropertyName)
            {
                default:
                case "ExternalToken0":
                    return Geryon.Config.ExternalToken0;
                case "ExternalToken1":
                    return Geryon.Config.ExternalToken1;
                case "ExternalToken2":
                    return Geryon.Config.ExternalToken2;
                case "ExternalToken3":
                    return Geryon.Config.ExternalToken3;
                case "ExternalToken4":
                    return Geryon.Config.ExternalToken4;
            }
        }

        #region Private methods

        /// <summary>
        /// Asynchronousle awaits for Geryon to be Initialized, so we can safely
        /// access any of its properties/values with reflection
        /// </summary>
        /// <returns></returns>
        public static async Task WaitForInitialization()
        {
            if (! Geryon.Config.Initialized)
                await new EventTask(observer => Geryon.Config.OnInitialized += observer);
        }

        #endregion
    }
}

