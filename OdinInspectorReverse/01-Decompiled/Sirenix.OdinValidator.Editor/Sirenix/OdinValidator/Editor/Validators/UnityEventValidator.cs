using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Serialization;
using Sirenix.Utilities.Editor.Expressions.Internal;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinValidator.Editor.Validators
{
	public class UnityEventValidator : ValueValidator<UnityEvent>
	{
		private static readonly Expressionator expressionator = new Expressionator(null);

		protected override void Validate(ValidationResult result)
		{
			UnityEventBase @event = base.ValueEntry.SmartValue;
			int eventCount = @event.GetPersistentEventCount();
			expressionator.Context["event"] = @event;
			object persistentCallGroup = expressionator.Expr("$event.m_PersistentCalls");
			for (int i = 0; i < eventCount; i++)
			{
				UnityEngine.Object target = @event.GetPersistentTarget(i);
				string methodName = @event.GetPersistentMethodName(i);
				expressionator.Context["i"] = i;
				expressionator.Context["persistentCallGroup"] = persistentCallGroup;
				object persistentCall = expressionator.Expr("$persistentCallGroup.GetListener($i)");
				expressionator.Context["persistentCall"] = persistentCall;
				PersistentListenerMode mode = expressionator.Expr<PersistentListenerMode>("$persistentCall.m_Mode");
				string objectArgumentAssemblyTypeName = expressionator.Expr<string>("$persistentCall.m_Arguments.m_ObjectArgumentAssemblyTypeName");
				Type argumentType = null;
				if (!string.IsNullOrWhiteSpace(objectArgumentAssemblyTypeName))
				{
					argumentType = TwoWaySerializationBinder.Default.BindToType(objectArgumentAssemblyTypeName);
				}
				MethodInfo targetMethod = GetMethodInfo(target, methodName, mode, argumentType);
				if (targetMethod == null && target != null)
				{
					result.AddError("UnityEvent is missing a function.");
				}
				else if (target == null && targetMethod != null)
				{
					result.AddError("UnityEvent has no target.");
				}
			}
		}

		private MethodInfo GetMethodInfo(object target, string methodName, PersistentListenerMode mode, Type argumentType)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				return null;
			}
			switch (mode)
			{
			case PersistentListenerMode.Void:
				argumentType = null;
				break;
			case PersistentListenerMode.Object:
				argumentType = argumentType ?? typeof(UnityEngine.Object);
				break;
			case PersistentListenerMode.Int:
				argumentType = typeof(int);
				break;
			case PersistentListenerMode.Float:
				argumentType = typeof(float);
				break;
			case PersistentListenerMode.String:
				argumentType = typeof(string);
				break;
			case PersistentListenerMode.Bool:
				argumentType = typeof(bool);
				break;
			}
			return UnityEventBase.GetValidMethodInfo(target, methodName, (argumentType == null) ? Array.Empty<Type>() : new Type[1] { argumentType });
		}
	}
}
