﻿using System;
using System.ComponentModel;
using Avalonia.Controls;

#nullable enable

namespace Avalonia.Markup.Xaml.MarkupExtensions
{
    /// <summary>
    /// Loads a resource dictionary from a specified URL.
    /// </summary>
    public class ResourceInclude : IResourceProvider
    {
        private Uri? _baseUri;
        private IResourceDictionary? _loaded;        

        /// <summary>
        /// Gets the loaded resource dictionary.
        /// </summary>
        public IResourceDictionary Loaded
        {
            get
            {
                if (_loaded is null)
                {                    
                    var loader = new AvaloniaXamlLoader();
                    _loaded = (IResourceDictionary)loader.Load(Source, _baseUri);
                }

                return _loaded;
            }
        }

        public IResourceHost? Owner => Loaded.Owner;

        /// <summary>
        /// Gets or sets the source URL.
        /// </summary>
        public Uri? Source { get; set; }

        bool IResourceNode.HasResources => Loaded.HasResources;

        public event EventHandler OwnerChanged
        {
            add => Loaded.OwnerChanged += value;
            remove => Loaded.OwnerChanged -= value;
        }

        bool IResourceNode.TryGetResource(object key, out object? value)
        {
            return Loaded.TryGetResource(key, out value);
        }

        void IResourceProvider.AddOwner(IResourceHost owner) => Loaded.AddOwner(owner);
        void IResourceProvider.RemoveOwner(IResourceHost owner) => Loaded.RemoveOwner(owner);

        public ResourceInclude ProvideValue(IServiceProvider serviceProvider)
        {
            var tdc = (ITypeDescriptorContext)serviceProvider;
            _baseUri = tdc?.GetContextBaseUri();
            return this;
        }
    }
}
