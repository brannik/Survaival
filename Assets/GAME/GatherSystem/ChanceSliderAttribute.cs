using UnityEngine;

public class ChanceSliderAttribute : PropertyAttribute
{
    public float Min { get; }
    public float Max { get; }
    public float Step { get; }

    public ChanceSliderAttribute(float min, float max, float step = 1f)
    {
        Min = min;
        Max = max;
        Step = step;
    }
}
