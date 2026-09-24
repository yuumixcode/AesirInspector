using System;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// This class gathers info about the current architecture for the purpose of determinining
	/// the unaligned read/write capabilities that we have to work with.
	/// </summary>
	public static class ArchitectureInfo
	{
		public static bool Architecture_Supports_Unaligned_Float32_Reads;

		/// <summary>
		/// This will be false on some ARM architectures, such as ARMv7.
		/// In these cases, we will have to perform slower but safer int-by-int read/writes of data.
		/// <para />
		/// Since this value will never change at runtime, performance hits from checking this 
		/// everywhere should hopefully be negligible, since branch prediction from speculative
		/// execution will always predict it correctly.
		/// </summary>
		public static bool Architecture_Supports_All_Unaligned_ReadWrites;

		static ArchitectureInfo()
		{
			Architecture_Supports_Unaligned_Float32_Reads = true;
			Architecture_Supports_All_Unaligned_ReadWrites = true;
		}

		internal unsafe static void SetRuntimePlatform(RuntimePlatform platform)
		{
			switch (platform)
			{
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.PS3:
			case RuntimePlatform.XBOX360:
			case RuntimePlatform.LinuxPlayer:
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.PS4:
			case RuntimePlatform.XboxOne:
			case RuntimePlatform.WiiU:
				try
				{
					byte[] testArray = new byte[8];
					fixed (byte* test = testArray)
					{
						for (int i = 0; i < 4; i++)
						{
							float value = *(float*)(test + i);
						}
						Architecture_Supports_Unaligned_Float32_Reads = true;
					}
				}
				catch (NullReferenceException)
				{
					Architecture_Supports_Unaligned_Float32_Reads = false;
				}
				if (Architecture_Supports_Unaligned_Float32_Reads)
				{
					Debug.Log("Odin Serializer detected whitelisted runtime platform " + platform.ToString() + " and memory read test succeeded; enabling all unaligned memory read/writes.");
					Architecture_Supports_All_Unaligned_ReadWrites = true;
				}
				else
				{
					Debug.Log("Odin Serializer detected whitelisted runtime platform " + platform.ToString() + " and memory read test failed; disabling all unaligned memory read/writes.");
				}
				break;
			default:
				Architecture_Supports_Unaligned_Float32_Reads = false;
				Architecture_Supports_All_Unaligned_ReadWrites = false;
				Debug.Log("Odin Serializer detected non-white-listed runtime platform " + platform.ToString() + "; disabling all unaligned memory read/writes.");
				break;
			}
		}
	}
}
