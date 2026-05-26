#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="${1:-}"

if [[ -z "$VERSION" ]]; then
  if git -C "$ROOT_DIR" describe --tags --always --dirty >/dev/null 2>&1; then
    VERSION="$(git -C "$ROOT_DIR" describe --tags --always --dirty)"
  else
    VERSION="dev"
  fi
fi

DIST_DIR="$ROOT_DIR/dist"
STAGE_DIR="$DIST_DIR/stage"

rm -rf "$DIST_DIR"
mkdir -p "$STAGE_DIR"

copy_common_files() {
  local target="$1"
  cp "$ROOT_DIR/LICENSE" "$target/LICENSE"
  cp "$ROOT_DIR/README.md" "$target/README.md"
}

make_archive() {
  local name="$1"
  local target="$STAGE_DIR/$name"

  mkdir -p "$target"
  copy_common_files "$target"

  shift
  for path in "$@"; do
    mkdir -p "$target/$(dirname "$path")"
    cp -R "$ROOT_DIR/$path" "$target/$path"
  done

  (
    cd "$STAGE_DIR"
    zip -qr "$DIST_DIR/$name-$VERSION.zip" "$name"
  )
}

make_archive "WalkSimController-src" \
  "src/WalkSimController"

make_archive "WalkSimController-godot" \
  "src/WalkSimController" \
  "samples/Godot"

make_archive "WalkSimController-unity" \
  "src/WalkSimController" \
  "samples/Unity"

rm -rf "$STAGE_DIR"

printf 'Created release archives in %s:\n' "$DIST_DIR"
find "$DIST_DIR" -maxdepth 1 -name '*.zip' -type f -print | sort
