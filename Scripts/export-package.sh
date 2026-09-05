#!/usr/bin/env bash
# 导出 Aesir Inspector .unitypackage；可选 --release 同时创建 GitHub Release
#
# 用法：
#   Scripts/export-package.sh             导出 .unitypackage 到 Builds/
#   Scripts/export-package.sh --release   导出 + 打 tag 推送 + 创建 GitHub Release（附包与 CHANGELOG 说明）
#
# 环境变量：
#   UNITY_PATH  Unity 可执行文件路径（默认按 ProjectVersion.txt 自动拼接）
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PKG="$ROOT/Assets/Runestone/AesirInspector"
VERSION="$(grep -m1 '"version"' "$PKG/package.json" | sed -E 's/.*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')"
UNITY_VERSION="$(awk '/m_EditorVersion:/ {print $2}' "$ROOT/ProjectSettings/ProjectVersion.txt" | head -n1)"
UNITY_PATH="${UNITY_PATH:-/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity}"
BUILD_DIR="$ROOT/Builds"
ARTIFACT="$BUILD_DIR/AesirInspector-$VERSION.unitypackage"

echo "==> Version: $VERSION"
echo "==> Unity:   $UNITY_PATH"
echo "==> Output:  $ARTIFACT"

if [[ ! -x "$UNITY_PATH" ]]; then
    echo "ERROR: Unity not found at $UNITY_PATH (set UNITY_PATH to override)" >&2
    exit 1
fi

mkdir -p "$BUILD_DIR"
LOG="$BUILD_DIR/unity-export.log"

echo "==> Exporting package via Unity batchmode (first run may take a few minutes)..."
if ! "$UNITY_PATH" -batchmode -quit -projectPath "$ROOT" \
    -executeMethod Runestone.AesirInspector.Editor.AesirInspectorPackageExporter.ExportCurrentVersion \
    -logFile "$LOG"; then
    echo "ERROR: export failed, last 40 log lines:" >&2
    tail -n 40 "$LOG" >&2
    exit 1
fi

if [[ ! -f "$ARTIFACT" ]]; then
    echo "ERROR: artifact not produced, last 40 log lines:" >&2
    tail -n 40 "$LOG" >&2
    exit 1
fi
echo "==> Exported: $ARTIFACT"

if [[ "${1:-}" == "--release" ]]; then
    TAG="v$VERSION"
    cd "$ROOT"
    if git rev-parse -q --verify "refs/tags/$TAG" >/dev/null; then
        echo "==> Tag $TAG already exists, skip tagging"
    else
        git tag "$TAG"
        git push origin "$TAG"
        echo "==> Tag pushed: $TAG"
    fi

    NOTES="$(mktemp)"
    awk -v v="$VERSION" '
        index($0, "## [" v "]") == 1 {found = 1; next}
        found && /^## \[/ {exit}
        found {print}
    ' "$PKG/CHANGELOG.md" > "$NOTES"
    if [[ ! -s "$NOTES" ]]; then
        echo "ERROR: no changelog section found for $VERSION" >&2
        exit 1
    fi

    if gh release view "$TAG" >/dev/null 2>&1; then
        echo "==> Release $TAG already exists, uploading artifact"
        gh release upload "$TAG" "$ARTIFACT" --clobber
    else
        gh release create "$TAG" "$ARTIFACT" --title "Aesir Inspector $VERSION" --notes-file "$NOTES"
    fi
    rm -f "$NOTES"
    echo "==> Release ready: https://github.com/yuumixcode/AesirInspector/releases/tag/$TAG"
fi
