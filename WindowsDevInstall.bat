@echo off
setlocal enabledelayedexpansion
set KSPDIR=KerbalSpaceProgram
set INSTALL=install\KerbalSimpit\

@REM if exist %INSTALL% (
@REM     echo Install Directory Exists
@REM ) else (
@REM     echo Missing Install Directory - Creating it now
@REM     mkdir %INSTALL%
@REM )

robocopy  Bin %INSTALL% KerbalSimpit.dll
robocopy  Localisation %INSTALL%\Localisation
::cd %KSPDIR%
::echo %KSPDIR%
::dir
::echo %ksp_dir%