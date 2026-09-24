using System.Globalization;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Not yet documented.
	/// </summary>
	public sealed class Vector2DictionaryKeyPathProvider : BaseDictionaryKeyPathProvider<Vector2>
	{
		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override string ProviderID => "v2";

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override int Compare(Vector2 x, Vector2 y)
		{
			int result = x.x.CompareTo(y.x);
			if (result == 0)
			{
				result = x.y.CompareTo(y.y);
			}
			return result;
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override Vector2 GetKeyFromPathString(string pathStr)
		{
			int sep = pathStr.IndexOf('|');
			string x = pathStr.Substring(1, sep - 1).Trim();
			string y = pathStr.Substring(sep + 1, pathStr.Length - (sep + 2)).Trim();
			return new Vector2(float.Parse(x), float.Parse(y));
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		public override string GetPathStringFromKey(Vector2 key)
		{
			string x = key.x.ToString("R", CultureInfo.InvariantCulture);
			string y = key.y.ToString("R", CultureInfo.InvariantCulture);
			return ("(" + x + "|" + y + ")").Replace('.', ',');
		}
	}
}
