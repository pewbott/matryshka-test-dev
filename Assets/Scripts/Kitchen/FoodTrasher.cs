using System;

using UnityEngine;

using JetBrains.Annotations;

namespace CookingPrototype.Kitchen {
	[RequireComponent(typeof(FoodPlace))]
	public sealed class FoodTrasher : MonoBehaviour {

		FoodPlace _place = null;
		float _timer = 0f;

		void Start() {
			_place = GetComponent<FoodPlace>();
			_timer = Time.realtimeSinceStartup;
		}

		bool isFirstTapped = false;
		float dobuleTapTimer = 0.5f;
		float timer = 0f;

		private void Update() {
			if ( isFirstTapped ) {
				timer += Time.deltaTime;
			}

			if ( timer > dobuleTapTimer ) {
				isFirstTapped = false;
				timer = 0;
			}
		}

		/// <summary>
		/// Освобождает место по двойному тапу если еда на этом месте сгоревшая.
		/// </summary>
		[UsedImplicitly]
		public void TryTrashFood() {

			if ( !isFirstTapped )
				isFirstTapped = true;
			else {
				if (_place.CurFood != null && _place.CurFood.CurStatus == Food.FoodStatus.Overcooked ) {
					_place.FreePlace();
				}
			}
		}
	}
}