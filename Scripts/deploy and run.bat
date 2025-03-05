set "installPath=G:\SteamLibrary\steamapps\common\Subnautica"
set "buildPath=..\..\BuildTarget"

rmdir /Q /S "%installPath%\BepInEx\plugins\Echelon"
mkdir "%installPath%\BepInEx\plugins\Echelon"

copy /Y "%buildPath%\Subnautica Echelon_Data\Managed\EchelonScripts.dll" "%installPath%\BepInEx\plugins\Echelon"
copy /Y "..\Unity\Assets\AssetBundles\Windows\echelon" "%installPath%\BepInEx\plugins\Echelon"
copy /Y "..\Unity\Assets\AssetBundles\OSX\echelon" "%installPath%\BepInEx\plugins\Echelon\echelon.osx"
copy /Y "..\Plugin\bin\Release\net4.7.2\Subnautica Echelon.dll" "%installPath%\BepInEx\plugins\Echelon"
mkdir "%installPath%\BepInEx\plugins\Echelon\images"
copy /Y "..\images\*.*" "%installPath%\BepInEx\plugins\Echelon\images"

"%installPath%\Subnautica.exe"