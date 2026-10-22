#!/usr/bin/env bash
# Serve the WebGL build on localhost.
#
# The build is compressed with Gzip and has the decompression fallback on, which means the
# loader unpacks the .unityweb files in JavaScript when the server does not send
# Content-Encoding: gzip. A plain static server is therefore enough, and that is the whole
# reason the fallback is enabled — it costs a little load time and removes the one way a
# WebGL build comes up blank for someone who just wants to look at it.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
build="$root/Build/WebGL"
port="${1:-8080}"

if [[ ! -f "$build/index.html" ]]; then
    echo "No build at $build." >&2
    echo "Run tools/build-webgl.sh first (about five minutes, needs the editor free)." >&2
    exit 1
fi

echo "Neko Maid Inc. — http://localhost:$port"
echo "Ctrl+C to stop."
exec python3 -m http.server "$port" --directory "$build" --bind 127.0.0.1
