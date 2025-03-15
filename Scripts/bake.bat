set "buildPath=..\..\BuildTarget"

del /Q echelon.zip
rmdir /Q /S .\Echelon
mkdir .\Echelon
mkdir .\Echelon\images
mkdir .\Echelon\Localization
copy /Y "%buildPath%\Subnautica Echelon_Data\Managed\EchelonScripts.dll" .\Echelon
copy /Y "..\Unity\Assets\AssetBundles\Windows\echelon" .\Echelon
copy /Y "..\Unity\Assets\AssetBundles\OSX\echelon" .\Echelon\echelon.osx
copy /Y "..\Plugin\bin\Release\net4.7.2\Subnautica Echelon.dll" .\Echelon
copy /Y "..\images\*.*" .\Echelon\images
copy /Y "..\Localization\*.*" ".\Echelon\Localization"

powershell Compress-Archive .\Echelon echelon.zip
