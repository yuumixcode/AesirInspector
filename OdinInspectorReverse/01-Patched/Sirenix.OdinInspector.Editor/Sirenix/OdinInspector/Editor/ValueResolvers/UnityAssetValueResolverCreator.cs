using System.IO;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class UnityAssetValueResolverCreator : ValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			if (typeof(Object).IsAssignableFrom(context.ResultType))
			{
				return "Assets/UnityAssetPath.ext\nUnity GlobalObjectId string";
			}
			return null;
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrWhiteSpace(context.ResolvedString) || !typeof(Object).IsAssignableFrom(context.ResultType))
			{
				return null;
			}
			if (GlobalObjectId.TryParse(context.ResolvedString, out var id))
			{
				Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
				if (obj != null)
				{
					if (context.ResultType.IsAssignableFrom(obj.GetType()))
					{
						return delegate
						{
							return (TResult)(object)obj;
						};
					}
					context.ErrorMessage = "GlobalObjectId '" + context.ResolvedString + "' mapped to object '" + obj.name + "' of type '" + obj.GetType().GetNiceName() + "', but an object of type '" + context.ResultType.GetNiceName() + "' was expected.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				context.ErrorMessage = "GlobalObjectId '" + context.ResolvedString + "' did not map to any object.";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			if (File.Exists(context.ResolvedString))
			{
				Object obj2 = AssetDatabase.LoadAssetAtPath(context.ResolvedString, context.ResultType);
				if (obj2 == null)
				{
					obj2 = AssetDatabase.LoadAssetAtPath(context.ResolvedString, typeof(Object));
				}
				if (obj2 != null)
				{
					if (context.ResultType.IsAssignableFrom(obj2.GetType()))
					{
						return delegate
						{
							return (TResult)(object)obj2;
						};
					}
					context.ErrorMessage = "Asset path '" + context.ResolvedString + "' contained object '" + obj2.name + "' of type '" + obj2.GetType().GetNiceName() + "', but an object of type '" + context.ResultType.GetNiceName() + "' was expected.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				context.ErrorMessage = "Asset path '" + context.ResolvedString + "' could be loaded as a valid asset.";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			return null;
		}
	}
}
