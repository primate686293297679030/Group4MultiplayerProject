using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class GameTimer : MonoBehaviour
{
   
        [SerializeField] private float time = 10;
        [SerializeField] private bool active;
        public static GameTimer instance = null;

        private float _currentTime;
        public UnityEvent onTimerEnd;
        public UnityEvent onTimerStart;
   [SerializeField] int seconds;

        private void Awake()
        {

            if (instance == null)
            {

                instance = this;
            }
            else

            {

                Destroy(this);
            }
        }
        public void Enable()
        {
            active = true;
            onTimerStart.Invoke();
        }

        public void Disable()
        {
            active = false;
            onTimerEnd.Invoke();
        }

        private void Update()
        {
            if (active)
            {
            seconds = Convert.ToInt32(_currentTime % 60);
                _currentTime -= Time.deltaTime;
           // Debug.Log(seconds);
            if (_currentTime <= 0)
                {
                    _currentTime = time;
                  
                    Disable();
                }
            }
        }

        private void OnValidate()
        {
            _currentTime = time;
        }
    
}
