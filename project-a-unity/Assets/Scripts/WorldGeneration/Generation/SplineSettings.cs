using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectArcane.WorldGeneration
{
    [Serializable]
    public struct SplinePoint
    {
        public float Input;
        public float Output;

        public SplinePoint(float input, float output)
        {
            Input = input;
            Output = output;
        }
    }

    [Serializable]
    public sealed class SplineSettings
    {
        [SerializeField] private List<SplinePoint> points = new List<SplinePoint>
        {
            new SplinePoint(-1f, 0f),
            new SplinePoint(1f, 0f)
        };

        public List<SplinePoint> Points => points;

        public float Evaluate(float input)
        {
            if (points == null || points.Count == 0)
            {
                return 0f;
            }

            if (points.Count == 1)
            {
                return points[0].Output;
            }

            SplinePoint previous = points[0];
            SplinePoint next = points[points.Count - 1];
            for (int i = 1; i < points.Count; i++)
            {
                if (input <= points[i].Input)
                {
                    previous = points[i - 1];
                    next = points[i];
                    break;
                }
            }

            if (input <= points[0].Input)
            {
                return points[0].Output;
            }

            if (input >= points[points.Count - 1].Input)
            {
                return points[points.Count - 1].Output;
            }

            float range = Mathf.Max(0.0001f, next.Input - previous.Input);
            float t = Mathf.Clamp01((input - previous.Input) / range);
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(previous.Output, next.Output, t);
        }

        public SplineSettings Clone()
        {
            SplineSettings clone = new SplineSettings();
            clone.points = new List<SplinePoint>(points);
            return clone;
        }

        public static SplineSettings Create(params SplinePoint[] splinePoints)
        {
            SplineSettings settings = new SplineSettings();
            settings.points = new List<SplinePoint>(splinePoints);
            return settings;
        }
    }
}
