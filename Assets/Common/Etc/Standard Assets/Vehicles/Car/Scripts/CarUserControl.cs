using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use
		public float hor;
		public float ver;
		public float breaks;


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate(){
			if(Input.GetKey("u")){
				ver = 1f;
			} else if(Input.GetKey("j")){
				ver = -1f;
			} else{
				ver = 0f;
			}
			if(Input.GetKey("h")){
				hor = 1f;
			} else if(Input.GetKey("k")){
				hor = -1f;
			} else{
				hor = 0f;
			}
			if(Input.GetKey("y")){
				breaks = 1f;
			} else{
				breaks = 0f;
			}
            // pass the input to the car!
            float h = -hor;
			float v = ver;
#if !MOBILE_INPUT
			float handbrake = breaks;
            m_Car.Move(h, v, v, handbrake);
#else
            m_Car.Move(h, v, v, 0f);
#endif
        }
    }
}
