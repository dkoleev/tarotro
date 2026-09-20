// Minimal UnityEngine surface, ONLY for compile-checking the port outside Unity.
// Not shipped.
using System;

namespace UnityEngine
{
    public static class Mathf
    {
        public const float Rad2Deg = 57.29578f;
        public static float Exp(float f) => (float)Math.Exp(f);
        public static float Sin(float f) => (float)Math.Sin(f);
        public static float Sqrt(float f) => (float)Math.Sqrt(f);
        public static float Abs(float f) => Math.Abs(f);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : (v > hi ? hi : v);
    }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0, 0);
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
    }

    public struct Quaternion
    {
        public static Quaternion Euler(float x, float y, float z) => new Quaternion();
    }

    public static class Random
    {
        static readonly System.Random R = new System.Random(1);
        public static float value => (float)R.NextDouble();
    }

    public static class Time
    {
        public static float unscaledDeltaTime => 1f / 60f;
    }

    public class Object { }

    public class Transform : Object
    {
        public Vector3 localPosition { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; }
    }

    public class Component : Object
    {
        public Transform transform { get; } = new Transform();
    }

    public class Behaviour : Component { }

    public class MonoBehaviour : Behaviour { }

    public class SpriteRenderer : Component { }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    [AttributeUsage(AttributeTargets.All)] public class SerializeField : Attribute { }
    [AttributeUsage(AttributeTargets.All)] public class HeaderAttribute : Attribute { public HeaderAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    [AttributeUsage(AttributeTargets.All)] public class CreateAssetMenuAttribute : Attribute { public string menuName; public string fileName; }
    [AttributeUsage(AttributeTargets.All)] public class DisallowMultipleComponent : Attribute { }
    [AttributeUsage(AttributeTargets.All)] public class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int o) { } }
}
