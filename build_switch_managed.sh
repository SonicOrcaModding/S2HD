#!/bin/sh
set -e

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
OUT_DIR="$SCRIPT_DIR/bin/Switch/Release"
PACKAGE_ROOT="$SCRIPT_DIR/bin/Switch/package"
SWITCH_DIR="$PACKAGE_ROOT/switch"
PACKAGE_DIR="$SWITCH_DIR/S2HD"
MONO_DIR="$PACKAGE_ROOT/mono"
MONO_NX_DIR="${MONO_NX_DIR:-$SCRIPT_DIR/../mono-nx}"
MONO_NX_ROOT="${MONO_NX_ROOT:-$MONO_NX_DIR/dotnet_runtime}"
MONO_NX_CONFIGURATION="${MONO_NX_CONFIGURATION:-Debug}"
ICU_NX_INSTALL_DIR="${ICU_NX_INSTALL_DIR:-$MONO_NX_DIR/icu/libnx}"

export NUGET_PACKAGES="${NUGET_PACKAGES:-$SCRIPT_DIR/bin/Switch/nuget-packages}"

find_default_opentk() {
  for candidate in \
    "$SCRIPT_DIR/../OpenTK/src/OpenTK/bin/Switch/Release/OpenTK.dll" \
    "$SCRIPT_DIR/../OpenTK/src/OpenTK/bin/Release/OpenTK.dll" \
    "$NUGET_PACKAGES/opentk/3.3.3/lib/net20/OpenTK.dll"
  do
    if [ -f "$candidate" ]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  return 1
}

if [ -z "${OPENTK_DLL:-}" ]; then
  OPENTK_DLL="$(find_default_opentk || true)"
fi

if [ -z "$OPENTK_DLL" ]; then
  printf 'Missing expected package file: OpenTK.dll. Set OPENTK_DLL or build the OpenTK fork.\n' >&2
  exit 1
fi

PROJECTS="
external/SonicOrca.Common/SonicOrca.Common.csproj
external/SonicOrca.Resources/SonicOrca.Resources.csproj
external/SonicOrca/SonicOrca.csproj
external/SonicOrca.Drawing/SonicOrca.Drawing.csproj
external/SonicOrca.SDL2/SonicOrca.SDL2.csproj
S2HD.csproj
"

for project in $PROJECTS; do
  dotnet restore "$SCRIPT_DIR/$project" -p:Platform=Switch -p:OpenTKSwitchAssembly="$OPENTK_DLL"
done

for project in $PROJECTS; do
  dotnet build "$SCRIPT_DIR/$project" -c Release -p:Platform=Switch -p:OpenTKSwitchAssembly="$OPENTK_DLL" --no-restore
done

rm -rf "$PACKAGE_ROOT"
mkdir -p "$PACKAGE_DIR"
cp -R "$OUT_DIR"/. "$PACKAGE_DIR"/

copy_required_file() {
  source_path="$1"
  if [ -f "$source_path" ]; then
    cp "$source_path" "$PACKAGE_DIR"/
  else
    printf 'Missing expected package file: %s\n' "$source_path" >&2
    exit 1
  fi
}

copy_required_file "$OPENTK_DLL"

if [ -f "$OPENTK_DLL.config" ]; then
  cp "$OPENTK_DLL.config" "$PACKAGE_DIR"/
fi

copy_required_file "$NUGET_PACKAGES/system.drawing.common/8.0.10/lib/net8.0/System.Drawing.Common.dll"

if [ -d "$SCRIPT_DIR/data" ]; then
  cp -R "$SCRIPT_DIR/data" "$PACKAGE_DIR"/data
fi

if [ -d "$SCRIPT_DIR/Shaders.Switch" ]; then
  python3 "$SCRIPT_DIR/tools/validate_switch_shaders.py" "$SCRIPT_DIR/Shaders.Switch"
  cp -R "$SCRIPT_DIR/Shaders.Switch" "$PACKAGE_DIR"/Shaders
elif [ -d "$SCRIPT_DIR/Shaders" ]; then
  cp -R "$SCRIPT_DIR/Shaders" "$PACKAGE_DIR"/Shaders
fi

if [ -d "$SCRIPT_DIR/mods" ]; then
  cp -R "$SCRIPT_DIR/mods" "$PACKAGE_DIR"/mods
fi

MONO_NX_SD_MONO_DIR="${MONO_NX_SD_MONO_DIR:-$SCRIPT_DIR/../mono-nx/sd_files/mono}"
if [ -d "$MONO_NX_SD_MONO_DIR" ]; then
  mkdir -p "$MONO_DIR"
  cp -R "$MONO_NX_SD_MONO_DIR"/. "$MONO_DIR"/
fi

MONO_LIB_DIR="$MONO_NX_ROOT/artifacts/bin/mono/libnx.arm64.$MONO_NX_CONFIGURATION"
if [ ! -d "$MONO_LIB_DIR" ]; then
  printf 'Missing mono runtime dir: %s\n' "$MONO_LIB_DIR" >&2
  exit 1
fi
mkdir -p "$MONO_DIR/lib_net9.0"
cp "$MONO_LIB_DIR"/*.dll "$MONO_DIR/lib_net9.0"/

MONO_FRAMEWORK_DIR="$MONO_NX_ROOT/artifacts/bin/runtime/net9.0-libnx-$MONO_NX_CONFIGURATION-arm64"
if [ ! -d "$MONO_FRAMEWORK_DIR" ]; then
  printf 'Missing mono framework dir: %s\n' "$MONO_FRAMEWORK_DIR" >&2
  exit 1
fi
mkdir -p "$MONO_DIR/framework_net9.0"
cp "$MONO_FRAMEWORK_DIR"/*.dll "$MONO_DIR/framework_net9.0"/

if [ -f "$ICU_NX_INSTALL_DIR/share/icu/77.1/icudt77l.dat" ]; then
  mkdir -p "$MONO_DIR/etc"
  cp "$ICU_NX_INSTALL_DIR/share/icu/77.1/icudt77l.dat" "$MONO_DIR/etc"/
fi

if [ -f "$MONO_NX_DIR/native/interpreter/mono_nx.nro" ]; then
  mkdir -p "$MONO_DIR"
  cp "$MONO_NX_DIR/native/interpreter/mono_nx.nro" "$MONO_DIR"/
fi

if [ -f "$SCRIPT_DIR/switch/mono/config.s2hd.ini" ]; then
  mkdir -p "$MONO_DIR"
  cp "$SCRIPT_DIR/switch/mono/config.s2hd.ini" "$MONO_DIR"/config.ini
fi

printf 'Switch SD package staged at %s\n' "$PACKAGE_ROOT"
