using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Validation;

namespace Sirenix.OdinValidator.Editor
{
	public class PersistentValidationResult : IEquatable<PersistentValidationResult>
	{
		public sealed class PersistentValidationResultEqualityComparer : IEqualityComparer<PersistentValidationResult>
		{
			public bool Equals(PersistentValidationResult x, PersistentValidationResult y)
			{
				if ((object)x == y)
				{
					return true;
				}
				if ((object)x == null)
				{
					return false;
				}
				if ((object)y == null)
				{
					return false;
				}
				if (object.Equals(x.DynamicObjectAddress, y.DynamicObjectAddress) && object.Equals(x.ValidatorType, y.ValidatorType) && x.Path == y.Path && x.ValidatorIndex == y.ValidatorIndex)
				{
					return x.BatchIndex == y.BatchIndex;
				}
				return false;
			}

			public int GetHashCode(PersistentValidationResult obj)
			{
				int hashCode = ((obj.DynamicObjectAddress != null) ? obj.DynamicObjectAddress.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ((obj.ValidatorType != null) ? obj.ValidatorType.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ((obj.Path != null) ? obj.Path.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ obj.ValidatorIndex;
				return (hashCode * 397) ^ obj.BatchIndex;
			}
		}

		public static PersistentValidationResultEqualityComparer Comparer = new PersistentValidationResultEqualityComparer();

		private Type getGenericValidatorType_Cached;

		public readonly PersistentValidationResultBatch Batch;

		public readonly DynamicObjectAddress DynamicObjectAddress;

		public readonly Type ValidatorType;

		public readonly int BatchIndex;

		public readonly string Path;

		public readonly int ValidatorIndex;

		public PersistentResultItem Result;

		public ref PersistenceData Data => ref Result.Data;

		public ref ValidationResultType ResultType => ref Result.ResultType;

		public ref string Message => ref Result.Message;

		public DynamicObjectAddress SelectionObjectAddress
		{
			get
			{
				if (Data.SelectionObjectAddress != null)
				{
					return Data.SelectionObjectAddress;
				}
				return DynamicObjectAddress;
			}
		}

		public PersistentValidationResult(PersistentValidationResultBatch batch, int batchIndex)
		{
			Batch = batch;
			ValidatorType = batch.ValidatorType;
			ValidatorIndex = batch.ValidatorIndex;
			DynamicObjectAddress = batch.DynamicObjectAddress;
			Path = batch.Path;
			BatchIndex = batchIndex;
			Result = batch[batchIndex];
		}

		public string ToNiceLogString()
		{
			if (ResultType == ValidationResultType.IgnoreResult)
			{
				return "Ignore";
			}
			string result = $"{ResultType}: {Message}\n\nFrom member '{Path}' in Unity object: \n{DynamicObjectAddress?.LatestAddress.ToString(prettyPrint: true)}";
			return StringExtensions.IndentString(result, skipFirstLine: true);
		}

		public override string ToString()
		{
			return ToNiceLogString();
		}

		internal SceneReference GetSceneReference()
		{
			if (DynamicObjectAddress == null)
			{
				return default(SceneReference);
			}
			switch (DynamicObjectAddress.LatestAddress.Type)
			{
			case ObjectAddress.AddressType.SceneGameObject:
			case ObjectAddress.AddressType.SceneComponent:
				return new SceneReference(DynamicObjectAddress.LatestAddress.AssetGUID);
			case ObjectAddress.AddressType.Asset:
				if (DynamicObjectAddress.LatestAddress.IsSceneAsset())
				{
					return new SceneReference(DynamicObjectAddress.LatestAddress.AssetGUID);
				}
				break;
			}
			return default(SceneReference);
		}

		internal Type GetGenericValidatorType()
		{
			if (getGenericValidatorType_Cached == null)
			{
				if (ValidatorType == null)
				{
					return typeof(NullReferenceException);
				}
				if (ValidatorType.IsGenericType)
				{
					getGenericValidatorType_Cached = ValidatorType.GetGenericTypeDefinition();
				}
				else
				{
					getGenericValidatorType_Cached = ValidatorType;
				}
			}
			return getGenericValidatorType_Cached;
		}

		internal Type GetObjectType()
		{
			if (DynamicObjectAddress == null)
			{
				return typeof(UnknownType);
			}
			return DynamicObjectAddress.LatestAddress.ObjectType.Type ?? typeof(NullReferenceException);
		}

		public static bool operator ==(PersistentValidationResult left, PersistentValidationResult right)
		{
			return Comparer.Equals(left, right);
		}

		public static bool operator !=(PersistentValidationResult left, PersistentValidationResult right)
		{
			return !Comparer.Equals(left, right);
		}

		public bool Equals(PersistentValidationResult other)
		{
			return Comparer.Equals(this, other);
		}
	}
}
