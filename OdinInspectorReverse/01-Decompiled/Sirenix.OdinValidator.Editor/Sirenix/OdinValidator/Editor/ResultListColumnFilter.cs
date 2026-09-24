using System;
using UnityEngine;

namespace Sirenix.OdinValidator.Editor
{
	[Flags]
	public enum ResultListColumnFilter
	{
		[HideInInspector]
		None = 0,
		[HideInInspector]
		Message = 2,
		Path = 4,
		Location = 8,
		Object = 0x10,
		Validator = 0x20
	}
}
