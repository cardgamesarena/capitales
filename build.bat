@echo off
"C:\Program Files\Unity\Hub\Editor\2022.3.62f3\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\belkh\projets\unity\test" -executeMethod BuildScript.BuildWebGLCLI -logFile "C:\Users\belkh\projets\unity\test\build.log"
echo Exit code: %ERRORLEVEL%
