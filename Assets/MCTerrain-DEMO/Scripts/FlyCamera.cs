using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple Fly Camera script.
/// </summary>

namespace MCTerrain
{
    public class FlyCamera : MonoBehaviour
    {
        [SerializeField]
        private float _mainSpeed = 10.0f;
        [SerializeField]
        private float _shiftAdd = 15.0f;
        [SerializeField]
        private float _maxShift = 200f;
        [SerializeField]
        private float _camSens = 0.25f;

        // Set the last mouse position near to the middle of the screen, rather than at the top (play)
        private Vector3 _lastMouse = new(255, 255, 255); 
        private float _totalRun = 1.0f;

        void Update()
        {
            _lastMouse = Input.mousePosition - _lastMouse;
            _lastMouse = new Vector3(-_lastMouse.y * _camSens, _lastMouse.x * _camSens, 0);
            _lastMouse = new Vector3(transform.eulerAngles.x + _lastMouse.x, transform.eulerAngles.y + _lastMouse.y, 0);
            transform.eulerAngles = _lastMouse;
            _lastMouse = Input.mousePosition;

            //Keyboard commands
            Vector3 velocity = GetBaseInput();

            if (Input.GetKey(KeyCode.LeftShift))
            {
                _totalRun += Time.deltaTime;
                velocity = velocity * _totalRun * _shiftAdd;
                velocity.x = Mathf.Clamp(velocity.x, -_maxShift, _maxShift);
                velocity.y = Mathf.Clamp(velocity.y, -_maxShift, _maxShift);
                velocity.z = Mathf.Clamp(velocity.z, -_maxShift, _maxShift);
            }
            else
            {
                _totalRun = Mathf.Clamp(_totalRun * 0.5f, 1, 1000);
                velocity *= _mainSpeed;
            }

            velocity *= Time.deltaTime;

                transform.Translate(velocity);
  
        }

        /// <summary>
        /// Returns to input velocity to add to the current position.
        /// </summary>
        /// <returns></returns>
        private Vector3 GetBaseInput()
        { 
            
            Vector3 inputVelocity = new Vector3();

            if (Input.GetKey(KeyCode.W))
            {
                inputVelocity += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputVelocity += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputVelocity += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputVelocity += Vector3.right;
            }

            return inputVelocity;
        }

    }

}