#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project_file="$project_root/Project/App4di.Dotnet.ChronoView.AvaloniaUI/App4di.Dotnet.ChronoView.AvaloniaUI.csproj"
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

echo "Done. Outputs are under $output_root"
