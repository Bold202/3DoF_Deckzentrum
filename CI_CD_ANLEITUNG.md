# CI/CD Automatisierung - APK Build

> **Anleitung zur automatischen APK-Erstellung mit GitHub Actions**

## 📋 Übersicht

Dieses Repository enthält einen GitHub Actions Workflow, der automatisch eine Android APK baut, wenn Änderungen gepusht werden. Die APK kann dann direkt heruntergeladen und auf dem Endgerät getestet werden.

## 🚀 Wie funktioniert es?

### Automatische Builds

Der Workflow wird automatisch ausgelöst bei:

| Trigger | Beschreibung |
|---------|--------------|
| **Push auf main/master** | Bei jedem Push auf den Hauptbranch |
| **Pull Request** | Bei PRs gegen main/master |
| **Manuell** | Über "Actions" → "Run workflow" |

### Was wird gebaut?

- **Android APK** für alle Android-Geräte (ARM64 + ARMv7)
- Optimiert für ARCore (AR-Funktionen)
- Release- oder Debug-Build (wählbar)

## ⚙️ Einrichtung (Einmalig erforderlich)

### Schritt 1: Unity-Lizenz einrichten

Der Workflow benötigt eine Unity-Lizenz. Es gibt zwei Möglichkeiten:

#### Option A: Unity Personal Lizenz (Kostenlos)

1. **Unity License File erstellen:**
   ```bash
   # Unity im Batch-Modus starten
   Unity -batchmode -createManualActivationFile
   ```
   Dies erstellt eine `.alf` Datei.

2. **Lizenz aktivieren:**
   - Gehe zu: https://license.unity3d.com/manual
   - Lade die `.alf` Datei hoch
   - Wähle "Unity Personal"
   - Lade die `.ulf` Datei herunter

3. **Lizenz als Secret speichern:**
   - Gehe zu: Repository → Settings → Secrets and variables → Actions
   - Neues Secret erstellen: `UNITY_LICENSE`
   - Inhalt: Der gesamte Inhalt der `.ulf` Datei

#### Option B: Unity Plus/Pro Lizenz

1. **Secrets erstellen:**
   
   | Secret Name | Beschreibung |
   |-------------|--------------|
   | `UNITY_EMAIL` | Deine Unity-Account E-Mail |
   | `UNITY_PASSWORD` | Dein Unity-Account Passwort |
   | `UNITY_LICENSE` | (Kann leer bleiben bei Plus/Pro) |

### Schritt 2: Android Keystore einrichten (Optional, empfohlen für Releases)

Für signierte APKs (erforderlich für Play Store):

1. **Keystore erstellen (falls nicht vorhanden):**
   ```bash
   keytool -genkey -v -keystore d8planer.keystore -alias d8planer -keyalg RSA -keysize 2048 -validity 10000
   ```

2. **Keystore als Base64 kodieren:**
   ```bash
   base64 d8planer.keystore > keystore_base64.txt
   ```

3. **Secrets erstellen:**

   | Secret Name | Beschreibung |
   |-------------|--------------|
   | `ANDROID_KEYSTORE_BASE64` | Inhalt von keystore_base64.txt |
   | `ANDROID_KEYSTORE_PASS` | Keystore-Passwort |
   | `ANDROID_KEYALIAS_NAME` | Alias-Name (z.B. "d8planer") |
   | `ANDROID_KEYALIAS_PASS` | Alias-Passwort |

### Schritt 3: Secrets in GitHub konfigurieren

1. Gehe zu: `https://github.com/Bold202/3DoF_Deckzentrum/settings/secrets/actions`
2. Klicke auf "New repository secret"
3. Füge die erforderlichen Secrets hinzu:

**Minimale Secrets (für Personal Lizenz):**
```
UNITY_LICENSE = [Inhalt der .ulf Datei]
```

**Alle Secrets (für signierte Releases):**
```
UNITY_LICENSE = [Inhalt der .ulf Datei]
UNITY_EMAIL = [optional]
UNITY_PASSWORD = [optional]
ANDROID_KEYSTORE_BASE64 = [Base64-kodierter Keystore]
ANDROID_KEYSTORE_PASS = [Keystore-Passwort]
ANDROID_KEYALIAS_NAME = [Alias-Name]
ANDROID_KEYALIAS_PASS = [Alias-Passwort]
```

## 📥 APK herunterladen

### Nach erfolgreichem Build:

1. Gehe zu: **Actions** Tab im Repository
2. Klicke auf den erfolgreichen Workflow-Run
3. Scrolle zu **Artifacts**
4. Klicke auf `D8-Planer-XR-APK-[Build-Nummer]`
5. ZIP-Datei wird heruntergeladen
6. Entpacken → APK auf Gerät übertragen

### Direkter Link:
```
https://github.com/Bold202/3DoF_Deckzentrum/actions
```

## 🎯 Manueller Build starten

1. Gehe zu: **Actions** → **Build Android APK**
2. Klicke auf **"Run workflow"**
3. Wähle:
   - **Branch:** main (oder anderen Branch)
   - **Build Type:** Release oder Debug
4. Klicke auf **"Run workflow"**

## 📱 APK auf Gerät installieren

### Via USB (adb):
```bash
# Gerät verbinden und USB-Debugging aktivieren
adb install -r D8-Planer-XR.apk
```

### Via Dateiübertragung:
1. APK auf Gerät kopieren (Cloud/USB/E-Mail)
2. Dateimanager öffnen
3. APK antippen
4. "Installation aus unbekannten Quellen" erlauben
5. Installieren

## ⏱️ Build-Zeiten

| Phase | Dauer (ca.) |
|-------|-------------|
| **Erster Build** | 30-45 Minuten |
| **Folgende Builds** | 10-20 Minuten (mit Cache) |
| **Nach großen Änderungen** | 20-30 Minuten |

> **Tipp:** Der Cache beschleunigt Builds erheblich. Er wird bei Änderungen an Assets/Packages automatisch invalidiert.

## 🔧 Fehlerbehebung

### Build schlägt fehl: "No Unity license"

**Lösung:** Unity-Lizenz-Secret nicht konfiguriert.
- Siehe [Schritt 1: Unity-Lizenz einrichten](#schritt-1-unity-lizenz-einrichten)

### Build schlägt fehl: "Missing Android SDK"

**Lösung:** Der Workflow verwendet game-ci/unity-builder, der Android SDK automatisch installiert. Falls Probleme auftreten:
- Prüfe ob `Android Build Support` im Unity-Projekt aktiviert ist
- Öffne das Projekt einmal lokal in Unity und wechsle zu Android Platform

### Keystore-Fehler

**Lösung bei unsignierten Builds:**
Die Keystore-Secrets können leer bleiben. Unity erstellt dann eine Debug-signierte APK.

**Lösung bei signierten Builds:**
- Prüfe ob alle Keystore-Secrets korrekt sind
- Keystore muss im Base64-Format sein
- Passwörter müssen exakt übereinstimmen

### APK installiert nicht

**Mögliche Ursachen:**
1. **Versionsnummer nicht erhöht** - Bei Updates muss `Bundle Version Code` erhöht werden
2. **Signatur unterschiedlich** - Alte App deinstallieren, dann neu installieren
3. **Speicherplatz** - Mindestens 500 MB frei auf dem Gerät

## 📊 Build-Status Badge

Füge diesen Badge zu deiner README hinzu:

```markdown
[![Build Android APK](https://github.com/Bold202/3DoF_Deckzentrum/actions/workflows/build-apk.yml/badge.svg)](https://github.com/Bold202/3DoF_Deckzentrum/actions/workflows/build-apk.yml)
```

## 🔄 Workflow-Diagramm

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Push/PR auf   │────▶│  GitHub Actions │────▶│   APK Artifact  │
│   main Branch   │     │  Unity Builder  │     │   zum Download  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Bei Tag v*.*   │
                        │  → GitHub       │
                        │    Release      │
                        └─────────────────┘
```

## 📝 Nächste Schritte

Nach erfolgreicher Einrichtung:

1. ✅ Erstes Mal: Unity-Lizenz als Secret speichern
2. ✅ Optional: Android Keystore für signierte Builds
3. ✅ Push auf main → Automatischer Build
4. ✅ APK aus Artifacts herunterladen
5. ✅ Auf Gerät installieren und testen

## 🔗 Weiterführende Links

- [game-ci/unity-builder Dokumentation](https://game.ci/docs/github/builder)
- [Unity Lizenzierung](https://license.unity3d.com/manual)
- [GitHub Actions Dokumentation](https://docs.github.com/en/actions)
- [Android APK signieren](https://developer.android.com/studio/publish/app-signing)

---

*Bei Fragen: GitHub Issues erstellen oder Dokumentation konsultieren.*
