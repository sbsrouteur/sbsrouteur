; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "sbsRouteur"
#define MyAppVersion "0.34"
#define MyAppPublisher "sbsRouteur"
#define MyAppURL "http://code.google.com/p/sbsrouteur/"
#define MyAppExeName "Routeur.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppID={{1520F400-7692-4C1B-926F-AE4701DA0A1B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=C:\Projets\01_Perso\HG_Routeur-google_code\Setup\Setup_32
OutputBaseFilename=RouteurSetup_{#MyAppVersion}
SetupIconFile=C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\Graphics\Yacht.ico
Compression=lzma/Max
SolidCompression=true

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: Redists; Description: "Install redistribuables"; 

[Files]
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Debug\Routeur.exe; DestDir: {pf32}\sbs\Routeur\Routeur; Flags: ignoreversion; 
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
;Grib2
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygjasper-1-701-1.dll; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygjpeg-62.dll; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygpng12.dll; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygwin1.dll; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygz.dll; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\wgrib2.exe; DestDir: {pf32}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_c.b; DestDir: {pf32}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_i.b; DestDir: {pf32}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_l.b; DestDir: {pf32}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Debug\WriteableBitmapEx.Wpf.dll; DestDir: {pf32}\sbs\Routeur\Routeur;

;SQLite 
Source: "C:\Program Files (x86)\System.Data.SQLite\2010\bin\System.Data.SQLite.dll"; DestDir: {pf32}\sbs\Routeur\Routeur; 
Source: "C:\Program Files (x86)\System.Data.SQLite\2010\bin\SQLite.Interop.dll"; DestDir: {pf32}\sbs\Routeur\Routeur\x86; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Setup\Redist\vcredist_x86.exe; DestDir: {tmp}; Flags: deleteafterinstall 32bit; 

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{pf32}\sbs\Routeur\Routeur\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{pf32}\sbs\Routeur\Routeur\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: {tmp}\vcredist_x86.exe; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait skipifsilent RunAsCurrentUser; 
Filename: "{pf32}\sbs\Routeur\Routeur\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


[Dirs]

[Components]
