@echo off
:: You can comment out the next 2 lines if you are debuging and already have fake installed
::echo Installing FAKE
::".nuget\NuGet.exe" "Install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion"
"packages\FAKE\tools\Fake.exe" build.fsx %1
