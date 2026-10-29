; =====================================================================
; Script Inno Setup Professionnel pour PharmaGestion
; Application de Gestion de Pharmacie Hors Ligne (.NET Moderne / SQLite)
; =====================================================================

#define MyAppName "PharmaGestion"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "MOUBELE DIVIN Open Source"
#define MyAppURL "https://github.com/pharmagestion/pharmagestion"
#define MyAppExeName "PharmaGestion.UI.exe"
#define MyPublishSourceDir "..\publish\win-x64-standalone"

[Setup]
; Identifiant unique d'application (généré pour PharmaGestion)
AppId={{8B163B34-D72E-4A47-9750-7F16C6E41D99}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases

; Emplacement d'installation par défaut modifiable par l'utilisateur
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=no

; Intégration de la licence Open Source (accord préalable requis)
LicenseFile=LICENSE.txt

; Dossier de sortie de l'installateur généré
OutputDir=output
OutputBaseFilename=PharmaGestion_Setup_v{#MyAppVersion}_win-x64

; Algorithme de compression maximale
Compression=lzma2/ultra64
SolidCompression=yes

; Interface moderne et support 64-bit
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

; Gestion des privilèges d'installation (Demande élévation si nécessaire, mais autorise l'installation par utilisateur)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Icône et désinstallation
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} (Gestionnaire Officinal)

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "Créer un raccourci dans le menu Démarrer"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; Fichier exécutable principal autonome (Self-Contained avec SQLite inclus)
Source: "{#MyPublishSourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; Copie de la licence et documentation
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; DestName: "README.txt"; Flags: ignoreversion isreadme

[Icons]
; Raccourci Menu Démarrer
Name: "{autoprograms}\{#MyAppName}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startmenuicon
Name: "{autoprograms}\{#MyAppName}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; Tasks: startmenuicon

; Raccourci Bureau optionnel
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Proposition de lancer l'application immédiatement après l'installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// =====================================================================
// Gestion de la Base de Données SQLite et Préservation des Données
// =====================================================================
// L'application PharmaGestion initialise automatiquement la base SQLite
// dans %LocalAppData%\PharmaGestion\pharmacie.db au premier lancement.
// Lors de la désinstallation, ce code propose à l'utilisateur de conserver
// ses données d'officine (stocks, lots, utilisateurs) ou de les purger.
// =====================================================================

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: string;
  MsgResult: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataDir := ExpandConstant('{localappdata}\PharmaGestion');
    if DirExists(DataDir) then
    begin
      MsgResult := MsgBox(
        'Souhaitez-vous également supprimer la base de données locale SQLite et l''historique de la pharmacie ?' + #13#10 + #13#10 +
        'Emplacement : ' + DataDir + #13#10 + #13#10 +
        '- Cliquez sur "Oui" pour TOUT supprimer définitivement.' + #13#10 +
        '- Cliquez sur "Non" pour CONSERVER vos données d''officine (recommandé en cas de mise à jour).',
        mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
        
      if MsgResult = IDYES then
      begin
        DelTree(DataDir, True, True, True);
      end;
    end;
  end;
end;
