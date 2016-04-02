
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

namespace FlightKit
{
	/// <summary>
	/// Performs the actions when a level is failed.
	/// </summary>
	public class LevelFailController : MonoBehaviour
	{
        [Tooltip ("If true, level fail menu will be shown. Otherwise, scene will be restarted.")]
        /// <summary>
        /// If true, level fail menu will be shown. Otherwise, scene will be restarted.
        /// </summary>
        public bool showLevelFailMenu = true;
        
        [Tooltip ("Game over screen.")]
        /// <summary>
        /// Game over screen.
        /// </summary>
        public CanvasGroup levelFailMenu;
        
        private float _defaultBloomIntensity;
        private float _defaultBloomThreshold;
        
        void OnEnable()
        {
            FuelController.OnFuelEmptyEvent += HandleLevelFailed;
            RevivePermissionProvider.OnReviveGranted += HandleReviveGranted;
        }
        
        void OnDisable()
        {
            FuelController.OnFuelEmptyEvent -= HandleLevelFailed;
            RevivePermissionProvider.OnReviveGranted -= HandleReviveGranted;
        }
        
		/// <summary>
		/// Level is failed, show a dialog or go to menu from here.
		/// </summary>
		public virtual void HandleLevelFailed()
		{
            if (showLevelFailMenu)
            {
                StartCoroutine(FadeOutCoroutine());
            }
            else
            {
                Time.timeScale = 1;
                // Restart game.
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
		}
        
        private IEnumerator FadeOutCoroutine()
        {
            // Make a small pause so that user realizes what happened.
            yield return new WaitForSeconds(1.0f);
            
            // If there is a bloom effect in the scene, use it for a nice fade out effect.
            var bloom = GameObject.FindObjectOfType<BloomOptimized>();
            if (bloom == null)
            {
                // Pause game.
                Time.timeScale = 0;
                
                if (levelFailMenu != null)
                {
                    levelFailMenu.gameObject.SetActive(true);
                }
                
                yield break;
            }
            
            // Tween out.
            float targetIntensity = 2.5f;
            var wait = new WaitForEndOfFrame();
            float tween = 1f;
            _defaultBloomIntensity = bloom.intensity;
            _defaultBloomThreshold = bloom.threshold;
            
            if (levelFailMenu != null)
            {
                levelFailMenu.alpha = 0;
                levelFailMenu.gameObject.SetActive(true);
            }
            
            float prevTime = Time.realtimeSinceStartup;
            float deltaTime = 0;
            
            while (tween > 0.1)
            {
                deltaTime = Time.realtimeSinceStartup - prevTime;
                prevTime = Time.realtimeSinceStartup;
                
                bloom.intensity = Mathf.Lerp(bloom.intensity, targetIntensity, 1.5f * deltaTime);
                bloom.threshold = Mathf.Lerp(bloom.threshold, 0f, 1.5f * deltaTime);

                tween = Mathf.Lerp(tween, 0f, 1.5f * deltaTime);
                
                Time.timeScale = tween;
                
                if (levelFailMenu != null)
                {
                    levelFailMenu.alpha = 1 - tween;
                }
                
                yield return wait;
            }
            
            if (levelFailMenu != null)
            {
                levelFailMenu.alpha = 1;
            }
            
            // Pause game.
            Time.timeScale = 0;
        }
        
        private void HandleReviveGranted()
        {
            // Unpause game.
            Time.timeScale = 1;
            levelFailMenu.gameObject.SetActive(false);
            StartCoroutine(TweenIn());
        }
        
        private IEnumerator TweenIn()
        {
            // If there is a bloom effect in the scene, use it for a nice fade effect.
            var bloom = GameObject.FindObjectOfType<BloomOptimized>();
            if (bloom == null)
            {
                yield break;
            }
            // Tween in.
            var wait = new WaitForEndOfFrame();
            float targetIntensity = _defaultBloomIntensity;
            float tween = 1f;
            while (tween > 0.1)
            {
                bloom.intensity = Mathf.Lerp(bloom.intensity, targetIntensity, 2f * Time.deltaTime);
                bloom.threshold = Mathf.Lerp(bloom.threshold, 0f, 2f * Time.deltaTime);

                tween = Mathf.Lerp(tween, 0f, 3f * Time.deltaTime);

                yield return wait;
            }
            
            bloom.intensity = _defaultBloomIntensity;
            bloom.threshold = _defaultBloomThreshold;
        }
	}
}