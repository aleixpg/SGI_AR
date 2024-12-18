using UnityEngine;

public static class LagrangeMath
{
    public static float Basis(float t, int i, int count)
    {
        float result = 1.0f;
        for (int j = 0; j < count; j++)
        {
            if (j != i)
            {
                result *= (t - (float)j / (count - 1)) / ((float)i / (count - 1) - (float)j / (count - 1));
            }
        }
        return result;
    }

    public static float DerivativeBasis(float t, int i, int n)
    {
        float result = 0f;
        for (int j = 0; j <= n; j++)
        {
            if (j != i)
            {
                float product = 1f;
                for (int k = 0; k <= n; k++)
                {
                    if (k != i && k != j)
                    {
                        product *= (t - (float)k / n) / ((float)i / n - (float)k / n);
                    }
                }
                result += product / ((float)i / n - (float)j / n);
            }
        }
        return result;
    }
}
