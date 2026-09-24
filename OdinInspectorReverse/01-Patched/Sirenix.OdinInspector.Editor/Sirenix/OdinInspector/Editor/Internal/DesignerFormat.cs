using System.Text.RegularExpressions;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerFormat
	{
		public const string EXTENSION = "ovdf";

		public const string HEADER = "OVDF";

		public const char SYMBOL_BLOCK = '#';

		public const char SYMBOL_REF = '$';

		public const char SYMBOL_PATCH_ASSIGNMENT = ':';

		public const char SYMBOL_MEMBER_ASSIGNMENT = '=';

		public const char SYMBOL_ADD_ATTRIBUTE = '+';

		public const char SYMBOL_MODIFY_ATTRIBUTE = '*';

		public const char SYMBOL_REMOVE_ATTRIBUTE = '-';

		public const string REF_SELF = "self";

		public const string REF_ROOT = "root";

		public const string META_GUID_PREFIX = "MetaGuid:";

		public const string PATCH_VALUE_PARENT_ID = "Parent";

		public const string PATCH_VALUE_DESIRED_INDEX = "Desired-Index";

		public const string PATCH_VALUE_VISIBILITY = "Visibility";

		public const string HEADER_REGEX_PATTERN = "^\\s*OVDF\\s*v\\d+\\.\\d+\\s*$";

		public static DesignerVersion CurrentVersion => new DesignerVersion(1, 1);

		public static bool IsValidHeader(string header)
		{
			return Regex.IsMatch(header, "^\\s*OVDF\\s*v\\d+\\.\\d+\\s*$");
		}
	}
}
