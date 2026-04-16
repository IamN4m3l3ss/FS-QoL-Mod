#!/usr/bin/env bash

# Usage (Linux):
# chmod +x Dependency_Importer.sh
# ./Dependency_Importer.sh /path/to/game/folder/

# Destination directory based on your prompt
DEST_DIR="/home/nameless/RiderProjects/FS_Mod/Dependencies/"
# Game directory to pull files from.
HARDCODED_GAME_DIR="/home/nameless/Games/FriendlyShooter/"

# List of target files extracted from your ls output
TARGET_FILES=(
    "0Harmony.dll"
    "Assembly-CSharp.dll"
    "MelonLoader.dll"
    "UnityEngine.AudioModule.dll"
    "UnityEngine.CoreModule.dll"
    "UnityEngine.dll"
    "UnityEngine.IMGUIModule.dll"
    "UnityEngine.InputLegacyModule.dll"
    "UnityEngine.InputModule.dll"
    "UnityEngine.PhysicsModule.dll"
    "UnityEngine.TextRenderingModule.dll"
    "UnityEngine.UI.dll"
)

# Determine which directory to use (Terminal Argument overrides Hardcoded path)
if [ -n "$1" ]; then
    GAME_DIR="$1"
elif [ -n "$HARDCODED_GAME_DIR" ]; then
    GAME_DIR="$HARDCODED_GAME_DIR"
else
    echo "❌ Error: No game directory provided."
    echo "Usage: $0 [/path/to/game] OR set HARDCODED_GAME_DIR inside the script."
    exit 1
fi

# Verify the target directory exists
if [ ! -d "$GAME_DIR" ]; then
    echo "❌ Error: Directory '$GAME_DIR' does not exist."
    exit 1
fi

# Ensure the destination directory exists
mkdir -p "$DEST_DIR"

echo "📂 Searching in: $GAME_DIR"
echo "🎯 Destination: $DEST_DIR"
echo "------------------------------------------------"

for file in "${TARGET_FILES[@]}"; do
    # Find the file (2>/dev/null hides 'permission denied' spam if searching deep)
    FOUND_PATHS=$(find "$GAME_DIR" -type f -name "$file" 2>/dev/null)

    if [ -n "$FOUND_PATHS" ]; then
        echo "$FOUND_PATHS" | while IFS= read -r filepath; do
            DEST_FILE="$DEST_DIR/$file"

            # Check if file already exists in destination AND is identical byte-for-byte
            if [ -f "$DEST_FILE" ] && cmp -s "$filepath" "$DEST_FILE"; then
                echo "⏭️  Skipped: $file (Unchanged)"
            else
                cp "$filepath" "$DEST_DIR/"
                echo "✅ Copied: $file"
            fi
        done
    else
        echo "❌ Missing: $file (Not found in game folder)"
    fi
done

echo "------------------------------------------------"
echo "🎉 Sync complete!"
