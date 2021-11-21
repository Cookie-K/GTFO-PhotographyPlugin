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

         public static Quaternion Diff(Quaternion q1, Quaternion q2)
         {
             return q1 * Quaternion.Inverse(q2);
         }
         
         // Quaternion SmoothDamp
         public static Quaternion QuaternionSmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
             if (Time.deltaTime < Mathf.Epsilon) return rot;
             // account for double-cover
             var Dot = Quaternion.Dot(rot, target);
             var Multi = Dot > 0f ? 1f : -1f;
             target.x *= Multi;
             target.y *= Multi;
             target.z *= Multi;
             target.w *= Multi;
             // smooth damp (nlerp approx)
             var Result = new Vector4(
                 Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                 Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                 Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                 Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
             ).normalized;
		
             // ensure deriv is tangent
             var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
             deriv.x -= derivError.x;
             deriv.y -= derivError.y;
             deriv.z -= derivError.z;
             deriv.w -= derivError.w;		
		
             return new Quaternion(Result.x, Result.y, Result.z, Result.w);
         }
    }
}