using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Delegate property drawer. This drawer is rather simplistic for now, and will receive significant upgrades in the future.
	/// </summary>
	[DrawerPriority(0.51, 0.0, 0.0)]
	public class DelegateDrawer<T> : OdinValueDrawer<T> where T : class
	{
		private static MethodInfo invokeMethodField;

		private static bool gotInvokeMethod;

		private UnityEngine.Object contextObj;

		private static MethodInfo InvokeMethod
		{
			get
			{
				if (!gotInvokeMethod)
				{
					invokeMethodField = typeof(T).GetMethod("Invoke");
					gotInvokeMethod = true;
				}
				return invokeMethodField;
			}
		}

		/// <summary>
		/// See <see cref="M:Sirenix.OdinInspector.Editor.OdinDrawer.CanDrawTypeFilter(System.Type)" />.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			if (!type.IsAbstract && typeof(Delegate).IsAssignableFrom(type))
			{
				return InvokeMethod != null;
			}
			return false;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			Delegate del = (Delegate)(object)entry.SmartValue;
			GUIContent content = GUIHelper.TempContent((string)null);
			bool conflict = false;
			bool targetConflict = false;
			bool isNull = false;
			bool anyMethodsNull = false;
			bool anyMethodsNotNull = false;
			for (int i = 0; i < entry.ValueCount; i++)
			{
				Delegate del2 = (Delegate)(object)entry.Values[i];
				if ((object)del2 != null && del2.Method == null)
				{
					anyMethodsNull = true;
				}
				else
				{
					anyMethodsNotNull = true;
				}
			}
			if (entry.ValueState == PropertyValueState.NullReference || (anyMethodsNull && !anyMethodsNotNull))
			{
				conflict = true;
				isNull = true;
				content.text = "Null";
			}
			else if (entry.ValueState == PropertyValueState.ReferenceValueConflict || anyMethodsNull)
			{
				conflict = true;
				content.text = "Multiselection Value Conflict";
			}
			else
			{
				MethodInfo method = del.Method;
				object target = del.Target;
				for (int j = 1; j < entry.ValueCount; j++)
				{
					Delegate otherDel = (Delegate)(object)entry.Values[j];
					if (otherDel.Method != method)
					{
						conflict = true;
					}
					if (otherDel.Target != target)
					{
						targetConflict = true;
					}
				}
				if (conflict)
				{
					content.text = "Multiselection Method Conflict";
				}
				else
				{
					content.text = method.GetFullName();
					if (method.IsStatic)
					{
						content.text = "static " + content.text;
					}
				}
			}
			if (isNull)
			{
				content.text = typeof(T).GetNiceName();
			}
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			rect = ((label == null) ? EditorGUI.IndentedRect(rect) : EditorGUI.PrefixLabel(rect, label));
			UnityEngine.Object obj = (((object)del == null) ? null : (del.Target as UnityEngine.Object));
			if (obj == null)
			{
				obj = contextObj;
			}
			rect.width *= 0.5f;
			if (GUI.Button(rect, content, EditorStyles.popup))
			{
				Popup(entry, rect, obj);
			}
			bool previousMixedValue = EditorGUI.showMixedValue;
			if (targetConflict)
			{
				EditorGUI.showMixedValue = true;
			}
			rect.x += rect.width;
			EditorGUI.BeginChangeCheck();
			UnityEngine.Object newTarget = EditorGUI.ObjectField(rect, obj, typeof(UnityEngine.Object), allowSceneObjects: true);
			bool changed = EditorGUI.EndChangeCheck();
			if ((object)del != null && Event.current.type == EventType.Repaint && obj != null)
			{
				MethodInfo method2 = del.Method;
				string labelName = (targetConflict ? "Target conflict" : ((!(obj is Component)) ? obj.GetType().GetNiceName() : (obj as Component).gameObject.name));
				GUIContent text = new GUIContent(labelName, AssetPreview.GetMiniThumbnail(obj));
				GUI.Label(rect, text, EditorStyles.objectField);
			}
			else if (obj == null)
			{
				GUIContent text2 = new GUIContent("None", AssetPreview.GetMiniThumbnail(obj));
				GUI.Label(rect, text2, EditorStyles.objectField);
			}
			if (newTarget != obj && changed)
			{
				for (int k = 0; k < entry.ValueCount; k++)
				{
					entry.Values[k] = null;
				}
				contextObj = newTarget;
			}
			if (targetConflict)
			{
				EditorGUI.showMixedValue = previousMixedValue;
			}
		}

		private void Popup(IPropertyValueEntry<T> entry, Rect rect, UnityEngine.Object target)
		{
			Type returnType = InvokeMethod.ReturnType;
			Type[] parameters = (from n in InvokeMethod.GetParameters()
				select n.ParameterType).ToArray();
			GenericMenu menu = new GenericMenu();
			if (target == null)
			{
				menu.AddDisabledItem(new GUIContent("No target selected"));
			}
			else
			{
				GameObject targetGameObject = target as GameObject;
				Component targetComponent = target as Component;
				if (targetGameObject == null && targetComponent != null)
				{
					targetGameObject = targetComponent.gameObject;
				}
				if (targetGameObject != null)
				{
					RegisterGameObject(menu, entry, "", targetGameObject, returnType, parameters);
				}
				else
				{
					RegisterUnityObject(menu, entry, "", target, returnType, parameters);
				}
				if (menu.GetItemCount() == 0)
				{
					menu.AddDisabledItem(new GUIContent("No suitable method found on target"));
				}
			}
			menu.DropDown(rect);
		}

		private void RegisterGameObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, GameObject go, Type returnType, Type[] parameters)
		{
			RegisterUnityObject(menu, entry, path + "/GameObject", go, returnType, parameters);
			Component[] components = go.GetComponents<Component>();
			foreach (Component component in components)
			{
				RegisterUnityObject(menu, entry, path + "/" + component.GetType().GetNiceName(), component, returnType, parameters);
			}
		}

		private void RegisterUnityObject(GenericMenu menu, IPropertyValueEntry<T> entry, string path, UnityEngine.Object obj, Type returnType, Type[] parameters)
		{
			MethodInfo[] methods = obj.GetType().GetAllMembers<MethodInfo>(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate(MethodInfo n)
			{
				if (n.ReturnType != returnType)
				{
					return false;
				}
				ParameterInfo[] parameters2 = n.GetParameters();
				if (parameters2.Length != parameters.Length)
				{
					return false;
				}
				for (int i = 0; i < parameters2.Length; i++)
				{
					if (parameters2[i].ParameterType != parameters[i])
					{
						return false;
					}
				}
				return true;
			})
				.ToArray();
			MethodInfo[] array = methods;
			foreach (MethodInfo method in array)
			{
				string name = method.GetFullName();
				MethodInfo closureMethod = method;
				if (method.DeclaringType != obj.GetType())
				{
					name = method.DeclaringType.GetNiceName() + "/" + name;
				}
				if (method.IsStatic)
				{
					name += " (static)";
				}
				GenericMenu.MenuFunction func = delegate
				{
					entry.Property.Tree.DelayActionUntilRepaint(delegate
					{
						Delegate obj2 = ((!closureMethod.IsStatic) ? Delegate.CreateDelegate(typeof(T), obj, closureMethod) : Delegate.CreateDelegate(typeof(T), null, closureMethod));
						for (int i = 0; i < entry.ValueCount; i++)
						{
							entry.Values[i] = (T)(object)obj2;
						}
						contextObj = null;
					});
				};
				menu.AddItem(new GUIContent((path + "/" + name).TrimStart(new char[1] { '/' })), on: false, func);
			}
		}
	}
}
