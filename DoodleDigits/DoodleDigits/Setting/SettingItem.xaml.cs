using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoodleDigits.Setting;

[ContentProperty(nameof(Children))]
public partial class SettingItem : UserControl {

    public string SettingName {
        get { return (string)GetValue(SettingNameProperty); }
        set { SetValue(SettingNameProperty, value); }
    }
    public static readonly DependencyProperty SettingNameProperty = DependencyProperty.Register(
        nameof(SettingName),
        typeof(string),
        typeof(SettingItem),
        new PropertyMetadata()
    );

    public string Description {
        get { return (string)GetValue(DescriptionProperty); }
        set { SetValue(DescriptionProperty, value); }
    }
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(SettingItem),
        new PropertyMetadata()
    );

    public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
        nameof(Children), 
        typeof(UIElementCollection),
        typeof(SettingItem),
        new PropertyMetadata());

    public UIElementCollection Children {
        get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
        set { SetValue(ChildrenProperty, value); }
    }

    public SettingItem() {
        InitializeComponent();
        Children = ChildrenPresenter.Children;
    }
}
