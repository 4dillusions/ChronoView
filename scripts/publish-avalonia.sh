#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project_file="$project_root/Project/App4di.Dotnet.ChronoView.AvaloniaUI/App4di.Dotnet.ChronoView.AvaloniaUI.csproj"
linux_icon_source="$project_root/Project/App4di.Dotnet.ChronoView.AvaloniaUI/Assets/ChronoViewLogo.png"
configuration="${1:-Release}"
output_root="$project_root/artifacts/publish/avalonia"

runtimes=(
  "win-x64"
  "linux-x64"
)

for runtime in "${runtimes[@]}"; do
  output_dir="$output_root/$runtime"

  echo "Publishing $runtime ($configuration) to $output_dir"

  dotnet publish "$project_file" \
    -c "$configuration" \
    -r "$runtime" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=false \
    -o "$output_dir"
done

linux_output_dir="$output_root/linux-x64"
linux_executable="$linux_output_dir/App4di.Dotnet.ChronoView.AvaloniaUI"

if [[ -f "$linux_executable" ]]; then
  cp "$linux_icon_source" "$linux_output_dir/ChronoViewLogo.png"
  cat > "$linux_output_dir/ChronoView.desktop" <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=ChronoView
Comment=ChronoView timeline viewer
Exec=$linux_executable
Icon=$linux_output_dir/ChronoViewLogo.png
Terminal=false
Categories=Utility;
StartupWMClass=ChronoView
EOF
  chmod +x "$linux_output_dir/ChronoView.desktop"
fi

echo "Done. Outputs are under $output_root"
