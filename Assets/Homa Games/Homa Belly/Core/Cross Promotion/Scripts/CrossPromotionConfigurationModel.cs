using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using Random = System.Random;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Model holding all the required configuration for Cross Promotion feature
    /// </summary>
    public class CrossPromotionConfigurationModel
    {
        /// <summary>
        /// List of Cross Promotion items to be applied to this app.
        /// If `null`, no cross promotion should be shown
        /// </summary>
        public List<CrossPromotionItem> Items;

        public string CrossPromotionStatus;

        /// <summary>
        /// Random seed to be used for selecting cross promotion items
        /// </summary>
        private Random random = new Random();

        /// <summary>
        /// Given the whole remote configuration json fetched from server, deserializes
        /// and builds the cross promotion configuration model
        /// </summary>
        /// <param name="crossPromotionData">The remote configuration fetched from server</param>
        /// <returns>A CrossPromotionConfigurationModel instance. Items inside the instance may be null</returns>
        public static CrossPromotionConfigurationModel FromRemoteConfigurationDictionary(JsonObject crossPromotionData)
        {
            CrossPromotionConfigurationModel model = new CrossPromotionConfigurationModel();

            if (crossPromotionData != null)
            {
                // Obtain cross promo status
                crossPromotionData.TryGetString("e_cross_promotion_status", crossPromotionStatus =>
                {
                    model.CrossPromotionStatus = crossPromotionStatus;
                    HomaGamesLog.Debug($"[Cross Promotion] Status: {model.CrossPromotionStatus}");
                });

                // Obtain cross promo items
                if (crossPromotionData.TryGetJsonList("ao_cross_promotion_items", out var itemList) &&
                    (string.IsNullOrEmpty(model.CrossPromotionStatus) || model.CrossPromotionStatus == "OK"))
                {
                    List<CrossPromotionItem> tmpItems = new List<CrossPromotionItem>();

                    for (int i = 0; i < itemList.Count; i++)
                    {
                        if (itemList.TryGetJsonObject(i, out var itemData))
                        {
                            CrossPromotionItem item = CrossPromotionItem.FromDictionary(itemData.ToRawData());
                            tmpItems.Add(item);
                        }
                    }

                    model.Items = tmpItems;
                }
            }

            return model;
        }

        /// <summary>
        /// Obtains a random CrossPromotionItem depending on its weights and its
        /// local file availability
        /// </summary>
        /// <returns></returns>
        public CrossPromotionItem RandomElementByWeightAndLocalAvailability()
        {
            if (Items == null || Items.Count == 0)
            {
                return null;
            }

            List<CrossPromotionItem> locallyAvailableItems = Items.Where(e => e.IsLocalVideoFileAvailable()).ToList();
            return RandomElementByWeight(locallyAvailableItems);
        }

        /// <summary>
        /// Obtains a random CrossPromotionItem depending on its weights and
        /// its local file is not available
        /// </summary>
        /// <returns></returns>
        public CrossPromotionItem RandomElementByWeightToBeDownloaded()
        {
            if (Items == null || Items.Count == 0)
            {
                return null;
            }

            List<CrossPromotionItem> itemsToBeDownloaded = Items.Where(e => !e.IsLocalVideoFileAvailable()).ToList();
            return RandomElementByWeight(itemsToBeDownloaded);
        }

        private CrossPromotionItem RandomElementByWeight(List<CrossPromotionItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return null;
            }

            // Obtain the total weight of all items.
            double totalWeight = items.Sum(e => e.Weight);

            // Get a random double between 0 and totalWeight
            double randomWeight = random.NextDouble() * totalWeight;
            double currentWeight = 0;

            // Loop over each Item accumulating its weight until reaching the random one
            foreach (var item in from weightedItem in items select new { Value = weightedItem, weightedItem.Weight })
            {
                currentWeight += item.Weight;
                if (currentWeight >= randomWeight)
                    return item.Value;

            }

            // Just in case, return first item
            HomaGamesLog.Warning("Could not select weighted cross promotion item. Returning first one");
            return items[0];
        }
    }
}