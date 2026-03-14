#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
publish_root="${1:-$project_root/artifacts/publish/avalonia/linux-x64}"
executable_path="$publish_root/App4di.Dotnet.ChronoView.AvaloniaUI"
desktop_dir="${XDG_DATA_HOME:-$HOME/.local/share}/applications"
icon_dir="${XDG_DATA_HOME:-$HOME/.local/share}/icons/hicolor/512x512/apps"
desktop_file="$desktop_dir/ChronoView.desktop"
icon_file="$icon_dir/chronoview.png"

if [[ ! -x "$executable_path" ]]; then
  echo "Executable not found: $executable_path" >&2
  exit 1
fi

mkdir -p "$desktop_dir" "$icon_dir"
cp "$project_root/Project/App4di.Dotnet.ChronoView.AvaloniaUI/Assets/ChronoViewLogo.png" "$icon_file"

cat > "$desktop_file" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=ChronoView
Comment=ChronoView timeline viewer
Exec=$executable_path
Icon=chronoview
Terminal=false
Categories=Utility;
StartupWMClass=ChronoView
EOF

chmod +x "$desktop_file"

if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$desktop_dir" >/dev/null 2>&1 || true
fi

if command -v gtk-update-icon-cache >/dev/null 2>&1; then
  gtk-update-icon-cache "${XDG_DATA_HOME:-$HOME/.local/share}/icons/hicolor" >/dev/null 2>&1 || true
fi

echo "Installed desktop entry: $desktop_file"
echo "Installed icon: $icon_file"
