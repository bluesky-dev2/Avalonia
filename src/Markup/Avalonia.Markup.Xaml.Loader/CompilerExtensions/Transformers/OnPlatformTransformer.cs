﻿using System;
using System.Linq;
using XamlX.Ast;
using XamlX.Transform;
using XamlX.Transform.Transformers;
using XamlX.TypeSystem;

namespace Avalonia.Markup.Xaml.XamlIl.CompilerExtensions.Transformers;

internal class OnPlatformTransformer : IXamlAstTransformer
{
    public IXamlAstNode Transform(AstTransformationContext context, IXamlAstNode node)
    {
        if (node is XamlAstObjectNode xmlobj
            && xmlobj.Type is XamlAstXmlTypeReference xmlref
            && xmlref.Name.StartsWith("OnPlatform")
            && !xmlref.GenericArguments.Any())
        {
            IXamlType propertyType = null;

            if (context.ParentNodes().FirstOrDefault() is XamlAstXamlPropertyValueNode parentPropertyValueNode)
            {
                var property = (XamlAstNamePropertyReference)parentPropertyValueNode.Property;
                var declaringType = TypeReferenceResolver.ResolveType(context, (XamlAstXmlTypeReference)property.DeclaringType, context.StrictMode);
                propertyType = declaringType.Type.GetAllProperties().First(p => p.Name == property.Name).PropertyType;
            }
            else if (context.ParentNodes().FirstOrDefault() is XamlAstObjectNode parentNode)
            {
                var parentType = parentNode.Type is XamlAstClrTypeReference clrType
                    ? clrType.Type
                    : TypeReferenceResolver.ResolveType(context, (XamlAstXmlTypeReference)parentNode.Type, context.StrictMode).Type;
                var contentProperty = context.Configuration.FindContentProperty(parentType);
                propertyType = contentProperty.PropertyType;
            }

            if (propertyType is null)
            {
                throw new InvalidOperationException("Unable to find OnPlatform property type");
            }

            xmlobj.Type = TypeReferenceResolver.ResolveType(context, xmlref.XmlNamespace, xmlref.Name,
                xmlref.IsMarkupExtension, new[] { new XamlAstClrTypeReference(xmlref, propertyType, false) },
                xmlref, context.StrictMode);
        }

        if (node is XamlAstNamePropertyReference xmlprop
            && xmlprop.DeclaringType is XamlAstXmlTypeReference propxmlref
            && propxmlref.Name.StartsWith("OnPlatform")
            && !propxmlref.GenericArguments.Any())
        {
            var expectedType = context.ParentNodes().OfType<XamlAstObjectNode>()
                .First(n => n.Type is XamlAstClrTypeReference clrRef && clrRef.Type.Name.StartsWith("OnPlatform")
                            || n.Type is XamlAstXmlTypeReference xmlRef && xmlRef.Name.StartsWith("OnPlatform"))
                .Type;
            xmlprop.DeclaringType = expectedType;
            xmlprop.TargetType = expectedType;
        }

        // if (node is XamlAstObjectNode onobj
        //     && onobj.Type is XamlAstXmlTypeReference onref
        //     && onref.Name == "On")
        // {
        //     var platformStr = (onobj.Children.OfType<XamlAstXamlPropertyValueNode>()
        //             .FirstOrDefault(v => ((XamlAstNamePropertyReference)v.Property).Name == "Platform")?.Values.Single() as XamlAstTextNode)?
        //         .Text;
        //     if (string.IsNullOrWhiteSpace(platformStr))
        //     {
        //         throw new InvalidOperationException("On.Platform string must be set");
        //     }
        //     var content = onobj.Children.OfType<XamlAstObjectNode>().FirstOrDefault();
        //     if (content is null)
        //     {
        //         throw new InvalidOperationException("On content object must be set");
        //     }
        //
        //     var parentOnPlatformObject = context.ParentNodes().OfType<XamlAstObjectNode>()
        //         .First(n => n.Type is XamlAstClrTypeReference clrRef && clrRef.Type.Name.StartsWith("OnPlatform")
        //                     || n.Type is XamlAstXmlTypeReference xmlRef && xmlRef.Name.StartsWith("OnPlatform"));
        //     parentOnPlatformObject.Children.Remove(onobj);
        //     foreach (var platform in platformStr.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        //     {
        //         var propertyNode = new XamlAstXamlPropertyValueNode(onobj,
        //             new XamlAstNamePropertyReference(onobj, parentOnPlatformObject.Type, platform.Trim(),
        //                 parentOnPlatformObject.Type), content);
        //         parentOnPlatformObject.Children.Add(propertyNode);
        //     }
        //
        //     return parentOnPlatformObject.Children.Last();
        // }
        // if (node is XamlAstXamlPropertyValueNode propNode)
        // {
        //     var type = (propNode.Property as XamlAstNamePropertyReference).TargetType as XamlAstXmlTypeReference;
        //
        //     propNode.VisitChildren(new OnPlatformGenericTypeVisitor(type));
        //
        //     return node;
        // }

        return node;
    }

    private class OnPlatformGenericTypeVisitor : IXamlAstVisitor
    {
        private readonly XamlAstXmlTypeReference _type;
        public OnPlatformGenericTypeVisitor(IXamlAstTypeReference type)
        {

        }

        public IXamlAstNode Visit(IXamlAstNode node)
        {
            if (node is XamlAstXmlTypeReference { Name: "OnPlatform" } xmlref)
            {
                xmlref.GenericArguments.Add(_type);
            }

            return node;
        }

        public void Push(IXamlAstNode node)
        {
        }

        public void Pop()
        {
        }
    }
}
