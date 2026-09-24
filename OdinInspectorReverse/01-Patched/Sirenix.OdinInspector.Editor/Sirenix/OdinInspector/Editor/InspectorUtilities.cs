using System;
using System.ComponentModel;
using System.Text;
using Sirenix.Serialization.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Provides a variety of miscellaneous utilities widely used in the inspector.
	/// </summary>
	public static class InspectorUtilities
	{
		/// <summary>
		/// Converts an Odin property path to a deep reflection path.
		/// </summary>
		public static string ConvertToDeepReflectionPath(string odinPropertyPath)
		{
			return ConvertOdinPath(odinPropertyPath, isUnity: false);
		}

		/// <summary>
		/// Converts an Odin property path (without groups included) into a Unity property path.
		/// </summary>
		public static string ConvertToUnityPropertyPath(string odinPropertyPath)
		{
			return ConvertOdinPath(odinPropertyPath, isUnity: true);
		}

		private static string ConvertOdinPath(string odinPropertyPath, bool isUnity)
		{
			bool hasSpecialCharacters = false;
			for (int i = 0; i < odinPropertyPath.Length; i++)
			{
				if (odinPropertyPath[i] == '$' || odinPropertyPath[i] == '#')
				{
					hasSpecialCharacters = true;
					break;
				}
			}
			if (hasSpecialCharacters)
			{
				using (Cache<StringBuilder> sbCache = Cache<StringBuilder>.Claim())
				{
					StringBuilder sb = sbCache.Value;
					sb.Length = 0;
					bool skipUntilNextDot = false;
					for (int j = 0; j < odinPropertyPath.Length; j++)
					{
						char c = odinPropertyPath[j];
						if (c == '.')
						{
							skipUntilNextDot = false;
						}
						else if (skipUntilNextDot)
						{
							continue;
						}
						switch (c)
						{
						case '$':
							sb.Append(isUnity ? "Array.data[" : "[");
							for (j++; j < odinPropertyPath.Length && char.IsNumber(odinPropertyPath[j]); j++)
							{
								sb.Append(odinPropertyPath[j]);
							}
							sb.Append(']');
							j--;
							break;
						case '#':
							skipUntilNextDot = true;
							break;
						case '.':
							if (sb.Length > 0 && sb[sb.Length - 1] != '.')
							{
								sb.Append('.');
							}
							break;
						default:
							sb.Append(c);
							break;
						}
					}
					while (sb.Length > 0 && sb[0] == '.')
					{
						sb.Remove(0, 1);
					}
					while (sb.Length > 0 && sb[sb.Length - 1] == '.')
					{
						sb.Remove(sb.Length - 1, 1);
					}
					return sb.ToString();
				}
			}
			return odinPropertyPath;
		}

		/// <summary>
		/// Prepares a property tree for drawing, and handles management of undo, as well as marking scenes and drawn assets dirty.
		/// </summary>
		/// <param name="tree">The tree to be drawn.</param>
		/// <param name="withUndo">Whether to register undo commands for the changes made to the tree. This can only be set to true if the tree has a <see cref="T:UnityEditor.SerializedObject" /> to represent.</param>
		/// <exception cref="T:System.ArgumentNullException">tree is null</exception>
		[Obsolete("Use PropertyTree.BeginDraw instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void BeginDrawPropertyTree(PropertyTree tree, bool withUndo)
		{
			tree.BeginDraw(withUndo);
		}

		/// <summary>
		/// Ends drawing a property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.
		/// </summary>
		/// <param name="tree">The tree.</param>
		[Obsolete("Use PropertyTree.EndDraw instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void EndDrawPropertyTree(PropertyTree tree)
		{
			tree.EndDraw();
		}

		public static void RegisterUnityObjectDirty(UnityEngine.Object unityObj)
		{
			if (AssetDatabase.Contains(unityObj))
			{
				EditorUtility.SetDirty(unityObj);
			}
			else
			{
				if (Application.isPlaying)
				{
					return;
				}
				if (unityObj is UnityEngine.Component component)
				{
					if (!component.gameObject.scene.isDirty)
					{
						EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
					}
				}
				else if (unityObj is GameObject { scene: var scene } gameObject)
				{
					if (!scene.isDirty)
					{
						EditorSceneManager.MarkSceneDirty(gameObject.scene);
					}
				}
				else if (unityObj is EditorWindow || unityObj is ScriptableObject)
				{
					EditorUtility.SetDirty(unityObj);
				}
				else
				{
					EditorUtility.SetDirty(unityObj);
				}
			}
		}

		/// <summary>
		/// Draws all properties in a given property tree; must be wrapped by a <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.BeginDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree,System.Boolean)" /> and <see cref="M:Sirenix.OdinInspector.Editor.InspectorUtilities.EndDrawPropertyTree(Sirenix.OdinInspector.Editor.PropertyTree)" />.
		/// </summary>
		/// <param name="tree">The tree to be drawn.</param>
		public static void DrawPropertiesInTree(PropertyTree tree)
		{
			tree.DrawProperties();
		}

		/// <summary>
		/// Draws a property in the inspector using a given label.
		/// </summary>
		[Obsolete("Use InspectorProperty.Draw(label) instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void DrawProperty(InspectorProperty property, GUIContent label)
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			property.Draw(label);
		}

		public static FieldExpressionContext ToFieldExpressionContext(this InspectorProperty property)
		{
			InspectorProperty parent = property.ParentValueProperty;
			while (parent != null && parent.ChildResolver is ICollectionResolver)
			{
				parent = parent.ParentValueProperty;
			}
			if (parent == null)
			{
				parent = property.Tree.RootProperty;
			}
			if (parent == parent.Tree.RootProperty && parent.Tree.IsStatic)
			{
				return FieldExpressionContext.StaticContext(parent.ValueEntry.TypeOfValue);
			}
			return FieldExpressionContext.InstanceContext(parent.ValueEntry.WeakSmartValue);
		}
	}
}
