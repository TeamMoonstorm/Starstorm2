using System;

namespace RoR2EditorKit
{
    /// <summary>
    /// Shorthands for throwing Errors.
    /// </summary>
    public static class ErrorShorthands
    {
        public static NullReferenceException ThrowNullAssetName(string fieldName)
        {
            return new NullReferenceException($"Field {fieldName} cannot be Empty or Null");
        }

        public static NullReferenceException ThrowNullTokenPrefix()
        {
            return new NullReferenceException($"Your TokenPrefix in the RoR2EditorKit settings is Empty or Null");
        }

        public static NullReferenceException ThrowNullMainManifest()
        {
            return new NullReferenceException($"Your Main Manifest in the RoR2EditorKit Settings is Empty");
        }
    }
}