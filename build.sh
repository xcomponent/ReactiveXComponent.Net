#!/usr/bin/env bash

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This file was downloaded from https://github.com/cake-build/resources
# Feel free to change this file to fit your needs.
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
NUGET_EXE=$TOOLS_DIR/nuget.exe
PACKAGES_CSPROJ=$TOOLS_DIR/packages.csproj
PACKAGES_CSPROJ_MD5=$TOOLS_DIR/packages.csproj.md5sum

# Define md5sum or md5 depending on Linux/OSX
MD5_EXE=
if [[ "$(uname -s)" == "Darwin" ]]; then
    MD5_EXE="md5 -r"
else
    MD5_EXE="md5sum"
fi

# Define default arguments.
SCRIPT="./build.cake"
TARGET="Build"
CONFIGURATION="Release"
VERBOSITY="verbose"
DRYRUN=
SHOW_VERSION=false
SCRIPT_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN="-dryrun" ;;
        --version) SHOW_VERSION=true ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

echo "TEEEEEEEEEEEEEEEEEEEEEEEEESSSSSSSSSSSSSSSSSSSSSSST: " + $NUGET_EXE

# Download NuGet if it does not exist.
if [ ! -f "$NUGET_EXE" ]; then
    echo "Downloading NuGet..."
    curl -Lsfo "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/v4.4.1/nuget.exe
    if [ $? -ne 0 ]; then
        echo "An error occured while downloading nuget.exe."
        exit 1
    fi
fi

# Restore tools from NuGet.
pushd "$TOOLS_DIR" >/dev/null
if [ ! -f $PACKAGES_CSPROJ_MD5 ] || [ "$( cat $PACKAGES_CSPROJ_MD5 | sed 's/\r$//' )" != "$( $MD5_EXE $PACKAGES_CSPROJ | awk '{ print $1 }' )" ]; then
    find . -type d ! -name . | xargs rm -rf
fi

dotnet restore $PACKAGES_CSPROJ --packages $TOOLS_DIR
if [ $? -ne 0 ]; then
    echo "Could not restore NuGet packages."
    exit 1
fi

$MD5_EXE $PACKAGES_CSPROJ | awk '{ print $1 }' >| $PACKAGES_CSPROJ_MD5

popd >/dev/null

# Make sure that Cake has been installed.
CAKE_DLL="$(find ./tools/ -name Cake.dll)"
if [ ! -f "$CAKE_DLL" ]; then
    echo "Could not find Cake.dll in tools folder."
    exit 1
fi

# Start Cake
if $SHOW_VERSION; then
    dotnet "$CAKE_DLL" -version
else
    dotnet "$CAKE_DLL" $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET $DRYRUN ${SCRIPT_ARGUMENTS[@]}
fi