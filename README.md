# PharmaGestion - Application de Gestion de Pharmacie Hors Ligne

Application moderne de gestion d'officine pharmaceutique développée en **C# Windows Forms (.NET moderne)** avec base de données locale **SQLite** (`Microsoft.Data.Sqlite`).

---

## 🏛️ Architecture en Couches

Le projet est rigoureusement structuré en couches selon le principe de séparation des responsabilités :

```
PharmaGestion/
│
├── PharmaGestion.Core/           # Modèles de domaine, DTOs, Enums et Sécurité de base
│   ├── Models/                   # Utilisateur, Produit, Fournisseur, Lot
│   ├── Security/                 # IPasswordHasher & PasswordHasher (PBKDF2 SHA-256)
│   └── Common/                   # Constantes (RoleUtilisateur) & OperationResult<T>
│
├── PharmaGestion.DAL/            # Couche d'accès aux données (Data Access Layer)
│   ├── Database/                 # DatabaseConfig, DatabaseInitializer, SqliteExtensions
│   └── Repositories/             # Dépôts avec requêtes SQL paramétrées et contraintes d'intégrité
│       ├── UtilisateurRepository
│       ├── ProduitRepository
│       ├── FournisseurRepository
│       └── LotRepository
│
├── PharmaGestion.BLL/            # Couche Métier (Business Logic Layer)
│   └── Services/                 # Règles de gestion, validations et logique applicative
│       ├── AuthService           # Authentification & gestion de session
│       ├── ProduitService        # Catalogue & alertes stock minimum
│       ├── LotService            # Réception, traçabilité & alertes péremption (FEFO)
│       ├── FournisseurService    # Gestion des fournisseurs et laboratoires
│       └── UtilisateurService    # Gestion des utilisateurs & rôles (Admin)
│
├── PharmaGestion.UI/             # Interface utilisateur Windows Forms moderne
│   ├── Common/                   # Thème visuel UiTheme (palette officine, typography)
│   ├── Forms/                    # Écrans et dialogues
│   │   ├── LoginForm             # Authentification hors ligne
│   │   ├── MainForm              # Tableau de bord, Catalogue, Lots, Fournisseurs, Admin
│   │   ├── ProduitDialog         # Saisie / Modification produit avec validation
│   │   ├── LotDialog             # Réception / Traçabilité de lot
│   │   ├── FournisseurDialog     # Saisie fournisseur
│   │   └── UtilisateurDialog     # Création / Édition utilisateur et rôle
│   └── Program.cs                # Point d'entrée et injection des dépendances
│
└── PharmaGestion.Tests/          # Suite de tests unitaires et d'intégration xUnit (12 tests)
```

---

## 💾 Schéma SQLite & Tables Principales

La base de données est stockée localement dans `%LocalAppData%\PharmaGestion\pharmacie.db` (100% hors ligne, mode WAL activé, `PRAGMA foreign_keys = ON`).

### 1. `utilisateurs`
| Colonne | Type | Description |
|---|---|---|
| `id` | INTEGER PRIMARY KEY AUTOINCREMENT | Identifiant unique |
| `nom` | TEXT NOT NULL | Nom complet |
| `identifiant` | TEXT NOT NULL UNIQUE | Identifiant de connexion |
| `mot_de_passe_hash` | TEXT NOT NULL | Hash PBKDF2 sécurisé avec sel unique |
| `role` | TEXT NOT NULL | `Administrateur`, `Pharmacien`, `Caissier` |
| `date_creation` | TEXT NOT NULL | Horodatage ISO-8601 |

> **Compte administrateur par défaut initialisé automatiquement :**
> - **Identifiant :** `admin`
> - **Mot de passe :** `Admin123!`

### 2. `produits`
| Colonne | Type | Description |
|---|---|---|
| `id` | INTEGER PRIMARY KEY AUTOINCREMENT | Clé primaire |
| `reference_unique` | TEXT NOT NULL UNIQUE | Code barre / Réf CIP |
| `designation` | TEXT NOT NULL | Nom commercial du médicament |
| `categorie` | TEXT NOT NULL | Catégorie thérapeutique |
| `dosage_forme` | TEXT NOT NULL | Dosage et forme galénique |
| `unite_vente` | TEXT NOT NULL | Boîte, Flacon, etc. |
| `prix_vente` | TEXT NOT NULL | **Stocké en format décimal exact invariant** |
| `stock_minimum` | INTEGER NOT NULL DEFAULT 0 | Seuil d'alerte de réapprovisionnement |
| `actif` | INTEGER NOT NULL DEFAULT 1 | 1 = Actif, 0 = Archivé / Inactif |

### 3. `fournisseurs`
| Colonne | Type | Description |
|---|---|---|
| `id` | INTEGER PRIMARY KEY AUTOINCREMENT | Clé primaire |
| `nom` | TEXT NOT NULL | Nom ou raison sociale du laboratoire |
| `telephone` | TEXT | Contact téléphonique |
| `adresse` | TEXT | Adresse postale |
| `email` | TEXT | Adresse email |
| `date_creation` | TEXT NOT NULL | Horodatage de création |

### 4. `lots`
| Colonne | Type | Description |
|---|---|---|
| `id` | INTEGER PRIMARY KEY AUTOINCREMENT | Clé primaire |
| `produit_id` | INTEGER NOT NULL | Clé étrangère vers `produits(id)` |
| `fournisseur_id` | INTEGER NULL | Clé étrangère vers `fournisseurs(id)` |
| `numero_lot` | TEXT NOT NULL | Numéro de lot de fabrication (traçabilité) |
| `date_peremption` | TEXT NOT NULL | Date d'expiration |
| `quantite_initiale` | INTEGER NOT NULL | Quantité reçue |
| `quantite_actuelle` | INTEGER NOT NULL | Quantité encore disponible |
| `cout_achat` | TEXT NOT NULL | **Coût unitaire en type décimal exact** |
| `date_reception` | TEXT NOT NULL | Date de réception en pharmacie |

---

## 🔒 Sécurité : Hachage PBKDF2 (SHA-256)

- L'algorithme standard **PBKDF2** (`Rfc2898DeriveBytes.Pbkdf2`) est utilisé avec :
  - Sel cryptographique de **128 bits** généré via `RandomNumberGenerator.GetBytes`.
  - **100 000 itérations**.
  - Clé de hachage de **256 bits** (SHA-256).
  - Format : `PBKDF2$100000${salt_base64}${hash_base64}`.
- La vérification du mot de passe s'exécute en **temps constant** via `CryptographicOperations.FixedTimeEquals` pour neutraliser les attaques temporelles (*timing attacks*).

---

## 💶 Gestion des Types Décimaux (Zéro Erreur d'Arrondi)

Dans SQLite, les nombres à virgule flottante (`REAL`) utilisent la norme IEEE 754 binaire, provoquant des erreurs de précision inacceptables pour la comptabilité d'une officine (ex. `0.1 + 0.2 != 0.3`).

- En **C#** : Utilisation exclusive du type `decimal` (128 bits à virgule fixe).
- Dans **SQLite** : Les montants (`prix_vente`, `cout_achat`) sont enregistrés en tant que chaînes fixes normalisées (`SqliteType.Text` avec `CultureInfo.InvariantCulture`) et convertis fidèlement lors de la lecture.
- Aucune dérive ou perte de centime n'est possible, même lors de calculs agrégés de valorisation de stock.

---

## 🚀 Exécution et Tests

### Compiler la solution :
```powershell
dotnet build PharmaGestion.slnx
```

### Lancer les tests unitaires et d'intégration :
```powershell
dotnet test PharmaGestion.Tests/PharmaGestion.Tests.csproj
```

### Lancer l'application Windows Forms :
```powershell
dotnet run --project PharmaGestion.UI/PharmaGestion.UI.csproj
```

---

## 📦 Publication et Installateur Windows (Inno Setup)

- **Publication automatique (Release win-x64) :**
  ```powershell
  .\build\publish.ps1
  ```
  Génère l'exécutable autonome `PharmaGestion.UI.exe` dans `publish/win-x64-standalone/`.

- **Création de l'installateur :**
  ```powershell
  .\installer\build_installer.ps1
  ```
  Le script Inno Setup [`installer/PharmaGestion_Setup.iss`](./installer/PharmaGestion_Setup.iss) produit un programme d'installation autonome `PharmaGestion_Setup_v1.0.0_win-x64.exe` avec raccourcis, acceptation de licence et protection des données SQLite.

Pour plus de détails, consultez le [Guide de Déploiement Complet (DEPLOYMENT.md)](./DEPLOYMENT.md).

