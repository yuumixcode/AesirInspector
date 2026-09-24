#!/bin/bash
# =============================================================================
# Odin Inspector 反编译流水线 - Step 2a: 生成 Unity 参照程序集「公开化」副本
# -----------------------------------------------------------------------------
# 目的: 让 Sirenix.Reflection.Editor 能从源码重建。
#       该程序集直接使用 Unity 的 internal 类型：
#         UnityEngine.GUILayoutEntry / GUILayoutGroup / ScrollViewState
#         UnityEngine.UIElements.Panel / UnityEditor 的 ContainerWindow ... (flags=0x100000)
#       而 Unity 官方参照程序集不对外开放这些类型，且 Roslyn 不采纳
#       IgnoresAccessChecksTo。因此生成一份成员全公开的参照副本，仅用于编译期。
#
# 输出: 02-BuildTest/publicized/*.dll
# =============================================================================
set -o pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WORK="$ROOT/02-BuildTest/publicize-build"
OUT="$ROOT/02-BuildTest/publicized"

A="/Applications/Unity/Hub/Editor/2021.3.45f2c1/Unity.app/Contents"
UNITY_MANAGED="$A/Managed"
UNITY_MODULES="$A/Managed/UnityEngine"
UNITY_48API="$A/UnityReferenceAssemblies/unity-4.8-api"
MONO_API="/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.7.1-api"

DOTNET="$HOME/.dotnet/dotnet"
CSC="$HOME/.dotnet/sdk/8.0.424/Roslyn/bincore/csc.dll"
CECIL="$UNITY_MANAGED/Unity.Cecil.dll"

mkdir -p "$WORK" "$OUT"

[ -f "$CECIL" ] || { echo "未找到 Unity.Cecil.dll: $CECIL"; exit 1; }

echo "=== 1) 编译公开化工具 ==="
"$DOTNET" exec "$CSC" /noconfig /nostdlib+ /nologo /target:exe \
  /langversion:9.0 /warn:0 \
  /out:"$WORK/PublicizeUnity.exe" \
  "/r:$MONO_API/mscorlib.dll" "/r:$MONO_API/System.dll" "/r:$MONO_API/System.Core.dll" \
  "/r:$CECIL" \
  "$ROOT/tools/publicize/PublicizeUnity.cs" 2>&1 | head -20

[ -f "$WORK/PublicizeUnity.exe" ] || { echo "工具编译失败"; exit 1; }
echo "  -> $WORK/PublicizeUnity.exe"

# Unity.Cecil 是强名称程序集，运行时必须与 exe 同目录（或可被探测到）
for d in Unity.Cecil.dll Unity.Cecil.Pdb.dll Unity.Cecil.Mdb.dll Unity.Cecil.Rocks.dll; do
  [ -f "$UNITY_MANAGED/$d" ] && cp -f "$UNITY_MANAGED/$d" "$WORK/"
done

echo
echo "=== 2) 公开化 Unity 模块 ==="
export PATH="/Library/Frameworks/Mono.framework/Versions/Current/Commands:$PATH"
mono "$WORK/PublicizeUnity.exe" "$OUT" "$UNITY_MODULES" "$UNITY_MANAGED" "$UNITY_48API" "$MONO_API" 2>&1 | tail -25

echo
echo "输出程序集数: $(ls "$OUT"/*.dll 2>/dev/null | wc -l | tr -d ' ')"
