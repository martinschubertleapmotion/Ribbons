using UnityEngine;

namespace FlightKit
{
    public class FuelController : MonoBehaviour
    {
        /// <summary>
        /// The current amount of fuel the user has, between 0 and 1.
        /// </summary>
        public float fuelAmount { get; protected set; }
        
        /// <summary>
        /// The event is fired once the user-controlled airplane's fuel gets below 25% of capacity.
        /// </summary>
        public static event GameActions.SimpleAction OnFuelLowEvent;

        /// <summary>
        /// The event is fired once the user-controlled airplane has ran out of fuel.
        /// </summary>
        public static event GameActions.SimpleAction OnFuelEmptyEvent;
        
        private const float LOW_FUEL_PERCENT = 0.25f;

        [Header ("How fast the fuel is used. Higher number - harder gameplay.")]
        /// <summary>
        /// How fast the fuel is used. Higher number - harder gameplay.
        /// </summary>
        public float consumptionRate = 1;
        
        [Header ("How much fuel is added by each pickup. Higher number - easier gameplay.")]
        /// <summary>
        /// /// How much fuel is added by each pickup. Higher number - easier gameplay.
        /// </summary>
        public float pickupFuelAmount = 0.25f;
        
        [Header ("Amount of fuel user receives on reviving.")]
        /// <summary>
        /// Amount of fuel user receives on reviving.
        /// </summary>
        public float reviveFuelAmount = 0.5f;

        private bool _isConsuming = false;
        
        private bool _lowFuelRegistered = false;
        
        void Start()
        {
            fuelAmount = 1.0f;
        }
        
        void OnEnable()
        {
            PickupSphere.OnCollectEvent += HandlePickupCollected;
            TakeOffPublisher.OnTakeOffEvent += HandleTakeOff;
            RevivePermissionProvider.OnReviveGranted += HandleRevive;
        }

        void OnDisable()
        {
            PickupSphere.OnCollectEvent -= HandlePickupCollected;
            TakeOffPublisher.OnTakeOffEvent -= HandleTakeOff;
            RevivePermissionProvider.OnReviveGranted -= HandleRevive;
        }
        
        private void HandlePickupCollected()
        {
            fuelAmount += pickupFuelAmount;
            if (fuelAmount >= LOW_FUEL_PERCENT)
            {
                _lowFuelRegistered = false;
            }
        }
        
        private void HandleTakeOff()
        {
            _isConsuming = true;
        }
        
        private void HandleRevive()
        {
            _isConsuming = true;
            fuelAmount = reviveFuelAmount;
            
            if (fuelAmount > LOW_FUEL_PERCENT)
            {
                _lowFuelRegistered = false;
            }
            
            gameObject.SetActive(true);
        }
        
        void Update()
        {
            if (_isConsuming)
            {
                fuelAmount -= consumptionRate * Time.deltaTime * 0.01f;
                if (!_lowFuelRegistered && fuelAmount < LOW_FUEL_PERCENT)
                {
                    _lowFuelRegistered = true;
                    if (OnFuelLowEvent != null)
                    {
                        OnFuelLowEvent();
                    }
                }
            }
            if (fuelAmount <= 0 && _isConsuming)
            {
                if (OnFuelEmptyEvent != null)
                {
                    _isConsuming = false;
                    OnFuelEmptyEvent();
                }
            }
        }
    }
}
