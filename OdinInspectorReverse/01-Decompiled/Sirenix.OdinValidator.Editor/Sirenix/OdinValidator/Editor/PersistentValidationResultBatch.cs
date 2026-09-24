using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.OdinInspector.Editor.Validation.Internal;
using UnityEditor;

namespace Sirenix.OdinValidator.Editor
{
	public class PersistentValidationResultBatch
	{
		public class Comparer : IEqualityComparer<PersistentValidationResultBatch>
		{
			public bool Equals(PersistentValidationResultBatch x, PersistentValidationResultBatch y)
			{
				if (x == null || y == null)
				{
					return x == y;
				}
				if (x.DynamicObjectAddress != y.DynamicObjectAddress)
				{
					return false;
				}
				if (x.Path != y.Path)
				{
					return false;
				}
				if (x.ValidatorIndex != y.ValidatorIndex)
				{
					return false;
				}
				Type t1 = x.GetGenericValidatorType();
				Type t2 = y.GetGenericValidatorType();
				if (t1 != t2)
				{
					return false;
				}
				return true;
			}

			public int GetHashCode(PersistentValidationResultBatch obj)
			{
				int hashCode = -1202384790;
				hashCode = hashCode * -1521134295 + EqualityComparer<DynamicObjectAddress>.Default.GetHashCode(obj.DynamicObjectAddress);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(obj.Path);
				hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(obj.GetGenericValidatorType());
				return hashCode * -1521134295 + obj.ValidatorIndex.GetHashCode();
			}
		}

		public static readonly PersistentValidationResultBatch Ignore = new PersistentValidationResultBatch
		{
			firstItem = new PersistentResultItem
			{
				ResultType = ValidationResultType.IgnoreResult
			}
		};

		public DynamicObjectAddress DynamicObjectAddress;

		public string Path;

		public Type ValidatorType;

		public int ValidatorIndex;

		public double ValidationTimeMS;

		public int ErrorCount;

		public int WarningCount;

		internal int highestSeverityIndex;

		private PersistentResultItem firstItem;

		private PersistentResultItem[] items;

		private int itemsCount;

		private bool firstItemExist;

		private int countOffset;

		private Type getGenericValidatorType_Cached;

		private static PersistentResultItem[] SingleResultBuffer = new PersistentResultItem[1];

		public ref PersistentResultItem HighestSeverityResult
		{
			get
			{
				if (Count == 0)
				{
					return ref firstItem;
				}
				return ref this[highestSeverityIndex];
			}
		}

		public int Count => itemsCount + countOffset;

		public ref PersistentResultItem this[int index]
		{
			get
			{
				if (firstItemExist)
				{
					if (index == 0)
					{
						return ref firstItem;
					}
					if (items == null)
					{
						throw new IndexOutOfRangeException();
					}
					return ref items[index - 1];
				}
				if (items == null)
				{
					throw new IndexOutOfRangeException();
				}
				return ref items[index];
			}
		}

		private PersistentValidationResultBatch()
		{
		}

		internal PersistentValidationResultBatch(DynamicObjectAddress address, Type validatorType, int validatorIndex, ResultItem[] items)
		{
			this.items = ResultItemPersistor.CreatePersistentResultItems(items, default(ResultItemPersistor.PersistenceContext));
			firstItemExist = false;
			itemsCount = items.Length;
			DynamicObjectAddress = address;
			ValidatorIndex = validatorIndex;
			ValidatorType = validatorType;
			CountErrorsAndWarnings();
		}

		public PersistentValidationResultBatch(string path, string message, Type validatorType, int validatorIndex, ValidationResultType resultType)
			: this(path, message, validatorType, validatorIndex, resultType, DynamicObjectAddress.Unknown)
		{
		}

		public PersistentValidationResultBatch(string path, string message, Type validatorType, int validatorIndex, ValidationResultType resultType, DynamicObjectAddress objectAddress)
		{
			Path = path;
			ValidatorType = validatorType;
			ValidatorIndex = validatorIndex;
			DynamicObjectAddress = objectAddress;
			firstItem.Message = message;
			firstItem.ResultType = resultType;
			highestSeverityIndex = 0;
			firstItemExist = true;
			countOffset = 1;
			if (resultType == ValidationResultType.Warning)
			{
				WarningCount = 1;
			}
			if (resultType == ValidationResultType.Error)
			{
				ErrorCount = 1;
			}
		}

		public PersistentValidationResultBatch(ValidationResult result, DynamicObjectAddress address)
		{
			Path = result.Path;
			ValidatorType = result.Setup.Validator.GetType();
			ValidationTimeMS = result.ValidationTimeMS;
			if (result.Setup.Validator is IAttributeValidator attrValidator)
			{
				ValidatorIndex = attrValidator.AttributeNumber;
			}
			int resultCount = result.Count;
			if (resultCount == 1)
			{
				ResultItemPersistor.CreatePersistentResultItems(result, ref SingleResultBuffer);
				firstItem = SingleResultBuffer[0];
				firstItemExist = true;
				countOffset = 1;
			}
			else
			{
				items = ResultItemPersistor.CreatePersistentResultItems(result);
				itemsCount = resultCount;
				firstItemExist = false;
				countOffset = 0;
			}
			if (address == null)
			{
				if (result.Setup.Validator is SceneValidator)
				{
					SceneValidator sceneValidator = result.Setup.Validator as SceneValidator;
					DynamicObjectAddress = DynamicObjectAddress.GetOrCreate(AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneValidator.ValidatedScene.Path), new ObjectAddress(sceneValidator.ValidatedScene));
				}
				else
				{
					if (!(result.Setup.Validator is GlobalValidator))
					{
						throw new Exception("No dynamic object address was passed.");
					}
					DynamicObjectAddress = null;
				}
			}
			else
			{
				DynamicObjectAddress = address;
			}
			CountErrorsAndWarnings();
		}

		private void CountErrorsAndWarnings()
		{
			int warningIndex = -1;
			int errorIndex = -1;
			for (int i = 0; i < Count; i++)
			{
				ref PersistentResultItem item = ref this[i];
				if (item.ResultType == ValidationResultType.Error)
				{
					ErrorCount++;
					warningIndex = i;
				}
				else if (item.ResultType == ValidationResultType.Warning)
				{
					WarningCount++;
					errorIndex = i;
				}
			}
			if (errorIndex != -1)
			{
				highestSeverityIndex = errorIndex;
			}
			else if (warningIndex != -1)
			{
				highestSeverityIndex = warningIndex;
			}
			else if (Count > 0)
			{
				highestSeverityIndex = 0;
			}
			else
			{
				highestSeverityIndex = -1;
			}
		}

		public string ToNiceLogString()
		{
			if (Count == 0)
			{
				return "Ignore";
			}
			ref PersistentResultItem highestSeverity = ref HighestSeverityResult;
			if (Count == 1 && highestSeverity.ResultType == ValidationResultType.IgnoreResult)
			{
				return "Ignore";
			}
			string result = $"{highestSeverity.ResultType}: {highestSeverity.Message}\n\nFrom member '{Path}' in Unity object: \n{DynamicObjectAddress.LatestAddress.ToString(prettyPrint: true)}";
			if (Count > 1)
			{
				StringBuilder sb = new StringBuilder(result);
				bool hasAddedMessage = false;
				for (int i = 0; i < Count; i++)
				{
					ref PersistentResultItem subResult = ref this[i];
					if (i != highestSeverityIndex && subResult.ResultType != ValidationResultType.IgnoreResult)
					{
						if (!hasAddedMessage)
						{
							sb.AppendLine();
							sb.AppendLine("Extra messages:");
							hasAddedMessage = true;
						}
						sb.Append("    - ");
						sb.AppendLine($"{subResult.ResultType}: {subResult.Message}");
					}
				}
				if (hasAddedMessage)
				{
					result = sb.ToString();
				}
			}
			return result;
		}

		public IEnumerable<PersistentValidationResult> Explode()
		{
			int count = Count;
			for (int i = 0; i < count; i++)
			{
				yield return new PersistentValidationResult(this, i);
			}
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
				return default(SceneReference);
			default:
				return default(SceneReference);
			}
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
			return DynamicObjectAddress?.LatestAddress?.ObjectType.Type ?? typeof(NullReferenceException);
		}
	}
}
