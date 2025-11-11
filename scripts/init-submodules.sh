#!/usr/bin/env bash
#
# init-submodules.sh
# Cross-platform helper to initialize submodules using .gitmodules.
# Usage: ./scripts/init-submodules.sh
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
GITMODULES="$REPO_ROOT/.gitmodules"

if [ ! -f "$GITMODULES" ]; then
  echo "No .gitmodules file found at $GITMODULES. Nothing to do."
  exit 0
fi

echo "Trying: git -C $REPO_ROOT submodule update --init --recursive"
if git -C "$REPO_ROOT" submodule update --init --recursive; then
  echo "Submodules initialized successfully."
  exit 0
fi

echo "Falling back to cloning submodules listed in .gitmodules"
git config --file "$GITMODULES" --name-only --get-regexp path | while read -r path_key; do
  path=$(git config --file "$GITMODULES" --get "$path_key")
  url_key=$(echo "$path_key" | sed 's/\.path$/.url/')
  url=$(git config --file "$GITMODULES" --get "$url_key")

  target="$REPO_ROOT/$path"
  if [ -d "$target" ]; then
    echo "Path '$path' already exists — skipping."
    continue
  fi

  if [ -z "$url" ]; then
    echo "No URL for $path — skipping."
    continue
  fi

  echo "Cloning $url into $path"
  git -C "$REPO_ROOT" clone --depth 1 "$url" "$path" || echo "Failed to clone $url into $path"
done

echo "Done. If any submodules still missing, try: git submodule update --init --recursive"
