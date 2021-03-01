using UnityEngine;

namespace Tenshi.UnitySoku
{
    /// <summary> Debug utility from Tenshi! </summary>
    public static class TLog
    {
        private static readonly string Hr = "\n\n-------------------------------------------------------------------------------";
        private static readonly string Prefix = "=>    ";
        public static void Msg(this object msg) => (msg.ToString() + Hr).Print();
        public static void Warn(this object msg) => Debug.LogWarning(msg.ToString() + Hr);
        public static void Error(this object msg) => Debug.LogError(msg.ToString() + Hr);


        public static void Vector(Vector2 vector, string title = "") => $"{title} {Prefix} Vector2({vector.x}, {vector.y})".Bold().Msg();
        public static void Vector(Vector3 vector, string title = "") => $"{title} {Prefix} Vector3({vector.x}, {vector.y}, {vector.z})".Bold().Msg();
        public static void Quaternion(Quaternion qua, string title = "") => $"{title} {Prefix} Quaternion({qua.x}, {qua.y}, {qua.z}, {qua.w})".Bold().Msg();
        public static void EulerAngles(Quaternion qua, string title = "") => $"{title} {Prefix} EulerAngles({qua.eulerAngles.x}, {qua.eulerAngles.y}, {qua.eulerAngles.z})".Bold().Msg();
        public static void Colour(Color col, string title = "") => $"{title} {Prefix} RGBA({col.r}, {col.g}, {col.b}, {col.a})".Bold().Msg();
    }
}
