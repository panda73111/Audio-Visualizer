import sys
from PyQt5.QtWidgets import QApplication

from ReVidiaGUI import ReVidiaMain

bar_count = 20

app = QApplication(sys.argv)
ReVidiaMain()
sys.exit(app.exec())
