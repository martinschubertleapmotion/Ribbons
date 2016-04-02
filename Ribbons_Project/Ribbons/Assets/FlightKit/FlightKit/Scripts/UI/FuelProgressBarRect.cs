using UnityEngine;
using UnityEngine.UI;

namespace FlightKit
{
    /// <summary>
    /// Rectangular version of the fuel slider GUI.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class FuelProgressBarRect : MonoBehaviour
    {
        /// <summary>
        /// Texture of the full fuel fill bar.
        /// </summary>
        public Image barFuelFull;
        /// <summary>
        /// Texture of the low fuel fill bar.
        /// </summary>
        public Image barFuelLow;

        /// <summary>
        /// Normalized value where full fuel bar starts to disappear.
        /// </summary>
        [Tooltip("Normalized value where full fuel bar starts to disappear.")]
        [Space] public float blendStart = 0.5f;

        /// <summary>
        /// Normalized value where full fuel bar is no longer visible at all.
        /// </summary>
        [Tooltip("Normalized value where full fuel bar is no longer visible at all.")]
        public float blendEnd = 0.2f;

        private Slider _slider;

        protected FuelController _fuelController;
        protected CanvasGroup _canvasGroup;
        
        void Start()
        {
            _fuelController = GameObject.FindObjectOfType<FuelController>();
            if (_fuelController == null)
            {
                Debug.LogError("FuelController not found.");
                this.enabled = false;
                return;
            }
            
            _canvasGroup = GetComponent<CanvasGroup>();
            
            // Hide itself.
            _canvasGroup.alpha = 0;
            
            _slider = GetComponent<Slider>();
        }
        
        void OnEnable()
        {
            TakeOffPublisher.OnTakeOffEvent += FadeIn;
            PauseController.OnPauseEvent += FadeOut;
            PauseController.OnUnPauseEvent += FadeIn;
            RevivePermissionProvider.OnReviveGranted += FadeIn;
            FuelController.OnFuelEmptyEvent += FadeOut;
        }
        
        void OnDisable()
        {
            TakeOffPublisher.OnTakeOffEvent -= FadeIn;
            PauseController.OnPauseEvent -= FadeOut;
            PauseController.OnUnPauseEvent -= FadeIn;
            RevivePermissionProvider.OnReviveGranted -= FadeIn;
            FuelController.OnFuelEmptyEvent -= FadeOut;
        }
        
        private void FadeIn()
        {
            Fader.FadeAlpha(this, _canvasGroup, true, 1);
        }
        
        private void FadeOut()
        {
            Fader.FadeAlpha(this, _canvasGroup, false, 1);
        }
        
        void Update()
        {
            _slider.value = _fuelController.fuelAmount;
        }
        
        public virtual void HandleSliderValueChanged()
        {
            barFuelLow.rectTransform.anchorMax = barFuelFull.rectTransform.anchorMax;

            if (_slider.value < blendStart)
            {
                barFuelFull.color = new Color(1, 1, 1, (_slider.value - blendEnd) / (blendStart - blendEnd));
            }
        }
        
    }

}
