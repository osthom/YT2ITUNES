#!/bin/bash
set -e

# ===== CONFIGURATION =====
APP_NAME="YT2ITUNES"                       
DOTNET_FRAMEWORK="net9.0"              
RUNTIME="osx-arm64"                    
VERSION="1.0"                          
IDENTIFIER="com.osthom.yt2itunes"         

# ===== BUILD WITH DOTNET =====
echo "Publishing .NET app..."
dotnet publish -c Release -r osx-arm64 

PUBLISH_DIR="bin/Release/$DOTNET_FRAMEWORK/$RUNTIME/publish"


# ===== CREATE .app STRUCTURE =====
echo "📂 Creating .app bundle..."
rm -rf "$APP_NAME.app"
mkdir -p "$APP_NAME.app/Contents/MacOS"
mkdir -p "$APP_NAME.app/Contents/Resources"
cp  yt2itunes_icon.icns "$APP_NAME.app/Contents/Resources/"

# Copy published files into MacOS directory
cp -R "$PUBLISH_DIR"/* "$APP_NAME.app/Contents/MacOS/"

# ===== CREATE Info.plist =====
cat > "$APP_NAME.app/Contents/Info.plist" <<EOL
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" 
    "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>
    <string>YT2ITUNES</string>
    <key>CFBundleDisplayName</key>
    <string>YT2ITUNES</string>
    <key>CFBundleExecutable</key>
    <string>launch_script</string>
    <key>CFBundleIdentifier</key>
    <string>com.osthom.yt2itunes</string>
    <key>CFBundleVersion</key>
    <string>1.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>CFBundleIconFile</key>
    <string>yt2itunes_icon.icns</string>
</dict>
</plist>

EOL

# === Create launch_script === 
cat > "$APP_NAME.app/Contents/MacOS/launch_script" <<'EOF'
#!/bin/bash
cd "${0%/*}"
./YT2ITUNES
EOF

# ===== MAKE EXECUTABLE =====
chmod +x "$APP_NAME.app/Contents/MacOS/$APP_NAME"
chmod +x "$APP_NAME.app/Contents/MacOS/launch_script"


# # ===== CREATE DMG =====
echo "💿 Creating DMG..."
DMG_NAME="$APP_NAME-$VERSION.dmg"

# Install create-dmg if missing
if ! command -v create-dmg &> /dev/null
then
    echo "Installing create-dmg..."
    brew install create-dmg
fi

# Remove old DMG
rm -f "$DMG_NAME"

# Create the DMG
create-dmg \
  --volname "$APP_NAME" \
  --window-pos 200 120 \
  --window-size 800 400 \
  --icon-size 100 \
  --app-drop-link 600 185 \
  "$DMG_NAME" \
  "$APP_NAME.app"

echo "✅ Done!"
echo "DMG created: $DMG_NAME"