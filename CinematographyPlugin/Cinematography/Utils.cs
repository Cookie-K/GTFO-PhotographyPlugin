using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public static class Utils
    {
         public static float SmoothDampNoOvershootProtection(float current, float target, ref float currentVelocity, float smoothTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Mathf.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * Time.deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;

            float temp = (currentVelocity + omega * change) * Time.deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            /*
            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
            output = originalTo;
            currentVelocity = (output - originalTo) / deltaTime;
            }
            */

            return output;
        }
    }
}