using System.Collections.Generic;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This partial class will be completed automatically by HomaBridgeDependenciesCodeGenerator script.
    /// This allow us to add/remove new services updating the manifest and use direct references to new services instead of using reflection.
    /// </summary>
    public static partial class HomaBridgeDependencies
    {
        // WARNING: If you rename this field you have to change HomaBridgeDependenciesCodeGenerator
        private static readonly List<MediatorBase> mediators = new List<MediatorBase>();
        // WARNING: If you rename this field you have to change HomaBridgeDependenciesCodeGenerator
        private static readonly List<IMediator> oldMediators = new List<IMediator>();
        // WARNING: If you rename this field you have to change HomaBridgeDependenciesCodeGenerator
        private static readonly List<IAttribution> attributions = new List<IAttribution>();
        // WARNING: If you rename this field you have to change HomaBridgeDependenciesCodeGenerator
        private static readonly List<IAnalytics> analytics = new List<IAnalytics>();
        // WARNING: If you rename this field you have to change HomaBridgeDependenciesCodeGenerator
        private static CustomerSupportImplementation customerSupport = null;
        
        // WARNING: If you rename this method you have to change HomaBridgeDependenciesCodeGenerator
        /// <summary>
        /// Body auto generated in another file based on present Mediators in the project.
        /// </summary>
        static partial void PartialInstantiateMediators();
        
        // WARNING: If you rename this method you have to change HomaBridgeDependenciesCodeGenerator
        /// <summary>
        /// Body auto generated in another file based on present old Mediators in the project.
        /// </summary>
        static partial void PartialInstantiateOldMediators();
        
        // WARNING: If you rename this method you have to change HomaBridgeDependenciesCodeGenerator
        /// <summary>
        /// Body auto generated in another file based on present Attributions in the project
        /// </summary>
        static partial void PartialInstantiateAttributions();
        
        // WARNING: If you rename this method you have to change HomaBridgeDependenciesCodeGenerator
        /// <summary>
        /// Body auto generated in another file based on present Analytics in the project
        /// </summary>
        static partial void PartialInstantiateAnalytics();
        
        // WARNING: If you rename this method you have to change HomaBridgeDependenciesCodeGenerator
        /// <summary>
        /// Body auto generated in another file based on present Customer Support in the project
        /// </summary>
        static partial void PartialInstantiateCustomerSupport();

        #region Public Interface
        
        /// <summary>
        /// Create Mediation, Attribution and Analytics services.
        /// </summary>
        public static int InstantiateServices()
        {
            PartialInstantiateMediators();
            PartialInstantiateOldMediators();
            PartialInstantiateAttributions();
            PartialInstantiateAnalytics();
            PartialInstantiateCustomerSupport();

            return mediators.Count + attributions.Count + analytics.Count + (customerSupport!=null?1:0);
        }

        public static bool GetMediators(out List<MediatorBase> mediators)
        {
            mediators = HomaBridgeDependencies.mediators;
            return mediators != null && mediators.Count > 0;
        }
        
        public static bool GetOldMediators(out List<IMediator> oldMediators)
        {
            oldMediators = HomaBridgeDependencies.oldMediators;
            return oldMediators != null && oldMediators.Count > 0;
        }

        public static bool GetAttributions(out List<IAttribution> attributions)
        {
            attributions = HomaBridgeDependencies.attributions;
            return attributions != null && attributions.Count > 0;
        }
        public static bool GetAnalytics(out List<IAnalytics> analytics)
        {
            analytics = HomaBridgeDependencies.analytics;
            return analytics != null && analytics.Count > 0;
        }
        
        public static bool GetCustomerSupport(out CustomerSupportImplementation customerSupport)
        {
            customerSupport = HomaBridgeDependencies.customerSupport;
            return customerSupport != null;
        }

        #endregion
    }
}