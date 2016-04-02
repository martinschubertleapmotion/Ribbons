using UnityEngine;
using UnityStandardAssets.ImageEffects;

namespace FlightKit
{
	/// <summary>
	/// Makes the object grow slowly and be picked up by the user.
	/// </summary>
	public class PickupSphere : MonoBehaviour
	{
        public delegate void OnCollectAction();

		/// <summary>
		/// The event is fired every time the user-controlled airplane has picked up a sphere.
		/// </summary>
		public static event OnCollectAction OnCollectEvent;

		/// <summary>
		/// Should all PickupSpheres grow?
		/// </summary>
		public static bool growingEnabled = false;

		/// <summary>
		/// How fast does this object grow.
		/// </summary>
		public float growthSpeed = 0.1f;
		/// <summary>
		/// At what scale should growing stop.
		/// </summary>
		public float maxScale = 40f;
		/// <summary>
		/// How fast should the rings rotate?
		/// </summary>
		public float ringRotationSpeed = 150f;

		public GameObject ring1;
		public GameObject ring2;
		public GameObject sphere;

		private bool _activated;
		private BloomOptimized _bloom;
		private float _bloomInitValue;
		private bool _isTweeningOut = false;
		private bool _isDestroyed = false;

		void Start()
		{
			// Init vars.
			_bloom = GameObject.FindObjectOfType<BloomOptimized>();
            if (_bloom != null)
            {
                _bloomInitValue = _bloom.intensity;
            }

			// Randomize initial rotation.
			transform.localRotation = Random.rotation;
		}

		void Update()
		{
			// Do not update if picked up and tweened out.
			if (_isDestroyed)
			{
				return;
			}

			// Rotate rings.
			ring1.transform.Rotate(Vector3.right, ringRotationSpeed*Time.deltaTime);
			ring2.transform.Rotate(Vector3.up, ringRotationSpeed*Time.deltaTime);

			// Grow in size over time.
			if (growingEnabled && transform.localScale.x < maxScale)
			{
				transform.localScale += Vector3.one*growthSpeed*Time.deltaTime;
			}

			// Check if tweening out.
			if (_isTweeningOut)
			{
				transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 5f*Time.deltaTime);
                if (_bloom != null)
                {
				    _bloom.intensity = Mathf.Lerp(_bloom.intensity, _bloomInitValue, 5f*Time.deltaTime);
                }
			}
		}

		void OnTriggerEnter(Collider collider)
		{
			if (_activated)
			{
				return;
			}

			if (collider.gameObject.CompareTag(Tags.Player))
			{
				_activated = true;

                if (OnCollectEvent != null)
                {
                    OnCollectEvent();
                }

				var sound = GetComponentInParent<AudioSource>();
                if (sound != null) 
                {
				    sound.Play();
                }

				// Tween out the sphere
				_isTweeningOut = true;

				// Flash screen
                if (_bloom != null)
                {
				    _bloom.intensity = 0.5f;
                }

				// Disperse boids
				var boids = GetComponentInParent<BoidMaster>();
				if (boids != null)
				{
					boids.neighborDistance = 250f;
				}

				// Self destroy after delay (can't destroy this GO completely, since that would break boids)
				Invoke("DestroyNow", 1f);
			}
		}

		public void TweenBloom(float value)
		{
			_bloom.intensity = value;
		}

		private void DestroyNow()
		{
			_bloom.intensity = _bloomInitValue;

			DestroyObject(sphere);
			DestroyObject(ring1);
			DestroyObject(ring2);

			_isDestroyed = true;
		}
	}
}