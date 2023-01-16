using MahApps.Metro.Controls;

namespace DoodleDigits.Setting;
/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : MetroWindow {


    public SettingsViewModel Settings { get; }
    public PresentationProperties PresentationProperties { get; }

    public SettingsWindow(SettingsViewModel settings, PresentationProperties presentationProperties) {
        InitializeComponent();
        Settings = settings;
        PresentationProperties = presentationProperties;
        presentationProperties.RegisterWindow(this);
    }
}
