; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "sbsRouteur"
#define MyAppVersion "0.31"
#define MyAppPublisher "sbsRouteur"
#define MyAppURL "http://code.google.com/p/sbsrouteur/"
#define MyAppExeName "Routeur.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{48BFBA13-794B-46BA-B7C9-C5E53289DBA0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf64}\sbs\Routeur
DefaultGroupName={#MyAppName}
OutputDir=C:\Projets\01_Perso\HG_Routeur-google_code\Setup\Setup_64
OutputBaseFilename=RouteurSetup64_{#MyAppVersion}
SetupIconFile=C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\Graphics\Yacht.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: Redists; Description: "Install redistribuables"; 

[Files]
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Debug_64\Routeur.exe; DestDir: {pf64}\sbs\Routeur\Routeur; Flags: ignoreversion; 
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
;Grib2
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygjasper-1-701-1.dll; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygjpeg-62.dll; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygpng12.dll; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygwin1.dll; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\cygz.dll; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Grib2\wgrib2.exe; DestDir: {pf64}\sbs\Routeur\Grib2; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_c.b; DestDir: {pf64}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_i.b; DestDir: {pf64}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\gshhs\gshhs_l.b; DestDir: {pf64}\sbs\Routeur\Gshhs; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Debug_64\WriteableBitmapEx.Wpf.dll; DestDir: {pf64}\sbs\Routeur\Routeur; 
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Routeur\bin\Debug\WriteableBitmapEx.Wpf.dll; DestDir: {pf64}\sbs\Routeur\Routeur;

;SQLite 
Source: "C:\Program Files (x86)\System.Data.SQLite\2010\bin\System.Data.SQLite.dll"; DestDir: {pf64}\sbs\Routeur\Routeur; 
Source: "C:\Program Files\System.Data.SQLite\2010\bin\SQLite.Interop.dll"; DestDir: {pf64}\sbs\Routeur\Routeur\x64; 

;VC 2010 Redists
Source: C:\Projets\01_Perso\HG_Routeur-google_code\Setup\Redist\vcredist_x64.exe; DestDir: {tmp}; Flags: deleteafterinstall 64bit; 

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{pf64}\sbs\Routeur\Routeur\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{pf64}\sbs\Routeur\Routeur\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: {tmp}\vcredist_x64.exe; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait skipifsilent RunAsCurrentUser; 
Filename: "{pf64}\sbs\Routeur\Routeur\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent


[Dirs]
