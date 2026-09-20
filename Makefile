NAME := CharacterForge
VERSION ?= 1.0.0
ICON_EXTENSION := .png
ICON_FILE := src/CharacterForge.Desktop/Assets/Icon$(ICON_EXTENSION)
DIST_DIR := .dist

.PHONY: all build-windows pkg-windows build-linux pkg-linux clean

all: build-windows build-linux

WINDOWS_DIST_DIR = $(DIST_DIR)/Windows
WINDOWS_OUTPUT_FILE = $(WINDOWS_DIST_DIR)/$(NAME)-$(VERSION)-Windows-x86_64.zip

build-windows:
	dotnet publish --configuration Release --runtime win-x64 --self-contained

# [alexis] The dot and asterisk are needed at the start and end of the source path so
#          7-Zip doesn't preserve the leading directories.
pkg-windows:
	rm -f "$(WINDOWS_OUTPUT_FILE)"

	mkdir -p "$(WINDOWS_DIST_DIR)/"
	7z a -mx=9 -mmt=$(nproc) "$(WINDOWS_OUTPUT_FILE)" "./src/CharacterForge.Desktop/.build/bin/Release/win-x64/publish/*"

LINUX_DIST_DIR = $(DIST_DIR)/Linux
LINUX_APPIMAGE_BIN_INTERNAL_DIR = /usr/bin
LINUX_APPIMAGE_EXEC_INTERNAL_FILE = $(LINUX_APPIMAGE_BIN_INTERNAL_DIR)/CharacterForge.Desktop
LINUX_APPIMAGE_DIR = $(LINUX_DIST_DIR)/AppDir
LINUX_APPIMAGE_BIN_DIR = $(LINUX_APPIMAGE_DIR)$(LINUX_APPIMAGE_BIN_INTERNAL_DIR)
LINUX_APPIMAGE_ICON_DIR = $(LINUX_APPIMAGE_DIR)/usr/share/icons/hicolor/256x256/apps
LINUX_APPIMAGE_APPRUN_FILE = $(LINUX_APPIMAGE_DIR)/AppRun
LINUX_OUTPUT_FILE = $(LINUX_DIST_DIR)/$(NAME)-$(VERSION)-Linux-x86_64.AppImage

build-linux:
	dotnet publish --configuration Release --runtime linux-x64 --self-contained

pkg-linux:
	rm -rf "$(LINUX_APPIMAGE_DIR)"
	rm -f "$(LINUX_OUTPUT_FILE)"

	mkdir -p "$(LINUX_APPIMAGE_BIN_DIR)/"
	cp -r "src/CharacterForge.Desktop/.build/bin/Release/linux-x64/publish/." "$(LINUX_APPIMAGE_BIN_DIR)/"

	mkdir -p "$(LINUX_APPIMAGE_ICON_DIR)/"
	cp "$(ICON_FILE)" "$(LINUX_APPIMAGE_DIR)/$(NAME)$(ICON_EXTENSION)"
	cp "$(ICON_FILE)" "$(LINUX_APPIMAGE_DIR)/.DirIcon"
	cp "$(ICON_FILE)" "$(LINUX_APPIMAGE_ICON_DIR)/$(NAME)$(ICON_EXTENSION)"

	@printf '%s\n' \
	'[Desktop Entry]' \
	'Type=Application' \
	'Name=$(NAME)' \
	'Comment=Character Card editor' \
	'Exec="$(LINUX_APPIMAGE_EXEC_INTERNAL_FILE)" %U' \
	'Icon=$(NAME)' \
	'Categories=Utility' \
	'Terminal=false' \
	'StartupWMClass=$(NAME)' \
	> "$(LINUX_APPIMAGE_DIR)/$(NAME).desktop"

	@printf '%s\n' \
	'#!/usr/bin/env bash' \
	'' \
	'HERE="$$(dirname "$$(readlink -f "$${0}")")"' \
	'' \
	'export PATH="$${HERE}$(LINUX_APPIMAGE_BIN_INTERNAL_DIR):$${PATH}"' \
	'export LD_LIBRARY_PATH="$${HERE}/usr/lib:$${LD_LIBRARY_PATH}"' \
	'' \
	'exec "$${HERE}$(LINUX_APPIMAGE_EXEC_INTERNAL_FILE)" "$$@"' \
	> "$(LINUX_APPIMAGE_APPRUN_FILE)"
	chmod +x "$(LINUX_APPIMAGE_APPRUN_FILE)"

	ARCH=x86_64 appimagetool --no-appstream "$(LINUX_APPIMAGE_DIR)" "$(LINUX_OUTPUT_FILE)"

clean:
	rm -rf **/.build/
	rm -rf **/bin/
	rm -rf **/obj/

ifeq ("$(RESTORE)", "YES")
	dotnet restore
endif
