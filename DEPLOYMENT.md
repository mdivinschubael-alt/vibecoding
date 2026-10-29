# Guide de Déploiement et de Distribution - PharmaGestion

Ce guide détaille l'ensemble des étapes nécessaires pour compiler, empaqueter et distribuer l'application **PharmaGestion** (.NET Moderne, Windows x64, SQLite hors ligne) auprès des officines clientes.

---

## 1. 📜 Licence Open Source & Mentions Légales

Le projet est configuré sous **Licence MIT** :
- **Fichier de licence :** [`LICENSE`](./LICENSE) et [`installer/LICENSE.txt`](./installer/LICENSE.txt) pour l'installateur.
- **Métadonnées d'assemblage :** Les fichiers de projet (`.csproj`) intègrent la balise `<PackageLicenseExpression>MIT</PackageLicenseExpression>` ainsi que les mentions de copyright 2026.
- **Alternative GPL v3 :** Si vous préférez une licence Copyleft stricte (obligeant toute distribution modifiée à rester open-source), remplacez le contenu de `LICENSE` par la [GNU General Public License v3](https://www.gnu.org/licenses/gpl-3.0.txt) et mettez à jour la balise dans les fichiers `.csproj` en `<PackageLicenseExpression>GPL-3.0-only</PackageLicenseExpression>`.

---

## 2. ⚙️ Compilation et Publication (`dotnet publish`)

Deux modes de distribution sont configurés :

### Option A (Recommandée pour la Pharmacie) : Mode Autonome (*Self-Contained Single-File*)
Ce mode intègre l'ensemble du runtime .NET ainsi que le moteur SQLite natif (`e_sqlite3.dll`) dans **un seul fichier exécutable** (`PharmaGestion.UI.exe`). Le poste de l'officine n'a besoin d'aucun prérequis ni d'accès à Internet.

Commande CLI :
```powershell
dotnet publish PharmaGestion.UI/PharmaGestion.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o ./publish/win-x64-standalone
```

### Option B : Mode Dépendant du Framework (*Framework-Dependent*)
Génère un binaire ultra-compact (~5 Mo), mais nécessite que le poste dispose du **.NET Desktop Runtime** installé.

Commande CLI :
```powershell
dotnet publish PharmaGestion.UI/PharmaGestion.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o ./publish/win-x64-framework-dependent
```

### Scripts Automatisés Clés en Main
Pour simplifier ces opérations, deux scripts sont disponibles dans le dossier `build/` :
- **PowerShell :** `.\build\publish.ps1` (exécute les tests, nettoie et publie les deux versions).
- **Batch / Invite de commande :** `build\publish.bat` (double-cliquable).

---

## 3. 📦 Création de l'Installateur Professionnel (Inno Setup)

Un script complet Inno Setup est fourni dans [`installer/PharmaGestion_Setup.iss`](./installer/PharmaGestion_Setup.iss).

### Caractéristiques de l'installateur :
1. **Écran de licence Open Source :** Oblige l'utilisateur à accepter la licence MIT avant de continuer.
2. **Choix du répertoire :** Propose par défaut `{autopf}\PharmaGestion` tout en laissant l'utilisateur choisir un autre dossier ou disque.
3. **Raccourcis :** Crée automatiquement les entrées dans le menu Démarrer et propose une case à cocher pour un raccourci sur le Bureau.
4. **Gestion de la Base de Données SQLite :**
   - L'application est autonome : elle crée et initialise la base SQLite (`pharmacie.db`) avec son administrateur par défaut au tout premier lancement dans `%LocalAppData%\PharmaGestion\`.
   - **Protection des données lors de la désinstallation :** Le désinstallateur demande explicitement si l'utilisateur souhaite conserver sa base de données (vital pour préserver les stocks et historiques de vente en cas de réinstallation ou mise à jour).
5. **Désinstallation propre :** Enregistré dans le panneau de configuration Windows "Applications et fonctionnalités".

### Comment générer le fichier d'installation `.exe` :
1. Téléchargez et installez **Inno Setup 6** (gratuit, open-source) depuis [jrsoftware.org/isdl.php](https://jrsoftware.org/isdl.php).
2. Lancez le script automatisé :
   ```powershell
   .\installer\build_installer.ps1
   ```
   *Ou ouvrez `installer\PharmaGestion_Setup.iss` dans l'éditeur Inno Setup Compiler et appuyez sur **Ctrl + F9** (Compile).*
3. Le fichier final `PharmaGestion_Setup_v1.0.0_win-x64.exe` sera généré dans le sous-dossier `installer\output\`.

---

## 4. 🚀 Procédure de Livraison Client (Checklist)

Pour déployer chez une pharmacie cliente :

1. **Générer l'installateur :**
   ```powershell
   .\build\publish.ps1 -Target Standalone -Clean
   .\installer\build_installer.ps1
   ```
2. **Copier l'installateur :** Transférez `PharmaGestion_Setup_v1.0.0_win-x64.exe` sur le poste cible (via clé USB ou réseau local).
3. **Exécuter l'installation :** Lancez le fichier `.exe`, acceptez la licence et terminez l'assistant.
4. **Premier Démarrage & Connexion :**
   - Lancez **PharmaGestion** depuis le raccourci Bureau ou Menu Démarrer.
   - Connectez-vous avec le compte administrateur initial :
     - **Identifiant :** `admin`
     - **Mot de passe :** `Admin123!`
   - *Recommandation de sécurité :* Modifiez immédiatement le mot de passe depuis l'onglet **Utilisateurs**.
