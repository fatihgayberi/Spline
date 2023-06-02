using System;
using UnityEngine;

namespace Wonnasmith.Spline
{
    public class SplineFollower : MonoBehaviour
    {
        [SerializeField] private SplineBase spline;
        [SerializeField] private bool isMove;
        [SerializeField] private float moveSpeed;

        private float current_t;

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            if (!isMove) return;

            current_t = Mathf.MoveTowards(current_t, 1f, moveSpeed * Time.deltaTime);

            current_t = Mathf.Clamp01(current_t);

            transform.position = spline.BernsteinPositionCalculator(current_t);
        }
    }
}