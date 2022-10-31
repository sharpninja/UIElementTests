using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UIElementTests
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var propNames = new[]
            {
                nameof(Border.BorderThicknessProperty),
                "BorderBrushProperty",
                "TextProperty",
                "ContentProperty",
                "HeaderProperty",
                "WidthProperty",
                "BackgroundProperty"
            };

            List<PropertyInfo> properties = new ();

            foreach(var type in typeof(FrameworkElement).Assembly.GetTypes())
            {
                if (!type.IsAssignableTo(typeof(UIElement))) { continue; }
                foreach(var pi in type.GetProperties(System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.Public))
                {
                    if(pi.PropertyType != typeof(DependencyProperty)) { continue; }

                    if (!propNames.Contains(pi.Name)){ continue; }

                    properties.Add(pi);
                }
            }

            List<IGrouping<string, PropertyInfo>> groups = properties
                .GroupBy(pi => pi.Name)
                .OrderBy(g => g.Key)
                .ToList();

            groups
                .SelectMany(g => g)
                .ToList()
                .ForEach(p => Debug.WriteLine($"{p.DeclaringType.Name}: {p.Name}"));

            AddElement<Border>(groups);
            AddElement<TextBox>(groups);
            AddElement<Expander>(groups);

        }

        private void AddElement<TElement>(List<IGrouping<string, PropertyInfo>> groups)
            where TElement : UIElement, new()
        {
            TElement uiElement = new ()
            {
                Visibility = Visibility.Visible,
                RenderTransform = new TranslateTransform(),
            };

            if(uiElement is Border border)
            {
                border.Child = new TextBlock { Text = $"{uiElement.GetType().Name}: Child Test" };
            }

            foreach(var group in groups)
            {
                foreach(PropertyInfo p in group)
                {
                    if(!typeof(TElement).IsAssignableTo(p.DeclaringType)) { continue; }

                    object value = p.Name switch
                    {
                        nameof(Border.BorderThicknessProperty) => new Thickness(1),
                        "BorderBrushProperty" =>
                            new SolidColorBrush(Color.FromArgb(0xFF, 0x88, 0x88, 0x88)),
                        "BackgroundProperty" =>
                            new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88)),
                        "TextProperty" => $"{uiElement.GetType().Name}: Text Test",
                        "HeaderProperty" => $"{uiElement.GetType().Name}: Header Test",
                        "ContentProperty" =>
                            new TextBlock { Text = $"{uiElement.GetType().Name}: Content Test" },
                        "WidthProperty" => 150,
                        _ => null,
                    };

                    if (value is null) { continue; }

                    uiElement.SetValue(
                        (DependencyProperty)p.GetValue(null),
                        value);
                }
            }

            int offset = Canvas.Children.Count * 160;

            Canvas.Children.Add(uiElement);

            MoveElement(uiElement, new(offset, 0));
        }

        internal void MoveElement(UIElement element, Point point)
        {
            TranslateTransform MoveTransform =
                element.RenderTransform as TranslateTransform;

            var left = Canvas.GetLeft(element);
            var top = Canvas.GetTop(element);

            if (MoveTransform != null)
            {
                MoveTransform.X = left + point.X;
                MoveTransform.Y = top + point.Y;
            }
        }

    }
}
