using System;
using UnityEngine;

namespace FlightKit
{
    /// <summary>
    /// Possible outcomes of showing a rewarded ad.
    /// </summary>
    public enum AdShowResult
    {
        Finished,
        Skipped,
        Failed
    }
    
    /// <summary>
    /// A universal interface for any rewarded ad provider. 
    /// </summary>
    abstract public class AbstractAdsProvider : MonoBehaviour
    {
        public virtual void ShowRewardedAd(Action<AdShowResult> resultCallback) {}
    }
}