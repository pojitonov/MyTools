﻿using System;
using UnityEngine;

namespace Example1
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private GameObject _inputGameObject;
        [SerializeField] private float _force = 0.05f;

        private Rigidbody _rigidbody;
        private IInput _input;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = _inputGameObject.GetComponent<IInput>();
        }

        private void InputHandler(MovementDirection direction)
        {
            Vector3 moment = GetDirectionVector(direction) * _force;
            _rigidbody.AddForce(moment, ForceMode.Impulse);
        }

        private static Vector3 GetDirectionVector(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.Up:
                    return new Vector3(0, 0, 1); 
                case MovementDirection.Right:
                    return new Vector3(1, 0, 0); 
                case MovementDirection.Down:
                    return new Vector3(0, 0, -1); 
                case MovementDirection.Left:
                    return new Vector3(-1, 0, 0); 
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private void OnEnable()
        {
            _input.InputEvent += InputHandler;
        }

        private void OnDisable()
        {
            _input.InputEvent += InputHandler;
        }
    }
}