using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ControlzEx.Theming;
using DoodleDigits.Setting;

namespace DoodleDigits;

public class PresentationProperties : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private readonly Uri imageSourceDark = new Uri("/Resources/grid_dark.png", UriKind.Relative);
    private readonly Brush inputTextColorDark = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
    private readonly Brush labelTextColorDark = new SolidColorBrush(Color.FromRgb(0x2E, 0xA0, 0xFF));

    private readonly Uri imageSourceLight = new Uri("/Resources/grid.png", UriKind.Relative);
    private readonly Brush inputTextColorLight = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11));
    private readonly Brush labelTextColorLight = new SolidColorBrush(Color.FromRgb(0x1E, 0x90, 0xFF));

    private List<FrameworkElement> registeredWindows;

    public PresentationProperties(SettingsViewModel settings) {
        registeredWindows = new();
        imageSourceField = imageSourceLight;
        inputTextColorField = inputTextColorLight;
        labelTextColorField = labelTextColorLight;

        DarkMode = settings.DarkMode;
        ZoomTicks = settings.ZoomTicks;
        AlwaysOnTop= settings.AlwaysOnTop;

        settings.PropertyChanged += (s, e) => {
            switch (e.PropertyName) {
                case nameof(settings.DarkMode):
                    DarkMode = settings.DarkMode;
                    break;
                case nameof(settings.ZoomTicks):
                    ZoomTicks = settings.ZoomTicks;
                    break;
                case nameof(settings.AlwaysOnTop):
                    AlwaysOnTop = settings.AlwaysOnTop;
                    break;
            }
        };
    }

    public void RegisterWindow(FrameworkElement frameworkElement) {
        registeredWindows.Add(frameworkElement);
        ThemeManager.Current.ChangeTheme(frameworkElement, GetThemeName(darkModeField));
    }
    public void UnregisterWindow(FrameworkElement frameworkElement) {
        registeredWindows.Remove(frameworkElement);
    }

    private Uri imageSourceField;
    public Uri ImageSource {
        get => imageSourceField;
        private set {
            imageSourceField = value;
            OnPropertyChanged();
        }
    }

    private int zoomTicksField;

    public int ZoomTicks {
        get => zoomTicksField;
        set {
            zoomTicksField = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Zoom));
        }
    }
    public float Zoom => 1 + ZoomTicks * 0.1f;

    private bool darkModeField;
    public bool DarkMode {
        get => darkModeField;
        private set {
            darkModeField = value;
            if (darkModeField) {
                ImageSource = imageSourceDark;
                InputTextColor = inputTextColorDark;
                LabelTextColor = labelTextColorDark;
            }
            else {
                ImageSource = imageSourceLight;
                InputTextColor = inputTextColorLight;
                LabelTextColor = labelTextColorLight;
            }
            foreach (var window in registeredWindows) {
                ThemeManager.Current.ChangeTheme(window, GetThemeName(darkModeField));
            }
            OnPropertyChanged();
        }
    }

    private string GetThemeName(bool isDarkTheme) => isDarkTheme ? "Dark.Blue" : "Light.Blue";


    private bool forceOnTopField;
    public bool AlwaysOnTop {
        get => forceOnTopField; 
        private set {
            forceOnTopField = value;
            OnPropertyChanged();
        }
    }

    private Brush inputTextColorField;
    public Brush InputTextColor {
        get => inputTextColorField;
        private set {
            inputTextColorField = value;
            OnPropertyChanged();
        }
    }

    private Brush labelTextColorField;
    public Brush LabelTextColor {
        get => labelTextColorField;
        private set {
            labelTextColorField = value;
            OnPropertyChanged();
        }
    }
}
