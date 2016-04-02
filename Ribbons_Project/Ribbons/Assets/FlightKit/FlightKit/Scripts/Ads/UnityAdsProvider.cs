using System;

namespace FlightKit
{
    public class UnityAdsProvider : AbstractAdsProvider
    {
        
#if UNITY_ADS
        private Action<AdShowResult> _resultCallback;
#endif
        
        override public void ShowRewardedAd(Action<AdShowResult> resultCallback)
        {
#if UNITY_ADS
            _resultCallback = resultCallback;
            if (Advertisement.IsReady("rewardedVideo"))
            {
                var options = new ShowOptions { resultCallback = HandleShowResult };
                Advertisement.Show("rewardedVideo", options);
            }
            else
            {
                HandleShowResult(ShowResult.Failed);
            }
#endif
        }

#if UNITY_ADS
        protected void HandleShowResult(ShowResult result)
        {
            switch (result)
            {
            case ShowResult.Finished:
                _resultCallback(AdShowResult.Finished);
                break;
            case ShowResult.Skipped:
                _resultCallback(AdShowResult.Skipped);
                break;
            case ShowResult.Failed:
                _resultCallback(AdShowResult.Failed);
                break;
            }
        }
#endif
    }
}