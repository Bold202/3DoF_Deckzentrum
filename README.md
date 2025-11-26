# 3DoF_Deckzentrum
App zur XR_Übersicht über tragende Sauen in einem Deckzentrum

# Übersicht:

Die Viture Luma Ultra in Kombination mit dem Neckband Pro erzeugt eine Übersicht über tragende Sauen. Während der Arbeit im Deckzentrum werden die zu den jeweiligen Futter-Ventilen zugeordneten Sauen unter den Ventilen mit einem Ampelsystem angezeigt. Die Ventile im Stall, an denen bis zu 10 Sauen stehen können, werden mithilfe von QR Codes makiert. Die Brille erkennt den QR Code, ermittelt daraus die Ventilnummer und greift auf eine importierte .csv Datei aus dem Sauenplaner zu.

Wir nutzen das Android SDK von Virture um beim vorbei gehen unter jedem erkannten Ventil eine Anzeige einzublenden. Auf der Anzeige ist dann übereinander geschrieben eine Auflistung der Sauen, die sich an diesem Ventil befinden.

Aufgaben für die KI:
- erstelle eine APK Datei, die wir auf dem Neckband installieren können, die autark ohne Netzwerkverbindung anhand einer lokal gespeicherten .csv Datei die Ventilnummern den Ohrmarkennummern zuordnen kann. Erstelle eine QR Code erkennung, die die QR Codes erkennt und einen Bildschirm darunter abbildet (Es reichen auch die Zahlen direkt im Bild eingeblendet.

Ein extra Tool sollte die QR Codes für die Ventilnummern erzeugen. Diese sollen über einen Bluetooth Labelprinter ausgedruckt werden. Dieses Tool kann en extra tool werden, nur die QR Codes sollten bei beiden Tool identisch und kompatibel sein
