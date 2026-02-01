using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
    // makes the docked editor window minimum size just a bit smaller to fit some nice controls
    // must use reflection because unity hid this parameter under a private field
    // see this discussion for solution. https://discussions.unity.com/t/editorwindow-minsize-not-working-if-docked/876523/11
    [InitializeOnLoad]
    internal static class SetDockedMinSize
    {
        static SetDockedMinSize()
        {
            // https://github.com/Unity-Technologies/UnityCsReference/blob/bf25390e5c79172c3d3e9a6b755680679e1dbd50/Editor/Mono/HostView.cs#L94
            var type = typeof(Editor).Assembly.GetType("UnityEditor.HostView");
            var fieldInfo = type.GetField("k_DockedMinSize", BindingFlags.Static | BindingFlags.NonPublic);

            fieldInfo!.SetValue(null, new Vector2(48, 44));
        }
    }
}