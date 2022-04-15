from tkinter import *
from tkinter import ttk

bar_count = 20
# bar in red, but no text
window = Tk()
frame = Frame(window, width=500, height=200)
window.title("Audio visualizer")
frame.grid()


def add_styles():
    low_style = ttk.Style()
    low_style.theme_use("default")
    low_style.configure("low.Vertical.TProgressbar", foreground="green", background="green")
    medium_style = ttk.Style()
    medium_style.theme_use("default")
    medium_style.configure("medium.Vertical.TProgressbar", foreground="yellow", background="yellow")
    high_style = ttk.Style()
    high_style.theme_use("default")
    high_style.configure("high.Vertical.TProgressbar", foreground="red", background="red")


def set_bar_value(progress_bar: ttk.Progressbar, value):
    maximum = progress_bar["maximum"]
    if value > (2 / 3) * maximum:
        progress_bar["style"] = "high.Vertical.TProgressbar"
    elif value > 1 / 3 * maximum:
        progress_bar["style"] = "medium.Vertical.TProgressbar"
    else:
        progress_bar["style"] = "low.Vertical.TProgressbar"
    progress_bar["value"] = value


add_styles()
for i in range(bar_count):
    bar = ttk.Progressbar(frame,
                          style="high.Vertical.TProgressbar",
                          orient="vertical",
                          mode="determinate",
                          maximum=100)
    bar.grid(row=0, column=i)
    set_bar_value(bar, (i / (bar_count - 1) * 100))

frame.pack()
window.mainloop()
window.quit()
