#!/usr/bin/env bash
# 导出 AesirInspector .unitypackage —— 与 .github/workflows/export-package.yml 同一方案（纯 .NET，无需启动 Unity）
#
# 用法：
#   Scripts/export-package.sh    导出到 Builds/AesirInspector-<version>.unitypackage
#
# 发布：推送 v* 标签（如 v0.14.1），GitHub Actions 自动导出并创建 Release。
#
# 首次运行会自动安装 .NET 8 SDK 到 ~/.dotnet（用户目录，免 sudo），
# 并将导出工具缓存到 ~/.cache/aesir-inspector/（固定 commit）。
set -euo pipefail

TOOL_REPO="https://github.com/Guardingpearsoftware/public-unity-package-exporter"
TOOL_REPO_SSH="git@github.com:Guardingpearsoftware/public-unity-package-exporter.git"
TOOL_COMMIT="91d0bcbd9169f36eff01ce07a8ff3b7a64c6a252"
PACKAGE_DIR="Assets/Runestone/AesirInspector"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

# --- dotnet: PATH -> ~/.dotnet -> auto install (user dir, no sudo) ---
if ! command -v dotnet >/dev/null 2>&1; then
    if [ -x "$HOME/.dotnet/dotnet" ]; then
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$DOTNET_ROOT:$PATH"
    else
        echo "==> dotnet not found, installing .NET 8 SDK to $HOME/.dotnet ..."
        curl -sSL https://dot.net/v1/dotnet-install.sh -o "${TMPDIR:-/tmp}/dotnet-install.sh"
        bash "${TMPDIR:-/tmp}/dotnet-install.sh" --channel 8.0 --install-dir "$HOME/.dotnet" --no-path
        export DOTNET_ROOT="$HOME/.dotnet"
        export PATH="$DOTNET_ROOT:$PATH"
    fi
fi

# --- exporter tool: pinned-commit cache ---
CACHE_DIR="${XDG_CACHE_HOME:-$HOME/.cache}/aesir-inspector/unity-package-exporter"
if [ ! -f "$CACHE_DIR/.git/HEAD" ] || [ "$(git -C "$CACHE_DIR" rev-parse HEAD 2>/dev/null)" != "$TOOL_COMMIT" ]; then
    echo "==> Fetching exporter tool: $TOOL_COMMIT"
    rm -rf "$CACHE_DIR"
    git init -q "$CACHE_DIR"
    git -C "$CACHE_DIR" remote add origin "$TOOL_REPO"
    if ! git -C "$CACHE_DIR" fetch -q --depth 1 origin "$TOOL_COMMIT" 2>/dev/null; then
        echo "==> HTTPS unreachable, falling back to SSH ..."
        git -C "$CACHE_DIR" remote set-url origin "$TOOL_REPO_SSH"
        git -C "$CACHE_DIR" fetch -q --depth 1 origin "$TOOL_COMMIT"
    fi
    git -C "$CACHE_DIR" checkout -q FETCH_HEAD
fi
BIN="$CACHE_DIR/bin"
if [ ! -f "$BIN/UnityPackageExporter.dll" ]; then
    echo "==> Building exporter tool ..."
    dotnet publish -c Release -o "$BIN" "$CACHE_DIR/UnityPackageExporter"
fi

# --- version & export (excludes must match the workflow; hidden ~ dirs have no .meta) ---
VERSION="$(grep -m1 '"version"' "$PACKAGE_DIR/package.json" | sed -E 's/.*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/')"
ARTIFACT="Builds/AesirInspector-$VERSION.unitypackage"
mkdir -p Builds

echo "==> Version: $VERSION"
echo "==> Output:  $ARTIFACT"
dotnet "$BIN/UnityPackageExporter.dll" . "$ARTIFACT" \
    -a "$PACKAGE_DIR/**" \
    -e "Library/**" \
    -e "**/.*" \
    -e "**/Samples~/**" \
    -e "**/Documentation~/**" \
    --skip-dependency-check \
    -v Warning

[ -f "$ARTIFACT" ] || { echo "ERROR: export failed" >&2; exit 1; }
echo "==> Exported: $ARTIFACT"
echo "==> To release: push a v* tag (e.g. v$VERSION), GitHub Actions will build the Release"
