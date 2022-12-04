namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class representing a Video Ad reward with:
    /// - Placament name (if any)
    /// - Reward amount
    /// - Reward name
    /// </summary>
    public class VideoAdReward
    {
		private string rewardName;
		private int rewardAmount;
		private string placementName;

		public VideoAdReward(string rewardName, int rewardAmount)
		{
			this.placementName = "default";
			this.rewardName = rewardName;
			this.rewardAmount = rewardAmount;
		}

		public VideoAdReward(string placementName, string rewardName, int rewardAmount)
		{
			this.placementName = placementName;
			this.rewardName = rewardName;
			this.rewardAmount = rewardAmount;
		}

		public string getRewardName()
		{
			return rewardName;
		}

		public int getRewardAmount()
		{
			return rewardAmount;
		}

		public string getPlacementName()
		{
			return placementName;
		}

		public override string ToString()
		{
			return placementName + " : " + rewardName + " : " + rewardAmount;
		}
	}
}
