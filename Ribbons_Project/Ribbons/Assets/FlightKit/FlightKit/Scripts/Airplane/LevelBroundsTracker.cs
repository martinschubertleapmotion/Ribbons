using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.Utility;

namespace FlightKit
{
    public class LevelBroundsTracker : MonoBehaviour {
        public string levelBoundsTag;
        
        private int _currentSensorsCount;
        private AudioSource _soundSource;
        
		/// <summary>
		/// The sound of being reset when leaving the level..
		/// </summary>
        public AudioClip resetSound;
        
        void Start()
        {
            _soundSource = GetComponent<AudioSource>();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag(levelBoundsTag))
            {
                _currentSensorsCount++;
            }
        }
        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.CompareTag(levelBoundsTag))
            {
                _currentSensorsCount--;
                if (_currentSensorsCount <= 0)
                {
                    RegisterAbandonedLevel();
                }
            }
        }

        private void RegisterAbandonedLevel()
        {
            // Play hit sound fx.
            if (_soundSource != null && resetSound != null)
            {
                _soundSource.PlayOneShot(resetSound);
            }

            // Clear trails of the plane.
            var trails = GetComponent<AirplaneTrails>();
            trails.DeactivateTrails();
            trails.ClearTrails();
            
            // Fade bloom.
            StartCoroutine(FadeOutCoroutine());
        }
        
        private IEnumerator FadeOutCoroutine()
        {
            var bloom = GameObject.FindObjectOfType<BloomOptimized>();
            if (bloom == null)
            {
                ResetAirplane();
                yield break;
            }
            
            // Tween out.
            float targetIntensity = 2.5f;
            var wait = new WaitForEndOfFrame();
            float tween = 1f;
            float initIntensity = bloom.intensity;
            float initThreshold = bloom.threshold;
            while (tween > 0.1)
            {
                bloom.intensity = Mathf.Lerp(bloom.intensity, targetIntensity, 1.5f * Time.deltaTime);
                bloom.threshold = Mathf.Lerp(bloom.threshold, 0f, 1.5f * Time.deltaTime);

                tween = Mathf.Lerp(tween, 0f, 1.5f * Time.deltaTime);

                yield return wait;
            }
            
            // Reset.
            ResetAirplane();
            
            // Tween in.
            targetIntensity = initIntensity;
            tween = 1f;
            while (tween > 0.1)
            {
                bloom.intensity = Mathf.Lerp(bloom.intensity, targetIntensity, 2f * Time.deltaTime);
                bloom.threshold = Mathf.Lerp(bloom.threshold, 0f, 2f * Time.deltaTime);

                tween = Mathf.Lerp(tween, 0f, 3f * Time.deltaTime);

                yield return wait;
            }
            
            bloom.intensity = initIntensity;
            bloom.threshold = initThreshold;
        }

        private void ResetAirplane()
        {
            var reset = GetComponent<ObjectResetter>();
            if (reset != null)
            {
                reset.DelayedReset(0f);
            }
        }
    }
}