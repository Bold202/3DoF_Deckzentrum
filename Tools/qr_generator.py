#!/usr/bin/env python3
"""
QR-Code Generator für D8-Planer XR
Erstellt QR-Codes für Ventilnummern 1-199
Standalone-Tool, unabhängig von Unity
"""

import qrcode
from PIL import Image, ImageDraw, ImageFont
import os
from pathlib import Path

class VentilQRCodeGenerator:
    """QR-Code Generator für Ventilnummern"""
    
    def __init__(self, output_dir="QRCodes"):
        """
        Initialisiert den Generator
        
        Args:
            output_dir: Ausgabeverzeichnis für QR-Codes
        """
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(exist_ok=True)
        
        # QR-Code Einstellungen
        self.qr_version = 1  # Version 1 = kleinste Größe
        self.error_correction = qrcode.constants.ERROR_CORRECT_H  # Höchste Fehlerkorrektur (30%)
        self.box_size = 10  # Pixel pro Box
        self.border = 1  # Minimaler Rand
        
        # Template für QR-Code Inhalt
        self.content_template = "VENTIL-{:03d}"
        
        # Datei-Optionen
        self.file_prefix = "Ventil_"
        self.file_extension = ".png"
        
    def generate_single_qr(self, ventil_number, add_label=True):
        """
        Generiert einen einzelnen QR-Code
        
        Args:
            ventil_number: Ventilnummer (1-199)
            add_label: Fügt menschenlesbare Nummer hinzu
            
        Returns:
            PIL.Image: QR-Code als Bild
        """
        # QR-Code Inhalt erstellen
        content = self.content_template.format(ventil_number)
        
        # QR-Code Generator initialisieren
        qr = qrcode.QRCode(
            version=self.qr_version,
            error_correction=self.error_correction,
            box_size=self.box_size,
            border=self.border,
        )
        
        # Daten hinzufügen
        qr.add_data(content)
        qr.make(fit=True)
        
        # Bild erstellen
        img = qr.make_image(fill_color="black", back_color="white")
        
        # Label hinzufügen falls gewünscht
        if add_label:
            img = self._add_label(img, ventil_number)
        
        return img
    
    def _add_label(self, qr_image, ventil_number):
        """
        Fügt eine menschenlesbare Nummer unter dem QR-Code hinzu
        
        Args:
            qr_image: QR-Code Bild
            ventil_number: Ventilnummer
            
        Returns:
            PIL.Image: QR-Code mit Label
        """
        # Dimensionen
        qr_width, qr_height = qr_image.size
        label_height = 60
        total_height = qr_height + label_height
        
        # Neues Bild mit Platz für Label
        labeled_img = Image.new('RGB', (qr_width, total_height), 'white')
        
        # QR-Code einfügen (oben)
        labeled_img.paste(qr_image, (0, 0))
        
        # Text zeichnen
        draw = ImageDraw.Draw(labeled_img)
        text = f"Ventil {ventil_number:03d}"
        
        # Versuche Font zu laden, sonst Standardfont
        try:
            font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 40)
        except:
            try:
                font = ImageFont.truetype("arial.ttf", 40)
            except:
                font = ImageFont.load_default()
        
        # Text zentrieren
        bbox = draw.textbbox((0, 0), text, font=font)
        text_width = bbox[2] - bbox[0]
        text_x = (qr_width - text_width) // 2
        text_y = qr_height + 10
        
        # Text zeichnen
        draw.text((text_x, text_y), text, fill='black', font=font)
        
        return labeled_img
    
    def generate_all_qr_codes(self, start=1, end=199, add_label=True):
        """
        Generiert alle QR-Codes für den angegebenen Bereich
        
        Args:
            start: Start-Ventilnummer
            end: End-Ventilnummer
            add_label: Fügt menschenlesbare Nummern hinzu
        """
        print(f"Starte QR-Code Generierung für Ventile {start} bis {end}")
        print(f"Ausgabeverzeichnis: {self.output_dir.absolute()}")
        
        success_count = 0
        error_count = 0
        
        for ventil_number in range(start, end + 1):
            try:
                # QR-Code generieren
                img = self.generate_single_qr(ventil_number, add_label)
                
                # Dateiname erstellen
                filename = f"{self.file_prefix}{ventil_number:03d}{self.file_extension}"
                filepath = self.output_dir / filename
                
                # Speichern
                img.save(filepath, "PNG")
                success_count += 1
                
                # Fortschritt anzeigen
                if ventil_number % 10 == 0:
                    print(f"Fortschritt: {ventil_number}/{end} QR-Codes erstellt")
                    
            except Exception as e:
                print(f"Fehler bei Ventil {ventil_number}: {e}")
                error_count += 1
        
        print(f"\nQR-Code Generierung abgeschlossen!")
        print(f"Erfolgreich: {success_count}, Fehler: {error_count}")
        print(f"Dateien gespeichert in: {self.output_dir.absolute()}")

def main():
    """Hauptfunktion"""
    import argparse
    
    parser = argparse.ArgumentParser(description='QR-Code Generator für D8-Planer XR Ventile')
    parser.add_argument('--start', type=int, default=1, help='Start-Ventilnummer (default: 1)')
    parser.add_argument('--end', type=int, default=199, help='End-Ventilnummer (default: 199)')
    parser.add_argument('--output', type=str, default='QRCodes', help='Ausgabeverzeichnis (default: QRCodes)')
    parser.add_argument('--no-label', action='store_true', help='Keine menschenlesbare Nummer hinzufügen')
    parser.add_argument('--single', type=int, help='Nur einen einzelnen QR-Code generieren')
    
    args = parser.parse_args()
    
    # Generator erstellen
    generator = VentilQRCodeGenerator(output_dir=args.output)
    
    if args.single:
        # Einzelner QR-Code
        img = generator.generate_single_qr(args.single, add_label=not args.no_label)
        filename = f"Ventil_{args.single:03d}.png"
        filepath = generator.output_dir / filename
        img.save(filepath)
        print(f"QR-Code erstellt: {filepath}")
    else:
        # Alle QR-Codes generieren
        generator.generate_all_qr_codes(
            start=args.start,
            end=args.end,
            add_label=not args.no_label
        )

if __name__ == '__main__':
    main()
