using System;
using System.Reflection;
using System.Text;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	/// <summary>
	/// <para>An ActionResolver resolves a string to an action, given an InspectorProperty instance to use as context. Call <see cref="M:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolver.Get(Sirenix.OdinInspector.Editor.InspectorProperty,System.String)" /> to get an instance of an ActionResolver.</para>
	/// <para>Action resolvers are a globally extendable system that can be hooked into and modified or changed by creating and registering an <see cref="T:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverCreator" />.</para>
	/// <para>See Odin's tutorials for details and examples of how to use ActionResolvers.</para>
	/// </summary>
	public sealed class ActionResolver
	{
		private static readonly StringBuilder SB = new StringBuilder();

		/// <summary>
		/// The context of this ActionResolver, containing all of its configurations and values it needs to function. For performance and simplicity reasons, this is a single very large struct that is passed around by ref to anything that needs it.
		/// </summary>
		public ActionResolverContext Context;

		/// <summary>
		/// The delegate that executes the actual action. You should not call this manually, but instead call <see cref="M:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolver.DoAction(System.Int32)" />.
		/// </summary>
		public ResolvedAction Action;

		/// <summary>
		/// The current error message that the resolver has, or null if there is no error message. This is a shortcut for writing "resolver.Context.ErrorMessage".
		/// </summary>
		public string ErrorMessage => Context.ErrorMessage;

		/// <summary>
		/// Whether there is an error message at the moment. This is a shortcut for writing "resolver.Context.ErrorMessage != null".
		/// </summary>
		public bool HasError => Context.ErrorMessage != null;

		/// <summary>
		/// Draws an error message box if there is an error, and does nothing if there is no error.
		/// </summary>
		public void DrawError()
		{
			if (HasError)
			{
				SirenixEditorGUI.MessageBox(ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
		}

		/// <summary>
		/// Executes the resolved action for a given selection index.
		/// </summary>
		/// <param name="selectionIndex">The selection index to execute the action on. Defaults to 0.</param>
		public void DoAction(int selectionIndex = 0)
		{
			if (selectionIndex < 0 || selectionIndex >= Context.Property.ParentValues.Count)
			{
				throw new IndexOutOfRangeException();
			}
			Context.NamedValues.UpdateValues(ref Context, selectionIndex);
			try
			{
				Action(ref Context, selectionIndex);
				if (Context.ErrorMessage != null && Context.ErrorMessageIsDueToException)
				{
					Context.ErrorMessage = null;
					Context.ErrorMessageIsDueToException = false;
				}
			}
			catch (Exception innerException)
			{
				if (Event.current != null && innerException.IsExitGUIException())
				{
					throw innerException.AsExitGUIException();
				}
				while (innerException is TargetInvocationException)
				{
					innerException = innerException.InnerException;
				}
				Context.ErrorMessage = "Action execution for '" + Context.ResolvedString + "' threw an exception: " + innerException.Message + "\n\n" + innerException.StackTrace;
				Context.ErrorMessageIsDueToException = true;
				if (Context.LogExceptions)
				{
					Debug.LogException(innerException);
				}
			}
		}

		/// <summary>
		/// Executes the action for all selection indices.
		/// </summary>
		public void DoActionForAllSelectionIndices()
		{
			int count = Context.Property.ParentValues.Count;
			for (int i = 0; i < count; i++)
			{
				DoAction(i);
			}
		}

		/// <summary>
		/// Creates a new action resolver instance from a pre-built context struct. This is a more advanced use that requires you to
		/// know how the context needs to be set up before action resolution happens. However, this allows you to do more advanced
		/// things like adjust various context values before string resolution happens.
		/// </summary>
		/// <param name="context">The pre-built context that should be used to get a resolver.</param>
		public static ActionResolver GetFromContext(ref ActionResolverContext context)
		{
			return ActionResolverCreator.GetResolverFromContext(ref context);
		}

		/// <summary>
		/// Creates a new action resolver instance for a given string.
		/// </summary>
		/// <param name="property">The property that is the context for the resolution to happen in.</param>
		/// <param name="resolvedString">The string that should be resolved to an action.</param>
		public static ActionResolver Get(InspectorProperty property, string resolvedString)
		{
			return ActionResolverCreator.GetResolver(property, resolvedString);
		}

		/// <summary>
		/// Creates a new action resolver instance for a given string.
		/// </summary>
		/// <param name="property">The property that is the context for the resolution to happen in.</param>
		/// <param name="resolvedString">The string that should be resolved to an action.</param>
		/// <param name="namedArgs">The extra named args that this resolver has access to. Passing in a named arg that already exists will silently override the pre-existing named arg.</param>
		public static ActionResolver Get(InspectorProperty property, string resolvedString, params NamedValue[] namedArgs)
		{
			return ActionResolverCreator.GetResolver(property, resolvedString, namedArgs);
		}

		/// <summary>
		/// Gets a nicely formatted string that lists all the errors in the given set of action resolvers. The returned value is null if there are no errors.
		/// </summary>
		public static string GetCombinedErrors(ActionResolver r1 = null, ActionResolver r2 = null, ActionResolver r3 = null, ActionResolver r4 = null, ActionResolver r5 = null, ActionResolver r6 = null, ActionResolver r7 = null, ActionResolver r8 = null)
		{
			return GetCombinedErrors(r1, r2, r3, r4, r5, r6, r7, r8, (ActionResolver[])null);
		}

		/// <summary>
		/// Gets a nicely formatted string that lists all the errors in the given set of action resolvers. The returned value is null if there are no errors.
		/// </summary>
		public static string GetCombinedErrors(ActionResolver r1, ActionResolver r2, ActionResolver r3, ActionResolver r4, ActionResolver r5, ActionResolver r6, ActionResolver r7, ActionResolver r8, params ActionResolver[] remainder)
		{
			SB.Length = 0;
			if (r1 != null && r1.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r1.ErrorMessage);
			}
			if (r2 != null && r2.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r2.ErrorMessage);
			}
			if (r3 != null && r3.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r3.ErrorMessage);
			}
			if (r4 != null && r4.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r4.ErrorMessage);
			}
			if (r5 != null && r5.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r5.ErrorMessage);
			}
			if (r6 != null && r6.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r6.ErrorMessage);
			}
			if (r7 != null && r7.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r7.ErrorMessage);
			}
			if (r8 != null && r8.ErrorMessage != null)
			{
				if (SB.Length > 0)
				{
					SB.AppendLine();
					SB.AppendLine();
					SB.AppendLine("And,");
					SB.AppendLine();
				}
				SB.Append(r8.ErrorMessage);
			}
			if (remainder != null)
			{
				for (int i = 0; i < remainder.Length; i++)
				{
					if (remainder[i] != null && remainder[i].HasError)
					{
						if (SB.Length > 0)
						{
							SB.AppendLine();
							SB.AppendLine();
							SB.AppendLine("And,");
							SB.AppendLine();
						}
						SB.Append(remainder[i].ErrorMessage);
					}
				}
			}
			if (SB.Length != 0)
			{
				return SB.ToString();
			}
			return null;
		}

		/// <summary>
		/// Draws error boxes for all errors in the given action resolvers, or does nothing if there are no errors. This is equivalent to calling DrawError() on all resolvers passed to this method.
		/// </summary>
		public static void DrawErrors(ActionResolver r1 = null, ActionResolver r2 = null, ActionResolver r3 = null, ActionResolver r4 = null, ActionResolver r5 = null, ActionResolver r6 = null, ActionResolver r7 = null, ActionResolver r8 = null)
		{
			DrawErrors(r1, r2, r3, r4, r5, r6, r7, r8, (ActionResolver[])null);
		}

		/// <summary>
		/// Draws error boxes for all errors in the given action resolvers, or does nothing if there are no errors. This is equivalent to calling DrawError() on all resolvers passed to this method.
		/// </summary>
		public static void DrawErrors(ActionResolver r1 = null, ActionResolver r2 = null, ActionResolver r3 = null, ActionResolver r4 = null, ActionResolver r5 = null, ActionResolver r6 = null, ActionResolver r7 = null, ActionResolver r8 = null, params ActionResolver[] remainder)
		{
			r1?.DrawError();
			r2?.DrawError();
			r3?.DrawError();
			r4?.DrawError();
			r5?.DrawError();
			r6?.DrawError();
			r7?.DrawError();
			r8?.DrawError();
			if (remainder == null)
			{
				return;
			}
			for (int i = 0; i < remainder.Length; i++)
			{
				if (remainder[i] != null)
				{
					remainder[i].DrawError();
				}
			}
		}
	}
}
