using UnityEngine;

/// <summary>
/// Original Author: Max Kaufmann (max.kaufmann@gmail.com)
/// </summary>
public static class QuaternionUtil
{
	
	public static Quaternion AngVelToDeriv(Quaternion current, Vector3 angVel)
    {
        Quaternion spin = new Quaternion(angVel.x, angVel.y, angVel.z, 0f);
        Quaternion result = spin * current;

		return new Quaternion(0.5f * result.x, 0.5f * result.y, 0.5f * result.z, 0.5f * result.w);
	} 

	public static Vector3 DerivToAngVel(Quaternion current, Quaternion deriv)
    {
        Quaternion result = deriv * Quaternion.Inverse(current);

		return new Vector3(2f * result.x, 2f * result.y, 2f * result.z);
	}

	public static Quaternion IntegrateRotation(Quaternion rotation, Vector3 angularVelocity, float deltaTime)
    {
        Quaternion deriv = AngVelToDeriv(rotation, angularVelocity);
        Vector4 Pred = new Vector4(
				rotation.x + deriv.x * deltaTime,
				rotation.y + deriv.y * deltaTime,
				rotation.z + deriv.z * deltaTime,
				rotation.w + deriv.w * deltaTime
		).normalized;

		return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
	}
	
	public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Quaternion angularVelocity, float smoothTime)
    {
        // Account for double-cover
        float dot = Quaternion.Dot(current, target);
        float multiplier = dot > 0f ? 1f : -1f;
		target.x *= multiplier;
		target.y *= multiplier;
		target.z *= multiplier;
		target.w *= multiplier;

        // Smooth damp (nlerp approx)
        Vector4 result = new Vector4(
			Mathf.SmoothDamp(current.x, target.x, ref angularVelocity.x, smoothTime),
			Mathf.SmoothDamp(current.y, target.y, ref angularVelocity.y, smoothTime),
			Mathf.SmoothDamp(current.z, target.z, ref angularVelocity.z, smoothTime),
			Mathf.SmoothDamp(current.w, target.w, ref angularVelocity.w, smoothTime)
		).normalized;

        // Compute deriv
        float dtInv = 1f / Time.deltaTime;
		angularVelocity.x = (result.x - current.x) * dtInv;
		angularVelocity.y = (result.y - current.y) * dtInv;
		angularVelocity.z = (result.z - current.z) * dtInv;
		angularVelocity.w = (result.w - current.w) * dtInv;

		return new Quaternion(result.x, result.y, result.z, result.w);
	}
}
