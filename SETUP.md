# Capitales du Monde – Setup Unity

## Prérequis
- Unity 2022.3 LTS ou plus récent
- Package **TextMeshPro** (Window > Package Manager > TextMeshPro)

## Mise en place

### 1. Créer le projet Unity
1. Ouvrir Unity Hub
2. New Project → **2D (URP)** ou **2D** → nommer "CapitalesDuMonde"
3. Copier tous les fichiers `Assets/Scripts/*.cs` dans le dossier `Assets/Scripts/` du projet

### 2. Importer TextMeshPro
Window > Package Manager → chercher "TextMeshPro" → Install
Quand la fenêtre s'ouvre : **Import TMP Essentials**

### 3. Générer la scène automatiquement
Menu **Tools > Build Quiz Scene**
→ Toute la hiérarchie UI est créée automatiquement.

### 4. Tester en Play Mode
Appuyer sur ▶ — l'écran Stats apparaît.
Cliquer "Commencer" pour lancer le quiz.

---

## Build WebGL
1. File > Build Settings → choisir **WebGL**
2. Player Settings :
   - Compression Format : **Gzip** ou **Brotli**
   - Publishing Settings : décocher "Decompression Fallback" si serveur supporté
3. Build → déployer le dossier sur n'importe quel serveur web (GitHub Pages, Netlify…)

## Build Android
1. File > Build Settings → choisir **Android**
2. Player Settings :
   - Minimum API Level : **21** (Android 5.0)
   - Scripting Backend : **IL2CPP**
   - Target Architecture : **ARM64 + ARMv7**
3. Build & Run

---

## Architecture du code

```
CapitalData.cs          → Base de données des 85+ capitales
SpacedRepetition.cs     → Algorithme SM-2 + persistance PlayerPrefs
QuizManager.cs          → Logique du jeu (state machine)
UIManager.cs            → Affichage et interactions UI
SceneBuilder.cs         → Outil éditeur pour générer la scène (#if UNITY_EDITOR)
AudioFeedback.cs        → Sons procéduraux (correct/faux)
```

## Algorithme de répétition espacée (SM-2 simplifié)

Chaque capitale a un **CardRecord** avec :
- `repetitions` : succès consécutifs
- `easeFactor` : facteur de facilité (2.5 par défaut, min 1.3)
- `intervalDays` : jours avant prochaine révision
- `nextReviewTicks` : timestamp de la prochaine révision

**Priorité de sélection** :
1. Cartes "dues" (révision en retard) avec le score de réussite le plus faible
2. Nouvelles cartes jamais vues
3. Mix 70/30 entre dues et nouvelles

**Après une bonne réponse** : intervalle × easeFactor (exponentiel)
**Après une mauvaise réponse** : retour à 1 jour

## Personnalisation

- Ajouter des capitales dans `CapitalData.cs` → liste `CapitalDatabase.All`
- Changer le nombre de choix : `QuizManager.numberOfChoices`
- Filtrer par continent via le dropdown dans l'UI
