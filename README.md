# BlurredBackground.WPF

BlurredBackground is a powerful library for C# WPF applications that allows developers to easily add a blurred background effect to UI elements in their WPF applications without affecting the element's content. Enhance your application’s visual appeal by creating a frosted glass effect on the backgrounds of specified controls.

<p align="center">
        <img src="https://via.placeholder.com/800x400" width="80%">  <!-- Replace with an actual image link -->
</p>

## Features

- **Blur Background**: Easily enable a blur effect on any WPF `Border`.
- **Customizable Blur Radius**: Set the intensity of the blur effect through the `BlurRadius` property.
- **Dynamic Merging**: Control the opacity of the blurred effect with the `Merging` property.
- **DPI Awareness**: Adjusts the blur effect based on the DPI property.

## Getting Started

### Prerequisites

- .NET Framework or .NET Core with WPF support.
- Visual Studio or any compatible IDE for WPF development.

### Installation

Install the [BlurredBackground.WPF NuGet package](https://www.nuget.org/packages/BlurredBackground.WPF) :

```bash
Install-Package BlurredBackground.WPF
```

### Usage

#### 1. Add a Blurred Background to Your XAML

Include the `BlurredBackground` effect on a `Border` in your main window or the appropriate user control.

```xml
<Window x:Class="YourNamespace.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:local="clr-namespace:BlurBackground.WPF"
        Title="Blurred Background Example" Height="350" Width="525">
    <Grid>
        <Border local:BlurredBackground.EnableBlur="True"
                local:BlurredBackground.BlurRadius="10"
                local:BlurredBackground.Merging="0.5"
                local:BlurredBackground.Dpi="96"
                Background="Transparent">
            <TextBlock Text="Hello, World!" 
                       HorizontalAlignment="Center" 
                       VerticalAlignment="Center" 
                       FontSize="32" 
                       Foreground="White"/>
        </Border>
    </Grid>
</Window>
```

#### 2. Configure Properties

- **EnableBlur**: Set to `true` to activate the blur effect.
- **BlurRadius**: Adjusts the strength of the blur. The default value is `20.0`.
- **Merging**: Sets the opacity of the blurred background. The default is `0.5`.
- **Dpi**: Adjusts the DPI settings for the rendered blur effect. Default is `96` A suitable value for performance is `30`.

The library hooks into the `Loaded` and `SizeChanged` events of the `Border` control to apply and update the blur effect dynamically as needed.

## Customization

You can customize the blur appearance by adjusting the `BlurRadius`, `Merging`, and `Dpi` properties on the `Border` control.

<p align="center">
        <img src="https://via.placeholder.com/800x400" width="80%">  <!-- Replace with an actual image link -->
</p>

## Contributing

Contributions are welcome! Please submit pull requests or open issues to discuss potential improvements.

---

Enhance your application's visual appeal with BlurredBackground!