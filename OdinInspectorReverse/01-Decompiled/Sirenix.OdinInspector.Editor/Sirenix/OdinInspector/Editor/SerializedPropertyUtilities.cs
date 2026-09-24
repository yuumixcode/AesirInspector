using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public static class SerializedPropertyUtilities
	{
		private static Dictionary<Type, Delegate> PrimitiveValueGetters = new Dictionary<Type, Delegate>
		{
			{
				typeof(int),
				(Func<SerializedProperty, int>)((SerializedProperty p) => p.intValue)
			},
			{
				typeof(bool),
				(Func<SerializedProperty, bool>)((SerializedProperty p) => p.boolValue)
			},
			{
				typeof(float),
				(Func<SerializedProperty, float>)((SerializedProperty p) => p.floatValue)
			},
			{
				typeof(string),
				(Func<SerializedProperty, string>)((SerializedProperty p) => p.stringValue)
			},
			{
				typeof(Color),
				(Func<SerializedProperty, Color>)((SerializedProperty p) => p.colorValue)
			},
			{
				typeof(LayerMask),
				(Func<SerializedProperty, LayerMask>)((SerializedProperty p) => p.intValue)
			},
			{
				typeof(Vector2),
				(Func<SerializedProperty, Vector2>)((SerializedProperty p) => p.vector2Value)
			},
			{
				typeof(Vector3),
				(Func<SerializedProperty, Vector3>)((SerializedProperty p) => p.vector3Value)
			},
			{
				typeof(Vector4),
				(Func<SerializedProperty, Vector4>)((SerializedProperty p) => p.vector4Value)
			},
			{
				typeof(Vector2Int),
				(Func<SerializedProperty, Vector2Int>)((SerializedProperty p) => p.vector2IntValue)
			},
			{
				typeof(Vector3Int),
				(Func<SerializedProperty, Vector3Int>)((SerializedProperty p) => p.vector3IntValue)
			},
			{
				typeof(RectInt),
				(Func<SerializedProperty, RectInt>)((SerializedProperty p) => p.rectIntValue)
			},
			{
				typeof(Rect),
				(Func<SerializedProperty, Rect>)((SerializedProperty p) => p.rectValue)
			},
			{
				typeof(char),
				(Func<SerializedProperty, char>)((SerializedProperty p) => (char)p.intValue)
			},
			{
				typeof(AnimationCurve),
				(Func<SerializedProperty, AnimationCurve>)((SerializedProperty p) => p.animationCurveValue)
			},
			{
				typeof(Bounds),
				(Func<SerializedProperty, Bounds>)((SerializedProperty p) => p.boundsValue)
			},
			{
				typeof(Quaternion),
				(Func<SerializedProperty, Quaternion>)((SerializedProperty p) => p.quaternionValue)
			},
			{
				typeof(BoundsInt),
				(Func<SerializedProperty, BoundsInt>)((SerializedProperty p) => p.boundsIntValue)
			}
		};

		private static Dictionary<Type, Delegate> PrimitiveValueSetters = new Dictionary<Type, Delegate>
		{
			{
				typeof(int),
				(Action<SerializedProperty, int>)delegate(SerializedProperty p, int v)
				{
					p.intValue = v;
				}
			},
			{
				typeof(bool),
				(Action<SerializedProperty, bool>)delegate(SerializedProperty p, bool v)
				{
					p.boolValue = v;
				}
			},
			{
				typeof(float),
				(Action<SerializedProperty, float>)delegate(SerializedProperty p, float v)
				{
					p.floatValue = v;
				}
			},
			{
				typeof(string),
				(Action<SerializedProperty, string>)delegate(SerializedProperty p, string v)
				{
					p.stringValue = v;
				}
			},
			{
				typeof(Color),
				(Action<SerializedProperty, Color>)delegate(SerializedProperty p, Color v)
				{
					p.colorValue = v;
				}
			},
			{
				typeof(LayerMask),
				(Action<SerializedProperty, LayerMask>)delegate(SerializedProperty p, LayerMask v)
				{
					p.intValue = v;
				}
			},
			{
				typeof(Vector2),
				(Action<SerializedProperty, Vector2>)delegate(SerializedProperty p, Vector2 v)
				{
					p.vector2Value = v;
				}
			},
			{
				typeof(Vector3),
				(Action<SerializedProperty, Vector3>)delegate(SerializedProperty p, Vector3 v)
				{
					p.vector3Value = v;
				}
			},
			{
				typeof(Vector4),
				(Action<SerializedProperty, Vector4>)delegate(SerializedProperty p, Vector4 v)
				{
					p.vector4Value = v;
				}
			},
			{
				typeof(Vector2Int),
				(Action<SerializedProperty, Vector2Int>)delegate(SerializedProperty p, Vector2Int v)
				{
					p.vector2IntValue = v;
				}
			},
			{
				typeof(Vector3Int),
				(Action<SerializedProperty, Vector3Int>)delegate(SerializedProperty p, Vector3Int v)
				{
					p.vector3IntValue = v;
				}
			},
			{
				typeof(RectInt),
				(Action<SerializedProperty, RectInt>)delegate(SerializedProperty p, RectInt v)
				{
					p.rectIntValue = v;
				}
			},
			{
				typeof(Rect),
				(Action<SerializedProperty, Rect>)delegate(SerializedProperty p, Rect v)
				{
					p.rectValue = v;
				}
			},
			{
				typeof(char),
				(Action<SerializedProperty, char>)delegate(SerializedProperty p, char v)
				{
					p.intValue = v;
				}
			},
			{
				typeof(AnimationCurve),
				(Action<SerializedProperty, AnimationCurve>)delegate(SerializedProperty p, AnimationCurve v)
				{
					p.animationCurveValue = v;
				}
			},
			{
				typeof(Bounds),
				(Action<SerializedProperty, Bounds>)delegate(SerializedProperty p, Bounds v)
				{
					p.boundsValue = v;
				}
			},
			{
				typeof(Quaternion),
				(Action<SerializedProperty, Quaternion>)delegate(SerializedProperty p, Quaternion v)
				{
					p.quaternionValue = v;
				}
			},
			{
				typeof(BoundsInt),
				(Action<SerializedProperty, BoundsInt>)delegate(SerializedProperty p, BoundsInt v)
				{
					p.boundsIntValue = v;
				}
			}
		};

		private static Dictionary<string, Type> UnityTypes;

		private static Type GetUnityTypeWithName(string name)
		{
			if (UnityTypes == null)
			{
				UnityTypes = new Dictionary<string, Type>();
				foreach (Type type in from n in AssemblyUtilities.GetTypes(AssemblyCategory.UnityEngine)
					where typeof(UnityEngine.Object).IsAssignableFrom(n)
					select n)
				{
					if (UnityTypes.ContainsKey(type.Name))
					{
						UnityTypes[type.Name] = null;
					}
					else
					{
						UnityTypes[type.Name] = type;
					}
				}
			}
			UnityTypes.TryGetValue(name, out var result);
			return result;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static string GetProperTypeName(this SerializedProperty property)
		{
			if (property.type.StartsWith("PPtr<"))
			{
				return property.type.Substring(5).Trim('<', '>', '$');
			}
			return property.type;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool IsCompatibleWithType(this SerializedProperty property, Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			switch (property.propertyType)
			{
			case SerializedPropertyType.Generic:
				return property.type == type.Name;
			case SerializedPropertyType.Integer:
				return type == typeof(int);
			case SerializedPropertyType.Boolean:
				return type == typeof(bool);
			case SerializedPropertyType.Float:
				return type == typeof(float);
			case SerializedPropertyType.String:
				return type == typeof(string);
			case SerializedPropertyType.Color:
				return type == typeof(Color);
			case SerializedPropertyType.ObjectReference:
			{
				if ((object)property.objectReferenceValue != null)
				{
					return property.objectReferenceValue.GetType().IsAssignableFrom(type);
				}
				string typeName = property.GetProperTypeName();
				if (typeName == "Prefab")
				{
					return type == typeof(GameObject);
				}
				Type possibleType = GetUnityTypeWithName(typeName);
				if (possibleType != null)
				{
					return possibleType.IsAssignableFrom(type);
				}
				return false;
			}
			case SerializedPropertyType.LayerMask:
				return type == typeof(LayerMask);
			case SerializedPropertyType.Enum:
			{
				if (!type.IsEnum)
				{
					return false;
				}
				string[] enumNames = Enum.GetNames(type);
				string[] propNames = property.enumNames;
				if (enumNames.Length != propNames.Length)
				{
					return false;
				}
				for (int i = 0; i < enumNames.Length; i++)
				{
					if (!string.Equals(enumNames[i], propNames[i].Replace(" ", ""), StringComparison.InvariantCultureIgnoreCase))
					{
						return false;
					}
				}
				return true;
			}
			case SerializedPropertyType.Vector2:
				return type == typeof(Vector2);
			case SerializedPropertyType.Vector3:
				return type == typeof(Vector3);
			case SerializedPropertyType.Vector4:
				return type == typeof(Vector4);
			case SerializedPropertyType.Vector2Int:
				return type == typeof(Vector2Int);
			case SerializedPropertyType.Vector3Int:
				return type == typeof(Vector3Int);
			case SerializedPropertyType.RectInt:
				return type == typeof(RectInt);
			case SerializedPropertyType.Rect:
				return type == typeof(Rect);
			case SerializedPropertyType.ArraySize:
				return false;
			case SerializedPropertyType.Character:
				return type == typeof(char);
			case SerializedPropertyType.AnimationCurve:
				return type == typeof(AnimationCurve);
			case SerializedPropertyType.Bounds:
				return type == typeof(Bounds);
			case SerializedPropertyType.Gradient:
				return type == typeof(Gradient);
			case SerializedPropertyType.Quaternion:
				return type == typeof(Quaternion);
			case SerializedPropertyType.BoundsInt:
				return type == typeof(BoundsInt);
			default:
				return false;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Type GuessContainedType(this SerializedProperty property)
		{
			switch (property.propertyType)
			{
			case SerializedPropertyType.Generic:
				return null;
			case SerializedPropertyType.Integer:
				return typeof(int);
			case SerializedPropertyType.Boolean:
				return typeof(bool);
			case SerializedPropertyType.Float:
				return typeof(float);
			case SerializedPropertyType.String:
				return typeof(string);
			case SerializedPropertyType.Color:
				return typeof(Color);
			case SerializedPropertyType.ObjectReference:
			{
				if ((object)property.objectReferenceValue != null)
				{
					return property.objectReferenceValue.GetType();
				}
				string typeName = property.GetProperTypeName();
				List<Type> possibles = (from n in AssemblyUtilities.GetTypes(AssemblyCategory.UnityEngine)
					where n.Name == typeName && typeof(UnityEngine.Object).IsAssignableFrom(n)
					select n).ToList();
				if (possibles.Count == 1)
				{
					return possibles[0];
				}
				return null;
			}
			case SerializedPropertyType.LayerMask:
				return typeof(LayerMask);
			case SerializedPropertyType.Enum:
				return null;
			case SerializedPropertyType.Vector2:
				return typeof(Vector2);
			case SerializedPropertyType.Vector3:
				return typeof(Vector3);
			case SerializedPropertyType.Vector4:
				return typeof(Vector4);
			case SerializedPropertyType.Vector2Int:
				return typeof(Vector2Int);
			case SerializedPropertyType.Vector3Int:
				return typeof(Vector3Int);
			case SerializedPropertyType.RectInt:
				return typeof(RectInt);
			case SerializedPropertyType.Rect:
				return typeof(Rect);
			case SerializedPropertyType.ArraySize:
				return null;
			case SerializedPropertyType.Character:
				return typeof(char);
			case SerializedPropertyType.AnimationCurve:
				return typeof(AnimationCurve);
			case SerializedPropertyType.Bounds:
				return typeof(Bounds);
			case SerializedPropertyType.Gradient:
				return typeof(Gradient);
			case SerializedPropertyType.Quaternion:
				return typeof(Quaternion);
			case SerializedPropertyType.BoundsInt:
				return typeof(BoundsInt);
			default:
				return null;
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static bool CanSetGetValue(Type type)
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return true;
			}
			return PrimitiveValueGetters.ContainsKey(type);
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Func<SerializedProperty, T> GetValueGetter<T>()
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
			{
				return (SerializedProperty p) => (T)(object)p.objectReferenceValue;
			}
			PrimitiveValueGetters.TryGetValue(typeof(T), out var result);
			return (Func<SerializedProperty, T>)result;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public static Action<SerializedProperty, T> GetValueSetter<T>()
		{
			if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
			{
				return delegate(SerializedProperty p, T v)
				{
					p.objectReferenceValue = (UnityEngine.Object)(object)v;
				};
			}
			PrimitiveValueSetters.TryGetValue(typeof(T), out var result);
			return (Action<SerializedProperty, T>)result;
		}
	}
}
