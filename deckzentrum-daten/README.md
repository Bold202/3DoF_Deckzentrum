# Deckzentrum Daten Sync

Dieses Repository dient als Datenaustausch zwischen dem DB-Planer und der Deckzentrum App (mobile Geräte).

## 📋 Übersicht

- **Zweck:** Zentrale Ablage für CSV-Dateien mit Sauendaten
- **Quelle:** DB-Planer / Sauenplaner Software
- **Ziel:** D8-Planer XR App auf mobilen Geräten

---

## 📁 Ordnerstruktur

```
deckzentrum-daten/
├── README.md          # Diese Dokumentation
├── .gitignore         # Ignoriert temporäre Dateien
└── data/              # CSV-Dateien
    └── beispiel.csv   # Musterdatei als Vorlage
```

---

## 🔐 Personal Access Token erstellen

Um Dateien hochzuladen oder per API abzurufen, benötigen Sie einen **Personal Access Token (PAT)**.

### Schritt-für-Schritt Anleitung

1. **GitHub öffnen** und einloggen
   - Gehen Sie zu: https://github.com

2. **Einstellungen öffnen**
   - Klicken Sie oben rechts auf Ihr Profilbild
   - Wählen Sie "Settings" (Einstellungen)

3. **Developer Settings aufrufen**
   - Scrollen Sie links ganz nach unten
   - Klicken Sie auf "Developer settings"

4. **Personal Access Token erstellen**
   - Klicken Sie auf "Personal access tokens"
   - Wählen Sie "Tokens (classic)"
   - Klicken Sie auf "Generate new token" → "Generate new token (classic)"

5. **Token konfigurieren**
   - **Note:** `Deckzentrum Daten Sync` (oder ein eigener Name)
   - **Expiration:** Wählen Sie eine Gültigkeitsdauer (z.B. 90 Tage)
   - **Scopes:** Aktivieren Sie:
     - ✅ `repo` (Vollzugriff auf private Repositories)

6. **Token generieren und speichern**
   - Klicken Sie auf "Generate token"
   - ⚠️ **WICHTIG:** Kopieren Sie den Token sofort und speichern Sie ihn sicher!
   - Der Token wird nur einmal angezeigt!

### Token sicher aufbewahren

- Speichern Sie den Token in einem Passwort-Manager
- Teilen Sie den Token niemals mit anderen
- Bei Verdacht auf Kompromittierung: Token sofort widerrufen

---

## 📤 CSV-Datei hochladen

### Methode 1: Manuell über die GitHub-Website

1. **Repository öffnen**
   - Navigieren Sie zu Ihrem `deckzentrum-daten` Repository

2. **In den data-Ordner wechseln**
   - Klicken Sie auf den Ordner `data/`

3. **Datei hochladen**
   - Klicken Sie auf "Add file" → "Upload files"
   - Ziehen Sie Ihre CSV-Datei hinein oder klicken Sie auf "choose your files"
   - Geben Sie eine Beschreibung ein (z.B. "Aktualisierte Sauendaten vom 27.11.2024")
   - Klicken Sie auf "Commit changes"

### Methode 2: Via curl (Kommandozeile)

**Voraussetzungen:**
- curl installiert (bei Windows: Git Bash verwenden)
- Personal Access Token vorhanden

**Datei hochladen:**

```bash
# Variablen anpassen
TOKEN="ghp_IhrPersonalAccessToken"
OWNER="IhrBenutzername"
REPO="deckzentrum-daten"
FILEPATH="data/sauen.csv"
LOCALFILE="/pfad/zu/ihrer/sauen.csv"

# Datei in Base64 konvertieren
CONTENT=$(base64 -w 0 "$LOCALFILE")

# Datei hochladen (neu erstellen)
curl -X PUT \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  -d "{\"message\":\"Aktualisierte Sauendaten\",\"content\":\"$CONTENT\"}" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH"
```

**Datei aktualisieren (wenn bereits vorhanden):**

```bash
# Zuerst SHA der bestehenden Datei abrufen
SHA=$(curl -s \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH" \
  | grep '"sha"' | head -1 | cut -d '"' -f 4)

# Datei mit SHA aktualisieren
curl -X PUT \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  -d "{\"message\":\"Aktualisierte Sauendaten\",\"content\":\"$CONTENT\",\"sha\":\"$SHA\"}" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH"
```

### Methode 3: Upload-Script (Windows)

Erstellen Sie eine Datei `upload_csv.bat`:

```batch
@echo off
setlocal EnableDelayedExpansion

:: === Konfiguration ===
set TOKEN=ghp_IhrPersonalAccessToken
set OWNER=IhrBenutzername
set REPO=deckzentrum-daten
set FILEPATH=data/sauen.csv
set LOCALFILE=C:\Pfad\zu\sauen.csv

echo === Deckzentrum CSV Upload ===
echo Lade Datei hoch: %LOCALFILE%

:: PowerShell für Base64-Konvertierung nutzen
for /f "delims=" %%i in ('powershell -Command "[Convert]::ToBase64String([IO.File]::ReadAllBytes('%LOCALFILE%'))"') do set CONTENT=%%i

:: Upload durchführen
curl -X PUT ^
  -H "Authorization: token %TOKEN%" ^
  -H "Accept: application/vnd.github.v3+json" ^
  -d "{\"message\":\"Aktualisierte Sauendaten\",\"content\":\"%CONTENT%\"}" ^
  "https://api.github.com/repos/%OWNER%/%REPO%/contents/%FILEPATH%"

echo.
echo Upload abgeschlossen!
pause
```

### Methode 4: Upload-Script (Linux/Mac)

Erstellen Sie eine Datei `upload_csv.sh`:

```bash
#!/bin/bash

# === Konfiguration ===
TOKEN="ghp_IhrPersonalAccessToken"
OWNER="IhrBenutzername"
REPO="deckzentrum-daten"
FILEPATH="data/sauen.csv"
LOCALFILE="/pfad/zu/sauen.csv"

echo "=== Deckzentrum CSV Upload ==="
echo "Lade Datei hoch: $LOCALFILE"

# Datei in Base64 konvertieren
CONTENT=$(base64 -w 0 "$LOCALFILE")

# Prüfen ob Datei existiert
RESPONSE=$(curl -s \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH")

SHA=$(echo "$RESPONSE" | grep '"sha"' | head -1 | cut -d '"' -f 4)

if [ -z "$SHA" ]; then
  echo "Erstelle neue Datei..."
  curl -X PUT \
    -H "Authorization: token $TOKEN" \
    -H "Accept: application/vnd.github.v3+json" \
    -d "{\"message\":\"Sauendaten hochgeladen\",\"content\":\"$CONTENT\"}" \
    "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH"
else
  echo "Aktualisiere bestehende Datei..."
  curl -X PUT \
    -H "Authorization: token $TOKEN" \
    -H "Accept: application/vnd.github.v3+json" \
    -d "{\"message\":\"Sauendaten aktualisiert\",\"content\":\"$CONTENT\",\"sha\":\"$SHA\"}" \
    "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH"
fi

echo ""
echo "Upload abgeschlossen!"
```

Ausführbar machen:
```bash
chmod +x upload_csv.sh
./upload_csv.sh
```

---

## 📥 CSV-Datei abrufen (via API)

### Methode 1: Einfacher Download via curl

```bash
# Variablen anpassen
TOKEN="ghp_IhrPersonalAccessToken"
OWNER="IhrBenutzername"
REPO="deckzentrum-daten"
FILEPATH="data/sauen.csv"

# Dateiinhalt abrufen (Base64-kodiert)
curl -s \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3+json" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH" \
  | grep '"content"' | cut -d '"' -f 4 | base64 -d > sauen.csv
```

### Methode 2: Raw-Download (direkter Inhalt)

```bash
curl -s \
  -H "Authorization: token $TOKEN" \
  -H "Accept: application/vnd.github.v3.raw" \
  "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH" \
  > sauen.csv
```

### Methode 3: In einer App (Android/Unity)

**C# Beispiel für Unity:**

```csharp
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class GitHubCSVDownloader : MonoBehaviour
{
    // Konfiguration
    private const string TOKEN = "ghp_IhrPersonalAccessToken";
    private const string OWNER = "IhrBenutzername";
    private const string REPO = "deckzentrum-daten";
    private const string FILEPATH = "data/sauen.csv";
    
    // Lokaler Speicherpfad
    private string localPath;
    
    void Start()
    {
        localPath = Application.persistentDataPath + "/sauen.csv";
        StartCoroutine(DownloadCSV());
    }
    
    IEnumerator DownloadCSV()
    {
        string url = $"https://api.github.com/repos/{OWNER}/{REPO}/contents/{FILEPATH}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Header setzen
            request.SetRequestHeader("Authorization", $"token {TOKEN}");
            request.SetRequestHeader("Accept", "application/vnd.github.v3.raw");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                // CSV speichern
                System.IO.File.WriteAllText(localPath, request.downloadHandler.text);
                Debug.Log($"CSV heruntergeladen: {localPath}");
            }
            else
            {
                Debug.LogError($"Download fehlgeschlagen: {request.error}");
            }
        }
    }
}
```

**Kotlin Beispiel für Android:**

```kotlin
import okhttp3.*
import java.io.File
import java.io.IOException

class GitHubCSVDownloader(private val context: Context) {
    
    companion object {
        private const val TOKEN = "ghp_IhrPersonalAccessToken"
        private const val OWNER = "IhrBenutzername"
        private const val REPO = "deckzentrum-daten"
        private const val FILEPATH = "data/sauen.csv"
    }
    
    private val client = OkHttpClient()
    
    fun downloadCSV(callback: (success: Boolean, content: String?) -> Unit) {
        val url = "https://api.github.com/repos/$OWNER/$REPO/contents/$FILEPATH"
        
        val request = Request.Builder()
            .url(url)
            .addHeader("Authorization", "token $TOKEN")
            .addHeader("Accept", "application/vnd.github.v3.raw")
            .build()
        
        client.newCall(request).enqueue(object : Callback {
            override fun onFailure(call: Call, e: IOException) {
                callback(false, null)
            }
            
            override fun onResponse(call: Call, response: Response) {
                if (response.isSuccessful) {
                    val content = response.body?.string()
                    // Datei speichern
                    val file = File(context.filesDir, "sauen.csv")
                    file.writeText(content ?: "")
                    callback(true, content)
                } else {
                    callback(false, null)
                }
            }
        })
    }
}
```

---

## 📋 CSV-Format

### Erforderliche Spalten

| Spalte | Beschreibung | Beispiel |
|--------|--------------|----------|
| Ventilnummer | Ventil-ID (1-199) | 163 |
| Ohrmarkennummer | Eindeutige Sau-ID | DE0123456 |
| Deckdatum | Datum der Deckung | 2024-01-15 |
| Trächtigkeitsstatus | Status | Trächtig, Unsicher, Nicht trächtig |

### Beispiel

```csv
Ventilnummer;Ohrmarkennummer;Deckdatum;Trächtigkeitsstatus;Bemerkungen
163;DE0123456;2024-01-15;Trächtig;
163;DE0123457;2024-01-18;Trächtig;Kontrolle morgen
164;DE0123458;2024-02-05;Unsicher;
```

---

## ⚠️ Wichtige Hinweise

1. **Token-Sicherheit**
   - Speichern Sie den Token niemals im Quellcode
   - Verwenden Sie Umgebungsvariablen oder sichere Konfigurationsdateien
   - Token regelmäßig erneuern

2. **CSV-Encoding**
   - Verwenden Sie **UTF-8** für korrekte Darstellung von Umlauten
   - Trennzeichen: Semikolon (`;`)

3. **Automatische Synchronisation**
   - Richten Sie einen Cronjob/Scheduled Task ein
   - Empfehlung: Täglich vor Arbeitsbeginn synchronisieren

4. **Offline-Nutzung**
   - Die App speichert die CSV-Datei lokal
   - Internetverbindung nur für Synchronisation erforderlich

---

## 🆘 Problemlösung

### "401 Unauthorized"
- Token ist ungültig oder abgelaufen
- Erstellen Sie einen neuen Token

### "404 Not Found"
- Repository-Pfad prüfen
- Datei existiert nicht im angegebenen Pfad

### Umlaute werden falsch angezeigt
- CSV-Datei in UTF-8 speichern
- Im DB-Planer beim Export UTF-8 wählen

### Rate Limit erreicht
- GitHub erlaubt 5.000 API-Aufrufe pro Stunde
- Bei normaler Nutzung nicht relevant

---

## 📞 Support

Bei Fragen:
1. Diese Dokumentation nochmals lesen
2. GitHub Issues im Hauptprojekt erstellen

---

**Version:** 1.0  
**Letzte Aktualisierung:** 2024-11-27
