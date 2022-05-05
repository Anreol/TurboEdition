﻿using System;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Base ImportExtension for building GameImport extension points
    /// </summary>
    [Serializable]
    public abstract class ImportExtension : ScriptableObject
    {
        private string extensionName;

        public virtual string Name => string.IsNullOrEmpty(extensionName) ? extensionName = ObjectNames.NicifyVariableName(GetType().Name) : extensionName;
        public abstract int Priority { get; }

    }
}