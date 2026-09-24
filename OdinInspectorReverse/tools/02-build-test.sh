#!/bin/bash
# =============================================================================
# Odin Inspector 反编译流水线 - Step 2: 源码可编译性验证
# -----------------------------------------------------------------------------
# 目的: 用 Roslyn csc + Unity 官方 netstandard2.1 参照 + Unity 模块引用，
#       按拓扑序重建 7 个程序集，量化"源码版 UPM 包"路线的真实可行度。
#
# 关键点:
#   * 参照集必须用 Unity 的 Contents/NetStandard/ref/2.1.0/netstandard.dll
#     （而不是 Mono 的 4.7.1-api），否则 System.Index / System.Range 等
#     netstandard2.1 才有的类型缺失，Sirenix.Utilities 会挂在 2 个错误上。
#   * Sirenix.Reflection.Editor 直接访问 UnityEngine 的 internal 类型
#     (GUILayoutEntry / GUILayoutGroup / ScrollViewState / Panel 等，flags=0x100000)。
#     原厂 DLL 是用"公开化(publicized)参照程序集"编译的；这里改用
#     IgnoresAccessChecksTo 特性让 Roslyn 跳过可见性校验来等效复现。
#
# 输入: 01-Decompiled/<Assembly>/**/*.cs
# 输出: 02-BuildTest/bin/<Assembly>.dll
# =============================================================================
set -o pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="$ROOT/01-Decompiled"
PATCHED="$ROOT/01-Patched"
BIN="$ROOT/02-BuildTest/bin"
SHIM="$ROOT/02-BuildTest/shim"
REPORT="$ROOT/report"
PATCHLOG="$REPORT/patches-applied.txt"

# ---------------------------------------------------------------------------
# Step 0: 生成「最小修补」源码副本
#   01-Decompiled/ 保持原始反编译产物纯净，所有修补只作用于 01-Patched/。
#   目前仅一类修补：
#     ILSpy 把原始 IL 中的枚举字面量 512 渲染成了 .NET Core 才有的名字
#     MethodImplOptions.AggressiveOptimization。经核查，该成员在
#     Unity netfx48 / netstandard2.1 / Unity Mono 全部 profile 中均不存在。
#     替换为 (MethodImplOptions)512 是纯 JIT 提示位，无语义影响。
# ---------------------------------------------------------------------------
rm -rf "$PATCHED"
mkdir -p "$PATCHED"
cp -R "$SRC"/. "$PATCHED"/
: > "$PATCHLOG"

# 用 Python 做修补：BSD sed 的 -i '' 与转义行为在循环里不可靠（曾静默失败），
# Python 的字符串替换语义明确、可校验。
python3 - "$PATCHED" "$PATCHLOG" <<'PY'
import sys, os, io

root, logpath = sys.argv[1], sys.argv[2]

# 修补规则：(说明, 查找串, 替换串)
RULES = [
    (
        "ILSpy 把 IL 中的枚举字面量 512 渲染成了 .NET Core 专有名 AggressiveOptimization；"
        "该成员在 Unity netfx48 / netstandard2.1 / Unity Mono 全部 profile 中均不存在。"
        "512 是纯 JIT 提示位，改为字面量无语义影响。",
        "MethodImplOptions.AggressiveOptimization",
        "(MethodImplOptions)512",
    ),
    (
        "ILSpy 还原时丢失了原始的类型限定，导致 StackFrame 在 UnityEditor.StackFrame "
        "与 System.Diagnostics.StackFrame 之间二义。该处原意显然是后者，显式限定即可。",
        "StackFrame[] stackFrames = new StackTrace().GetFrames();",
        "System.Diagnostics.StackFrame[] stackFrames = new StackTrace().GetFrames();",
    ),
    (
        "同上：foreach 中的 StackFrame。",
        "foreach (StackFrame frame in stackFrames)",
        "foreach (System.Diagnostics.StackFrame frame in stackFrames)",
    ),
    (
        "ILSpy 把 FileAttributes 的字面量 0 渲染成了不存在的成员 None。"
        "System.IO.FileAttributes 只有 Normal(=128)，0 无具名成员，改回字面量。",
        "FileAttributes.None",
        "(FileAttributes)0",
    ),
    (
        "ILSpy 把原始的 for 循环改写成了 LINQ 查询，导致 ref 参数落在范围变量上（CS1939）。"
        "改为调用语义等价的包装方法 CanCreateInstanceSafe（见下条规则插入的实现）。",
        "select (!CanCreateInstance(type, ref t)) ? null : t into t",
        "select CanCreateInstanceSafe(type, t) into t",
    ),
    (
        "插入上一条规则所需的包装方法：保持 ref 传参语义，"
        "并在不可创建时返回 null，与原三元表达式行为一致。",
        "private static bool CanCreateInstance(Type baseType, ref Type type)",
        "private static Type CanCreateInstanceSafe(Type baseType, Type candidate)\n"
        "\t\t{\n"
        "\t\t\tType t = candidate;\n"
        "\t\t\treturn CanCreateInstance(baseType, ref t) ? t : null;\n"
        "\t\t}\n"
        "\n"
        "\t\tprivate static bool CanCreateInstance(Type baseType, ref Type type)",
    ),
]

# 按文件作用域规则：(文件名集合, 查找串, 替换串)
# Clipboard 二义性：UnityEditor.Clipboard 与 Sirenix.Utilities.Editor.Clipboard 同名，
# ILSpy 丢失了原始限定。核查用法（如 Clipboard.CanPaste(typeof(Color))）可确认
# 调用的是 Odin 自己的 Clipboard（UnityEditor.Clipboard 只有 Copy(string)/Paste()）。
# 用 using 别名精确消歧，比逐个加限定更小侵入。
_CLIPBOARD_FILES = {
    "PropertyContextMenuDrawer.cs", "OdinInternalEditorFields.cs", "ColorDrawer.cs",
    "DesignerAttributePopup.cs", "AttributeExamplePreview.cs", "CollectionDrawer.cs",
    "ValidationDrawer.cs", "OdinMenuStyle.cs", "DesignerEditorContext.cs", "DesignerEditor.cs",
}
SCOPED_RULES = [
    (_CLIPBOARD_FILES, "using System;",
     "using System;\nusing Clipboard = Sirenix.Utilities.Editor.Clipboard;"),
]

lines = []
total = 0
for dirpath, _, filenames in os.walk(root):
    for fn in filenames:
        if not fn.endswith(".cs"):
            continue
        path = os.path.join(dirpath, fn)
        with io.open(path, "r", encoding="utf-8", errors="surrogateescape") as f:
            src = f.read()
        new = src
        hits = []
        for desc, find, repl in RULES:
            n = new.count(find)
            if n:
                new = new.replace(find, repl)
                hits.append((find[:44], n))
                total += n
        # 按文件作用域的规则：仅在指定文件内替换
        for fnames, find, repl in SCOPED_RULES:
            if fn in fnames:
                n = new.count(find)
                if n:
                    new = new.replace(find, repl, 1)
                    hits.append(("[" + fn + "] " + find[:30], n))
                    total += n
        if new != src:
            with io.open(path, "w", encoding="utf-8", errors="surrogateescape") as f:
                f.write(new)
            rel = os.path.relpath(path, os.path.dirname(root))
            for find, n in hits:
                lines.append("%s  <- %d\u5904  %s" % (rel, n, find))

with io.open(logpath, "w", encoding="utf-8") as f:
    f.write("\n".join(lines) + ("\n" if lines else ""))

print("  修补替换总数: %d" % total)
PY

DOTNET="$HOME/.dotnet/dotnet"
CSC="$HOME/.dotnet/sdk/8.0.424/Roslyn/bincore/csc.dll"

UNITY_APP="/Applications/Unity/Hub/Editor/2021.3.45f2c1/Unity.app/Contents"
UNITY_NETSTD="$UNITY_APP/NetStandard/ref/2.1.0/netstandard.dll"
UNITY_SHIMS="$UNITY_APP/NetStandard/compat/2.1.0/shims/netfx"
UNITY_48API="$UNITY_APP/UnityReferenceAssemblies/unity-4.8-api"
UNITY_MODULES="$UNITY_APP/Managed/UnityEngine"

# 参照集档位: netstd21（Unity .NET Standard 2.1 级别） | netfx48（Unity .NET Framework 4.8 级别）
# Odin 源码同时需要 System.Index(2.1) + AppDomain.DefineDynamicAssembly(netfx)，
# 实测只有 unity-4.8-api 同时具备，故选 netfx48。
REFSET="${REFSET:-netfx48}"

rm -rf "$BIN" "$SHIM"
mkdir -p "$BIN" "$REPORT" "$SHIM"
LOG="$REPORT/build-test.log"

# ---------------------------------------------------------------------------
# 生成 IgnoresAccessChecksTo 破障垫片（每个程序集私有 internal 定义，避免类型冲突）
# ---------------------------------------------------------------------------
gen_shim () {
  local asm="$1"; local out="$SHIM/IgnoresAccessChecksTo_$asm.cs"
  {
    # 规则：程序集级特性必须位于文件中所有类型定义之前，故先输出 [assembly: ...]
    for m in UnityEngine.CoreModule UnityEngine.IMGUIModule UnityEngine.UIElementsModule \
             UnityEngine.AnimationModule UnityEngine.PhysicsModule UnityEngine.TextRenderingModule \
             UnityEngine.JSONSerializeModule UnityEngine.UnityWebRequestModule \
             UnityEngine.ImageConversionModule UnityEditor UnityEditor.CoreModule \
             UnityEditor.UIElementsModule UnityEditor.GraphViewModule; do
      echo "[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo(\"$m\")]"
    done
    echo
    echo 'namespace System.Runtime.CompilerServices'
    echo '{'
    echo '	[global::System.AttributeUsage(global::System.AttributeTargets.Assembly, AllowMultiple = true)]'
    echo '	internal sealed class IgnoresAccessChecksToAttribute : global::System.Attribute'
    echo '	{'
    echo '		public IgnoresAccessChecksToAttribute(string assemblyName) { AssemblyName = assemblyName; }'
    echo '		public string AssemblyName { get; }'
    echo '	}'
    echo '}'
  } > "$out"
  echo "$out"
}

# --- 基准参照集（对齐 Unity 官方 csc 调用）---------------------------------
REFS=()
if [ "$REFSET" = "netfx48" ]; then
  # Unity .NET Framework 4.8 级别。
  # 注意：绝不能把整个 unity-4.8-api 目录 + Facades 一起引用——
  # Facades/System.Runtime.dll 与 mscorlib.dll 会重复暴露 System.Type，
  # 导致 "调用具有二义性" 的海量伪错误（实测 144 个）。故精选核心程序集。
  for a in mscorlib System System.Core System.Xml System.Xml.Linq \
           System.Runtime.Serialization System.Drawing System.Numerics System.Data \
           Microsoft.CSharp System.Configuration System.Runtime.Remoting; do
    [ -f "$UNITY_48API/$a.dll" ] && REFS+=("/r:$UNITY_48API/$a.dll")
  done
  # 仅补 netstandard 门面：Unity 模块的 assemblyref 指向 netstandard 2.1，
  # 在 netfx 档下需要这个门面把类型转发回 mscorlib。
  # 注意只加 netstandard.dll 一个，绝不可把 Facades/ 整个目录加进来
  # （Facades/System.Runtime.dll 会与 mscorlib 重复暴露 System.Type，产生海量二义性）。
  [ -f "$UNITY_48API/Facades/netstandard.dll" ] && REFS+=("/r:$UNITY_48API/Facades/netstandard.dll")
else
  # Unity .NET Standard 2.1 级别：netstandard 参照 + netfx 兼容垫片
  REFS+=("/r:$UNITY_NETSTD")
  for f in "$UNITY_SHIMS"/*.dll; do REFS+=("/r:$f"); done
fi
# Unity 模块（UnityEngine.* / UnityEditor.*）
# 若存在公开化副本，则「只引用公开化副本」，避免同一类型出现两份导致二义性。
PUBDIR="$ROOT/02-BuildTest/publicized"
if [ -d "$PUBDIR" ] && [ "$(ls "$PUBDIR"/*.dll 2>/dev/null | wc -l | tr -d ' ')" -gt 0 ]; then
  for f in "$PUBDIR"/*.dll; do REFS+=("/r:$f"); done
  echo "[信息] 使用公开化 Unity 参照: $(ls "$PUBDIR"/*.dll | wc -l | tr -d ' ') 个程序集"
else
  for f in "$UNITY_MODULES"/*.dll; do REFS+=("/r:$f"); done
  echo "[信息] 使用 Unity 原始参照（Reflection.Editor 预期会因 internal 访问失败）"
fi

ORDER=(
  "Sirenix.Utilities"
  "Sirenix.OdinInspector.Attributes"
  "Sirenix.Reflection.Editor"
  "Sirenix.Serialization.Config"
  "Sirenix.Serialization"
  "Sirenix.Utilities.Editor"
  "Sirenix.OdinInspector.Editor"
)

echo "csc         = $CSC"
echo "netstandard = $UNITY_NETSTD"
echo "Unity 模块  = $(ls "$UNITY_MODULES"/*.dll | wc -l | tr -d ' ') 个"
echo "参照总数    = ${#REFS[@]}"
echo "==============================================================="
echo "ASM|状态|错误数|Top 错误码" > "$REPORT/build-summary.txt"

for asm in "${ORDER[@]}"; do
  dir="$PATCHED/$asm"
  [ -d "$dir" ] || { echo "[SKIP] $asm"; continue; }

  SRCS=$(find "$dir" -name '*.cs' | sort)
  n=$(echo "$SRCS" | wc -l | tr -d ' ')
  out="$BIN/$asm.dll"
  errf="$REPORT/build-$asm.errors.txt"

  EXTRA=()
  for b in "$BIN"/Sirenix.*.dll; do
    [ -f "$b" ] || continue
    # 关键：绝不能把「正在编译的这个程序集」的上一次产物当引用喂回去，
    # 否则会造成自身类型重复定义，报出 "…GetNiceName(System.Type) 与自身二义性" 这类伪错误。
    [ "$(basename "$b")" = "$asm.dll" ] && continue
    EXTRA+=("/r:$b")
  done
  # 注：曾尝试用 IgnoresAccessChecksTo 垫片绕过 Unity internal 访问限制，
  #     经最小复现实测 Roslyn 不采纳该特性（public/internal 定义均报 CS0122），故弃用。

  rsp="$REPORT/rsp-$asm.rsp"
  {
    echo "/noconfig"; echo "/nostdlib+"; echo "/nologo"
    echo "/target:library"; echo "/langversion:9.0"; echo "/unsafe+"
    echo "/optimize-"; echo "/warn:0"
    echo "/out:\"$out\""
    printf '%s\n' "${REFS[@]}"
    if [ ${#EXTRA[@]} -gt 0 ]; then printf '%s\n' "${EXTRA[@]}"; fi
    echo "$SRCS" | sed 's/^/"/; s/$/"/'
  } > "$rsp"

  echo "[BUILD] $asm  ($n 文件) ..."
  "$DOTNET" exec "$CSC" "@$rsp" > "$errf" 2>&1

  nerr=$(grep -cE "error CS" "$errf")
  if [ -f "$out" ] && [ "$nerr" -eq 0 ]; then
    printf "[OK]   %-34s 错误=0\n" "$asm"
    echo "$asm|OK|0|" >> "$REPORT/build-summary.txt"
  else
    top=$(grep -oE "error CS[0-9]+" "$errf" | sort | uniq -c | sort -rn | head -4 | awk '{printf "%s×%s ", $2, $1}')
    printf "[FAIL] %-34s 错误=%s  %s\n" "$asm" "$nerr" "$top"
    echo "$asm|FAIL|$nerr|$top" >> "$REPORT/build-summary.txt"
  fi
done

echo "==============================================================="
echo "=== 汇总 ==="
cat "$REPORT/build-summary.txt"
echo
echo "=== 全局错误类型 Top15 ==="
cat "$REPORT"/build-Sirenix.*.errors.txt 2>/dev/null \
  | grep -oE "error CS[0-9]+: [^[]*" | sed 's/error CS[0-9]*: //' \
  | sort | uniq -c | sort -rn | head -15
