using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Data;
using Portable.Xaml.Markup;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace Avalonia.Markup.Xaml.XamlIl.Runtime
{
    public static class XamlIlRuntimeHelpers
    {
        public static Func<IServiceProvider, object> DeferredTransformationFactoryV1(Func<IServiceProvider, object> builder,
            IServiceProvider provider)
        {
            // We need to preserve the first IResourceNode from the parent XAML,
            // so Static/DynamicResource extensions can get it
            var resourceNode = provider.GetService<IAvaloniaXamlIlParentStackProvider>().Parents.OfType<IResourceNode>()
                .FirstOrDefault();

            return sp => builder(new DeferredParentServiceProvider(sp, resourceNode));
        }

        class DeferredParentServiceProvider : IAvaloniaXamlIlParentStackProvider, IServiceProvider
        {
            private readonly IServiceProvider _parentProvider;
            private readonly IResourceNode _parentResourceNode;

            public DeferredParentServiceProvider(IServiceProvider parentProvider, IResourceNode parentResourceNode)
            {
                _parentProvider = parentProvider;
                _parentResourceNode = parentResourceNode;
            }

            public IEnumerable<object> Parents => GetParents();

            IEnumerable<object> GetParents()
            {
                if(_parentResourceNode == null)
                    yield break;
                yield return _parentResourceNode;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IAvaloniaXamlIlParentStackProvider))
                    return this;
                return _parentProvider?.GetService(serviceType);
            }
        }


        public static void ApplyNonMatchingMarkupExtensionV1(object target, object property, IServiceProvider prov,
            object value)
        {
            if (value is IBinding b)
            {
                if (property is AvaloniaProperty p)
                    ((AvaloniaObject)target).Bind(p, b);
                else
                    throw new ArgumentException("Attempt to apply binding to non-avalonia property " + property);
            }
            else
                throw new ArgumentException("Don't know what to do with " + value.GetType());
        }

        public static IServiceProvider CreateInnerServiceProviderV1(IServiceProvider compiled) 
            => new InnerServiceProvider(compiled);
       
        class InnerServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _compiledProvider;
            private XamlTypeResolver _resolver;

            public InnerServiceProvider(IServiceProvider compiledProvider)
            {
                _compiledProvider = compiledProvider;
            }
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IXamlTypeResolver))
                    return _resolver ?? (_resolver = new XamlTypeResolver(
                               _compiledProvider.GetService<IAvaloniaXamlIlXmlNamespaceInfoProvider>()));
                return null;
            }
        }

        class XamlTypeResolver : IXamlTypeResolver
        {
            private readonly IAvaloniaXamlIlXmlNamespaceInfoProvider _nsInfo;

            public XamlTypeResolver(IAvaloniaXamlIlXmlNamespaceInfoProvider nsInfo)
            {
                _nsInfo = nsInfo;
            }
            
            public Type Resolve(string qualifiedTypeName)
            {
                var sp = qualifiedTypeName.Split(new[] {':'}, 2);
                var (ns, name) = sp.Length == 1 ? ("", qualifiedTypeName) : (sp[0], sp[1]);
                var namespaces = _nsInfo.XmlNamespaces;
                var dic = (Dictionary<string, IReadOnlyList<AvaloniaXamlIlXmlNamespaceInfo>>)namespaces;
                if (!namespaces.TryGetValue(ns, out var lst))
                    throw new ArgumentException("Unable to resolve namespace for type " + qualifiedTypeName);
                foreach (var entry in lst)
                {
                    var asm = Assembly.Load(new AssemblyName(entry.ClrAssemblyName));
                    var resolved = asm.GetType(entry.ClrNamespace + "." + name);
                    if (resolved != null)
                        return resolved;
                }

                throw new ArgumentException(
                    $"Unable to resolve type {qualifiedTypeName} from any of the following locations: " +
                    string.Join(",", lst.Select(e => $"`{e.ClrAssemblyName}:{e.ClrNamespace}.{name}`")));
            }
        }
    }
}
