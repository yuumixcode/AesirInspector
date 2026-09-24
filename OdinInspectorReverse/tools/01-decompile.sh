#!/bin/bash
# =============================================================================
# Odin Inspector 反编译流水线 - Step 1: DLL -> C# Source
# -----------------------------------------------------------------------------
# 工具链: .NET SDK 8.0.424 (~/.dotnet) + ilspycmd 9.1.0.7988 (ILSpy core)
# 输入:  00-OriginalAssemblies/*.dll (+ .pdb 用于恢复局部变量名)
# 输出:  01-Decompiled/<AssemblyName>/*.cs
# =============================================================================
set -uo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="$ROOT/00-OriginalAssemblies"
OUT="$ROOT/01-Decompiled"
LOG="$ROOT/report/decompile.log"

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

# Unity 2021.3 引用程序集目录（含 UnityEngine.* 与 UnityEditor.* 模块）
UNITY_MANAGED="/Applications/Unity/Hub/Editor/2021.3.45f2c1/Unity.app/Contents/Managed/UnityEngine"

# 需要反编译的 7 个托管程序集（排除 NoEditor / NoEmitAndNoEditor 变体）
ASSEMBLIES=(
  "Sirenix.Utilities"
  "Sirenix.Serialization.Config"
  "Sirenix.Serialization"
  "Sirenix.OdinInspector.Attributes"
  "Sirenix.Utilities.Editor"
  "Sirenix.Reflection.Editor"
  "Sirenix.OdinInspector.Editor"
)

mkdir -p "$OUT" "$(dirname "$LOG")"
: > "$LOG"

echo "ROOT      = $ROOT"
echo "ILSpy     = $(ilspycmd --version 2>&1 | head -1)"
echo "RefPath   = $UNITY_MANAGED"
echo "==============================================================="

for asm in "${ASSEMBLIES[@]}"; do
  dll="$SRC/$asm.dll"
  dest="$OUT/$asm"
  if [ ! -f "$dll" ]; then
    echo "[SKIP] $asm.dll 不存在" | tee -a "$LOG"
    continue
  fi
  echo "[DECOMPILE] $asm ..."
  rm -rf "$dest"

  # -p                       生成可编译工程结构
  # --nested-directories     目录按命名空间分层（便于后续映射到 asmdef 分区）
  # --use-varnames-from-pdb  借 PDB 还原局部变量名，显著提升可读性
  # -lv CSharp9_0            对齐 Unity 2021.3 的 C# 9 语言级别
  # -r                       注入 Unity 引用，提高类型解析率
  ilspycmd "$dll" \
    -o "$dest" \
    -p \
    --nested-directories \
    --use-varnames-from-pdb \
    -lv CSharp9_0 \
    -r "$UNITY_MANAGED" \
    --disable-updatecheck \
    > "$ROOT/report/$asm.log" 2>&1

  if [ -d "$dest" ]; then
    n=$(find "$dest" -name '*.cs' | wc -l | tr -d ' ')
    l=$(find "$dest" -name '*.cs' -exec cat {} + 2>/dev/null | wc -l | tr -d ' ')
    printf "[OK]   %-38s files=%-5s lines=%s\n" "$asm" "$n" "$l" | tee -a "$LOG"
  else
    echo "[FAIL] $asm  (见 report/$asm.log)" | tee -a "$LOG"
  fi
done

echo "==============================================================="
echo "汇总:"
find "$OUT" -name '*.cs' | wc -l | xargs echo "  总 .cs 文件数:"
find "$OUT" -name '*.cs' -exec cat {} + 2>/dev/null | wc -l | xargs echo "  总行数:"
