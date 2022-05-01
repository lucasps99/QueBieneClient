using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateComponent : MonoBehaviour
{
    float m_accumulatedTime = 0;

    public static float EaseInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }

    // Update is called once per frame
    void Update()
    {
        m_accumulatedTime += Time.deltaTime;
        m_accumulatedTime = m_accumulatedTime % 1.3f;
        transform.rotation = Quaternion.Euler(0,0, EaseInOutQuad(0,1,m_accumulatedTime / 1.3f)*-360);
    }
}
