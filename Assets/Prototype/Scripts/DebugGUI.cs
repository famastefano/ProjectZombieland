using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Prototype.Scripts.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Prototype.Scripts
{
    public class DebugGUI : MonoBehaviour
    {
        private class FieldData
        {
            public readonly FieldInfo Info;
            public readonly object DefaultValue;
            public string Text;

            public FieldData(Type type, FieldInfo info)
            {
                Info = info;
                DefaultValue = GetFieldValue(type, info);
                ResetText();
            }

            public void ResetText()
            {
                Text = DefaultValue == null ? "" : DefaultValue.ToString();
            }
        }

        private class PanelData
        {
            public readonly Type Type;
            public readonly string Name;
            public readonly FieldData[] Fields;
            public bool IsActive;

            public PanelData(Type type)
            {
                Type = type;
                Name = type.Name;
                IsActive = false;
                Fields = type
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                               BindingFlags.NonPublic)
                    .Where(field => field.GetCustomAttributes<SerializeField>().Any())
                    .Where(field => field.FieldType.IsPrimitive || field.FieldType.IsEnum)
                    .Select(field => new FieldData(type, field))
                    .ToArray();
            }
        }

        private InputAction _showGUIAction;

        private PanelData[] _types = Array.Empty<PanelData>();
        private bool _showGUI = false;

        private void Start()
        {
            _showGUIAction = InputSystem.actions.FindAction("ShowDebugGUI");
            _showGUIAction.performed += OnShowGUI;

            _types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttributes<DebugGUIAttribute>().Any())
                .Select(t => new PanelData(t))
                .ToArray();
        }

        private void OnShowGUI(InputAction.CallbackContext obj)
        {
            _showGUI = !_showGUI;
            if (_showGUI)
                InputSystem.actions.Disable();
            else
                InputSystem.actions.Enable();
            _showGUIAction.Enable();
        }

        private void OnGUI()
        {
            if (!_showGUI)
                return;

            GUILayout.Window(0, Screen.safeArea, _ =>
            {
                GUILayout.BeginScrollView(Vector2.zero);
                foreach (var data in _types)
                {
                    if (data.Fields.Length == 0)
                        continue;

                    if (GUILayout.Button(data.Name, GUILayout.ExpandWidth(false)))
                        data.IsActive = !data.IsActive;

                    if (!data.IsActive)
                        continue;

                    foreach (var field in data.Fields)
                    {
                        var type = field.Info.FieldType;
                        object value = GetFieldValue(data.Type, field.Info);
                        if (type == typeof(bool))
                        {
                            var curr = (bool)value;
                            bool newValue = GUILayout.Toggle(curr, "Toggle", GUILayout.ExpandWidth(false));
                            if (newValue != curr)
                                SetFieldValue(data.Type, field.Info, value);
                        }
                        else
                        {
                            string text = value.ToString();

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(field.Info.Name, GUILayout.ExpandWidth(false));
                            field.Text = GUILayout.TextField(field.Text, GUILayout.ExpandWidth(true),
                                GUILayout.ExpandWidth(true));

                            if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false)))
                            {
                                if (text != field.Text)
                                {
                                    object newValue = ExtractFieldValue(field.Info, field.Text);
                                    if (newValue != null)
                                        SetFieldValue(data.Type, field.Info, newValue);
                                }
                            }

                            if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                            {
                                SetFieldValue(data.Type, field.Info, field.DefaultValue);
                                field.ResetText();
                            }

                            GUILayout.EndHorizontal();
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUI.DragWindow();
            }, "DebugGUI", GUILayout.ExpandWidth(true));
        }

        private static object GetFieldValue(Type objectType, FieldInfo field)
        {
            var obj = FindObjectsByType(objectType, FindObjectsSortMode.None).First();
            return field.GetValue(obj);
        }

        private static void SetFieldValue(Type objectType, FieldInfo field, object value)
        {
            var objects = FindObjectsByType(objectType, FindObjectsSortMode.None);
            foreach (var obj in objects)
                field.SetValue(obj, value);
        }

        private static object ExtractFieldValue(FieldInfo field, string text)
        {
            var t = field.FieldType;
            object newValue = null;
            if (t.IsPrimitive)
            {
                if (t.IsAssignableFrom(typeof(double)))
                {
                    if (double.TryParse(text, out var _double))
                        return _double;
                }
                else
                {
                    if (t == typeof(bool) && bool.TryParse(text, out var _bool))
                        newValue = _bool;
                    else if (t == typeof(byte) && byte.TryParse(text, out var _byte))
                        newValue = _byte;
                    else if (t == typeof(sbyte) && sbyte.TryParse(text, out var _sbyte))
                        newValue = _sbyte;
                    else if (t == typeof(short) && short.TryParse(text, out var _short))
                        newValue = _short;
                    else if (t == typeof(ushort) && ushort.TryParse(text, out var _ushort))
                        newValue = _ushort;
                    else if (t == typeof(int) && int.TryParse(text, out var _int))
                        newValue = _int;
                    else if (t == typeof(uint) && uint.TryParse(text, out var _uint))
                        newValue = _uint;
                    else if (t == typeof(long) && long.TryParse(text, out var _long))
                        newValue = _long;
                    else if (t == typeof(ulong) && ulong.TryParse(text, out var _ulong))
                        newValue = _ulong;
                    else if (t == typeof(char) && char.TryParse(text, out var _char))
                        newValue = _char;
                    else if (t == typeof(double) && double.TryParse(text, out var _double))
                        newValue = _double;
                    else if (t == typeof(float) && float.TryParse(text, out var _float))
                        newValue = _float;

                    if (newValue != null)
                        return newValue;
                }
            }
            else if (t.IsEnum)
            {
                if (Enum.TryParse(t, text, true, out newValue))
                    return newValue;
            }

            return null;
        }
    }
}