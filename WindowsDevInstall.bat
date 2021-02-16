@echo off

:: Ensures the install variable is populated before use.
setlocal enabledelayedexpansion

:: Points to the install symlink, enabling this script to be installation agnostic when it comes to the game install location.
set INSTALL=install\KerbalSimpit\

:: Copies the mod files over to the required installation locations - update with new additions as required.
robocopy  Bin %INSTALL% KerbalSimpit.dll
robocopy  Localisation %INSTALL%\Localisation