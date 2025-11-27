#!/usr/bin/env python3
"""
QR-Code Generator für D8-Planer XR
Generiert QR-Codes für Ventile 1-199 und speichert sie als JPG-Dateien

Anforderungen:
- QR-Codes: 50x50mm (für Druck bei 300 DPI = ca. 590x590 px)
- Mit menschenlesbarer Nummer darunter (45x45mm QR + 5mm Text = 50x80mm gesamt)
- Format: JPG
- Dateiname: 1.jpg, 2.jpg, ..., 199.jpg
"""

import qrcode
from PIL import Image, ImageDraw, ImageFont
import os
import sys

class VentilQRGenerator:
    def __init__(self, font_size=72):
        # QR-Code Einstellungen
        self.qr_size_mm = 45  # QR-Code Größe in mm
        self.label_height_mm = 35  # Textbereich unterhalb
        self.total_width_mm = 50  # Gesamtbreite
        self.total_height_mm = 80  # Gesamthöhe
        
        # DPI für Druck (300 DPI = hohe Qualität)
        self.dpi = 300
        
        # Umrechnung mm -> Pixel bei 300 DPI
        # 1 inch = 25.4 mm, 1 inch = 300 pixels bei 300 DPI
        self.mm_to_px = self.dpi / 25.4
        
        # Größen in Pixel
        self.qr_size_px = int(self.qr_size_mm * self.mm_to_px)
        self.label_height_px = int(self.label_height_mm * self.mm_to_px)
        self.total_width_px = int(self.total_width_mm * self.mm_to_px)
        self.total_height_px = int(self.total_height_mm * self.mm_to_px)
        
        # QR-Code Inhalt Template
        self.content_template = "VENTIL-{:03d}"  # z.B. "VENTIL-001"
        
        # Ausgabeordner
        self.output_folder = "QRCodes"
        
        # Schriftgröße für Label
        self.font_size = font_size
        
    def generate_qr_code(self, ventil_number):
        """
        Generiert einen QR-Code für eine Ventilnummer
        """
        # QR-Code Inhalt
        content = self.content_template.format(ventil_number)
        
        # QR-Code erstellen mit hoher Fehlerkorrektur (H = 30%)
        qr = qrcode.QRCode(
            version=1,  # Kleinste Version, wird automatisch erhöht falls nötig
            error_correction=qrcode.constants.ERROR_CORRECT_H,
            box_size=10,  # Größe jedes Box in Pixeln
            border=2,  # Minimaler Rand (quiet zone)
        )
        
        qr.add_data(content)
        qr.make(fit=True)
        
        # QR-Code als Bild erstellen
        qr_img = qr.make_image(fill_color="black", back_color="white")
        
        # Auf gewünschte Größe skalieren
        qr_img = qr_img.resize((self.qr_size_px, self.qr_size_px), Image.LANCZOS)
        
        return qr_img
    
    def add_label(self, qr_img, ventil_number):
        """
        Fügt menschenlesbare Nummer unter dem QR-Code hinzu
        """
        # Neues Bild mit QR-Code + Label
        final_img = Image.new('RGB', (self.total_width_px, self.total_height_px), 'white')
        
        # QR-Code zentriert oben platzieren
        x_offset = (self.total_width_px - self.qr_size_px) // 2
        y_offset = 10  # Kleiner Abstand vom oberen Rand
        final_img.paste(qr_img, (x_offset, y_offset))
        
        # Text hinzufügen
        draw = ImageDraw.Draw(final_img)
        
        # Schriftgröße (groß und gut lesbar)
        font_size = self.font_size
        
        try:
            # Versuche System-Schriftart zu laden
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", font_size)
        except:
            try:
                # Alternative Linux-Schriftart
                font = ImageFont.truetype("/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf", font_size)
            except:
                # Fallback auf Standard-Schriftart
                font = ImageFont.load_default()
        
        # Text (Ventilnummer)
        text = str(ventil_number)
        
        # Textposition zentriert unter QR-Code
        text_y = self.qr_size_px + y_offset + 10
        
        # Text-Bounding-Box für Zentrierung
        bbox = draw.textbbox((0, 0), text, font=font)
        text_width = bbox[2] - bbox[0]
        text_x = (self.total_width_px - text_width) // 2
        
        # Text zeichnen
        draw.text((text_x, text_y), text, fill='black', font=font)
        
        return final_img
    
    def generate_single(self, ventil_number, output_path=None):
        """
        Generiert einen einzelnen QR-Code mit Label
        """
        # QR-Code generieren
        qr_img = self.generate_qr_code(ventil_number)
        
        # Label hinzufügen
        final_img = self.add_label(qr_img, ventil_number)
        
        # Speichern
        if output_path is None:
            output_path = os.path.join(self.output_folder, f"{ventil_number}.jpg")
        
        # Ordner erstellen falls nicht vorhanden
        os.makedirs(os.path.dirname(output_path), exist_ok=True)
        
        # Als JPG mit hoher Qualität speichern
        final_img.save(output_path, "JPEG", quality=95, dpi=(self.dpi, self.dpi))
        
        return output_path
    
    def generate_all(self, start=1, end=199):
        """
        Generiert alle QR-Codes von start bis end
        """
        # Ausgabeordner erstellen
        os.makedirs(self.output_folder, exist_ok=True)
        
        print(f"Generiere QR-Codes für Ventile {start} bis {end}...")
        print(f"Ausgabeordner: {os.path.abspath(self.output_folder)}")
        print(f"Größe: {self.total_width_mm}x{self.total_height_mm}mm ({self.total_width_px}x{self.total_height_px}px bei {self.dpi} DPI)")
        print("-" * 60)
        
        success_count = 0
        error_count = 0
        
        for ventil_num in range(start, end + 1):
            try:
                output_path = self.generate_single(ventil_num)
                success_count += 1
                
                # Fortschritt anzeigen (alle 10)
                if ventil_num % 10 == 0:
                    print(f"Fortschritt: {ventil_num}/{end} QR-Codes erstellt")
                    
            except Exception as e:
                print(f"FEHLER bei Ventil {ventil_num}: {e}")
                error_count += 1
        
        print("-" * 60)
        print(f"Fertig!")
        print(f"Erfolgreich: {success_count}")
        print(f"Fehler: {error_count}")
        print(f"Dateien gespeichert in: {os.path.abspath(self.output_folder)}")
        
        return success_count, error_count


def ask_font_size():
    """
    Fragt den Benutzer nach der gewünschten Schriftgröße
    """
    print("Wie groß soll die Schriftgröße der Nummer unter dem QR-Code sein?")
    print("(Standard: 72, Eingabe ohne Wert übernimmt Standard)")
    
    while True:
        try:
            user_input = input("Schriftgröße: ").strip()
            if user_input == "":
                return 72  # Standardwert
            font_size = int(user_input)
            if font_size < 1 or font_size > 500:
                print("Bitte eine Zahl zwischen 1 und 500 eingeben.")
                continue
            return font_size
        except ValueError:
            print("Bitte eine gültige Zahl eingeben.")


def main():
    """
    Hauptfunktion - Kommandozeilen-Interface
    """
    print("=" * 60)
    print("D8-Planer XR - QR-Code Generator")
    print("=" * 60)
    print()
    
    # Schriftgröße abfragen
    font_size = ask_font_size()
    print(f"Verwende Schriftgröße: {font_size}")
    print()
    
    # Generator erstellen
    generator = VentilQRGenerator(font_size=font_size)
    
    # Argumente prüfen
    if len(sys.argv) > 1:
        if sys.argv[1] == "--single" and len(sys.argv) > 2:
            # Einzelnen QR-Code generieren
            ventil_num = int(sys.argv[2])
            print(f"Generiere QR-Code für Ventil {ventil_num}...")
            path = generator.generate_single(ventil_num)
            print(f"QR-Code gespeichert: {path}")
        elif sys.argv[1] == "--range" and len(sys.argv) > 3:
            # Bereich generieren
            start = int(sys.argv[2])
            end = int(sys.argv[3])
            generator.generate_all(start, end)
        else:
            print("Verwendung:")
            print("  python qr_generator_batch.py                    # Generiert alle (1-199)")
            print("  python qr_generator_batch.py --single <num>     # Generiert einen")
            print("  python qr_generator_batch.py --range <s> <e>    # Generiert Bereich")
    else:
        # Standard: Alle QR-Codes generieren
        generator.generate_all(1, 199)


if __name__ == "__main__":
    main()
