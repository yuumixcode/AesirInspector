using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sirenix.OdinInspector.Editor.Validation
{
	public sealed class ValidationResult : ICollection<ResultItem>, IEnumerable<ResultItem>, IEnumerable
	{
		private static ResultItem NoResultItem;

		private ResultItem firstItem;

		private ResultItem[] items;

		private int itemsCount;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public string Path;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This value is no longer used for anything and is not passed further into the validation system.", false)]
		public object ResultValue;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public double ValidationTimeMS;

		public ValidationSetup Setup;

		public int Count
		{
			get
			{
				if (FirstItemExists())
				{
					return itemsCount + 1;
				}
				return itemsCount;
			}
		}

		public ref ResultItem this[int index]
		{
			get
			{
				if (FirstItemExists())
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ref string Message => ref firstItem.Message;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ref ValidationResultType ResultType => ref firstItem.ResultType;

		bool ICollection<ResultItem>.IsReadOnly => true;

		public ref ResultItem AddError(string error)
		{
			return ref Add(new ResultItem
			{
				Message = error,
				ResultType = ValidationResultType.Error
			});
		}

		public ref ResultItem AddWarning(string warning)
		{
			return ref Add(new ResultItem
			{
				Message = warning,
				ResultType = ValidationResultType.Warning
			});
		}

		public ref ResultItem Add(ResultItem item)
		{
			ResultItem[] its = items;
			if (its == null)
			{
				its = (items = new ResultItem[2]);
			}
			while (its.Length <= itemsCount + 1)
			{
				ResultItem[] expand = new ResultItem[its.Length * 2];
				for (int i = 0; i < its.Length; i++)
				{
					expand[i] = its[i];
				}
				its = expand;
				items = expand;
			}
			its[itemsCount] = item;
			return ref its[itemsCount++];
		}

		public ref ResultItem Add(ValidatorSeverity severity, string message)
		{
			switch (severity)
			{
			case ValidatorSeverity.Error:
				return ref Add(new ResultItem
				{
					Message = message,
					ResultType = ValidationResultType.Error
				});
			case ValidatorSeverity.Warning:
				return ref Add(new ResultItem
				{
					Message = message,
					ResultType = ValidationResultType.Warning
				});
			default:
				NoResultItem = default(ResultItem);
				return ref NoResultItem;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Explode(ref List<ValidationResult> results, double validationTimeMS = 0.0)
		{
			if (results == null)
			{
				results = new List<ValidationResult>();
			}
			int count = Count;
			if (count != 0)
			{
				if (validationTimeMS == 0.0)
				{
					validationTimeMS = ValidationTimeMS;
				}
				double msSplit = validationTimeMS / (double)count;
				for (int i = 0; i < count; i++)
				{
					ref ResultItem item = ref this[i];
					results.Add(new ValidationResult
					{
						firstItem = item,
						Path = Path,
						Setup = Setup,
						ValidationTimeMS = msSplit
					});
				}
			}
		}

		public bool IsMatch(ValidationResult other)
		{
			if (this == other)
			{
				return true;
			}
			if (this == null != (other == null))
			{
				return false;
			}
			if (this == null)
			{
				return false;
			}
			if (Message != other.Message || Path != other.Path || Setup.Validator != other.Setup.Validator || ResultType != other.ResultType)
			{
				return false;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Changes to the validation system mean that validation can no longer be safely rerun from a result.", true)]
		public void RerunValidation()
		{
			throw new NotSupportedException();
		}

		public ValidationResult CreateCopy()
		{
			ValidationResult copy = new ValidationResult();
			if (items != null)
			{
				ResultItem[] res = items;
				ResultItem[] copRes = new ResultItem[res.Length];
				for (int i = 0; i < res.Length; i++)
				{
					copRes[i] = res[i];
				}
				copy.items = copRes;
			}
			copy.Path = Path;
			copy.Message = Message;
			copy.ResultType = ResultType;
			copy.Setup = Setup;
			return copy;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool FirstItemExists()
		{
			if (firstItem.ResultType != ValidationResultType.Error && firstItem.ResultType != ValidationResultType.Warning)
			{
				if (itemsCount == 0)
				{
					return firstItem.ResultType == ValidationResultType.Valid;
				}
				return false;
			}
			return true;
		}

		IEnumerator<ResultItem> IEnumerable<ResultItem>.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		void ICollection<ResultItem>.Add(ResultItem item)
		{
			throw new NotSupportedException();
		}

		void ICollection<ResultItem>.Clear()
		{
			throw new NotSupportedException();
		}

		bool ICollection<ResultItem>.Contains(ResultItem item)
		{
			throw new NotSupportedException();
		}

		void ICollection<ResultItem>.CopyTo(ResultItem[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		bool ICollection<ResultItem>.Remove(ResultItem item)
		{
			throw new NotSupportedException();
		}
	}
}
