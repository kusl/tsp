#!/bin/bash

# TSP Solver Installation Script for Linux (Simple Version)
# Installs the latest kusl/tsp release to /opt/kusl-tsp

set -euo pipefail

# Configuration
GITHUB_REPO="kusl/tsp"
INSTALL_DIR="/opt"
BINARY_NAME="kusl-tsp"
INSTALL_PATH="${INSTALL_DIR}/${BINARY_NAME}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

echo "================================================="
echo " TSP Solver Installation Script"
echo " Repository: https://github.com/${GITHUB_REPO}"
echo "================================================="
echo

# Check if running as root
if [[ $EUID -ne 0 ]]; then
   echo -e "${RED}[ERROR]${NC} This script must be run as root (use sudo)"
   exit 1
fi

# Check for required commands
for cmd in curl jq; do
    if ! command -v "$cmd" &> /dev/null; then
        echo -e "${RED}[ERROR]${NC} Missing required command: $cmd"
        echo "Install with: apt-get update && apt-get install -y $cmd"
        exit 1
    fi
done

# Get the latest release info from GitHub
echo -e "${BLUE}[INFO]${NC} Fetching latest release from GitHub..."
RELEASE_JSON=$(curl -s "https://api.github.com/repos/${GITHUB_REPO}/releases/latest")

# Extract tag name
TAG_NAME=$(echo "$RELEASE_JSON" | jq -r '.tag_name')
if [[ -z "$TAG_NAME" ]] || [[ "$TAG_NAME" == "null" ]]; then
    echo -e "${RED}[ERROR]${NC} Could not fetch release information"
    exit 1
fi

echo -e "${BLUE}[INFO]${NC} Latest version: $TAG_NAME"

# Extract SHA from tag (format: v25.8.18.1138-76e0c299)
SHA=$(echo "$TAG_NAME" | grep -oE '[a-f0-9]{8}$' || true)
if [[ -z "$SHA" ]]; then
    echo -e "${RED}[ERROR]${NC} Could not extract SHA from tag: $TAG_NAME"
    exit 1
fi

# Construct download URL
DOWNLOAD_URL="https://github.com/${GITHUB_REPO}/releases/download/${TAG_NAME}/TSP-linux-x64-${SHA}"
echo -e "${BLUE}[INFO]${NC} Download URL: $DOWNLOAD_URL"

# Create temp directory
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

# Download the binary
echo -e "${BLUE}[INFO]${NC} Downloading binary..."
TEMP_BINARY="${TEMP_DIR}/tsp-binary"

if ! curl -L -f -o "$TEMP_BINARY" "$DOWNLOAD_URL" --progress-bar; then
    echo -e "${RED}[ERROR]${NC} Failed to download binary"
    exit 1
fi

# Check file size
FILE_SIZE=$(stat -c%s "$TEMP_BINARY" 2>/dev/null || echo 0)
if [[ "$FILE_SIZE" -lt 1000 ]]; then
    echo -e "${RED}[ERROR]${NC} Downloaded file too small (${FILE_SIZE} bytes)"
    echo "First 100 bytes: $(head -c 100 "$TEMP_BINARY" | tr '\n' ' ')"
    exit 1
fi

echo -e "${GREEN}[SUCCESS]${NC} Downloaded $((FILE_SIZE / 1048576)) MB"

# Backup existing installation if present
if [[ -f "$INSTALL_PATH" ]]; then
    BACKUP_PATH="${INSTALL_PATH}.backup.$(date +%Y%m%d-%H%M%S)"
    echo -e "${BLUE}[INFO]${NC} Backing up existing binary to $BACKUP_PATH"
    cp "$INSTALL_PATH" "$BACKUP_PATH"
fi

# Install the binary
echo -e "${BLUE}[INFO]${NC} Installing to $INSTALL_PATH..."
chmod +x "$TEMP_BINARY"
mv -f "$TEMP_BINARY" "$INSTALL_PATH"

# Create symlink
SYMLINK_PATH="/usr/local/bin/${BINARY_NAME}"
if [[ -d "/usr/local/bin" ]]; then
    ln -sf "$INSTALL_PATH" "$SYMLINK_PATH"
    echo -e "${GREEN}[SUCCESS]${NC} Symlink created: $SYMLINK_PATH"
fi

# Test installation
echo -e "${BLUE}[INFO]${NC} Testing installation..."
if "$INSTALL_PATH" --version &>/dev/null; then
    VERSION=$("$INSTALL_PATH" --version 2>/dev/null | head -n1 || echo "unknown")
    echo -e "${GREEN}[SUCCESS]${NC} Installation complete! Version: $VERSION"
else
    echo -e "${GREEN}[SUCCESS]${NC} Installation complete!"
    echo "Note: Could not verify version (this is normal for some binaries)"
fi

echo
echo "You can now run the TSP solver using:"
echo "  $INSTALL_PATH"
if [[ -L "$SYMLINK_PATH" ]]; then
    echo "  or simply: $BINARY_NAME"
fi
echo
echo "For help, run: ${BINARY_NAME} --help"