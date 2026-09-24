// =============================================================================
// Odin Inspector 反编译流水线 - Utility: Unity 参照程序集「公开化」(publicize)
// -----------------------------------------------------------------------------
// 背景: Sirenix.Reflection.Editor 直接使用 UnityEngine 的 internal 类型
//       (GUILayoutEntry / GUILayoutGroup / ScrollViewState / UIElements.Panel /
//        UnityEditor 的 ContainerWindow 等，TypeAttributes = 0x100000 NotPublic)。
//       而 Unity 官方参照程序集未对这些类型开放访问：
//         - UnityEditor.dll            : 无任何 InternalsVisibleTo
//         - UnityEngine.*Module.dll    : 仅对其它 Unity 内部模块开放
//       且实测 Roslyn 不采纳 IgnoresAccessChecksTo 破障特性。
//       => 唯一可行路径是生成一份"成员全部公开"的 Unity 参照程序集副本，
//          仅用于编译期，不参与运行时分发。
//
// 用法: PublicizeUnity.exe <输出目录> <待公开化程序集所在目录> [更多搜索目录...]
// =============================================================================
using System;
using System.IO;
using System.Linq;
using Mono.Cecil;

internal static class PublicizeUnity
{
	private static int Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.Error.WriteLine("用法: PublicizeUnity <outDir> <inDir> [searchDir...]");
			return 1;
		}

		string outDir = args[0];
		string inDir = args[1];
		Directory.CreateDirectory(outDir);

		var resolver = new DefaultAssemblyResolver();
		resolver.AddSearchDirectory(inDir);
		foreach (var d in args.Skip(2)) resolver.AddSearchDirectory(d);

		int ok = 0, fail = 0;
		foreach (var path in Directory.GetFiles(inDir, "*.dll").OrderBy(x => x))
		{
			string name = Path.GetFileName(path);
			try
			{
				var rp = new ReaderParameters
				{
					AssemblyResolver = resolver,
					ReadSymbols = false,
					InMemory = true,
					ReadingMode = ReadingMode.Immediate,
				};

				using (var asm = AssemblyDefinition.ReadAssembly(path, rp))
				{
					foreach (var type in asm.MainModule.GetTypes())
					{
						// 类型可见性：internal -> public（这是本工具的核心理由）
						if (type.IsNested)
						{
							if (!type.IsNestedPublic) type.IsNestedPublic = true;
						}
						else if (!type.IsPublic)
						{
							type.IsPublic = true;
						}

						// 字段可见性：全部提升
						foreach (var f in type.Fields)
							if (!f.IsPublic) f.IsPublic = true;

						// 方法可见性。
						// 关键约束：virtual / abstract 方法记录着「跨程序集重写契约」——
						// 例如 Unity 的 GUILayoutEntry.ApplyStyleSettings 是 protected virtual，
						// 而 Sirenix.Reflection.Editor 中以 protected override 覆写。
						// 若把它提升为 public，覆写方就会撞上 CS0507（无法更改访问修饰符）。
						// 故对可重写成员一律保持原可见性。
						foreach (var m in type.Methods)
							if (!m.IsPublic && !m.IsVirtual) m.IsPublic = true;

						// 事件后端方法同理
						foreach (var e in type.Events)
						{
							if (e.AddMethod != null && !e.AddMethod.IsPublic && !e.AddMethod.IsVirtual) e.AddMethod.IsPublic = true;
							if (e.RemoveMethod != null && !e.RemoveMethod.IsPublic && !e.RemoveMethod.IsVirtual) e.RemoveMethod.IsPublic = true;
							if (e.InvokeMethod != null && !e.InvokeMethod.IsPublic && !e.InvokeMethod.IsVirtual) e.InvokeMethod.IsPublic = true;
						}

						// 属性后端方法同理
						foreach (var p in type.Properties)
						{
							if (p.GetMethod != null && !p.GetMethod.IsPublic && !p.GetMethod.IsVirtual) p.GetMethod.IsPublic = true;
							if (p.SetMethod != null && !p.SetMethod.IsPublic && !p.SetMethod.IsVirtual) p.SetMethod.IsPublic = true;
						}
					}

					asm.Write(Path.Combine(outDir, name), new WriterParameters { WriteSymbols = false });
				}

				Console.WriteLine("[公开化] " + name);
				ok++;
			}
			catch (Exception ex)
			{
				Console.WriteLine("[跳过]   " + name + "  (" + ex.GetType().Name + ": " + ex.Message + ")");
				fail++;
			}
		}

		Console.WriteLine();
		Console.WriteLine($"完成: 成功 {ok}, 跳过 {fail}, 输出目录 {outDir}");
		return 0;
	}
}
