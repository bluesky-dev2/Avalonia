﻿using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
#nullable enable

namespace Avalonia.Themes.Default
{
    public class SimpleTheme : AvaloniaObject, IStyle, IResourceProvider
    {
        private readonly Uri _baseUri;
        private bool _isLoading;
        private Styles _simpleDark = new();
        private Styles _simpleLight = new();
        private IStyle? _loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTheme"/> class.
        /// </summary>
        /// <param name="baseUri">The base URL for the XAML context.</param>
        public SimpleTheme(Uri baseUri)
        {
            _baseUri = baseUri;
            InitStyles(_baseUri);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTheme"/> class.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        public SimpleTheme(IServiceProvider serviceProvider)
        {
            _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))).BaseUri;
            InitStyles(_baseUri);
        }

        public static readonly StyledProperty<SimpleThemeMode> ModeProperty =
        AvaloniaProperty.Register<SimpleTheme, SimpleThemeMode>(nameof(Mode));
        /// <summary>
        /// Gets or sets the mode of the fluent theme (light, dark).
        /// </summary>
        public SimpleThemeMode Mode
        {
            get => GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }
        public IStyle Loaded
        {
            get
            {
                if (_loaded == null)
                {
                    _isLoading = true;

                    if (Mode == SimpleThemeMode.Light)
                    {
                        _loaded = _simpleLight;
                    }
                    else if (Mode == SimpleThemeMode.Dark)
                    {
                        _loaded = _simpleDark;
                    }
                    _isLoading = false;
                }

                return _loaded!;
            }
        }

        public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

        bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

        IReadOnlyList<IStyle> IStyle.Children => _loaded?.Children ?? Array.Empty<IStyle>();

        public event EventHandler OwnerChanged
        {
            add
            {
                if (Loaded is IResourceProvider rp)
                {
                    rp.OwnerChanged += value;
                }
            }
            remove
            {
                if (Loaded is IResourceProvider rp)
                {
                    rp.OwnerChanged -= value;
                }
            }
        }

        public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

        public bool TryGetResource(object key, out object? value)
        {
            if (!_isLoading && Loaded is IResourceProvider p)
            {
                return p.TryGetResource(key, out value);
            }

            value = null;
            return false;
        }

        void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);
        void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);
        private void InitStyles(Uri baseUri)
        {

            _simpleLight = new Styles
            {
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml")
                },
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
                }
            };

            _simpleDark = new Styles
            {
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml")
                },
                new StyleInclude(baseUri)
                {
                    Source = new Uri("avares://Avalonia.Themes.Default/DefaultTheme.xaml")
                }
            };
        }

    }
}
