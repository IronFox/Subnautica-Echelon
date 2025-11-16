using UnityEngine;

public interface IColorListener
{

    void SetColors(
        Color mainColor,
        float mainSmoothness,
        Color stripeColor,
        float stripeSmoothness,
        bool forceReapply);
}