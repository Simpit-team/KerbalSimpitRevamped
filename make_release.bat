@ECHO OFF
REM Used to create a release package for Simpit
REM Arguments : ProjectDir OutDir

set ProjectDir=%1
set OutDir=%2

set "MyKSPDIR=%KSPDIR%"

if not defined MyKSPDIR (
  echo KSPDIR is not set, I cannot make a release
  exit /b 1
)

REM Read the version number
for /f "delims== tokens=1,2" %%G in (%ProjectDir%VERSION.txt) do set %%G=%%H

echo Version read %MAJOR%.%MINOR%.%PATCH%.%BUILD%

set OUTPUT_FOLDER="%ProjectDir%NextReleases\KerbalSimpitRevamp-v%MAJOR%.%MINOR%.%PATCH%\KerbalSimpitRevamp"

REM clean the output folder by removing and recreating it
if exist %OUTPUT_FOLDER% RMDIR /S /Q %OUTPUT_FOLDER%
MKDIR %OUTPUT_FOLDER%

REM Create the version file
(
echo {
echo   "NAME": "Kerbal Simpit Revamp",
echo   "DOWNLOAD": "https://github.com/Simpit-team/KerbalSimpitRevamped/releases",
echo   "GITHUB": {
echo     "USERNAME": "Simpit-team",
echo     "REPOSITORY": "KerbalSimpitRevamped"
echo   },
echo   "VERSION": {
echo     "MAJOR": %MAJOR%,
echo     "MINOR": %MINOR%,
echo     "PATCH": %PATCH%,
echo     "BUILD": %BUILD%
echo   },
echo   "KSP_VERSION_MIN": {
echo     "MAJOR": %KSPMAJOR%,
echo     "MINOR": %KSPMINOR%,
echo     "PATCH": %KSPPATCH%
echo   }
echo }
)>"%OUTPUT_FOLDER%\KerbalSimpitRevamp.version"

REM Copy all the files

xcopy /q %ProjectDir%%OutDir%KerbalSimpit.dll %OUTPUT_FOLDER%  
xcopy /q %ProjectDir%%OutDir%WindowsInput.dll %OUTPUT_FOLDER%

xcopy /q %ProjectDir%distrib\icon\Simpit_icon_green.png %OUTPUT_FOLDER%
xcopy /q %ProjectDir%distrib\icon\Simpit_icon_orange.png %OUTPUT_FOLDER%
xcopy /q %ProjectDir%distrib\icon\Simpit_icon_red.png %OUTPUT_FOLDER%

xcopy /q %ProjectDir%distrib\Localisation\en-us.cfg %OUTPUT_FOLDER%
xcopy /q %ProjectDir%distrib\Localisation\fr-fr.cfg %OUTPUT_FOLDER%
xcopy /q %ProjectDir%distrib\Localisation\de-de.cfg %OUTPUT_FOLDER%

xcopy /q %ProjectDir%distrib\PluginData\Settings.cfg.sample %OUTPUT_FOLDER%
rename %OUTPUT_FOLDER%\Settings.cfg.sample Settings.cfg

REM now include the Arduino lib.

set ARDUINOLIB_FOLDER=%ProjectDir%..\KerbalSimpitRevamped-Arduino\

echo %ARDUINOLIB_FOLDER%

if not exist %ARDUINOLIB_FOLDER% (
  echo Cannot locate the Arduino libs. Cannot create a release.
  exit /b 2
)

xcopy /q /S %ARDUINOLIB_FOLDER%src %OUTPUT_FOLDER%\KerbalSimpitRevamped-Arduino\src\
xcopy /q /S %ARDUINOLIB_FOLDER%examples %OUTPUT_FOLDER%\KerbalSimpitRevamped-Arduino\examples\
xcopy /q %ARDUINOLIB_FOLDER%keywords.txt %OUTPUT_FOLDER%\KerbalSimpitRevamped-Arduino
xcopy /q %ARDUINOLIB_FOLDER%library.properties %OUTPUT_FOLDER%\KerbalSimpitRevamped-Arduino


REM now compress it
tar -C %ProjectDir%NextReleases\KerbalSimpitRevamp-v%MAJOR%.%MINOR%.%PATCH% -acf %ProjectDir%NextReleases\KerbalSimpitRevamp-v%MAJOR%.%MINOR%.%PATCH%.zip KerbalSimpitRevamp
