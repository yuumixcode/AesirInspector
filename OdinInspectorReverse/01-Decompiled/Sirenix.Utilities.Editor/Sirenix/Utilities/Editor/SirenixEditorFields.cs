using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities.Editor.Expressions;
using Sirenix.Utilities.Editor.Internal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Field drawing functions for various types.
	/// </summary>
	public static class SirenixEditorFields
	{
		private delegate bool _ExpressionEvaluator_Evaluate<T>(string expression, out T result);

		[StructLayout(LayoutKind.Explicit, Size = 17)]
		private struct EditableNumber
		{
			public enum NumberType : byte
			{
				Invalid,
				Decimal,
				Double,
				Long
			}

			[FieldOffset(0)]
			public readonly NumberType Type;

			[FieldOffset(1)]
			private decimal vDecimal;

			[FieldOffset(1)]
			private double vDouble;

			[FieldOffset(1)]
			private long vLong;

			public decimal AsDecimal => Type switch
			{
				NumberType.Decimal => vDecimal, 
				NumberType.Double => (decimal)vDouble, 
				NumberType.Long => vLong, 
				_ => throw new Exception("Invalid type: " + Type), 
			};

			public double AsDouble => Type switch
			{
				NumberType.Decimal => (double)vDecimal, 
				NumberType.Double => vDouble, 
				NumberType.Long => vLong, 
				_ => throw new Exception("Invalid type: " + Type), 
			};

			public long AsLong => Type switch
			{
				NumberType.Decimal => (long)vDecimal, 
				NumberType.Double => (long)vDouble, 
				NumberType.Long => vLong, 
				_ => throw new Exception("Invalid type: " + Type), 
			};

			public float AsFloat => (float)AsDouble;

			public int AsInt => (int)AsLong;

			public bool IsInteger => Type == NumberType.Long;

			public EditableNumber(decimal value)
			{
				Type = NumberType.Decimal;
				vDouble = 0.0;
				vLong = 0L;
				vDecimal = value;
			}

			public EditableNumber(double value)
			{
				Type = NumberType.Double;
				vDecimal = default(decimal);
				vLong = 0L;
				vDouble = value;
			}

			public EditableNumber(long value)
			{
				Type = NumberType.Long;
				vDecimal = default(decimal);
				vDouble = 0.0;
				vLong = value;
			}

			public EditableNumber(float value)
				: this((double)value)
			{
			}

			public EditableNumber(int value)
				: this((long)value)
			{
			}

			public EditableNumber(NumberType type, decimal value)
			{
				Type = type;
				vDecimal = default(decimal);
				vDouble = 0.0;
				vLong = 0L;
				switch (type)
				{
				case NumberType.Decimal:
					vDecimal = value;
					break;
				case NumberType.Double:
					vDouble = (double)value;
					break;
				case NumberType.Long:
					if (value > 9223372036854775807m)
					{
						vLong = long.MaxValue;
					}
					else if (value < -9223372036854775808m)
					{
						vLong = long.MinValue;
					}
					else
					{
						vLong = (long)Math.Round(value, MidpointRounding.AwayFromZero);
					}
					break;
				default:
					throw new Exception("Invalid type.");
				}
			}

			public EditableNumber(NumberType type, double value)
			{
				Type = type;
				vDecimal = default(decimal);
				vDouble = 0.0;
				vLong = 0L;
				switch (type)
				{
				case NumberType.Decimal:
					vDecimal = (decimal)value;
					break;
				case NumberType.Double:
					vDouble = value;
					break;
				case NumberType.Long:
					vLong = (long)Math.Round(value, MidpointRounding.AwayFromZero);
					break;
				default:
					throw new Exception("Invalid type.");
				}
			}

			public EditableNumber(NumberType type, long value)
			{
				Type = type;
				vDecimal = default(decimal);
				vDouble = 0.0;
				vLong = 0L;
				switch (type)
				{
				case NumberType.Decimal:
					vDecimal = value;
					break;
				case NumberType.Double:
					vDouble = value;
					break;
				case NumberType.Long:
					vLong = value;
					break;
				default:
					throw new Exception("Invalid type.");
				}
			}

			public EditableNumber(NumberType type, EditableNumber value)
			{
				Type = type;
				vDecimal = default(decimal);
				vDouble = 0.0;
				vLong = 0L;
				switch (type)
				{
				case NumberType.Decimal:
					vDecimal = value.AsDecimal;
					break;
				case NumberType.Double:
					vDouble = value.AsDouble;
					break;
				case NumberType.Long:
					vLong = value.AsLong;
					break;
				default:
					throw new Exception("Invalid type.");
				}
			}

			public override bool Equals(object obj)
			{
				if (obj is EditableNumber b)
				{
					return this == b;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return Type switch
				{
					NumberType.Decimal => vDecimal.GetHashCode(), 
					NumberType.Double => vDouble.GetHashCode(), 
					NumberType.Long => vLong.GetHashCode(), 
					_ => 0, 
				};
			}

			public string ToString(string format, CultureInfo cultureInfo)
			{
				return Type switch
				{
					NumberType.Decimal => vDecimal.ToString(format, cultureInfo), 
					NumberType.Double => vDouble.ToString(format, cultureInfo), 
					NumberType.Long => vLong.ToString(format, cultureInfo), 
					_ => throw new Exception("Invalid type."), 
				};
			}

			public static bool TryParse(NumberType type, string s, out EditableNumber result)
			{
				switch (type)
				{
				case NumberType.Decimal:
				{
					if (decimal.TryParse(s, out var d))
					{
						result = new EditableNumber(d);
						return true;
					}
					break;
				}
				case NumberType.Double:
				{
					if (double.TryParse(s, out var f))
					{
						result = new EditableNumber(f);
						return true;
					}
					break;
				}
				case NumberType.Long:
				{
					if (long.TryParse(s, out var l))
					{
						result = new EditableNumber(l);
						return true;
					}
					break;
				}
				default:
					throw new Exception("Invalid type.");
				}
				result = default(EditableNumber);
				return false;
			}

			public static EditableNumber operator +(EditableNumber a, EditableNumber b)
			{
				return a.Type switch
				{
					NumberType.Decimal => new EditableNumber(a.vDecimal + b.AsDecimal), 
					NumberType.Double => new EditableNumber(a.vDouble + b.AsDouble), 
					NumberType.Long => new EditableNumber(a.vLong + b.AsLong), 
					_ => throw new Exception("Invalid type."), 
				};
			}

			public static bool operator ==(EditableNumber a, EditableNumber b)
			{
				return a.Type switch
				{
					NumberType.Decimal => a.vDecimal == b.AsDecimal, 
					NumberType.Double => a.vDouble == b.AsDouble, 
					NumberType.Long => a.vLong == b.AsLong, 
					_ => false, 
				};
			}

			public static bool operator !=(EditableNumber a, EditableNumber b)
			{
				return !(a == b);
			}

			public static bool operator ==(EditableNumber a, decimal b)
			{
				if (a.Type == NumberType.Decimal)
				{
					return a.vDecimal == b;
				}
				return false;
			}

			public static bool operator !=(EditableNumber a, decimal b)
			{
				return !(a == b);
			}

			public static bool operator ==(EditableNumber a, double b)
			{
				if (a.Type == NumberType.Double)
				{
					return a.vDouble == b;
				}
				return false;
			}

			public static bool operator !=(EditableNumber a, double b)
			{
				return !(a == b);
			}

			public static bool operator ==(EditableNumber a, long b)
			{
				if (a.Type == NumberType.Long)
				{
					return a.vLong == b;
				}
				return false;
			}

			public static bool operator !=(EditableNumber a, long b)
			{
				return !(a == b);
			}
		}

		private enum MinMaxSliderLocalControl
		{
			Min = 1,
			Max,
			Bar
		}

		private class QuaternionContextBuffer
		{
			public bool IsUsed;

			public QuaternionDrawMode DrawMode = (QuaternionDrawMode)(-1);

			private Vector4 buffer;

			public Vector3 Eulers
			{
				get
				{
					return UnityShims.Vector4.op_ImplicitVec3(buffer);
				}
				set
				{
					buffer = UnityShims.Vector4.op_Implicit(value);
				}
			}

			public Vector3 Axis
			{
				get
				{
					return UnityShims.Vector4.op_ImplicitVec3(buffer);
				}
				set
				{
					buffer.Set(value.x, value.y, value.z, buffer.w);
				}
			}

			public float Angle
			{
				get
				{
					return buffer.w;
				}
				set
				{
					buffer.w = value;
				}
			}

			public Vector4 Raw
			{
				get
				{
					return buffer;
				}
				set
				{
					buffer = value;
				}
			}

			public void Set(Quaternion quaternion, QuaternionDrawMode mode)
			{
				DrawMode = mode;
				switch (mode)
				{
				case QuaternionDrawMode.Eulers:
					buffer = UnityShims.Vector4.op_Implicit(quaternion.eulerAngles);
					break;
				case QuaternionDrawMode.AngleAxis:
				{
					quaternion.ToAngleAxis(out var angle, out var axis);
					buffer = new Vector4(axis.x, axis.y, axis.z, angle);
					break;
				}
				case QuaternionDrawMode.Raw:
					buffer = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
					break;
				}
			}
		}

		private static class PopupSelector
		{
			public static int CurrentSelectingPopupControlID;

			public static Action SelectAction;
		}

		private static class PopupSelector<T>
		{
			public static int CurrentSelectingPopupControlID;

			public static Func<T> SelectFunc;
		}

		private static class MaskMenu
		{
			public const string MASK_MENU_CHANGED_EVENT_NAME = "SirenixMaskMenuChanged";

			public static long ChangedMaskValueSigned { get; set; }

			public static ulong ChangedMaskValueUnsigned { get; set; }

			public static int CurrentEnumControlID { get; set; }

			public static bool EnumChanged { get; set; }
		}

		/// <summary>
		/// Wrapper for Unity's ExpressionEvaluator. It was moved from UnityEditor to UnityEngine in version 2023 and
		/// that *should* have been automatically fixed by the AssemblyUpdater, but that broke for one reason or another.
		/// </summary>
		private static class InternalExpressionEvaluator
		{
			private delegate bool Evaluate<T>(string expression, out T value);

			private delegate bool EvaluateWithDelayedExpression<T>(string expression, out T value, out object delayed);

			private static readonly Evaluate<double> evaluateDouble;

			private static readonly EvaluateWithDelayedExpression<double> evaluateDoubleWithDelayedExpression;

			private static readonly Evaluate<long> evaluateLong;

			private static readonly EvaluateWithDelayedExpression<long> evaluateLongWithDelayedExpression;

			static InternalExpressionEvaluator()
			{
				Type expressionEvaluatorType = Type.GetType("UnityEngine.ExpressionEvaluator, UnityEngine") ?? Type.GetType("UnityEditor.ExpressionEvaluator, UnityEditor");
				if (expressionEvaluatorType == null)
				{
					UnityEngine.Debug.LogError("Odin Inspector initialization error: Failed to find the ExpressionEvaluator type!");
					return;
				}
				evaluateDouble = (Evaluate<double>)expressionEvaluatorType.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeof(double)).CreateDelegate(typeof(Evaluate<double>));
				MethodInfo evaluateDoubleWithDelayedExpressionMethod = GetGenericEvaluateWithDelayedExpressionMethod(expressionEvaluatorType, typeof(double));
				if (evaluateDoubleWithDelayedExpressionMethod != null)
				{
					evaluateDoubleWithDelayedExpression = delegate(string expression, out double value, out object delayed)
					{
						object[] array = new object[3] { expression, null, null };
						bool result = (bool)evaluateDoubleWithDelayedExpressionMethod.Invoke(null, array);
						value = ((array[1] != null) ? ((double)array[1]) : 0.0);
						delayed = array[2];
						return result;
					};
				}
				evaluateLong = (Evaluate<long>)expressionEvaluatorType.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeof(long)).CreateDelegate(typeof(Evaluate<long>));
				MethodInfo evaluateLongWithDelayedExpressionMethod = GetGenericEvaluateWithDelayedExpressionMethod(expressionEvaluatorType, typeof(long));
				if (evaluateLongWithDelayedExpressionMethod != null)
				{
					evaluateLongWithDelayedExpression = delegate(string expression, out long value, out object delayed)
					{
						object[] array = new object[3] { expression, null, null };
						bool result = (bool)evaluateLongWithDelayedExpressionMethod.Invoke(null, array);
						value = ((array[1] != null) ? ((long)array[1]) : 0);
						delayed = array[2];
						return result;
					};
				}
			}

			public static bool EvaluateDouble(string expression, out double value)
			{
				value = 0.0;
				if (string.IsNullOrWhiteSpace(expression))
				{
					return false;
				}
				switch (expression.Trim().ToLowerInvariant())
				{
				case "inf":
				case "infinity":
					value = double.PositiveInfinity;
					return true;
				case "-inf":
				case "-infinity":
					value = double.NegativeInfinity;
					return true;
				case "nan":
					value = double.NaN;
					return true;
				default:
					if (evaluateDoubleWithDelayedExpression != null)
					{
						if (evaluateDoubleWithDelayedExpression(expression, out value, out var delayed) && delayed == null)
						{
							return true;
						}
						if (delayed != null)
						{
							MethodInfo evaluateMethod = delayed.GetType().GetMethod("Evaluate", BindingFlags.Instance | BindingFlags.Public)?.MakeGenericMethod(typeof(double));
							if (evaluateMethod == null)
							{
								UnityEngine.Debug.LogError("Failed to find the Evaluate<T> method on delayed Expression.");
								return false;
							}
							object[] args = new object[3] { value, 0, 1 };
							bool result = (bool)evaluateMethod.Invoke(delayed, args);
							value = (double)args[0];
							return result;
						}
						return false;
					}
					if (evaluateDouble != null)
					{
						return evaluateDouble(expression, out value);
					}
					return false;
				}
			}

			public static bool EvaluateLong(string expression, out long value)
			{
				value = 0L;
				if (string.IsNullOrWhiteSpace(expression))
				{
					return false;
				}
				if (evaluateLongWithDelayedExpression != null)
				{
					if (evaluateLongWithDelayedExpression(expression, out value, out var delayed) && delayed == null)
					{
						return true;
					}
					if (delayed != null)
					{
						MethodInfo evaluateMethod = delayed.GetType().GetMethod("Evaluate", BindingFlags.Instance | BindingFlags.Public)?.MakeGenericMethod(typeof(long));
						if (evaluateMethod == null)
						{
							UnityEngine.Debug.LogError("Failed to find the Evaluate<T> method on delayed Expression.");
							return false;
						}
						object[] args = new object[3] { value, 0, 1 };
						bool result = (bool)evaluateMethod.Invoke(delayed, args);
						value = (long)args[0];
						return result;
					}
					return false;
				}
				if (evaluateLong != null)
				{
					return evaluateLong(expression, out value);
				}
				return false;
			}

			private static MethodInfo GetGenericEvaluateWithDelayedExpressionMethod(Type evaluatorType, Type typeArgument)
			{
				return (from m in evaluatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
					where m.Name == "Evaluate" && m.IsGenericMethodDefinition
					select m).Where(delegate(MethodInfo m)
				{
					ParameterInfo[] parameters = m.GetParameters();
					return parameters.Length == 3 && parameters[0].ParameterType == typeof(string) && parameters[1].IsOut && parameters[2].IsOut;
				}).FirstOrDefault()?.MakeGenericMethod(typeArgument);
			}
		}

		private const int DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT = 30;

		private static readonly int slideKnobWidth = 14;

		private static readonly Color delayedActiveColor = Color.yellow;

		private static Vector4 vectorNormalBuffer;

		private static float vectorLengthBuffer;

		internal static int localHotControl;

		private static int delayedIntBuffer;

		private static long delayedLongBuffer;

		private static float delayedFloatBuffer;

		private static double delayedDoubleBuffer;

		private static string delayedTextBuffer;

		private static GUIStyle progressBarTextOverlayStyle = null;

		private static GUIStyle minMaxSliderStyle = null;

		private static GUIStyle sliderBackground = null;

		private static GUIStyle minMaxFloatingLabelStyle = null;

		private static List<int> layerNumbers = new List<int>();

		private static bool? responsiveVectorComponentFields;

		private static bool currentEnumControlHasValue = false;

		private static int currentEnumControlID = 0;

		private static Enum selectedEnumValue;

		private static int convertingUnitsControlId = -1;

		private static string unitConvertingNameBuffer = null;

		private static bool smartNumberTextIsDelaying;

		private static readonly Regex inputRegex = new Regex("(?<value>-?\\d*\\.?\\d*)(?<symbol>\\D*)");

		private static readonly EmitContext fieldEmitContext = new EmitContext();

		private static readonly FieldExpressionContext defaultExpressionContext = FieldExpressionContext.None();

		internal static readonly StringHistoryList expressionHistory = new StringHistoryList("SirenixEditorFields.ExpressionHistoryState", 100);

		public static string UnitFieldFormatStringFloat = "0.###";

		public static string UnitFieldFormatStringDouble = "0.#####";

		public static string UnitFieldFormatStringDecimal = "0.#####";

		public static string UnitFieldFormatStringInteger = "#######0";

		/// <summary>
		/// The width of the X, Y and Z labels in structs.
		/// </summary>
		public static readonly int SingleLetterStructLabelWidth = 13;

		/// <summary>
		/// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
		/// </summary>
		public static bool ResponsiveVectorComponentFields
		{
			get
			{
				if (!responsiveVectorComponentFields.HasValue)
				{
					responsiveVectorComponentFields = EditorPrefs.GetBool("SirenixEditorFields.ResponsiveVectorComponentFields", defaultValue: true);
				}
				return responsiveVectorComponentFields.Value;
			}
			set
			{
				responsiveVectorComponentFields = value;
				EditorPrefs.SetBool("SirenixEditorFields.ResponsiveVectorComponentFields", value);
			}
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		public static UnityEngine.Object UnityObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
		{
			return InternalOdinEditorWrapper.UnityObjectField(rect, label, value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		public static UnityEngine.Object UnityObjectField(Rect rect, string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
		{
			return UnityObjectField(rect, new GUIContent(label), value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		public static UnityEngine.Object UnityObjectField(Rect rect, UnityEngine.Object value, Type objectType, bool allowSceneObjects)
		{
			return UnityObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static UnityEngine.Object UnityObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			return UnityObjectField(rect, label, value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static UnityEngine.Object UnityObjectField(string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			return UnityObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static UnityEngine.Object UnityObjectField(UnityEngine.Object value, Type objectType, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(hasLabel: false, options);
			return UnityObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		public static TElement PreviewObjectField<TElement>(Rect rect, TElement value, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
		{
			int id = DragAndDropUtilities.GetDragAndDropId(rect);
			DragAndDropUtilities.DrawDropZone(rect, value, null, id);
			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, allowSceneObjects, id);
				value = DragAndDropUtilities.ObjectPickerZone(rect, value, allowSceneObjects, id);
			}
			value = DragAndDropUtilities.DragZone(rect, value, allowMove, allowSwap, id);
			return value;
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, Texture texture, Type type, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
		{
			int id = DragAndDropUtilities.GetDragAndDropId(rect);
			DragAndDropUtilities.DrawDropZone(rect, texture, null, id);
			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				value = DragAndDropUtilities.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
			}
			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}
			return value;
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type type, ObjectFieldAlignment alignment, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
		{
			int id = DragAndDropUtilities.GetDragAndDropId(rect);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, id, label);
			}
			rect = alignment switch
			{
				ObjectFieldAlignment.Left => rect.AlignLeft(rect.height), 
				ObjectFieldAlignment.Center => rect.AlignCenter(rect.height), 
				_ => rect.AlignRight(rect.height), 
			};
			DragAndDropUtilities.DrawDropZone(rect, value, null, id);
			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				value = DragAndDropUtilities.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
			}
			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}
			return value;
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Texture preview, Type type, ObjectFieldAlignment alignment, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
		{
			int id = DragAndDropUtilities.GetDragAndDropId(rect);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, id, label);
			}
			rect = alignment switch
			{
				ObjectFieldAlignment.Left => rect.AlignLeft(rect.height), 
				ObjectFieldAlignment.Center => rect.AlignCenter(rect.height), 
				_ => rect.AlignRight(rect.height), 
			};
			DragAndDropUtilities.DrawDropZone(rect, preview, null, id);
			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				value = DragAndDropUtilities.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
			}
			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}
			return value;
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, Type type, bool dragOnly = false, bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true)
		{
			return UnityPreviewObjectField(rect, null, value, type, ObjectFieldAlignment.Right, dragOnly, allowMove, allowSwap, allowSceneObjects);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			return UnityPreviewObjectField(rect, label, value, objectType, alignment, dragOnly: false, allowMove: true, allowSwap: true, allowSceneObjects);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="preview">The Texture to be used as the preview.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Texture preview, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			return UnityPreviewObjectField(rect, label, value, preview, objectType, alignment, dragOnly: false, allowMove: true, allowSwap: true, allowSceneObjects);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			return UnityPreviewObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(Rect rect, UnityEngine.Object value, Type objectType, bool allowSceneObjects, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			return UnityPreviewObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="height">The height or size of the square object field.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = 30f, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(rect, label, value, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="preview">The texture to be used as the preview.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="height">The height or size of the square object field.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(GUIContent label, UnityEngine.Object value, Texture preview, Type objectType, bool allowSceneObjects, float height = 30f, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(rect, label, value, preview, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="height">The height or size of the square object field.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(string label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = 30f, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(rect, GUIHelper.TempContent(label), value, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
		/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
		/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
		/// </summary>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="height">The height or size of the square object field.</param>
		/// <param name="alignment">How the square object field should be aligned.</param>
		public static UnityEngine.Object UnityPreviewObjectField(UnityEngine.Object value, Type objectType, bool allowSceneObjects, float height = 30f, ObjectFieldAlignment alignment = ObjectFieldAlignment.Right)
		{
			Rect rect = EditorGUILayout.GetControlRect(false, height);
			return UnityPreviewObjectField(rect, (GUIContent)null, value, objectType, allowSceneObjects, alignment);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static object PolymorphicObjectField(GUIContent label, object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var id, out var hasKeyboardFocus, out var rect, options);
			return PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, id);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The object type. This supports inheritance.</param>
		/// <param name="title">The title to be shown in the object picker.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static object PolymorphicObjectField(GUIContent label, object value, Type type, string title, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var id, out var hasKeyboardFocus, out var rect, options);
			return PolymorphicObjectField(rect, value, type, title, allowSceneObjects, hasKeyboardFocus, id);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		public static object PolymorphicObjectField(Rect rect, GUIContent label, object value, Type type, bool allowSceneObjects)
		{
			Rect totalRect = rect;
			Rect valueRect = rect;
			int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
			if (label == null)
			{
				valueRect = EditorGUI.IndentedRect(valueRect);
			}
			else
			{
				totalRect.xMin += (float)EditorGUI.indentLevel * 15f;
				valueRect = EditorGUI.PrefixLabel(valueRect, controlId, label);
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && totalRect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = controlId;
			}
			bool hasKeyboardFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;
			return PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, controlId);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		public static object PolymorphicObjectField(Rect rect, object value, Type type, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			return InternalOdinEditorWrapper.PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, id);
		}

		public static object PolymorphicObjectField(Rect rect, object value, Type type, string title, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			return InternalOdinEditorWrapper.PolymorphicObjectField(rect, value, type, allowSceneObjects, hasKeyboardFocus, id, disallowNullValues: false, readOnly: false, showBaseType: true, title);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The value.</param>
		/// <param name="type">The object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static object PolymorphicObjectField(string label, object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			return PolymorphicObjectField(GUIHelper.TempContent(label), value, type, allowSceneObjects, options);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="type">The object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static object PolymorphicObjectField(object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			return PolymorphicObjectField((GUIContent)null, value, type, allowSceneObjects, options);
		}

		/// <summary>
		/// Draws a polymorphic ObjectField.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="type">The object type. This supports inheritance.</param>
		/// <param name="title">The title to be shown in the object picker.</param>
		/// <param name="allowSceneObjects">Wheather or not to allow scene objects.</param>
		/// <param name="options">Layout options.</param>
		public static object PolymorphicObjectField(object value, Type type, string title, bool allowSceneObjects, params GUILayoutOption[] options)
		{
			return PolymorphicObjectField(null, value, type, title, allowSceneObjects, options);
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="layerMask">The layer mask to draw.</param>
		public static LayerMask LayerMaskField(Rect rect, GUIContent label, LayerMask layerMask)
		{
			string[] layers = InternalEditorUtility.layers;
			layerNumbers.Clear();
			for (int i = 0; i < layers.Length; i++)
			{
				layerNumbers.Add(LayerMask.NameToLayer(layers[i]));
			}
			int maskWithoutEmpty = 0;
			for (int j = 0; j < layerNumbers.Count; j++)
			{
				if (((1 << layerNumbers[j]) & layerMask.value) != 0)
				{
					maskWithoutEmpty |= 1 << j;
				}
			}
			maskWithoutEmpty = ((label == null) ? EditorGUI.MaskField(rect, maskWithoutEmpty, layers) : EditorGUI.MaskField(rect, label, maskWithoutEmpty, layers));
			int mask = 0;
			for (int k = 0; k < layerNumbers.Count; k++)
			{
				if ((maskWithoutEmpty & (1 << k)) > 0)
				{
					mask |= 1 << layerNumbers[k];
				}
			}
			layerMask.value = mask;
			return layerMask;
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="layerMask">The layer mask to draw.</param>
		public static LayerMask LayerMaskField(Rect rect, string label, LayerMask layerMask)
		{
			return LayerMaskField(rect, GUIHelper.TempContent(label), layerMask);
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="layerMask">The layer mask to draw.</param>
		public static LayerMask LayerMaskField(Rect rect, LayerMask layerMask)
		{
			return LayerMaskField(rect, (GUIContent)null, layerMask);
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="layerMask">The layer mask to draw.</param>
		/// <param name="options">Layout options.</param>
		public static LayerMask LayerMaskField(GUIContent label, LayerMask layerMask, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			return LayerMaskField(rect, label, layerMask);
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="layerMask">The layer mask to draw.</param>
		/// <param name="options">Layout options.</param>
		public static LayerMask LayerMaskField(string label, LayerMask layerMask, params GUILayoutOption[] options)
		{
			return LayerMaskField(GUIHelper.TempContent(label), layerMask, options);
		}

		/// <summary>
		/// Draws a field for a layer mask.
		/// </summary>
		/// <param name="layerMask">The layer mask to draw.</param>
		/// <param name="options">Layout options.</param>
		public static LayerMask LayerMaskField(LayerMask layerMask, params GUILayoutOption[] options)
		{
			return LayerMaskField((GUIContent)null, layerMask, options);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(Rect rect, GUIContent label, Guid value)
		{
			return GuidField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(Rect rect, Guid value)
		{
			return GuidField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(GUIContent label, Guid value)
		{
			return GuidField(label, value, (GUIStyle)null, (GUILayoutOption[])null);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(GUIContent label, Guid value, params GUILayoutOption[] options)
		{
			return GuidField(label, value, null, options);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(GUIContent label, Guid value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.textField, options);
			return GuidField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a Guid field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Guid GuidField(Rect rect, GUIContent label, Guid value, GUIStyle style)
		{
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label, style ?? EditorStyles.label);
			}
			string text = value.ToString("D");
			EditorGUI.BeginChangeCheck();
			string newText = EditorGUI.DelayedTextField(rect.SubXMax(75f), text, style ?? EditorStyles.textField);
			if (EditorGUI.EndChangeCheck() || newText != text)
			{
				text = newText;
				try
				{
					value = new Guid(text);
					GUI.changed = true;
				}
				catch
				{
				}
			}
			if (GUI.Button(rect.SetXMin(rect.xMax - 70f), GUIHelper.TempContent("New GUID")))
			{
				value = Guid.NewGuid();
				GUI.changed = true;
			}
			return value;
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(Rect rect, GUIContent label, int value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type == EventType.Layout)
			{
				value = ((label != null) ? EditorGUI.IntField(rect, label, value, style ?? EditorStyles.numberField) : EditorGUI.IntField(rect, value, style ?? EditorStyles.numberField));
				return value;
			}
			Rect slideRect = rect.AlignRight(slideKnobWidth);
			value = SirenixEditorGUI.SlideRectInt(slideRect, control, value);
			value = ((label != null) ? EditorGUI.IntField(rect, label, value, style ?? EditorStyles.numberField) : EditorGUI.IntField(rect, value, style ?? EditorStyles.numberField));
			DrawSlideKnob(rect, control);
			return value;
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(Rect rect, GUIContent label, int value)
		{
			return IntField(rect, label, value, null);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(Rect rect, string label, int value)
		{
			return IntField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(Rect rect, int value)
		{
			return IntField(rect, null, value, null);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return IntField(rect, label, value, style);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(GUIContent label, int value, params GUILayoutOption[] options)
		{
			return IntField(label, value, null, options);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(string label, int value, params GUILayoutOption[] options)
		{
			return IntField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws an int field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntField(int value, params GUILayoutOption[] options)
		{
			return IntField(null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(Rect rect, GUIContent label, int value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (OnLocalControlRelease(rect, control))
			{
				GUI.changed = true;
				value = delayedIntBuffer;
			}
			int buffer = value;
			if (localHotControl == control)
			{
				GUIHelper.PushColor(delayedActiveColor);
				buffer = delayedIntBuffer;
			}
			EditorGUI.BeginChangeCheck();
			buffer = IntField(rect, label, buffer, style);
			if (localHotControl == control)
			{
				GUIHelper.PopColor();
			}
			if (EditorGUI.EndChangeCheck())
			{
				GUI.changed = false;
				localHotControl = control;
				delayedIntBuffer = buffer;
			}
			return value;
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(Rect rect, GUIContent label, int value)
		{
			return DelayedIntField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(Rect rect, string label, int value)
		{
			return DelayedIntField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(Rect rect, int value)
		{
			return DelayedIntField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DelayedIntField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(GUIContent label, int value, params GUILayoutOption[] options)
		{
			return DelayedIntField(label, value, null, options);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(string label, int value, params GUILayoutOption[] options)
		{
			return DelayedIntField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed int field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int DelayedIntField(int value, params GUILayoutOption[] options)
		{
			return DelayedIntField(null, value, null, options);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(Rect rect, GUIContent label, int value, int min, int max, GUIStyle style)
		{
			if (label == null)
			{
				return (int)EditorGUI.Slider(rect, value, (min < max) ? min : max, (max > min) ? max : min);
			}
			return (int)EditorGUI.Slider(rect, label, value, (min < max) ? min : max, (max > min) ? max : min);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(Rect rect, GUIContent label, int value, int min, int max)
		{
			return RangeIntField(rect, label, value, min, max, null);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(Rect rect, string label, int value, int min, int max)
		{
			return RangeIntField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, min, max, null);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(Rect rect, int value, int min, int max)
		{
			return RangeIntField(rect, null, value, min, max, null);
		}

		/// <summary>
		/// Drwas a range field for ints.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(GUIContent label, int value, int min, int max, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return RangeIntField(rect, label, value, min, max, style);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(GUIContent label, int value, int min, int max, params GUILayoutOption[] options)
		{
			return RangeIntField(label, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(string label, int value, int min, int max, params GUILayoutOption[] options)
		{
			return RangeIntField((label != null) ? GUIHelper.TempContent(label) : null, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a range field for ints.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int RangeIntField(int value, int min, int max, params GUILayoutOption[] options)
		{
			return RangeIntField(null, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="valueLabel">Optional text for label to be drawn ontop of the progress bar. This value is only used if the DrawValueLabel option is enabled in the ProgressBarConfig.</param>
		public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config, string valueLabel)
		{
			rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out var controlId, out var focus);
			if (Event.current.type == EventType.Layout)
			{
				return value;
			}
			rect = rect.AlignCenterY(config.Height);
			if (GUI.enabled)
			{
				bool changed = false;
				if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) || (GUIUtility.hotControl == controlId && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)))
				{
					GUIUtility.hotControl = controlId;
					value = minValue + Math.Abs(maxValue - minValue) * (double)Mathf.Clamp01((Event.current.mousePosition.x - rect.xMin) / rect.width) * ((minValue <= maxValue) ? 1.0 : (-1.0));
					changed = true;
				}
				else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
				{
					value += (double)((minValue < maxValue) ? 1 : (-1));
					changed = true;
				}
				else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
				{
					value -= (double)((minValue < maxValue) ? 1 : (-1));
					changed = true;
				}
				else if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
				}
				if (changed)
				{
					GUI.changed = true;
					double low = Math.Min(minValue, maxValue);
					double high = Math.Max(minValue, maxValue);
					value = ((value <= low) ? low : ((value >= high) ? high : value));
					GUIHelper.RequestRepaint();
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.Repaint)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
				SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor, usePlaymodeTint: false);
				SirenixEditorGUI.DrawSolidRect(rect.Padding(1f), config.BackgroundColor, usePlaymodeTint: false);
				float progress = ((maxValue != minValue) ? MathUtilities.LinearStep((float)minValue, (float)maxValue, (float)value) : 1f);
				Rect foregroundRect = rect.Padding(1f).AlignLeft((rect.width - 2f) * progress);
				SirenixEditorGUI.DrawSolidRect(foregroundRect, config.ForegroundColor, usePlaymodeTint: false);
				if (config.DrawValueLabel || valueLabel != null)
				{
					if (valueLabel == null)
					{
						valueLabel = value.ToString((Math.Abs(maxValue - minValue) <= 1.0) ? "0.###" : "0.#");
						if (minValue == 0.0)
						{
							valueLabel = valueLabel + " / " + maxValue.ToString("0.#");
						}
					}
					GUIContent overlayContent = GUIHelper.TempContent(valueLabel);
					ProgressBarOverlayLabel(rect, overlayContent, progress, config);
				}
				GUIHelper.PopGUIEnabled();
			}
			return value;
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config)
		{
			return ProgressBarField(rect, label, value, minValue, maxValue, config, null);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static double ProgressBarField(Rect rect, string label, double value, double minValue, double maxValue, ProgressBarConfig config)
		{
			return ProgressBarField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static double ProgressBarField(Rect rect, double value, double minValue, double maxValue, ProgressBarConfig config)
		{
			return ProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static double ProgressBarField(Rect rect, GUIContent label, double value, double minValue, double maxValue)
		{
			return ProgressBarField(rect, label, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static double ProgressBarField(Rect rect, string label, double value, double minValue, double maxValue)
		{
			return ProgressBarField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static double ProgressBarField(Rect rect, double value, double minValue, double maxValue)
		{
			return ProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(GUIContent label, double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, ((float)config.Height < EditorGUIUtility.singleLineHeight) ? EditorGUIUtility.singleLineHeight : ((float)config.Height), options);
			return ProgressBarField(rect, label, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(string label, double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			return ProgressBarField((label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config, options);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(double value, double minValue, double maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			return ProgressBarField((GUIContent)null, value, minValue, maxValue, config, options);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(GUIContent label, double value, double minValue, double maxValue, params GUILayoutOption[] options)
		{
			return ProgressBarField(label, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(string label, double value, double minValue, double maxValue, params GUILayoutOption[] options)
		{
			return ProgressBarField((label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws a colored progress bar field.
		/// </summary>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static double ProgressBarField(double value, double minValue, double maxValue, params GUILayoutOption[] options)
		{
			return ProgressBarField((GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="valueLabel">Optional text for label to be drawn ontop of the progress bar. This value is only used if the DrawValueLabel option is enabled in the ProgressBarConfig.</param>
		public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config, string valueLabel)
		{
			rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out var controlId, out var focus);
			rect = rect.AlignCenterY(config.Height);
			if (GUI.enabled)
			{
				bool changed = false;
				if ((Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition)) || (GUIUtility.hotControl == controlId && (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)))
				{
					GUIUtility.hotControl = controlId;
					float tileSize = rect.width / Mathf.Abs(maxValue - minValue);
					value = ((!(Event.current.mousePosition.x < rect.xMin + tileSize * 0.1f)) ? (minValue + (long)(Mathf.Min(Event.current.mousePosition.x - rect.xMin, rect.width) / tileSize + 1f) * ((minValue <= maxValue) ? 1 : (-1))) : minValue);
					changed = true;
				}
				else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow)
				{
					value += ((minValue < maxValue) ? 1 : (-1));
					changed = true;
				}
				else if (focus && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow)
				{
					value -= ((minValue < maxValue) ? 1 : (-1));
					changed = true;
				}
				else if (GUIUtility.hotControl == controlId && Event.current.rawType == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
				}
				if (changed)
				{
					long low = Math.Min(minValue, maxValue);
					long high = Math.Max(minValue, maxValue);
					value = ((value <= low) ? low : ((value >= high) ? high : value));
					GUI.changed = true;
					GUIHelper.RequestRepaint();
					Event.current.Use();
				}
			}
			if (Event.current.type == EventType.Repaint)
			{
				GUIHelper.PushGUIEnabled(enabled: true);
				long range = Math.Abs(maxValue - minValue);
				if (range > 0)
				{
					long low2 = Math.Min(minValue, maxValue);
					float high2 = Mathf.Max(minValue, maxValue);
					float displayValue = ((value <= low2) ? ((float)low2) : (((float)value >= high2) ? high2 : ((float)value)));
					for (long i = 0L; i < range; i++)
					{
						Rect tile = rect.Expand(0.5f, 0f).Split((int)i, (int)Mathf.Abs(maxValue - minValue));
						SirenixEditorGUI.DrawSolidRect(tile.Padding(0.5f, 0f), SirenixGUIStyles.BorderColor, usePlaymodeTint: false);
						if ((minValue <= maxValue && (float)(minValue + i) < displayValue) || (float)(minValue - i) > displayValue)
						{
							SirenixEditorGUI.DrawSolidRect(tile.Padding(1.5f, 1f), config.ForegroundColor, usePlaymodeTint: false);
						}
						else
						{
							SirenixEditorGUI.DrawSolidRect(tile.Padding(1.5f, 1f), config.BackgroundColor, usePlaymodeTint: false);
						}
					}
				}
				else
				{
					SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.BorderColor, usePlaymodeTint: false);
					SirenixEditorGUI.DrawSolidRect(rect.Padding(1f), config.ForegroundColor, usePlaymodeTint: false);
				}
				if (config.DrawValueLabel || valueLabel != null)
				{
					if (valueLabel == null)
					{
						valueLabel = value.ToString();
						if (minValue == 0L)
						{
							valueLabel = valueLabel + " / " + maxValue;
						}
					}
					ProgressBarOverlayLabel(rect, GUIHelper.TempContent(valueLabel), (range > 0) ? MathUtilities.LinearStep(minValue, maxValue, value) : 1f, config);
				}
				GUIHelper.PopGUIEnabled();
			}
			return value;
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config)
		{
			return SegmentedProgressBarField(rect, label, value, minValue, maxValue, config, null);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static long SegmentedProgressBarField(Rect rect, string label, long value, long minValue, long maxValue, ProgressBarConfig config)
		{
			return SegmentedProgressBarField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		public static long SegmentedProgressBarField(Rect rect, long value, long minValue, long maxValue, ProgressBarConfig config)
		{
			return SegmentedProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static long SegmentedProgressBarField(Rect rect, GUIContent label, long value, long minValue, long maxValue)
		{
			return SegmentedProgressBarField(rect, label, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static long SegmentedProgressBarField(Rect rect, string label, long value, long minValue, long maxValue)
		{
			return SegmentedProgressBarField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		public static long SegmentedProgressBarField(Rect rect, long value, long minValue, long maxValue)
		{
			return SegmentedProgressBarField(rect, (GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(GUIContent label, long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, ((float)config.Height < EditorGUIUtility.singleLineHeight) ? EditorGUIUtility.singleLineHeight : ((float)config.Height), options);
			return SegmentedProgressBarField(rect, label, value, minValue, maxValue, config);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(string label, long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			return SegmentedProgressBarField((label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, config, options);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="config">The configuration for the progress bar field.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(long value, long minValue, long maxValue, ProgressBarConfig config, params GUILayoutOption[] options)
		{
			return SegmentedProgressBarField((GUIContent)null, value, minValue, maxValue, config, options);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(GUIContent label, long value, long minValue, long maxValue, params GUILayoutOption[] options)
		{
			return SegmentedProgressBarField(label, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(string label, long value, long minValue, long maxValue, params GUILayoutOption[] options)
		{
			return SegmentedProgressBarField((label != null) ? GUIHelper.TempContent(label) : null, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws a colored segmented progress bar field.
		/// </summary>
		/// <param name="value">The current value of the progress bar.</param>
		/// <param name="minValue">The left hand side value of the progress bar.</param>
		/// <param name="maxValue">The right hand side value of the progress bar.</param>
		/// <param name="options">Layout options.</param>
		public static long SegmentedProgressBarField(long value, long minValue, long maxValue, params GUILayoutOption[] options)
		{
			return SegmentedProgressBarField((GUIContent)null, value, minValue, maxValue, ProgressBarConfig.Default, options);
		}

		/// <summary>
		/// Draws an overlay on top of a progress bar field.
		/// </summary>
		/// <param name="rect">The rect used to draw the progress bar field with. (Minus the Rect for the prefix label, if any.)</param>
		/// <param name="label">The label to draw ontop of the progress bar field.</param>
		/// <param name="progress">The relative value of the progress bar, from 0 to 1.</param>
		/// <param name="config">The configuration used to draw the progress bar field.</param>
		public static void ProgressBarOverlayLabel(Rect rect, GUIContent label, float progress, ProgressBarConfig config)
		{
			if (progressBarTextOverlayStyle == null)
			{
				progressBarTextOverlayStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedGreyMiniLabel)
				{
					margin = new RectOffset(0, 0, 0, 0),
					padding = new RectOffset(0, 0, 0, 0),
					contentOffset = Vector2.zero
				};
			}
			if (Event.current.type == EventType.Repaint)
			{
				Rect foregroundRect = rect.Padding(1f).AlignLeft((rect.width - 2f) * Mathf.Clamp01(progress));
				Vector2 size = progressBarTextOverlayStyle.CalcSize(label);
				Rect overlayRect = rect.HorizontalPadding(4f, 0f).AlignCenterY(size.y);
				overlayRect = config.ValueLabelAlignment switch
				{
					TextAlignment.Left => overlayRect.AlignLeft(size.x), 
					TextAlignment.Right => overlayRect.AlignRight(size.x + 2f), 
					_ => overlayRect.AlignCenterX(size.x), 
				};
				if (foregroundRect.xMax < overlayRect.xMax)
				{
					float offset = overlayRect.xMin - Mathf.Max(foregroundRect.xMax, overlayRect.xMin);
					progressBarTextOverlayStyle.normal.textColor = config.ForegroundColor;
					progressBarTextOverlayStyle.contentOffset = new Vector2(offset, 0f);
					GUI.Label(overlayRect.AddX(Mathf.Abs(offset)), label, progressBarTextOverlayStyle);
				}
				if (foregroundRect.Overlaps(overlayRect))
				{
					progressBarTextOverlayStyle.normal.textColor = config.BackgroundColor;
					progressBarTextOverlayStyle.contentOffset = Vector2.zero;
					overlayRect = overlayRect.SetXMax(Mathf.Min(foregroundRect.xMax, overlayRect.xMax));
					GUI.Label(overlayRect, label, progressBarTextOverlayStyle);
				}
			}
		}

		/// <summary>
		/// Draws an overlay on top of a progress bar field.
		/// </summary>
		/// <param name="rect">The rect used to draw the progress bar field with. (Minus the Rect for the prefix label, if any.)</param>
		/// <param name="label">The label to draw ontop of the progress bar field.</param>
		/// <param name="progress">The relative value of the progress bar, from 0 to 1.</param>
		/// <param name="config">The configuration used to draw the progress bar field.</param>
		private static void ProgressBarOverlayLabel(Rect rect, string label, float progress, ProgressBarConfig config)
		{
			ProgressBarOverlayLabel(rect, GUIHelper.TempContent(label), progress, config);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(Rect rect, GUIContent label, long value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			Rect slideRect = rect.AlignRight(slideKnobWidth);
			value = SirenixEditorGUI.SlideRectLong(slideRect, control, value);
			value = ((label != null) ? EditorGUI.LongField(rect, label, value, style ?? EditorStyles.numberField) : EditorGUI.LongField(rect, value, style ?? EditorStyles.numberField));
			DrawSlideKnob(rect, control);
			return value;
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(Rect rect, GUIContent label, long value)
		{
			return LongField(rect, label, value, null);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(Rect rect, string label, long value)
		{
			return LongField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(Rect rect, long value)
		{
			return LongField(rect, null, value, null);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return LongField(rect, label, value, style);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(GUIContent label, long value, params GUILayoutOption[] options)
		{
			return LongField(label, value, null, options);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(string label, long value, params GUILayoutOption[] options)
		{
			return LongField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws an long field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongField(long value, params GUILayoutOption[] options)
		{
			return LongField(null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(Rect rect, GUIContent label, long value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (OnLocalControlRelease(rect, control))
			{
				value = delayedLongBuffer;
			}
			long buffer = value;
			if (localHotControl == control)
			{
				GUIHelper.PushColor(delayedActiveColor);
				buffer = delayedLongBuffer;
			}
			EditorGUI.BeginChangeCheck();
			buffer = LongField(rect, label, buffer, style);
			if (localHotControl == control)
			{
				GUIHelper.PopColor();
			}
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = control;
				delayedLongBuffer = buffer;
			}
			return value;
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(Rect rect, GUIContent label, long value)
		{
			return DelayedLongField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(Rect rect, string label, long value)
		{
			return DelayedLongField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(Rect rect, long value)
		{
			return DelayedLongField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DelayedLongField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(GUIContent label, long value, params GUILayoutOption[] options)
		{
			return DelayedLongField(label, value, null, options);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(string label, long value, params GUILayoutOption[] options)
		{
			return DelayedLongField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed long field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long DelayedLongField(long value, params GUILayoutOption[] options)
		{
			return DelayedLongField(null, value, null, options);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(Rect rect, GUIContent label, float value, GUIStyle style)
		{
			Rect slideRect = rect.AlignRight(slideKnobWidth);
			int control = GUIUtility.GetControlID(FocusType.Passive);
			value = SirenixEditorGUI.SlideRect(slideRect, control, value);
			value = ((label != null) ? EditorGUI.FloatField(rect, label, value, style ?? EditorStyles.numberField) : EditorGUI.FloatField(rect, value, style ?? EditorStyles.numberField));
			DrawSlideKnob(rect, control);
			return value;
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(Rect rect, GUIContent label, float value)
		{
			return FloatField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(Rect rect, string label, float value)
		{
			return FloatField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(Rect rect, float value)
		{
			return FloatField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return FloatField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(GUIContent label, float value, params GUILayoutOption[] options)
		{
			return FloatField(label, value, null, options);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(string label, float value, params GUILayoutOption[] options)
		{
			return FloatField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a float field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatField(float value, params GUILayoutOption[] options)
		{
			return FloatField(null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(Rect rect, GUIContent label, float value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (OnLocalControlRelease(rect, control))
			{
				value = delayedFloatBuffer;
				GUI.changed = true;
			}
			float buffer = value;
			if (localHotControl == control)
			{
				GUIHelper.PushColor(delayedActiveColor);
				buffer = delayedFloatBuffer;
			}
			EditorGUI.BeginChangeCheck();
			buffer = FloatField(rect, label, buffer, style);
			if (localHotControl == control)
			{
				GUIHelper.PopColor();
			}
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = control;
				delayedFloatBuffer = buffer;
				GUI.changed = false;
			}
			return value;
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(Rect rect, GUIContent label, float value)
		{
			return DelayedFloatField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(Rect rect, string label, float value)
		{
			return DelayedFloatField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(Rect rect, float value)
		{
			return DelayedFloatField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DelayedFloatField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(GUIContent label, float value, params GUILayoutOption[] options)
		{
			return DelayedFloatField(label, value, null, options);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(string label, float value, params GUILayoutOption[] options)
		{
			return DelayedFloatField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed float field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float DelayedFloatField(float value, params GUILayoutOption[] options)
		{
			return DelayedFloatField(null, value, null, options);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(Rect rect, GUIContent label, float value, float min, float max, GUIStyle style)
		{
			if (label == null)
			{
				return EditorGUI.Slider(rect, value, (min < max) ? min : max, (max > min) ? max : min);
			}
			return EditorGUI.Slider(rect, label, value, (min < max) ? min : max, (max > min) ? max : min);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(Rect rect, GUIContent label, float value, float min, float max)
		{
			return RangeFloatField(rect, label, value, min, max, null);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(Rect rect, string label, float value, float min, float max)
		{
			return RangeFloatField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, min, max, null);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(Rect rect, float value, float min, float max)
		{
			return RangeFloatField(rect, null, value, min, max, null);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(GUIContent label, float value, float min, float max, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return RangeFloatField(rect, label, value, min, max, style);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(GUIContent label, float value, float min, float max, params GUILayoutOption[] options)
		{
			return RangeFloatField(label, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(string label, float value, float min, float max, params GUILayoutOption[] options)
		{
			return RangeFloatField((label != null) ? GUIHelper.TempContent(label) : null, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a range field for floats.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float RangeFloatField(float value, float min, float max, params GUILayoutOption[] options)
		{
			return RangeFloatField(null, value, min, max, null, options);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(Rect rect, GUIContent label, double value, GUIStyle style)
		{
			Rect slideRect = rect.AlignRight(slideKnobWidth);
			int control = GUIUtility.GetControlID(FocusType.Passive);
			value = SirenixEditorGUI.SlideRectDouble(slideRect, control, value);
			value = ((label != null) ? EditorGUI.DoubleField(rect, label, value, style ?? EditorStyles.numberField) : EditorGUI.DoubleField(rect, value, style ?? EditorStyles.numberField));
			DrawSlideKnob(rect, control);
			return value;
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(Rect rect, GUIContent label, double value)
		{
			return DoubleField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(Rect rect, string label, double value)
		{
			return DoubleField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(Rect rect, double value)
		{
			return DoubleField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DoubleField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(GUIContent label, double value, params GUILayoutOption[] options)
		{
			return DoubleField(label, value, null, options);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(string label, double value, params GUILayoutOption[] options)
		{
			return DoubleField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a double field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleField(double value, params GUILayoutOption[] options)
		{
			return DoubleField(null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(Rect rect, GUIContent label, double value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (OnLocalControlRelease(rect, control))
			{
				value = delayedDoubleBuffer;
				GUI.changed = true;
			}
			double buffer = value;
			if (localHotControl == control)
			{
				GUIHelper.PushColor(delayedActiveColor);
				buffer = delayedDoubleBuffer;
			}
			EditorGUI.BeginChangeCheck();
			buffer = DoubleField(rect, label, buffer, style);
			if (localHotControl == control)
			{
				GUIHelper.PopColor();
			}
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = control;
				delayedDoubleBuffer = buffer;
				GUI.changed = false;
			}
			return value;
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(Rect rect, GUIContent label, double value)
		{
			return DelayedDoubleField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(Rect rect, string label, double value)
		{
			return DelayedDoubleField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(Rect rect, double value)
		{
			return DelayedDoubleField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DelayedDoubleField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(GUIContent label, double value, params GUILayoutOption[] options)
		{
			return DelayedDoubleField(label, value, null, options);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(string label, double value, params GUILayoutOption[] options)
		{
			return DelayedDoubleField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed double field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DelayedDoubleField(double value, params GUILayoutOption[] options)
		{
			return DelayedDoubleField(null, value, null, options);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(Rect rect, GUIContent label, decimal value, GUIStyle style)
		{
			double d = (double)value;
			EditorGUI.BeginChangeCheck();
			if (label != null)
			{
				Rect labelRect = rect.SetWidth(GUIHelper.BetterLabelWidth);
				rect = EditorGUI.PrefixLabel(rect, label);
				d = SirenixEditorGUI.SlideRectDouble(labelRect, GUIUtility.GetControlID(FocusType.Passive), d);
			}
			Rect slideRect = rect.AlignRight(slideKnobWidth);
			int control = GUIUtility.GetControlID(FocusType.Passive);
			d = SirenixEditorGUI.SlideRectDouble(slideRect, control, d);
			if (EditorGUI.EndChangeCheck())
			{
				value = (decimal)d;
			}
			string s = value.ToString(CultureInfo.InvariantCulture);
			s = DelayedTextField(rect, s);
			if (GUI.changed && decimal.TryParse(s, out var dec))
			{
				value = dec;
			}
			DrawSlideKnob(rect, control);
			return value;
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(Rect rect, GUIContent label, decimal value)
		{
			return DecimalField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(Rect rect, string label, decimal value)
		{
			return DecimalField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(Rect rect, decimal value)
		{
			return DecimalField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(GUIContent label, decimal value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return DecimalField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(GUIContent label, decimal value, params GUILayoutOption[] options)
		{
			return DecimalField(label, value, null, options);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(string label, decimal value, params GUILayoutOption[] options)
		{
			return DecimalField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a decimal field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalField(decimal value, params GUILayoutOption[] options)
		{
			return DecimalField(null, value, null, options);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(Rect rect, GUIContent label, string value, GUIStyle style)
		{
			if (label == null)
			{
				return EditorGUI.TextField(rect, value, style ?? EditorStyles.textField);
			}
			return EditorGUI.TextField(rect, label, value, style ?? EditorStyles.textField);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(Rect rect, GUIContent label, string value)
		{
			return TextField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(Rect rect, string label, string value)
		{
			return TextField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(Rect rect, string value)
		{
			return TextField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(GUIContent label, string value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return TextField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(GUIContent label, string value, params GUILayoutOption[] options)
		{
			return TextField(label, value, null, options);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(string label, string value, params GUILayoutOption[] options)
		{
			return TextField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a text field for strings.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string TextField(string value, params GUILayoutOption[] options)
		{
			return TextField(null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(Rect rect, GUIContent label, string value, GUIStyle style)
		{
			int control = GUIUtility.GetControlID(FocusType.Passive);
			if (OnLocalControlRelease(rect, control))
			{
				GUI.changed = true;
				value = delayedTextBuffer;
			}
			string buffer = value;
			if (localHotControl == control)
			{
				GUIHelper.PushColor(delayedActiveColor);
				buffer = delayedTextBuffer;
			}
			EditorGUI.BeginChangeCheck();
			buffer = TextField(rect, label, buffer, style);
			if (localHotControl == control)
			{
				GUIHelper.PopColor();
			}
			if (EditorGUI.EndChangeCheck())
			{
				GUI.changed = false;
				localHotControl = control;
				delayedTextBuffer = buffer;
			}
			return value;
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(Rect rect, GUIContent label, string value)
		{
			return DelayedTextField(rect, label, value, null);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(Rect rect, string label, string value)
		{
			return DelayedTextField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, null);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(Rect rect, string value)
		{
			return DelayedTextField(rect, null, value, null);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(GUIContent label, string value, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.textField, options);
			return DelayedTextField(rect, label, value, style);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(GUIContent label, string value, params GUILayoutOption[] options)
		{
			return DelayedTextField(label, value, null, options);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(string label, string value, params GUILayoutOption[] options)
		{
			return DelayedTextField((label != null) ? GUIHelper.TempContent(label) : null, value, null, options);
		}

		/// <summary>
		/// Draws a delayed text field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static string DelayedTextField(string value, params GUILayoutOption[] options)
		{
			return DelayedTextField(null, value, null, options);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="includeFileExtension">If <c>true</c> the file path will include the file's extension.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(Rect rect, GUIContent label, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, bool includeFileExtension = true)
		{
			bool needsProcessing = false;
			GUIHelper.PushGUIEnabled(enabled: true);
			if (label != null && !path.IsNullOrWhitespace() && rect.AlignLeft(GUIHelper.BetterLabelWidth).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.clickCount >= 2)
			{
				string fileToPing = "";
				if (!includeFileExtension)
				{
					string directoryPath = Path.GetDirectoryName(path);
					if (directoryPath != null)
					{
						string fileName = Path.GetFileName(path);
						string[] files = Directory.GetFiles(directoryPath, fileName + ".*");
						if (string.IsNullOrEmpty(extensions))
						{
							fileToPing = files.FirstOrDefault();
						}
						else
						{
							IEnumerable<string> splitExtensions = from text in extensions.Replace(" ", "").Split(new char[1] { ',' })
								select (!text.StartsWith(".")) ? ("." + text) : text;
							string[] array = files;
							foreach (string file in array)
							{
								string fileExtension = Path.GetExtension(file);
								if (splitExtensions.Contains(fileExtension))
								{
									fileToPing = file;
									break;
								}
							}
						}
					}
				}
				string highlightPath = GetRelativePath(includeFileExtension ? path : fileToPing, Directory.GetParent(Application.dataPath).FullName);
				if (!highlightPath.IsNullOrWhitespace())
				{
					UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(highlightPath.Replace('\\', '/'));
					if (obj != null)
					{
						EditorGUIUtility.PingObject(obj);
					}
				}
				Event.current.Use();
			}
			GUIHelper.PopGUIEnabled();
			UnityEngine.Object droppedObject = DragAndDropUtilities.DropZone<UnityEngine.Object>(rect, null, allowSceneObjects: false);
			if (droppedObject != null)
			{
				string pathBuffer = AssetDatabase.GetAssetPath(droppedObject);
				bool accept = true;
				if ((File.GetAttributes(pathBuffer) & FileAttributes.Directory) != FileAttributes.None)
				{
					accept = false;
				}
				else if (!extensions.IsNullOrWhitespace())
				{
					string e = Path.GetExtension(pathBuffer).Trim(new char[1] { '.' });
					if (e.IsNullOrWhitespace())
					{
						accept = false;
					}
					else if (!(from i in extensions.Split(',', ';')
						select i.Trim(' ', '.', '*')).DefaultIfEmpty(e).Any((string i) => i.Equals(e, StringComparison.CurrentCultureIgnoreCase)))
					{
						accept = false;
					}
				}
				if (accept)
				{
					path = pathBuffer;
					needsProcessing = true;
				}
			}
			EditorGUI.BeginChangeCheck();
			string pathBuffer2 = TextField(rect.AlignLeft(rect.width - 18f), label, path);
			if (EditorGUI.EndChangeCheck())
			{
				path = (useBackslashes ? pathBuffer2.Replace('/', '\\') : pathBuffer2.Replace('\\', '/'));
			}
			bool isEnabled = GUI.enabled;
			if (Event.current.type != EventType.Repaint)
			{
				GUI.enabled = true;
			}
			if (SirenixEditorGUI.IconButton(rect.AlignRight(18f).SetHeight(18f).SubY(1f)
				.AddX(1f), EditorIcons.Folder))
			{
				string directory = GetOpenExplorerPath(path, parentPath);
				if (isEnabled)
				{
					string pathBuffer3 = EditorUtility.OpenFilePanel("Select File", directory, GetFilePanelExtensions(extensions));
					if (!pathBuffer3.IsNullOrWhitespace())
					{
						path = pathBuffer3;
						needsProcessing = true;
					}
				}
				else
				{
					Process.Start(directory);
				}
			}
			if (Event.current.type != EventType.Repaint)
			{
				GUI.enabled = isEnabled;
			}
			if (!path.IsNullOrWhitespace() && needsProcessing)
			{
				path = Path.GetFullPath(path);
				if (!absolutePath)
				{
					path = GetRelativePath(path, parentPath.IsNullOrWhitespace() ? Directory.GetParent(Application.dataPath).FullName : parentPath);
				}
				if (!includeFileExtension)
				{
					path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				}
				path = (useBackslashes ? path.Replace('/', '\\') : path.Replace('\\', '/'));
				GUI.changed = true;
			}
			return path;
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="includeFileExtension">If <c>true</c> the file path will include the file's extension.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(Rect rect, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, bool includeFileExtension)
		{
			return FilePathField(rect, null, path, parentPath, extensions, absolutePath, useBackslashes, includeFileExtension);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(GUIContent label, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(options);
			return FilePathField(rect, label, path, parentPath, extensions, absolutePath, useBackslashes);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <param name="includeFileExtension">If <c>true</c> the file path will include the file's extension.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(GUIContent label, string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, bool includeFileExtension = true, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(options);
			return FilePathField(rect, label, path, parentPath, extensions, absolutePath, useBackslashes, includeFileExtension);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <param name="includeFileExtension">If <c>true</c> the file path will include the file's extension.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, bool includeFileExtension = true, params GUILayoutOption[] options)
		{
			return FilePathField(null, path, parentPath, extensions, absolutePath, useBackslashes, includeFileExtension, options);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a file.
		/// </summary>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="extensions">Comma separated list of allowed file extensions. Use <c>null</c> to allow any file extension.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A path to a file.</returns>
		public static string FilePathField(string path, string parentPath, string extensions, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
		{
			return FilePathField(null, path, parentPath, extensions, absolutePath, useBackslashes, options);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a folder.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <returns>A path to a folder.</returns>
		public static string FolderPathField(Rect rect, GUIContent label, string path, string parentPath, bool absolutePath, bool useBackslashes)
		{
			bool needsProcessing = false;
			GUIHelper.PushGUIEnabled(enabled: true);
			if (label != null && rect.AlignLeft(GUIHelper.BetterLabelWidth).Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.clickCount >= 2)
			{
				string highlightPath = GetRelativePath(path, Directory.GetParent(Application.dataPath).FullName);
				if (!highlightPath.IsNullOrWhitespace())
				{
					UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(highlightPath.Replace('\\', '/'));
					if (obj != null)
					{
						EditorGUIUtility.PingObject(obj);
					}
				}
				Event.current.Use();
			}
			GUIHelper.PopGUIEnabled();
			UnityEngine.Object droppedObject = DragAndDropUtilities.DropZone<UnityEngine.Object>(rect, null, allowSceneObjects: false);
			if (droppedObject != null)
			{
				string pathBuffer = AssetDatabase.GetAssetPath(droppedObject);
				if ((File.GetAttributes(pathBuffer) & FileAttributes.Directory) == 0)
				{
					pathBuffer = Path.GetDirectoryName(pathBuffer);
				}
				path = pathBuffer;
				needsProcessing = true;
			}
			EditorGUI.BeginChangeCheck();
			string pathBuffer2 = TextField(rect.AlignLeft(rect.width - 18f), label, path);
			if (EditorGUI.EndChangeCheck())
			{
				path = (useBackslashes ? pathBuffer2.Replace('/', '\\') : pathBuffer2.Replace('\\', '/'));
			}
			bool isEnabled = GUI.enabled;
			if (Event.current.type != EventType.Repaint)
			{
				GUI.enabled = true;
			}
			if (SirenixEditorGUI.IconButton(rect.AlignRight(18f).SetHeight(18f).SubY(1f)
				.AddX(1f), EditorIcons.Folder))
			{
				string directory = GetOpenExplorerPath(path, parentPath);
				if (isEnabled)
				{
					string pathBuffer3 = EditorUtility.OpenFolderPanel("Select File", directory, "");
					if (!pathBuffer3.IsNullOrWhitespace())
					{
						path = pathBuffer3;
						needsProcessing = true;
					}
				}
				else
				{
					Process.Start(directory);
				}
			}
			if (Event.current.type != EventType.Repaint)
			{
				GUI.enabled = isEnabled;
			}
			if (!path.IsNullOrWhitespace() && needsProcessing)
			{
				path = Path.GetFullPath(path);
				if (!absolutePath)
				{
					path = GetRelativePath(path, parentPath.IsNullOrWhitespace() ? Directory.GetParent(Application.dataPath).FullName : parentPath);
				}
				path = (useBackslashes ? path.Replace('/', '\\') : path.Replace('\\', '/'));
				GUI.changed = true;
			}
			return path;
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a folder.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <returns>A path to a folder.</returns>
		public static string FolderPathField(Rect rect, string path, string parentPath, bool absolutePath, bool useBackslashes)
		{
			return FolderPathField(rect, null, path, parentPath, absolutePath, useBackslashes);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a folder.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A path to a folder.</returns>
		public static string FolderPathField(GUIContent label, string path, string parentPath, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(options);
			return FolderPathField(rect, label, path, parentPath, absolutePath, useBackslashes);
		}

		/// <summary>
		/// Draws a field that lets the user select a path to a folder.
		/// </summary>
		/// <param name="path">The current value.</param>
		/// <param name="parentPath">A parent path the path needs to be relative to. Use <c>null</c> for Unity project directory.</param>
		/// <param name="absolutePath">If <c>true</c> the path will be absolute. Otherwise the path will be relative to parentPath or to the Unity project directory.</param>
		/// <param name="useBackslashes">If <c>true</c> the path will be enforced to use backslashes. Otherwise the path will be enforced to use forward slashes.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A path to a folder.</returns>
		public static string FolderPathField(string path, string parentPath, bool absolutePath, bool useBackslashes, params GUILayoutOption[] options)
		{
			return FolderPathField(null, path, parentPath, absolutePath, useBackslashes, options);
		}

		private static string GetRelativePath(string path, string parentPath)
		{
			if (parentPath.IsNullOrWhitespace())
			{
				return path;
			}
			if (path.IsNullOrWhitespace())
			{
				return null;
			}
			parentPath = Path.GetFullPath(parentPath);
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(parentPath, path);
				path = path.Replace('\\', '/');
			}
			return PathUtilities.MakeRelative(parentPath, path);
		}

		private static string GetOpenExplorerPath(string path, string parentPath)
		{
			if (path.IsNullOrWhitespace())
			{
				if (!parentPath.IsNullOrWhitespace())
				{
					return GetOpenExplorerPath(parentPath, null);
				}
				return string.Empty;
			}
			if (!parentPath.IsNullOrWhitespace() && !Path.IsPathRooted(path))
			{
				path = Path.Combine(parentPath, path);
			}
			if (!Path.IsPathRooted(path))
			{
				path = Path.GetFullPath(path);
			}
			if (File.Exists(path) && File.GetAttributes(path) != FileAttributes.Directory)
			{
				path = Path.GetDirectoryName(path);
			}
			path = path.Replace('/', '\\').TrimEnd(new char[1] { '/' });
			string result = FindFirstExistingPath(path);
			if (result.IsNullOrWhitespace() && !parentPath.IsNullOrWhitespace())
			{
				return GetOpenExplorerPath(parentPath, null);
			}
			return result;
		}

		private static string FindFirstExistingPath(string path)
		{
			if (path.IsNullOrWhitespace())
			{
				return string.Empty;
			}
			if (Directory.Exists(path))
			{
				return path.Replace('\\', '/').Trim(new char[1] { '/' });
			}
			DirectoryInfo parent = Directory.GetParent(path);
			if (parent == null)
			{
				return string.Empty;
			}
			return FindFirstExistingPath(parent.ToString());
		}

		private static string GetFilePanelExtensions(string extensions)
		{
			if (extensions.IsNullOrWhitespace())
			{
				return null;
			}
			StringBuilder builder = new StringBuilder();
			IEnumerator<string> e = (from i in extensions.Split(',', ';')
				select i.Trim(' ', '.', '*') into i
				where !i.IsNullOrWhitespace()
				select i).GetEnumerator();
			while (e.MoveNext())
			{
				if (builder.Length > 0)
				{
					builder.Append(";*.");
				}
				builder.Append(e.Current);
			}
			return builder.ToString();
		}

		/// <summary>
		/// Draws a prefix label for a vector field, that implements label dragging.
		/// </summary>
		public static Vector4 VectorPrefixSlideRect(Rect rect, Vector4 value)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type == EventType.Layout)
			{
				return value;
			}
			Vector4 normal = ((value.sqrMagnitude > 0f) ? value.normalized : Vector4.one);
			float length = value.magnitude;
			if (GUIUtility.hotControl == controlID)
			{
				normal = vectorNormalBuffer;
				length = vectorLengthBuffer;
			}
			else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				vectorNormalBuffer = normal;
				vectorLengthBuffer = length;
			}
			EditorGUI.BeginChangeCheck();
			length = SirenixEditorGUI.SlideRect(rect, controlID, length);
			if (EditorGUI.EndChangeCheck())
			{
				vectorLengthBuffer = length;
				value = normal * length;
				value.x = (float)Math.Round(value.x, 2);
				value.y = (float)Math.Round(value.y, 2);
				value.z = (float)Math.Round(value.z, 2);
				value.w = (float)Math.Round(value.w, 2);
			}
			return value;
		}

		/// <summary>
		/// Draws a prefix label for a vector field, that implements label dragging.
		/// </summary>
		/// <param name="totalRect">The position and total size of the field.</param>
		/// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
		/// <param name="value">The value for the vector field.</param>
		/// <returns>The vector scaled by label dragging.</returns>
		public static Vector4 VectorPrefixLabel(ref Rect totalRect, GUIContent label, Vector4 value)
		{
			if (label == null)
			{
				return value;
			}
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			Rect labelRect = new Rect(totalRect.x, totalRect.y, totalRect.width, totalRect.height);
			totalRect = EditorGUI.PrefixLabel(totalRect, label);
			labelRect.width -= totalRect.width;
			Vector4 normal = ((value.sqrMagnitude > 0f) ? value.normalized : Vector4.one);
			float length = value.magnitude;
			if (GUIUtility.hotControl == controlID)
			{
				normal = vectorNormalBuffer;
				length = vectorLengthBuffer;
			}
			else if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
			{
				vectorNormalBuffer = normal;
				vectorLengthBuffer = length;
			}
			EditorGUI.BeginChangeCheck();
			length = SirenixEditorGUI.SlideRect(labelRect, controlID, length);
			if (EditorGUI.EndChangeCheck())
			{
				vectorLengthBuffer = length;
				value = normal * length;
				value.x = (float)Math.Round(value.x, 2);
				value.y = (float)Math.Round(value.y, 2);
				value.z = (float)Math.Round(value.z, 2);
				value.w = (float)Math.Round(value.w, 2);
			}
			return value;
		}

		/// <summary>
		/// Draws a prefix label for a vector field, that implements label dragging.
		/// </summary>
		/// <param name="totalRect">The position and total size of the field.</param>
		/// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
		/// <param name="value">The value for the vector field.</param>
		/// <returns>The vector scaled by label dragging.</returns>
		public static Vector4 VectorPrefixLabel(ref Rect totalRect, string label, Vector4 value)
		{
			return VectorPrefixLabel(ref totalRect, GUIHelper.TempContent(label), value);
		}

		/// <summary>
		/// Draws a prefix label for a vector field, that implements label dragging.
		/// </summary>
		/// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
		/// <param name="value">The value for the vector field.</param>
		/// <returns>The vector scaled by label dragging.</returns>
		public static Vector4 VectorPrefixLabel(GUIContent label, Vector4 value)
		{
			if (label == null)
			{
				return value;
			}
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			EditorGUILayout.PrefixLabel(label);
			Rect labelRect = GUILayoutUtility.GetLastRect();
			Vector4 normal = ((value.sqrMagnitude > 0f) ? value.normalized : Vector4.one);
			float length = value.magnitude;
			if (GUIUtility.hotControl == controlID)
			{
				normal = vectorNormalBuffer;
				length = vectorLengthBuffer;
			}
			else if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
			{
				vectorNormalBuffer = normal;
				vectorLengthBuffer = length;
			}
			EditorGUI.BeginChangeCheck();
			length = SirenixEditorGUI.SlideRect(labelRect, controlID, length);
			if (EditorGUI.EndChangeCheck())
			{
				vectorLengthBuffer = length;
				value = normal * length;
				value.x = (float)Math.Round(value.x, 2);
				value.y = (float)Math.Round(value.y, 2);
				value.z = (float)Math.Round(value.z, 2);
				value.w = (float)Math.Round(value.w, 2);
			}
			return value;
		}

		/// <summary>
		/// Draws a prefix label for a vector field, that implements label dragging.
		/// </summary>
		/// <param name="label">The label content. If <c>null</c> this function does nothing.</param>
		/// <param name="value">The value for the vector field.</param>
		/// <returns>The vector scaled by label dragging.</returns>
		public static Vector4 VectorPrefixLabel(string label, Vector4 value)
		{
			return VectorPrefixLabel(GUIHelper.TempContent(label), value);
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(Rect rect, GUIContent label, Vector2 value)
		{
			value = UnityShims.Vector4.op_ImplicitVec2(VectorPrefixLabel(ref rect, label, UnityShims.Vector4.op_Implicit(value)));
			bool showLabels = !ResponsiveVectorComponentFields || !(rect.width < 185f);
			GUIHelper.PushLabelWidth(SingleLetterStructLabelWidth);
			GUIHelper.PushIndentLevel(0);
			value.x = FloatField(rect.Split(0, 3).HorizontalPadding(0f, 2f), showLabels ? "X" : null, value.x);
			value.y = FloatField(rect.Split(1, 3).HorizontalPadding(0f, 2f), showLabels ? "Y" : null, value.y);
			GUIHelper.PopIndentLevel();
			GUIHelper.PopLabelWidth();
			return value;
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(Rect rect, string label, Vector2 value)
		{
			return Vector2Field(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(Rect rect, Vector2 value)
		{
			return Vector2Field(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(GUIContent label, Vector2 value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return Vector2Field(rect, label, value);
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(string label, Vector2 value, params GUILayoutOption[] options)
		{
			return Vector2Field((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws a Vector2 field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector2 Vector2Field(Vector2 value, params GUILayoutOption[] options)
		{
			return Vector2Field((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(Rect rect, GUIContent label, Vector3 value)
		{
			value = UnityShims.Vector4.op_ImplicitVec3(VectorPrefixLabel(ref rect, label, UnityShims.Vector4.op_Implicit(value)));
			bool showLabels = !ResponsiveVectorComponentFields || !(rect.width < 185f);
			GUIHelper.PushLabelWidth(SingleLetterStructLabelWidth);
			GUIHelper.PushIndentLevel(0);
			value.x = FloatField(rect.Split(0, 3).HorizontalPadding(0f, 2f), showLabels ? "X" : null, value.x);
			value.y = FloatField(rect.Split(1, 3).HorizontalPadding(0f, 1f), showLabels ? "Y" : null, value.y);
			value.z = FloatField(rect.Split(2, 3).HorizontalPadding(1f, 0f), showLabels ? "Z" : null, value.z);
			GUIHelper.PopIndentLevel();
			GUIHelper.PopLabelWidth();
			return value;
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(Rect rect, string label, Vector3 value)
		{
			return Vector3Field(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(Rect rect, Vector3 value)
		{
			return Vector3Field(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(GUIContent label, Vector3 value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return Vector3Field(rect, label, value);
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(string label, Vector3 value, params GUILayoutOption[] options)
		{
			return Vector3Field((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws a Vector3 field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector3 Vector3Field(Vector3 value, params GUILayoutOption[] options)
		{
			return Vector3Field((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(Rect rect, GUIContent label, Vector4 value)
		{
			value = VectorPrefixLabel(ref rect, label, value);
			bool showLabels = !ResponsiveVectorComponentFields || !(rect.width < 185f);
			GUIHelper.PushLabelWidth(SingleLetterStructLabelWidth);
			GUIHelper.PushIndentLevel(0);
			value.x = FloatField(rect.Split(0, 4).HorizontalPadding(0f, 2f), showLabels ? "X" : null, value.x);
			value.y = FloatField(rect.Split(1, 4).HorizontalPadding(0f, 2f), showLabels ? "Y" : null, value.y);
			value.z = FloatField(rect.Split(2, 4).HorizontalPadding(0f, 2f), showLabels ? "Z" : null, value.z);
			value.w = FloatField(rect.Split(3, 4).HorizontalPadding(0f, 2f), showLabels ? "W" : null, value.w);
			GUIHelper.PopIndentLevel();
			GUIHelper.PopLabelWidth();
			return value;
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(Rect rect, string label, Vector4 value)
		{
			return Vector4Field(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(Rect rect, Vector4 value)
		{
			return Vector4Field(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(GUIContent label, Vector4 value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return Vector4Field(rect, label, value);
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(string label, Vector4 value, params GUILayoutOption[] options)
		{
			return Vector4Field((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws a Vector4 field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Vector4 Vector4Field(Vector4 value, params GUILayoutOption[] options)
		{
			return Vector4Field((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value"></param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(Rect rect, GUIContent label, Color value)
		{
			if (label == null)
			{
				return EditorGUI.ColorField(rect, value);
			}
			return EditorGUI.ColorField(rect, label, value);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value"></param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(Rect rect, string label, Color value)
		{
			return ColorField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value"></param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(Rect rect, Color value)
		{
			return ColorField(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value"></param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(GUIContent label, Color value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			return ColorField(rect, label, value);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value"></param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(string label, Color value, params GUILayoutOption[] options)
		{
			return ColorField((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws a Color field.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Color ColorField(Color value, params GUILayoutOption[] options)
		{
			return ColorField((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(Rect rect, GUIContent label, Vector2 value, Vector2 limits, bool showFields = false)
		{
			if (minMaxSliderStyle == null)
			{
				minMaxSliderStyle = "MinMaxHorizontalSliderThumb";
			}
			if (sliderBackground == null)
			{
				sliderBackground = GUI.skin.horizontalSlider;
			}
			Rect totalRect = rect;
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			int controlId = GUIUtility.GetControlID(FocusType.Passive);
			int fieldWidth = (showFields ? Mathf.RoundToInt(rect.width * 0.3f * 0.5f) : 0);
			Rect fieldRect = rect.AddX(fieldWidth).SetWidth(rect.width - (float)(fieldWidth * 2) - 11f).HorizontalPadding(4f);
			Rect controlRect = fieldRect.SetXMax(Mathf.RoundToInt(fieldRect.x + fieldRect.width * MathUtilities.LinearStep(limits.x, limits.y, value.y) + 11f)).AddXMin(Mathf.RoundToInt(fieldRect.width * MathUtilities.LinearStep(limits.x, limits.y, value.x)));
			if (showFields)
			{
				GUIHelper.PushIndentLevel(0);
				EditorGUI.BeginChangeCheck();
				float newX = FloatField(rect.AlignLeft(fieldWidth), value.x);
				if (EditorGUI.EndChangeCheck())
				{
					value.x = newX;
					value.x = Mathf.Clamp(value.x, limits.x, limits.y);
					GUI.changed = true;
				}
				EditorGUI.BeginChangeCheck();
				float newY = FloatField(rect.AlignRight(fieldWidth), value.y);
				if (EditorGUI.EndChangeCheck())
				{
					value.y = newY;
					value.y = Mathf.Clamp(value.y, limits.x, limits.y);
					GUI.changed = true;
				}
				GUIHelper.PopIndentLevel();
			}
			if (Event.current.IsHovering(fieldRect))
			{
				GUIHelper.RequestRepaint();
			}
			if (Event.current.OnMouseDown(fieldRect.SetWidth(fieldRect.width + 11f), 0))
			{
				GUIUtility.hotControl = controlId;
				localHotControl = (Event.current.control ? 3 : ((Event.current.mousePosition.x <= controlRect.xMin) ? 1 : ((Event.current.mousePosition.x >= controlRect.xMax) ? 2 : ((Mathf.Abs(controlRect.xMin - Event.current.mousePosition.x) < Mathf.Abs(controlRect.xMax - Event.current.mousePosition.x)) ? 1 : 2))));
				if (localHotControl == 1)
				{
					value.x = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
					value.x = Mathf.Min(value.x, value.y);
				}
				else if (localHotControl == 2)
				{
					value.y = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
					value.y = Mathf.Max(value.x, value.y);
				}
				GUI.changed = true;
			}
			else if (GUIUtility.hotControl == controlId)
			{
				if (Event.current.rawType == EventType.MouseUp)
				{
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				else if (Event.current.OnMouseMoveDrag())
				{
					if (localHotControl == 1)
					{
						value.x = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
						value.x = Mathf.Min(value.x, value.y);
					}
					else if (localHotControl == 2)
					{
						value.y = Mathf.Clamp(Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.xMin, fieldRect.xMax, Event.current.mousePosition.x)), limits.x, limits.y);
						value.y = Mathf.Max(value.x, value.y);
					}
					else
					{
						controlRect.x = Mathf.Clamp(controlRect.x + Event.current.delta.x, fieldRect.x, fieldRect.xMax + 11f - controlRect.width);
						value.x = Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.x, fieldRect.xMax, controlRect.x));
						value.y = Mathf.Lerp(limits.x, limits.y, MathUtilities.LinearStep(fieldRect.x, fieldRect.xMax, controlRect.xMax - 11f));
					}
					GUIHelper.RequestRepaint();
					GUI.changed = true;
				}
			}
			if (Event.current.OnRepaint())
			{
				EditorGUIUtility.AddCursorRect(controlRect, (Event.current.control || (GUIUtility.hotControl == controlId && localHotControl == 3)) ? MouseCursor.Link : MouseCursor.SlideArrow);
				if (UnityVersion.IsVersionOrGreater(2019, 3))
				{
					controlRect = controlRect.AddY(3.5f);
				}
				sliderBackground.Draw(fieldRect.SetWidth(fieldRect.width + 11f).AddY(-1f), GUIContent.none, 0);
				if (!EditorGUI.showMixedValue)
				{
					minMaxSliderStyle.Draw(controlRect.MinWidth(11f), GUIContent.none, controlId);
				}
				if (!EditorGUI.showMixedValue && (Event.current.IsHovering(totalRect) || GUIUtility.hotControl == controlId))
				{
					Rect floatRect = fieldRect.SetWidth(fieldRect.width + 11f);
					GUIContent xLabel = new GUIContent(MathUtilities.DiscardLeastSignificantDecimal(value.x).ToString());
					GUIContent yLabel = new GUIContent(MathUtilities.DiscardLeastSignificantDecimal(value.y).ToString());
					GUIContent minLabel = new GUIContent(limits.x.ToString());
					GUIContent maxLabel = new GUIContent(limits.y.ToString());
					if (minMaxFloatingLabelStyle == null)
					{
						minMaxFloatingLabelStyle = new GUIStyle("ProfilerBadge")
						{
							font = EditorStyles.miniButton.font,
							fontStyle = EditorStyles.miniButton.fontStyle,
							fontSize = EditorStyles.miniButton.fontSize,
							alignment = TextAnchor.MiddleCenter
						};
					}
					Vector2 size = minMaxFloatingLabelStyle.CalcSize(xLabel);
					Rect xRect = floatRect.SetSize(size).SetCenterX(controlRect.xMin).AddY(0f - size.y)
						.Expand(4f, 0f);
					size = minMaxFloatingLabelStyle.CalcSize(yLabel);
					Rect yRect = floatRect.SetSize(size).SetCenterX(controlRect.xMax).AddY(0f - size.y)
						.Expand(4f, 0f);
					size = minMaxFloatingLabelStyle.CalcSize(minLabel);
					Rect minRect = floatRect.SetSize(size).SetCenterX(fieldRect.xMin).AddY(0f - size.y)
						.Expand(4f, 0f);
					size = minMaxFloatingLabelStyle.CalcSize(maxLabel);
					Rect maxRect = floatRect.AlignRight(size.x).SetHeight(size.y).AddY(0f - size.y)
						.Expand(4f, 0f);
					if (xRect.xMax + 4f > yRect.xMin)
					{
						float d = xRect.xMax + 4f - yRect.xMin;
						xRect.x -= Mathf.RoundToInt(d * 0.5f);
						yRect.x += Mathf.RoundToInt(d * 0.5f);
					}
					if (minRect.xMax + 4f < xRect.xMin)
					{
						minMaxFloatingLabelStyle.Draw(minRect, minLabel, -1);
					}
					if (maxRect.xMin - 4f > yRect.xMax)
					{
						minMaxFloatingLabelStyle.Draw(maxRect, maxLabel, -1);
					}
					minMaxFloatingLabelStyle.Draw(xRect, xLabel, -1);
					minMaxFloatingLabelStyle.Draw(yRect, yLabel, -1);
				}
			}
			return value;
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(Rect rect, string label, Vector2 value, Vector2 limits, bool showFields = false)
		{
			return MinMaxSlider(rect, GUIHelper.TempContent(label), value, limits, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(Rect rect, Vector2 value, Vector2 limits, bool showFields)
		{
			return MinMaxSlider(rect, (GUIContent)null, value, limits, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(GUIContent label, Vector2 value, Vector2 limits, bool showFields = false, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			return MinMaxSlider(rect, label, value, limits, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(string label, Vector2 value, Vector2 limits, bool showFields = false, params GUILayoutOption[] options)
		{
			return MinMaxSlider(GUIHelper.TempContent(label), value, limits, showFields, options);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="limits">The min and max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>A Vector2 with X set as min value, and Y to set as max value.</returns>
		public static Vector2 MinMaxSlider(Vector2 value, Vector2 limits, bool showFields, params GUILayoutOption[] options)
		{
			return MinMaxSlider((GUIContent)null, value, limits, showFields, options);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		public static void MinMaxSlider(Rect rect, GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false)
		{
			Vector2 value = new Vector2(minValue, maxValue);
			Vector2 limits = new Vector2(minLimit, maxLimit);
			value = MinMaxSlider(rect, label, value, limits, showFields);
			minValue = value.x;
			maxValue = value.y;
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		public static void MinMaxSlider(Rect rect, string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false)
		{
			MinMaxSlider(rect, GUIHelper.TempContent(label), ref minValue, ref maxValue, minLimit, maxLimit, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		public static void MinMaxSlider(Rect rect, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields)
		{
			MinMaxSlider(rect, (GUIContent)null, ref minValue, ref maxValue, minLimit, maxLimit, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		public static void MinMaxSlider(GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, options);
			MinMaxSlider(rect, label, ref minValue, ref maxValue, minLimit, maxLimit, showFields);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		public static void MinMaxSlider(string label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields = false, params GUILayoutOption[] options)
		{
			MinMaxSlider(GUIHelper.TempContent(label), ref minValue, ref maxValue, minLimit, maxLimit, showFields, options);
		}

		/// <summary>
		/// Draws a slider for setting two values between a min and a max limit.
		/// </summary>
		/// <param name="minValue">Current min value.</param>
		/// <param name="maxValue">Current max value.</param>
		/// <param name="minLimit">The min limit for the value.</param>
		/// <param name="maxLimit">The max limit for the value.</param>
		/// <param name="showFields">Show fields for min and max value.</param>
		/// <param name="options">Layout options.</param>
		public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit, bool showFields, params GUILayoutOption[] options)
		{
			MinMaxSlider((GUIContent)null, ref minValue, ref maxValue, minLimit, maxLimit, showFields, options);
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(Rect rect, GUIContent label, Quaternion value, QuaternionDrawMode mode)
		{
			return mode switch
			{
				QuaternionDrawMode.Eulers => EulerField(rect, label, value), 
				QuaternionDrawMode.AngleAxis => AngleAxisField(rect, label, value), 
				QuaternionDrawMode.Raw => QuaternionField(rect, label, value), 
				_ => throw new NotImplementedException("Unknown draw mode: " + mode), 
			};
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(Rect rect, string label, Quaternion value, QuaternionDrawMode mode)
		{
			return RotationField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value, mode);
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(Rect rect, Quaternion value, QuaternionDrawMode mode)
		{
			return RotationField(rect, (GUIContent)null, value, mode);
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(GUIContent label, Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return RotationField(rect, label, value, mode);
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(string label, Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
		{
			return RotationField((label != null) ? GUIHelper.TempContent(label) : null, value, mode, options);
		}

		/// <summary>
		/// Draws a rotation field for a quaternion.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="mode">Draw mode for rotation field.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion RotationField(Quaternion value, QuaternionDrawMode mode, params GUILayoutOption[] options)
		{
			return RotationField((GUIContent)null, value, mode, options);
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(Rect rect, GUIContent label, Quaternion value)
		{
			int beginID = GUIUtility.GetControlID(FocusType.Passive);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			QuaternionContextBuffer context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.EulerField_ID:" + beginID, "QuaternionFieldBuffer").Value;
			if (localHotControl != beginID || (Event.current.type == EventType.Repaint && !context.IsUsed))
			{
				if (localHotControl == beginID)
				{
					localHotControl = 0;
				}
				context.Set(value, QuaternionDrawMode.Eulers);
			}
			EditorGUI.BeginChangeCheck();
			context.Eulers = Vector3Field(rect, context.Eulers);
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = beginID;
				value = Quaternion.Euler(context.Eulers);
				GUI.changed = true;
			}
			int endID = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type == EventType.Repaint)
			{
				context.IsUsed = false;
			}
			else
			{
				context.IsUsed = context.IsUsed || (GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID) || (GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID);
			}
			return value;
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(Rect rect, string label, Quaternion value)
		{
			return EulerField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(Rect rect, Quaternion value)
		{
			return EulerField(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return EulerField(rect, label, value);
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(string label, Quaternion value, params GUILayoutOption[] options)
		{
			return EulerField((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws an euler field for a quaternion.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion EulerField(Quaternion value, params GUILayoutOption[] options)
		{
			return EulerField((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(Rect rect, GUIContent label, Quaternion value)
		{
			int beginID = GUIUtility.GetControlID(FocusType.Passive);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			QuaternionContextBuffer context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.AngleAxisField_ID:" + beginID, "QuaternionFieldBuffer").Value;
			if (localHotControl != beginID || (Event.current.type == EventType.Repaint && !context.IsUsed))
			{
				if (localHotControl == beginID)
				{
					localHotControl = 0;
				}
				context.Set(value, QuaternionDrawMode.AngleAxis);
			}
			Rect axisRect = rect.SetWidth(rect.width * 0.65f);
			Rect angleRect = rect.AlignRight(rect.width - axisRect.width);
			bool showLabels = !ResponsiveVectorComponentFields || !(rect.width < 185f);
			EditorGUI.BeginChangeCheck();
			GUIHelper.PushIndentLevel(0);
			GUIHelper.PushLabelWidth(SingleLetterStructLabelWidth);
			Vector3 axis = context.Axis;
			axis.x = FloatField(axisRect.Split(0, 3), showLabels ? "X" : null, axis.x);
			axis.y = FloatField(axisRect.Split(1, 3), showLabels ? "Y" : null, axis.y);
			axis.z = FloatField(axisRect.Split(2, 3), showLabels ? "Z" : null, axis.z);
			context.Axis = axis;
			GUIHelper.PopLabelWidth();
			GUIHelper.PushLabelWidth(38f);
			context.Angle = FloatField(angleRect, showLabels ? "Angle" : null, context.Angle);
			GUIHelper.PopLabelWidth();
			GUIHelper.PopIndentLevel();
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = beginID;
				value = Quaternion.AngleAxis(MathUtilities.Wrap(context.Angle, 0f, 360f), context.Axis.normalized);
				GUI.changed = true;
			}
			int endID = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type == EventType.Repaint)
			{
				context.IsUsed = false;
			}
			else
			{
				context.IsUsed = context.IsUsed || (GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID) || (GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID);
			}
			return value;
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(Rect rect, string label, Quaternion value)
		{
			return AngleAxisField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(Rect rect, Quaternion value)
		{
			return AngleAxisField(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return AngleAxisField(rect, label, value);
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(string label, Quaternion value, params GUILayoutOption[] options)
		{
			return AngleAxisField((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws an angle axis field for a quaternion.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion AngleAxisField(Quaternion value, params GUILayoutOption[] options)
		{
			return AngleAxisField((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(Rect rect, GUIContent label, Quaternion value)
		{
			int beginID = GUIUtility.GetControlID(FocusType.Passive);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			QuaternionContextBuffer context = GUIHelper.GetTemporaryContext<QuaternionContextBuffer>("SirenixEditorFields.QuaternionField_ID:" + beginID, "QuaternionFieldBuffer").Value;
			if (localHotControl != beginID || (Event.current.type == EventType.Repaint && !context.IsUsed))
			{
				if (localHotControl == beginID)
				{
					localHotControl = 0;
				}
				context.Set(value, QuaternionDrawMode.Raw);
			}
			EditorGUI.BeginChangeCheck();
			GUIHelper.PushIndentLevel(0);
			context.Raw = Vector4Field(rect, context.Raw);
			GUIHelper.PopIndentLevel();
			if (EditorGUI.EndChangeCheck())
			{
				localHotControl = beginID;
				value.x = context.Raw.x;
				value.y = context.Raw.y;
				value.z = context.Raw.z;
				value.w = context.Raw.w;
				GUI.changed = true;
			}
			int endID = GUIUtility.GetControlID(FocusType.Passive);
			if (Event.current.type == EventType.Repaint)
			{
				context.IsUsed = false;
			}
			else
			{
				context.IsUsed = context.IsUsed || (GUIUtility.hotControl > beginID && GUIUtility.hotControl < endID) || (GUIUtility.keyboardControl > beginID && GUIUtility.keyboardControl < endID);
			}
			return value;
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(Rect rect, string label, Quaternion value)
		{
			return QuaternionField(rect, (label != null) ? GUIHelper.TempContent(label) : null, value);
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(Rect rect, Quaternion value)
		{
			return QuaternionField(rect, (GUIContent)null, value);
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(GUIContent label, Quaternion value, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField, options);
			return QuaternionField(rect, label, value);
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(string label, Quaternion value, params GUILayoutOption[] options)
		{
			return QuaternionField((label != null) ? GUIHelper.TempContent(label) : null, value, options);
		}

		/// <summary>
		/// Draws a quaternion field.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Quaternion QuaternionField(Quaternion value, params GUILayoutOption[] options)
		{
			return QuaternionField((GUIContent)null, value, options);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(Rect rect, GUIContent label, int selected, string[] itemNames, GUIStyle style)
		{
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			return EditorGUI.Popup(rect, selected, itemNames, style ?? EditorStyles.popup);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(Rect rect, GUIContent label, int selected, string[] itemNames)
		{
			return Dropdown(rect, label, selected, itemNames, null);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(Rect rect, string label, int selected, string[] itemNames)
		{
			return Dropdown(rect, (label != null) ? GUIHelper.TempContent(label) : null, selected, itemNames, null);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(Rect rect, int selected, string[] itemNames)
		{
			return Dropdown(rect, null, selected, itemNames, null);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(GUIContent label, int selected, string[] itemNames, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return Dropdown(rect, label, selected, itemNames, style);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(GUIContent label, int selected, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown(label, selected, itemNames, null, options);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(string label, int selected, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown((label != null) ? GUIHelper.TempContent(label) : null, selected, itemNames, null, options);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <param name="selected">Current value.</param>
		/// <param name="itemNames">Names of selectable items.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int Dropdown(int selected, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown(null, selected, itemNames, null, options);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items">Selectable items.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(Rect rect, GUIContent label, T selected, IList<T> items)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, controlID, label);
			}
			string display = null;
			display = ((!EditorGUI.showMixedValue) ? ((selected == null) ? "Null" : selected.ToString()) : "—");
			if (GUI.Button(rect, display, EditorStyles.popup))
			{
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < items.Count; i++)
				{
					int localI = i;
					bool isSelected = EqualityComparer<T>.Default.Equals(selected, items[i]);
					menu.AddItem(new GUIContent((items[i] == null) ? "Null" : (items[i]?.ToString() ?? "")), isSelected, delegate
					{
						PopupSelector<T>.CurrentSelectingPopupControlID = controlID;
						PopupSelector<T>.SelectFunc = () => items[localI];
					});
				}
				menu.DropDown(rect);
			}
			if (PopupSelector<T>.CurrentSelectingPopupControlID == controlID && PopupSelector<T>.SelectFunc != null)
			{
				selected = PopupSelector<T>.SelectFunc();
				PopupSelector<T>.CurrentSelectingPopupControlID = -1;
				PopupSelector<T>.SelectFunc = null;
				GUI.changed = true;
			}
			return selected;
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items">Selectable items.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(GUIContent label, T selected, IList<T> items)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.numberField);
			return Dropdown(rect, label, selected, items);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items">Selectable items.</param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(Rect rect, GUIContent label, T selected, T[] items, string[] itemNames, GUIStyle style)
		{
			int index = 0;
			for (int i = 0; i < items.Length; i++)
			{
				if (selected.Equals(items[i]))
				{
					index = i;
					break;
				}
			}
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, label);
			}
			index = EditorGUI.Popup(rect, index, itemNames, style ?? EditorStyles.popup);
			return items[index];
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(Rect rect, GUIContent label, T selected, T[] items, string[] itemNames)
		{
			return Dropdown(rect, label, selected, items, itemNames, null);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(Rect rect, string label, T selected, T[] items, string[] itemNames)
		{
			return Dropdown(rect, (label != null) ? GUIHelper.TempContent(label) : null, selected, items, itemNames, null);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(Rect rect, T selected, T[] items, string[] itemNames)
		{
			return Dropdown(rect, null, selected, items, itemNames, null);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(GUIContent label, T selected, T[] items, string[] itemNames, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return Dropdown(rect, label, selected, items, itemNames, style);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(GUIContent label, T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown(label, selected, items, itemNames, null, options);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(string label, T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown((label != null) ? GUIHelper.TempContent(label) : null, selected, items, itemNames, null, options);
		}

		/// <summary>
		/// Draws a generic dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="selected">Current value.</param>
		/// <param name="items"></param>
		/// <param name="itemNames">Names of selectable items. If <c>null</c> ToString() will be used instead.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static T Dropdown<T>(T selected, T[] items, string[] itemNames, params GUILayoutOption[] options)
		{
			return Dropdown(null, selected, items, itemNames, null, options);
		}

		private static Enum EnumDropdownImplementation(Rect buttonPosition, string display, int controlID, Type type, Enum selected, GUIStyle style)
		{
			if (GUI.Button(buttonPosition, display, style))
			{
				string[] names = Enum.GetNames(type);
				Array valuesArray = Enum.GetValues(type);
				currentEnumControlID = controlID;
				GenericMenu menu = new GenericMenu();
				for (int i = 0; i < names.Length; i++)
				{
					int localI = i;
					menu.AddItem(new GUIContent(names[i]), selected.Equals(valuesArray.GetValue(i)), delegate
					{
						currentEnumControlHasValue = true;
						selectedEnumValue = (Enum)valuesArray.GetValue(localI);
					});
				}
				menu.DropDown(buttonPosition);
			}
			if (currentEnumControlHasValue && controlID == currentEnumControlID)
			{
				currentEnumControlHasValue = false;
				if (selected != selectedEnumValue)
				{
					GUI.changed = true;
					selected = selectedEnumValue;
				}
				selectedEnumValue = null;
			}
			return selected;
		}

		private static Enum EnumFlagDropdownImplementation(Rect buttonPosition, string display, int controlID, Type type, Enum selected, GUIStyle style)
		{
			Type underlyingType = Enum.GetUnderlyingType(type);
			bool signed = underlyingType == typeof(sbyte) || underlyingType == typeof(int) || underlyingType == typeof(short) || underlyingType == typeof(long);
			selected = GetCurrentMaskValue(controlID, type, selected, signed);
			if (string.IsNullOrEmpty(display) || display == "0")
			{
				display = "None";
			}
			else if (display.Contains(",") && style.CalcSize(new GUIContent(display)).x > buttonPosition.width)
			{
				display = "Mixed (" + (display.Count((char n) => n == ',') + 1) + ")...";
			}
			if (GUI.Button(buttonPosition, display, style))
			{
				string[] names = Enum.GetNames(type);
				Array valuesArray = Enum.GetValues(type);
				GenericMenu menu = new GenericMenu();
				MaskMenu.CurrentEnumControlID = controlID;
				MaskMenu.EnumChanged = false;
				if (signed)
				{
					long selectedValue = Convert.ToInt64(selected, CultureInfo.InvariantCulture);
					List<long> values = (from n in valuesArray.FilterCast<object>()
						select Convert.ToInt64(n, CultureInfo.InvariantCulture)).ToList();
					int noneIndex = values.IndexOf(0L);
					int allIndex = values.FindIndex((long n) => n != 0L && values.All((long m) => (m & n) == n));
					long allValue = 0L;
					for (int i = 0; i < values.Count; i++)
					{
						allValue |= values[i];
					}
					if (values.Count >= 16)
					{
						if (allIndex == -1)
						{
							menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateSigned, allValue);
							menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateSigned, 0L);
						}
						if (allIndex == -1 || noneIndex == -1)
						{
							menu.AddSeparator("");
						}
					}
					for (int i2 = 0; i2 < names.Length; i2++)
					{
						long value = values[i2];
						bool hasFlag = ((value != 0L) ? ((value & selectedValue) == value) : (selectedValue == 0));
						menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(names[i2])), hasFlag, EnumMaskSetValueDelegateSigned, value);
					}
					if (values.Count < 16)
					{
						if (allIndex == -1 || noneIndex == -1)
						{
							menu.AddSeparator("");
						}
						if (allIndex == -1)
						{
							menu.AddItem(new GUIContent("All"), selectedValue == allValue, EnumMaskSetValueDelegateSigned, allValue);
							menu.AddItem(new GUIContent("None"), selectedValue == 0, EnumMaskSetValueDelegateSigned, 0L);
						}
					}
				}
				else
				{
					ulong selectedValue2 = Convert.ToUInt64(selected, CultureInfo.InvariantCulture);
					List<ulong> values2 = (from n in valuesArray.FilterCast<object>()
						select Convert.ToUInt64(n, CultureInfo.InvariantCulture)).ToList();
					int noneIndex2 = values2.IndexOf(0uL);
					int allIndex2 = values2.FindIndex((ulong n) => n != 0L && values2.All((ulong m) => (m & n) == n));
					ulong allValue2 = 0uL;
					for (int i3 = 0; i3 < values2.Count; i3++)
					{
						allValue2 |= values2[i3];
					}
					if (values2.Count >= 16)
					{
						if (allIndex2 == -1)
						{
							menu.AddItem(new GUIContent("All"), selectedValue2 == allValue2, EnumMaskSetValueDelegateUnsigned, allValue2);
							menu.AddItem(new GUIContent("None"), selectedValue2 == 0, EnumMaskSetValueDelegateUnsigned, 0uL);
						}
						if (allIndex2 == -1 || noneIndex2 == -1)
						{
							menu.AddSeparator("");
						}
					}
					for (int i4 = 0; i4 < names.Length; i4++)
					{
						ulong value2 = values2[i4];
						bool hasFlag2 = ((value2 != 0L) ? ((value2 & selectedValue2) == value2) : (selectedValue2 == 0));
						menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(names[i4])), hasFlag2, EnumMaskSetValueDelegateUnsigned, value2);
					}
					if (values2.Count < 16)
					{
						if (allIndex2 == -1 || noneIndex2 == -1)
						{
							menu.AddSeparator("");
						}
						if (allIndex2 == -1)
						{
							menu.AddItem(new GUIContent("All"), selectedValue2 == allValue2, EnumMaskSetValueDelegateUnsigned, allValue2);
							menu.AddItem(new GUIContent("None"), selectedValue2 == 0, EnumMaskSetValueDelegateUnsigned, 0uL);
						}
					}
				}
				menu.DropDown(buttonPosition);
			}
			return selected;
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(Rect rect, GUIContent label, Enum selected, GUIStyle style)
		{
			Type type = selected.GetType();
			int controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			string display = (EditorGUI.showMixedValue ? "—" : selected.ToString());
			Rect buttonPosition = ((label == null) ? rect : EditorGUI.PrefixLabel(rect, controlID, label, EditorStyles.label));
			style = style ?? EditorStyles.popup;
			if (label == null)
			{
				buttonPosition = EditorGUI.IndentedRect(buttonPosition);
			}
			if (type.IsDefined<FlagsAttribute>())
			{
				return EnumFlagDropdownImplementation(buttonPosition, display, controlID, type, selected, style);
			}
			return EnumDropdownImplementation(buttonPosition, display, controlID, type, selected, style);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(Rect rect, GUIContent label, Enum selected)
		{
			return EnumDropdown(rect, label, selected, null);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(Rect rect, string label, Enum selected)
		{
			return EnumDropdown(rect, (label != null) ? GUIHelper.TempContent(label) : null, selected, null);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="selected">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(Rect rect, Enum selected)
		{
			return EnumDropdown(rect, null, selected, null);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return EnumDropdown(rect, label, selected, style);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(GUIContent label, Enum selected, params GUILayoutOption[] options)
		{
			return EnumDropdown(label, selected, null, options);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(string label, Enum selected, params GUILayoutOption[] options)
		{
			return EnumDropdown((label != null) ? GUIHelper.TempContent(label) : null, selected, null, options);
		}

		/// <summary>
		/// Draws a dropdown for an enum or an enum mask.
		/// </summary>
		/// <param name="selected">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static Enum EnumDropdown(Enum selected, params GUILayoutOption[] options)
		{
			return EnumDropdown(null, selected, null, options);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(Rect rect, GUIContent label, IList<int> selected, IList<T> items, bool multiSelection)
		{
			int controlID = GUIUtility.GetControlID(FocusType.Keyboard, rect);
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, controlID, label);
			}
			string display = null;
			if (EditorGUI.showMixedValue)
			{
				display = "—";
			}
			else
			{
				for (int i = 0; i < selected.Count; i++)
				{
					T item = items[selected[i]];
					string name = ((item == null) ? "Null" : item.ToString());
					display = ((display != null) ? (name + ", " + display) : name);
				}
			}
			display = display ?? "None";
			if (GUI.Button(rect, display, EditorStyles.popup))
			{
				GenericMenu menu = new GenericMenu();
				for (int j = 0; j < items.Count; j++)
				{
					int localI = j;
					bool isSelected = selected.Contains(j);
					string numSelected = "";
					if (isSelected)
					{
						int selectedCount = selected.Count((int x) => x == j);
						if (selectedCount > 1)
						{
							numSelected = " (" + selectedCount + ")";
						}
					}
					menu.AddItem(new GUIContent(items[j]?.ToString() + numSelected), isSelected, delegate
					{
						PopupSelector.CurrentSelectingPopupControlID = controlID;
						PopupSelector.SelectAction = delegate
						{
							if (multiSelection)
							{
								if (isSelected)
								{
									for (int num = selected.Count - 1; num >= 0; num--)
									{
										if (selected[num] == localI)
										{
											selected.RemoveAt(num);
										}
									}
								}
								else
								{
									selected.Add(localI);
								}
							}
							else
							{
								selected.Clear();
								selected.Add(localI);
							}
						};
					});
				}
				menu.DropDown(rect);
			}
			if (PopupSelector.CurrentSelectingPopupControlID == controlID && PopupSelector.SelectAction != null)
			{
				PopupSelector.SelectAction();
				PopupSelector.CurrentSelectingPopupControlID = -1;
				PopupSelector.SelectAction = null;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(Rect rect, string label, IList<int> selected, IList<T> items, bool multiSelection)
		{
			return Dropdown(rect, (label != null) ? GUIHelper.TempContent(label) : null, selected, items, multiSelection);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(Rect rect, IList<int> selected, IList<T> items, bool multiSelection)
		{
			return Dropdown(rect, (GUIContent)null, selected, items, multiSelection);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <param name="options">Layout options.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(GUIContent label, IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, EditorStyles.popup, options);
			return Dropdown(rect, label, selected, items, multiSelection);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <param name="options">Layout options.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(string label, IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
		{
			return Dropdown((label != null) ? GUIHelper.TempContent(label) : null, selected, items, multiSelection, options);
		}

		/// <summary>
		/// Draws a dropdown.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="selected">Current selection.</param>
		/// <param name="items">Avaible items in the dropdown.</param>
		/// <param name="multiSelection">If <c>true</c> then the user can select multiple items. Otherwise the user can only select one item.</param>
		/// <param name="options">Layout options.</param>
		/// <returns><c>true</c> when the user has changed the selection. Otherwise <c>false</c>.</returns>
		public static bool Dropdown<T>(IList<int> selected, IList<T> items, bool multiSelection, params GUILayoutOption[] options)
		{
			return Dropdown((GUIContent)null, selected, items, multiSelection, options);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, decimal value, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber v = InternalSmartEditableNumberField(in expressionContext, rect, label, new EditableNumber(value), "", style);
			if (EditorGUI.EndChangeCheck())
			{
				value = v.AsDecimal;
			}
			return value;
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, Rect rect, string label, decimal value)
		{
			return SmartDecimalField(in expressionContext, rect, GUIHelper.TempContent(label), value, null);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, Rect rect, decimal value)
		{
			return SmartDecimalField(in expressionContext, rect, null, value, null);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, GUIContent label, decimal value, GUIStyle style, params GUILayoutOption[] options)
		{
			style = style ?? EditorStyles.numberField;
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style, options);
			return SmartDecimalField(in expressionContext, rect, label, value, style);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, GUIContent label, decimal value, params GUILayoutOption[] options)
		{
			return SmartDecimalField(in expressionContext, label, value, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, string label, decimal value, params GUILayoutOption[] options)
		{
			return SmartDecimalField(in expressionContext, GUIHelper.TempContent(label), value, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalField(in FieldExpressionContext expressionContext, decimal value, params GUILayoutOption[] options)
		{
			return SmartDecimalField(in expressionContext, null, value, null, options);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, double value, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber v = InternalSmartEditableNumberField(in expressionContext, rect, label, new EditableNumber(value), EditorGUI_Internals.kDoubleFieldFormatString, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = v.AsDouble;
			}
			return value;
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, Rect rect, string label, double value)
		{
			return SmartDoubleField(in expressionContext, rect, GUIHelper.TempContent(label), value, null);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, Rect rect, double value)
		{
			return SmartDoubleField(in expressionContext, rect, null, value, null);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, GUIContent label, double value, GUIStyle style, params GUILayoutOption[] options)
		{
			style = style ?? EditorStyles.numberField;
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style, options);
			return SmartDoubleField(in expressionContext, rect, label, value, style);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, GUIContent label, double value, params GUILayoutOption[] options)
		{
			return SmartDoubleField(in expressionContext, label, value, null, options);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, string label, double value, params GUILayoutOption[] options)
		{
			return SmartDoubleField(in expressionContext, GUIHelper.TempContent(label), value, null, options);
		}

		/// <summary>
		/// Draws a double field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleField(in FieldExpressionContext expressionContext, double value, params GUILayoutOption[] options)
		{
			return SmartDoubleField(in expressionContext, null, value, null, options);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, float value, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber v = InternalSmartEditableNumberField(in expressionContext, rect, label, new EditableNumber(value), EditorGUI_Internals.kFloatFieldFormatString, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = (float)v.AsDouble;
			}
			return value;
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, Rect rect, string label, float value)
		{
			return SmartFloatField(in expressionContext, rect, GUIHelper.TempContent(label), value, null);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, Rect rect, float value)
		{
			return SmartFloatField(in expressionContext, rect, null, value, null);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
		{
			style = style ?? EditorStyles.numberField;
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style, options);
			return SmartFloatField(in expressionContext, rect, label, value, style);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, GUIContent label, float value, params GUILayoutOption[] options)
		{
			return SmartFloatField(in expressionContext, label, value, null, options);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, string label, float value, params GUILayoutOption[] options)
		{
			return SmartFloatField(in expressionContext, GUIHelper.TempContent(label), value, null, options);
		}

		/// <summary>
		/// Draws a float field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatField(in FieldExpressionContext expressionContext, float value, params GUILayoutOption[] options)
		{
			return SmartFloatField(in expressionContext, null, value, null, options);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, long value, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber v = InternalSmartEditableNumberField(in expressionContext, rect, label, new EditableNumber(value), EditorGUI_Internals.kIntFieldFormatString, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = v.AsLong;
			}
			return value;
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, Rect rect, string label, long value)
		{
			return SmartLongField(in expressionContext, rect, GUIHelper.TempContent(label), value, null);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, Rect rect, long value)
		{
			return SmartLongField(in expressionContext, rect, null, value, null);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, GUIContent label, long value, GUIStyle style, params GUILayoutOption[] options)
		{
			style = style ?? EditorStyles.numberField;
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style, options);
			return SmartLongField(in expressionContext, rect, label, value, style);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, GUIContent label, long value, params GUILayoutOption[] options)
		{
			return SmartLongField(in expressionContext, label, value, null, options);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, string label, long value, params GUILayoutOption[] options)
		{
			return SmartLongField(in expressionContext, GUIHelper.TempContent(label), value, null, options);
		}

		/// <summary>
		/// Draws a long field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongField(in FieldExpressionContext expressionContext, long value, params GUILayoutOption[] options)
		{
			return SmartLongField(in expressionContext, null, value, null, options);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, int value, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber v = InternalSmartEditableNumberField(in expressionContext, rect, label, new EditableNumber((long)value), EditorGUI_Internals.kIntFieldFormatString, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = (int)v.AsLong;
			}
			return value;
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, Rect rect, string label, int value)
		{
			return SmartIntField(in expressionContext, rect, GUIHelper.TempContent(label), value, null);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, Rect rect, int value)
		{
			return SmartIntField(in expressionContext, rect, null, value, null);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
		{
			style = style ?? EditorStyles.numberField;
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style, options);
			return SmartIntField(in expressionContext, rect, label, value, style);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, GUIContent label, int value, params GUILayoutOption[] options)
		{
			return SmartIntField(in expressionContext, label, value, null, options);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, string label, int value, params GUILayoutOption[] options)
		{
			return SmartIntField(in expressionContext, GUIHelper.TempContent(label), value, null, options);
		}

		/// <summary>
		/// Draws a int field that supports Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntField(in FieldExpressionContext expressionContext, int value, params GUILayoutOption[] options)
		{
			return SmartIntField(in expressionContext, null, value, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber r = InternalSmartEditableUnitNumberField(in expressionContext, rect, label, new EditableNumber(value), UnitFieldFormatStringDecimal, baseUnitInfo, displayUnitInfo, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = r.AsDecimal;
			}
			return value;
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, Rect rect, string label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in expressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, Rect rect, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in expressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return SmartDecimalUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in expressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, string label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in expressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal SmartDecimalUnitField(in FieldExpressionContext expressionContext, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in expressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(Rect rect, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(Rect rect, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(Rect rect, string label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(Rect rect, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, style, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(string label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a decimal field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static decimal DecimalUnitField(decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDecimalUnitField(in defaultExpressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber r = InternalSmartEditableUnitNumberField(in expressionContext, rect, label, new EditableNumber(value), UnitFieldFormatStringDouble, baseUnitInfo, displayUnitInfo, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = r.AsDouble;
			}
			return value;
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, Rect rect, string label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in expressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, Rect rect, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in expressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return SmartDoubleUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in expressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, string label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in expressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double SmartDoubleUnitField(in FieldExpressionContext expressionContext, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in expressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber r = InternalSmartEditableUnitNumberField(in expressionContext, rect, label, new EditableNumber(value), UnitFieldFormatStringFloat, baseUnitInfo, displayUnitInfo, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = r.AsFloat;
			}
			return value;
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, Rect rect, string label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in expressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, Rect rect, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in expressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return SmartFloatUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in expressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, string label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in expressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float SmartFloatUnitField(in FieldExpressionContext expressionContext, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in expressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(Rect rect, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			return SmartFloatUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(Rect rect, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(Rect rect, string label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in defaultExpressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(Rect rect, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartFloatUnitField(in defaultExpressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, style, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(string label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in defaultExpressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a float field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static float FloatUnitField(float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartFloatUnitField(in defaultExpressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(Rect rect, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(Rect rect, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(Rect rect, string label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(Rect rect, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, style, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(string label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a double field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static double DoubleUnitField(double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartDoubleUnitField(in defaultExpressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber r = InternalSmartEditableUnitNumberField(in expressionContext, rect, label, new EditableNumber(value), UnitFieldFormatStringInteger, baseUnitInfo, displayUnitInfo, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = r.AsLong;
			}
			return value;
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, Rect rect, string label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in expressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, Rect rect, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in expressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return SmartLongUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in expressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, string label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in expressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		public static long SmartLongUnitField(in FieldExpressionContext expressionContext, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in expressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(Rect rect, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			return SmartLongUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(Rect rect, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(Rect rect, string label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in defaultExpressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(Rect rect, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartLongUnitField(in defaultExpressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, style, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(string label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in defaultExpressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a long field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static long LongUnitField(long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartLongUnitField(in defaultExpressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			EditorGUI.BeginChangeCheck();
			EditableNumber r = InternalSmartEditableUnitNumberField(in expressionContext, rect, label, new EditableNumber(value), UnitFieldFormatStringInteger, baseUnitInfo, displayUnitInfo, style);
			if (EditorGUI.EndChangeCheck())
			{
				value = r.AsInt;
			}
			return value;
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, Rect rect, string label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in expressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, Rect rect, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in expressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return SmartIntUnitField(in expressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in expressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions and Odin expressions.
		/// </summary>
		/// <param name="expressionContext">Context for expression support.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, string label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in expressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		public static int SmartIntUnitField(in FieldExpressionContext expressionContext, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in expressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(Rect rect, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			return SmartIntUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, style);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(Rect rect, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in defaultExpressionContext, rect, label, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(Rect rect, string label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in defaultExpressionContext, rect, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(Rect rect, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SmartIntUnitField(in defaultExpressionContext, rect, null, value, baseUnitInfo, displayUnitInfo, null);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, style, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in defaultExpressionContext, label, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(string label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in defaultExpressionContext, (label != null) ? GUIHelper.TempContent(label) : null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		/// <summary>
		/// Draws a int field that supports unit conversions.
		/// </summary>
		/// <param name="value">Current value.</param>
		/// <param name="baseUnitInfo">UnitInfo of the <c>value</c> and <c>return value</c>. Must have same <c>UnitCategory</c> as <c>displayUnitInfo</c>.</param>
		/// <param name="displayUnitInfo">UnitInfo of the displayed value in the field, converted from <c>baseUnitInfo</c>. Must have same <c>UnitCategory</c> as <c>baseUnitInfo</c>.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		public static int IntUnitField(int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, params GUILayoutOption[] options)
		{
			return SmartIntUnitField(in defaultExpressionContext, null, value, baseUnitInfo, displayUnitInfo, null, options);
		}

		private static EditableNumber InternalSmartEditableUnitNumberField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, EditableNumber value, string formatString, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo, GUIStyle style)
		{
			style = style ?? EditorStyles.numberField;
			EditableNumber.NumberType type = value.Type;
			bool isInteger = value.IsInteger;
			SirenixEditorGUI.BeginShakeableGroup();
			EditorGUI.BeginChangeCheck();
			string error;
			decimal displayValue = UnitNumberUtility.ConvertUnitFromToWithError(value.AsDecimal, baseUnitInfo, displayUnitInfo, out error);
			int controlId;
			bool hasKeyboardFocus;
			Rect valueRect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out controlId, out hasKeyboardFocus);
			EditableNumber slideValue = SlideRectEditableNumber(rect.AlignRight(slideKnobWidth), controlId, new EditableNumber(value.Type, displayValue));
			if (label != null)
			{
				slideValue = SlideRectEditableNumber(rect.SetXMax(valueRect.x), controlId, slideValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				displayValue = slideValue.AsDecimal;
				value = new EditableNumber(type, UnitNumberUtility.ConvertUnitFromToWithError(displayValue, displayUnitInfo, baseUnitInfo, out error));
				GUI.changed = true;
			}
			string vStr = displayValue.ToString(formatString, CultureInfo.InvariantCulture);
			if (InternalSmartNumberTextField(valueRect, controlId, label != null, ref vStr, style))
			{
				long l;
				double d;
				if (vStr.Length == 0)
				{
					GUI.changed = false;
					convertingUnitsControlId = -1;
					unitConvertingNameBuffer = null;
				}
				else if (vStr[0] == '@')
				{
					if (InternalProcessEditableNumberExpression(in expressionContext, value.Type, vStr, out var r))
					{
						value = new EditableNumber(type, UnitNumberUtility.ConvertUnitFromToWithError(r.AsDecimal, displayUnitInfo, baseUnitInfo, out error));
						GUI.changed = true;
					}
				}
				else if (isInteger && InternalExpressionEvaluator.EvaluateLong(vStr, out l))
				{
					value = new EditableNumber(type, UnitNumberUtility.ConvertUnitFromToWithError(l, displayUnitInfo, baseUnitInfo, out error));
					GUI.changed = true;
					convertingUnitsControlId = -1;
					unitConvertingNameBuffer = null;
				}
				else if (!isInteger && InternalExpressionEvaluator.EvaluateDouble(vStr, out d))
				{
					value = new EditableNumber(type, UnitNumberUtility.ConvertUnitFromToWithError((decimal)d, displayUnitInfo, baseUnitInfo, out error));
					GUI.changed = true;
					convertingUnitsControlId = -1;
					unitConvertingNameBuffer = null;
				}
				else
				{
					Match match = inputRegex.Match(vStr);
					string number = match.Groups["value"].Value.Trim();
					string unitSymbol = match.Groups["symbol"].Value.Trim();
					if (decimal.TryParse(number, out var parsedNumber))
					{
						if (UnitNumberUtility.TryMatchUnitInfoBySymbol(unitSymbol, baseUnitInfo.UnitCategory, out var unit))
						{
							value = new EditableNumber(type, UnitNumberUtility.ConvertUnitFromToWithError(parsedNumber, unit, baseUnitInfo, out error));
							convertingUnitsControlId = controlId;
							unitConvertingNameBuffer = unit.Name;
						}
						else
						{
							SirenixEditorGUI.StartShakingGroup();
							convertingUnitsControlId = controlId;
							unitConvertingNameBuffer = "?";
						}
					}
					else
					{
						SirenixEditorGUI.StartShakingGroup();
					}
				}
			}
			if (Event.current.type == EventType.Repaint && (localHotControl != controlId || !smartNumberTextIsDelaying))
			{
				string t = vStr;
				if (EditorGUI_Internals.RecycledEditor_IsEditingControl(controlId))
				{
					t = EditorGUI_Internals.RecycledEditor.text;
				}
				Vector2 size = EditorStyles.label.CalcSize(GUIHelper.TempContent(t));
				if (error != null)
				{
					Rect r2 = valueRect.AddX(size.x + 4f);
					float s = valueRect.height - 4f;
					GUIHelper.PushColor(Color.red);
					SdfIcons.DrawIcon(r2.SetWidth(s).AlignCenterY(s), SdfIconType.XCircleFill);
					GUI.Label(r2.AddX(s), error, SirenixGUIStyles.LeftAlignedWhiteMiniLabel);
					GUIHelper.PopColor();
				}
				else
				{
					string symbol = displayUnitInfo.Symbols[0];
					if (controlId == convertingUnitsControlId)
					{
						symbol = unitConvertingNameBuffer;
					}
					GUI.Label(valueRect.AddX(size.x), " " + symbol, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
				}
			}
			if (controlId == convertingUnitsControlId && !EditorGUIUtility.editingTextField)
			{
				convertingUnitsControlId = -1;
			}
			DrawSlideKnob(valueRect, controlId);
			SirenixEditorGUI.EndShakeableGroup();
			return value;
		}

		private static EditableNumber InternalSmartEditableNumberField(in FieldExpressionContext expressionContext, Rect rect, GUIContent label, EditableNumber value, string formatString, GUIStyle style)
		{
			style = style ?? EditorStyles.numberField;
			EditableNumber.NumberType type = value.Type;
			bool isInteger = value.IsInteger;
			SirenixEditorGUI.BeginShakeableGroup();
			EditorGUI.BeginChangeCheck();
			int slideControl;
			bool hasKeyboardFocus;
			Rect valueRect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out slideControl, out hasKeyboardFocus);
			EditableNumber slideValue = SlideRectEditableNumber(rect.AlignRight(slideKnobWidth), slideControl, value);
			if (label != null)
			{
				slideValue = SlideRectEditableNumber(rect.SetXMax(valueRect.x), slideControl, slideValue);
			}
			if (EditorGUI.EndChangeCheck())
			{
				value = new EditableNumber(type, slideValue);
				GUI.changed = true;
			}
			string vStr = value.ToString(formatString, CultureInfo.InvariantCulture);
			if (InternalSmartNumberTextField(valueRect, slideControl, label != null, ref vStr, style) && vStr.Length > 0)
			{
				long l;
				double d;
				if (vStr[0] == '@')
				{
					if (InternalProcessEditableNumberExpression(in expressionContext, value.Type, vStr, out var r))
					{
						value = r;
						GUI.changed = true;
					}
				}
				else if (isInteger && InternalExpressionEvaluator.EvaluateLong(vStr, out l))
				{
					value = new EditableNumber(type, l);
					GUI.changed = true;
				}
				else if (!isInteger && InternalExpressionEvaluator.EvaluateDouble(vStr, out d))
				{
					value = new EditableNumber(type, d);
					GUI.changed = true;
				}
			}
			DrawSlideKnob(valueRect, slideControl);
			SirenixEditorGUI.EndShakeableGroup();
			return value;
		}

		private static EditableNumber SlideRectEditableNumber(Rect rect, int control, EditableNumber value)
		{
			if (value.IsInteger)
			{
				long l = SirenixEditorGUI.SlideRectLong(rect, control, value.AsLong);
				return new EditableNumber(value.Type, l);
			}
			double d = SirenixEditorGUI.SlideRectDouble(rect, control, value.AsDouble);
			return new EditableNumber(value.Type, d);
		}

		private static bool InternalSmartNumberTextField(Rect rect, int controlId, bool hasLabel, ref string value, GUIStyle style)
		{
			bool applyDelayed = false;
			string buffer = value;
			if (Event.current.type == EventType.Layout && localHotControl == controlId && !EditorGUIUtility.editingTextField)
			{
				localHotControl = -1;
				expressionHistory.ReleaseControlId(controlId);
			}
			if (localHotControl == controlId && smartNumberTextIsDelaying)
			{
				if (OnLocalControlRelease(rect, controlId))
				{
					value = delayedTextBuffer;
					buffer = delayedTextBuffer;
					applyDelayed = true;
					expressionHistory.Apply(controlId, value);
				}
				else
				{
					GUIHelper.PushColor(delayedActiveColor);
					buffer = delayedTextBuffer;
				}
				if (Event.current.type == EventType.KeyDown)
				{
					string historyText = null;
					if (Event.current.keyCode == KeyCode.UpArrow)
					{
						historyText = expressionHistory.GetPrevious(controlId, buffer);
					}
					else if (Event.current.keyCode == KeyCode.DownArrow)
					{
						historyText = expressionHistory.GetNext(controlId, buffer);
					}
					if (historyText != null)
					{
						TextEditor editor = EditorGUI_Internals.RecycledEditor;
						if (editor != null)
						{
							editor.text = historyText;
							editor.cursorIndex = historyText.Length;
							editor.selectIndex = historyText.Length;
						}
						buffer = historyText;
						delayedTextBuffer = historyText;
						Event.current.Use();
					}
				}
			}
			string allowedCharacters = null;
			string v = EditorGUI_Internals.DoTextField(controlId, rect, value, style, allowedCharacters, out var changed, reset: false, multiline: false, passwordField: false);
			if (changed)
			{
				localHotControl = controlId;
			}
			if (localHotControl == controlId && smartNumberTextIsDelaying)
			{
				GUIHelper.PopColor();
			}
			if (applyDelayed)
			{
				changed = (GUI.changed = value.Length > 0);
				localHotControl = 0;
				smartNumberTextIsDelaying = false;
				return changed;
			}
			if (changed)
			{
				v = v.Trim();
				if (v.Length == 0 || v[0] != '@')
				{
					if (localHotControl == controlId)
					{
						smartNumberTextIsDelaying = false;
					}
					value = v;
					smartNumberTextIsDelaying = false;
					GUI.changed = true;
					return true;
				}
				smartNumberTextIsDelaying = true;
				delayedTextBuffer = v;
				GUI.changed = false;
				return false;
			}
			return false;
		}

		private static bool InternalProcessEditableNumberExpression(in FieldExpressionContext expressionContext, EditableNumber.NumberType type, string expression, out EditableNumber result)
		{
			if (expressionContext.Type == null)
			{
				throw new ArgumentException("Type");
			}
			if (!expressionContext.IsStatic && expressionContext.Instance == null)
			{
				throw new ArgumentException("Must have instance object for non-static expression context.", "Instance");
			}
			if (string.IsNullOrEmpty(expression) || expression.Length <= 1)
			{
				throw new ArgumentException("expression");
			}
			fieldEmitContext.IsStatic = expressionContext.IsStatic;
			fieldEmitContext.Type = expressionContext.Type;
			fieldEmitContext.ReturnType = null;
			if (fieldEmitContext.Parameters == null || fieldEmitContext.Parameters.Length != 0)
			{
				fieldEmitContext.Parameters = Type.EmptyTypes;
				fieldEmitContext.ParameterNames = Array.Empty<string>();
			}
			string error;
			Delegate func = ExpressionUtility.ParseExpression(expression.Substring(1), fieldEmitContext, out error);
			if (!string.IsNullOrWhiteSpace(error))
			{
				UnityEngine.Debug.LogError(error);
				result = default(EditableNumber);
				return false;
			}
			object r = ((!expressionContext.IsStatic) ? func.DynamicInvoke(expressionContext.Instance) : func.DynamicInvoke());
			switch (type)
			{
			case EditableNumber.NumberType.Decimal:
				result = new EditableNumber(ConvertUtility.Convert<decimal>(r));
				break;
			case EditableNumber.NumberType.Double:
				result = new EditableNumber(ConvertUtility.Convert<double>(r));
				break;
			case EditableNumber.NumberType.Long:
				result = new EditableNumber(ConvertUtility.Convert<long>(r));
				break;
			default:
				throw new Exception("IMPOSSIBLE!");
			}
			return true;
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <returns>Value assigned to the field.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		public static Enum EnumMaskDropdown(Rect rect, GUIContent label, Enum selected, GUIStyle style)
		{
			return EnumDropdown(rect, label, selected, style);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <returns>Value assigned to the field.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		public static Enum EnumMaskDropdown(Rect rect, GUIContent label, Enum selected)
		{
			return EnumMaskDropdown(rect, label, selected, null);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <returns>Value assigned to the field.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		public static Enum EnumMaskDropdown(Rect rect, string label, Enum selected)
		{
			return EnumMaskDropdown(rect, (label != null) ? GUIHelper.TempContent(label) : null, selected, null);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="rect">Position and size of the field.</param>
		/// <param name="selected">Current selection.</param>
		/// <returns>Value assigned to the field.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		public static Enum EnumMaskDropdown(Rect rect, Enum selected)
		{
			return EnumMaskDropdown(rect, null, selected, null);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="style">GUIStyle for drawing the field. Set to <c>null</c> for default.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Enum EnumMaskDropdown(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, EditorGUIUtility.singleLineHeight, style ?? EditorStyles.numberField, options);
			return EnumMaskDropdown(rect, label, selected, style);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Enum EnumMaskDropdown(GUIContent label, Enum selected, params GUILayoutOption[] options)
		{
			return EnumMaskDropdown(label, selected, null, options);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="label">Label of field. Set to <c>null</c> for no label.</param>
		/// <param name="selected">Current selection.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Enum EnumMaskDropdown(string label, Enum selected, params GUILayoutOption[] options)
		{
			return EnumMaskDropdown((label != null) ? GUIHelper.TempContent(label) : null, selected, null, options);
		}

		/// <summary>
		/// Draws a dropdown field for enum masks.
		/// </summary>
		/// <param name="selected">Current selection.</param>
		/// <param name="options">Layout options.</param>
		/// <returns>Value assigned to the field.</returns>
		[Obsolete("EnumDropdown now supports enum masks as well. Use EnumDropdown() instead", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Enum EnumMaskDropdown(Enum selected, params GUILayoutOption[] options)
		{
			return EnumMaskDropdown(null, selected, null, options);
		}

		internal static bool OnLocalControlRelease(Rect rect, int controlID)
		{
			if (localHotControl != 0 && localHotControl == controlID && (Event.current.rawType == EventType.MouseUp || (Event.current.rawType == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) || (Event.current.rawType == EventType.MouseDown && Event.current.button == 1) || (Event.current.rawType == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))))
			{
				localHotControl = 0;
				return true;
			}
			return false;
		}

		private static void DrawSlideKnob(Rect rect, int id)
		{
			EventType e = Event.current.type;
			if (e != EventType.Layout)
			{
				bool show = GUIUtility.hotControl == id || rect.Contains(Event.current.mousePosition);
				if (show && Event.current.type == EventType.MouseMove)
				{
					GUIHelper.RequestRepaint();
				}
				if (Event.current.type == EventType.Repaint && show)
				{
					Rect slideKnob = rect.AlignRight(slideKnobWidth - 2);
					slideKnob.y = slideKnob.center.y;
					slideKnob.y -= 9f;
					slideKnob.x -= 2f;
					slideKnob.height = 18f;
					int s = slideKnobWidth / 2;
					GUIHelper.PushColor((GUIUtility.hotControl == id || slideKnob.Contains(Event.current.mousePosition)) ? Color.white : new Color(1f, 1f, 1f, 0.35f));
					GUI.DrawTexture(slideKnob.AlignLeft(s), EditorIcons.TriangleLeft.Active);
					GUI.DrawTexture(slideKnob.AlignRight(s), EditorIcons.TriangleRight.Active);
					GUIHelper.PopColor();
				}
			}
		}

		private static void EnumMaskSetValueDelegateSigned(object value)
		{
			MaskMenu.EnumChanged = true;
			MaskMenu.ChangedMaskValueSigned = (long)value;
			EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("SirenixMaskMenuChanged"));
		}

		private static void EnumMaskSetValueDelegateUnsigned(object value)
		{
			MaskMenu.EnumChanged = true;
			MaskMenu.ChangedMaskValueUnsigned = (ulong)value;
			EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("SirenixMaskMenuChanged"));
		}

		private static Enum GetCurrentMaskValue(int controlId, Type enumType, Enum selected, bool signed)
		{
			Event current = Event.current;
			if (current.type == EventType.ExecuteCommand && current.commandName == "SirenixMaskMenuChanged" && controlId == MaskMenu.CurrentEnumControlID && MaskMenu.EnumChanged)
			{
				if (signed)
				{
					long value = Convert.ToInt64(selected, CultureInfo.InvariantCulture);
					value = ((MaskMenu.ChangedMaskValueSigned == 0L) ? 0 : (((MaskMenu.ChangedMaskValueSigned & value) != MaskMenu.ChangedMaskValueSigned) ? (value | MaskMenu.ChangedMaskValueSigned) : (value & ~MaskMenu.ChangedMaskValueSigned)));
					selected = (Enum)Enum.ToObject(enumType, value);
				}
				else
				{
					ulong value2 = Convert.ToUInt64(selected, CultureInfo.InvariantCulture);
					value2 = ((MaskMenu.ChangedMaskValueUnsigned == 0L) ? 0 : (((MaskMenu.ChangedMaskValueUnsigned & value2) != MaskMenu.ChangedMaskValueUnsigned) ? (value2 | MaskMenu.ChangedMaskValueUnsigned) : (value2 & ~MaskMenu.ChangedMaskValueUnsigned)));
					selected = (Enum)Enum.ToObject(enumType, value2);
				}
				GUI.changed = true;
				current.Use();
			}
			return selected;
		}
	}
}
