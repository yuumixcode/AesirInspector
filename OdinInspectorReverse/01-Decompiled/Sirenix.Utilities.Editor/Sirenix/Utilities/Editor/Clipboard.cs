using System;
using System.IO;
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Serialization.Utilities;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Functions for accessing the clipboard.
	/// </summary>
	public static class Clipboard
	{
		private static object obj;

		private static CopyModes copyMode;

		/// <summary>
		/// Gets the current copy mode.
		/// </summary>
		public static CopyModes CurrentCopyMode => copyMode;

		/// <summary>
		/// Copies the specified object.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj">The object.</param>
		/// <param name="copyMode">The copy mode.</param>
		public static void Copy<T>(T obj, CopyModes copyMode)
		{
			if (obj == null)
			{
				return;
			}
			Clipboard.copyMode = copyMode;
			Type type = obj.GetType();
			if (type == typeof(string) || (bool)(obj as UnityEngine.Object) || type.IsValueType || type.IsEnum)
			{
				copyMode = CopyModes.CopyReference;
				Clipboard.obj = obj;
			}
			else
			{
				switch (copyMode)
				{
				case CopyModes.DeepCopy:
				{
					using (MemoryStream stream = new MemoryStream())
					{
						using Cache<SerializationContext> serializationContext = Cache<SerializationContext>.Claim();
						using Cache<DeserializationContext> deserializationContext = Cache<DeserializationContext>.Claim();
						serializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
						deserializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
						SerializationUtility.SerializeValue(obj, stream, DataFormat.Binary, out var unityReferences, serializationContext);
						stream.Position = 0L;
						Clipboard.obj = SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences, deserializationContext);
					}
					break;
				}
				case CopyModes.ShallowCopy:
					if (obj.GetType().IsArray)
					{
						Array oldArray = (Array)(object)obj;
						Array newArray;
						if (oldArray.Rank > 1)
						{
							long[] lengths = new long[oldArray.Rank];
							for (int i = 0; i < lengths.Length; i++)
							{
								lengths[i] = oldArray.GetLongLength(i);
							}
							newArray = Array.CreateInstance(oldArray.GetType().GetElementType(), lengths);
						}
						else
						{
							newArray = Array.CreateInstance(oldArray.GetType().GetElementType(), oldArray.LongLength);
						}
						Array.Copy(oldArray, newArray, 0);
						Clipboard.obj = newArray;
					}
					else
					{
						Clipboard.obj = obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(obj, null);
					}
					break;
				default:
					Clipboard.obj = obj;
					break;
				}
			}
			if (obj is string)
			{
				GUIUtility.systemCopyBuffer = obj as string;
			}
			else
			{
				GUIUtility.systemCopyBuffer = null;
			}
		}

		/// <summary>
		/// Copies the specified object.
		/// </summary>
		public static void Copy<T>(T obj)
		{
			Copy(obj, CopyModes.DeepCopy);
		}

		/// <summary>
		/// Clears this instance.
		/// </summary>
		public static void Clear()
		{
			GUIUtility.systemCopyBuffer = null;
			obj = null;
		}

		/// <summary>
		/// Determines whether this instance can paste the specified type.
		/// </summary>
		public static bool CanPaste(Type type)
		{
			bool isNullSystemBuffer = string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
			if (type == typeof(string) && !isNullSystemBuffer)
			{
				return true;
			}
			if (isNullSystemBuffer && obj != null && obj.GetType().InheritsFrom(type))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether this instance can paste the specified type.
		/// </summary>
		public static bool CanPaste<T>()
		{
			return CanPaste(typeof(T));
		}

		/// <summary>
		/// Determines whether or not the Clipboard contains any instance.
		/// </summary>
		public static bool IsEmpty()
		{
			if (string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
			{
				return obj == null;
			}
			return false;
		}

		/// <summary>
		/// Tries the paste.
		/// </summary>
		public static bool TryPaste<T>(out T value)
		{
			if (!CanPaste<T>())
			{
				value = default(T);
				return false;
			}
			value = (T)Paste();
			return true;
		}

		/// <summary>
		/// Copies or gets the current object in the clipboard.
		/// </summary>
		public static T Paste<T>()
		{
			return (T)Paste();
		}

		/// <summary>
		/// Copies or gets the current object in the clipboard.
		/// </summary>
		public static object Paste()
		{
			if (IsEmpty())
			{
				throw new Exception("No object in clipboard. Check CanPaste() before calling Paste().");
			}
			if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
			{
				return GUIUtility.systemCopyBuffer;
			}
			if (copyMode == CopyModes.CopyReference)
			{
				return obj;
			}
			if (copyMode == CopyModes.DeepCopy)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using Cache<SerializationContext> serializationContext = Cache<SerializationContext>.Claim();
					using Cache<DeserializationContext> deserializationContext = Cache<DeserializationContext>.Claim();
					serializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
					deserializationContext.Value.Config.SerializationPolicy = SerializationPolicies.Everything;
					SerializationUtility.SerializeValue(obj, stream, DataFormat.Binary, out var unityReferences, serializationContext);
					stream.Position = 0L;
					return SerializationUtility.DeserializeValue<object>(stream, DataFormat.Binary, unityReferences, deserializationContext);
				}
			}
			if (copyMode == CopyModes.ShallowCopy)
			{
				return obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Invoke(obj, null);
			}
			return null;
		}
	}
}
