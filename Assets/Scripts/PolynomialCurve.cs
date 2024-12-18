using System.Collections.Generic;
using UnityEngine;

public static class PolynomialCurve
{
    public static Vector3 CalculatePoint(float t, List<Vector3> controlPoints)
    {
        int n = controlPoints.Count - 1;
        Vector3 point = Vector3.zero;

        for (int i = 0; i <= n; i++)
        {
            float basis = LagrangeMath.Basis(t, i, controlPoints.Count);
            point += basis * controlPoints[i];
        }

        return point;
    }

    public static Vector3 CalculateTangent(float t, List<Vector3> controlPoints)
    {
        int n = controlPoints.Count - 1;
        Vector3 tangent = Vector3.zero;

        for (int i = 0; i <= n; i++)
        {
            float derivativeBasis = LagrangeMath.DerivativeBasis(t, i, n);
            tangent += derivativeBasis * controlPoints[i];
        }

        return tangent.normalized;
    }
}
