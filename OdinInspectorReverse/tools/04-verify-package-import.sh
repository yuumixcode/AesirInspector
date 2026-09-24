#!/bin/bash
# =============================================================================
# Odin Inspector 反编译流水线 - Step 4: Unity 导入冒烟测试
# -----------------------------------------------------------------------------
# 目的: 在 /tmp 下建一个独立临时工程，把成品 UPM 包装进去，用 Unity 批处理模式
#       实际导入一次，验证：
#         1. 没有编译错误（重复程序集 / 缺失引用）
#         2. SirenixAssetPaths 的路径自举是否成功 —— 这是包化是否成立的核心判据。
#            失败时会打印 "There were some problems trying to locate where Odin
#            was installed"。
#       注意：完全不动用户现有工程。
# =============================================================================
set -o pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PKG="$ROOT/02-Package/com.sirenix.odin-inspector"
TEST=/tmp/OdinPkgTest
UNITY="/Applications/Unity/Hub/Editor/2021.3.45f2c1/Unity.app/Contents/MacOS/Unity"
VER="2021.3.45f2c1"

[ -d "$PKG" ] || { echo "成品包不存在，请先运行 03a-build-dll-package.sh"; exit 1; }
[ -x "$UNITY" ] || { echo "未找到 Unity: $UNITY"; exit 1; }

echo "=== 1) 搭建临时工程 $TEST ==="
rm -rf "$TEST"
mkdir -p "$TEST/Assets" "$TEST/Packages" "$TEST/ProjectSettings"

printf 'm_EditorVersion: %s\n' "$VER" > "$TEST/ProjectSettings/ProjectVersion.txt"

cat > "$TEST/Packages/manifest.json" <<'JSON'
{
  "dependencies": {
    "com.unity.ide.rider": "3.0.31",
    "com.unity.ide.visualstudio": "2.0.22",
    "com.unity.test-framework": "1.1.33",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.timeline": "1.6.5",
    "com.unity.ugui": "1.0.0",
    "com.unity.modules.ai": "1.0.0",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
    "com.unity.modules.terrain": "1.0.0",
    "com.unity.modules.tilemap": "1.0.0",
    "com.unity.modules.ui": "1.0.0",
    "com.unity.modules.uielements": "1.0.0",
    "com.unity.modules.umbra": "1.0.0",
    "com.unity.modules.unitywebrequest": "1.0.0",
    "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
    "com.unity.modules.video": "1.0.0"
  }
}
JSON

echo "  复制成品包到 Packages/ ..."
cp -R "$PKG" "$TEST/Packages/com.sirenix.odin-inspector"

# 探针：主动触发 SirenixAssetPaths 的静态构造并打印解析结果。
# 这是包化成立与否的判决性证据 —— 空工程里 Odin 的静态构造可能不会被触发。
mkdir -p "$TEST/Assets/Editor"
cat > "$TEST/Assets/Editor/OdinPathProbe.cs" <<'CS'
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public static class OdinPathProbe
{
    [InitializeOnLoadMethod]
    public static void Probe()
    {
        Debug.Log("[ODIN-PROBE] SirenixPluginPath    = " + SirenixAssetPaths.SirenixPluginPath);
        Debug.Log("[ODIN-PROBE] OdinPath             = " + SirenixAssetPaths.OdinPath);
        Debug.Log("[ODIN-PROBE] SirenixAssembliesPath= " + SirenixAssetPaths.SirenixAssembliesPath);
        Debug.Log("[ODIN-PROBE] OdinEditorConfigsPath= " + SirenixAssetPaths.OdinEditorConfigsPath);

        var loaded = System.AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetName().Name)
            .Where(n => n.StartsWith("Sirenix"))
            .OrderBy(n => n).ToArray();
        Debug.Log("[ODIN-PROBE] 已加载的 Sirenix 程序集 (" + loaded.Length + "): " + string.Join(", ", loaded));
    }
}
CS

echo
echo "=== 2) Unity 批处理导入 ==="
LOG="$TEST/unity-import.log"
"$UNITY" -batchmode -nographics -quit \
  -projectPath "$TEST" \
  -logFile "$LOG" \
  -accept-apiupdate 2>&1 | tail -5

echo "  退出码: $?"
echo
echo "=== 3) 结果分析 ==="
echo
echo "--- 编译错误 ---"
grep -E "error CS[0-9]+|Compilation failed|Assembly .* will not be loaded|Multiple precompiled assemblies" "$LOG" 2>/dev/null | sort -u | head -20
ec=$(grep -cE "error CS[0-9]+" "$LOG" 2>/dev/null)
echo "  error CS 计数: $ec"

echo
echo "--- 程序集重复检测 ---"
grep -iE "multiple precompiled|already contains|duplicate" "$LOG" 2>/dev/null | head -10
echo "  (空 = 无重复程序集)"

echo
echo "--- SirenixAssetPaths 路径自举（核心判据）---"
if grep -q "problems trying to locate where Odin was installed" "$LOG" 2>/dev/null; then
  echo "  ✘ 路径自举失败！Odin 无法定位自身安装位置"
  grep -A6 "problems trying to locate where Odin was installed" "$LOG" | head -12
else
  echo "  ✓ 未出现路径定位失败报错"
fi

echo
echo "--- 探针输出：路径解析与程序集加载（判决性证据）---"
grep "ODIN-PROBE" "$LOG" 2>/dev/null | sort -u
probe_n=$(grep -c "ODIN-PROBE" "$LOG" 2>/dev/null)
echo "  探针输出行数: $probe_n"
if grep -q "ODIN-PROBE" "$LOG" 2>/dev/null; then
  if grep -q "SirenixPluginPath    = Packages/com.sirenix.odin-inspector/" "$LOG" 2>/dev/null; then
    echo "  ✓ 路径自举成功：SirenixPluginPath 正确定位到包内"
  else
    echo "  ⚠ 路径已解析，请核对上面的实际取值是否符合预期"
  fi
else
  echo "  ✘ 探针未执行 —— 说明探针脚本编译失败或 Odin 类型不可用"
fi

echo
echo "--- Odin 相关日志行 ---"
grep -iE "sirenix|odin" "$LOG" 2>/dev/null | grep -viE "^\(Filename" | sort -u | head -20
echo "  (空 = Odin 静默导入，通常表示正常)"

echo
echo "--- Unity 导入是否正常结束 ---"
if grep -qE "Batchmode quit|Exiting batchmode|Application will quit" "$LOG" 2>/dev/null; then
  echo "  ✓ 批处理正常退出"
else
  echo "  ? 未见正常退出标记，请查看 $LOG"
fi
echo
echo "完整日志: $LOG"
