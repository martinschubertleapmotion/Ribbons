using UnityEngine;

namespace FlightKit
{
    public class SfxController : MonoBehaviour
    {
        [Tooltip ("2D audio source to play sound effects on.")]
        /// <summary>
        /// 2D audio source to play sound effects on.
        /// </summary>
        public AudioSource audioSource;
        
        [Space]
        
        [Tooltip ("The sound that is played when user's fuel gets low, but not completely gone yet.")]
        /// <summary>
        /// The sound that is played when user's fuel gets low, but not completely
        /// gone yet.
        /// </summary>
        public AudioClip lowFuelSound;
        
        [Tooltip ("The sound that is played when a level is failed.")]
        /// <summary>
        /// The sound that is played when a level is failed.
        /// </summary>
        public AudioClip levelFailSound;
        
        [Tooltip ("The sound that is played when user is revived after failing.")]
        /// <summary>
        /// The sound that is played when user is revived after failing.
        /// </summary>
        public AudioClip userRevivedSound;
        
        void OnEnable()
        {
            if (audioSource == null)
            {
                this.enabled = false;
                return;
            }
            
            FuelController.OnFuelLowEvent += HandleFuelLow;
            FuelController.OnFuelEmptyEvent += HandleFuelEmpty;
            RevivePermissionProvider.OnReviveGranted += HandleRevive;
        }
        
        void OnDisable()
        {
            FuelController.OnFuelLowEvent -= HandleFuelLow;
            FuelController.OnFuelEmptyEvent -= HandleFuelEmpty;
            RevivePermissionProvider.OnReviveGranted -= HandleRevive;
        }

        private void HandleFuelLow()
        {
            if (lowFuelSound != null)
            {
                audioSource.PlayOneShot(lowFuelSound);
            }
        }

        private void HandleFuelEmpty()
        {
            if (levelFailSound != null)
            {
                audioSource.PlayOneShot(levelFailSound);
            }
        }
        
        private void HandleRevive()
        {
            if (userRevivedSound != null)
            {
                audioSource.PlayOneShot(userRevivedSound);
            }
        }
    }
}
