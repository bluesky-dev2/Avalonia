using System.Linq;
using XamlX.Ast;
using XamlX.Emit;
using XamlX.IL;
using XamlX.Transform;
using XamlX.TypeSystem;

namespace Avalonia.Markup.Xaml.XamlIl.CompilerExtensions.Transformers
{
    class AvaloniaXamlIlConstructorServiceProviderTransformer : IXamlAstTransformer
    {
        public IXamlAstNode Transform(AstTransformationContext context, IXamlAstNode node)
        {
            if (node is XamlAstObjectNode on && on.Arguments.Count == 0)
            {
                var ctors = on.Type.GetClrType().Constructors;
                if (!ctors.Any(c => c.IsPublic && !c.IsStatic && c.Parameters.Count == 0))
                {
                    var sp = context.Configuration.TypeMappings.ServiceProvider;
                    if (ctors.Any(c =>
                        c.IsPublic && !c.IsStatic && c.Parameters.Count == 1 && c.Parameters[0]
                            .Equals(sp)))
                    {
                        on.Arguments.Add(new InjectServiceProviderNode(sp, on, true));
                    }
                }
            }

            return node;
        }

        internal class InjectServiceProviderNode : XamlAstNode, IXamlAstValueNode,IXamlAstNodeNeedsParentStack,
            IXamlAstEmitableNode<IXamlILEmitter, XamlILNodeEmitResult>
        {
            private readonly bool _inheritContext;

            public InjectServiceProviderNode(IXamlType type, IXamlLineInfo lineInfo, bool inheritContext) : base(lineInfo)
            {
                _inheritContext = inheritContext;
                Type = new XamlAstClrTypeReference(lineInfo, type, false);
            }

            public IXamlAstTypeReference Type { get; }
            public bool NeedsParentStack => _inheritContext;
            public XamlILNodeEmitResult Emit(XamlEmitContext<IXamlILEmitter, XamlILNodeEmitResult> context, IXamlILEmitter codeGen)
            {
                if (_inheritContext)
                {
                    codeGen.Ldloc(context.ContextLocal);
                }
                else
                {
                    var method = context.GetAvaloniaTypes().RuntimeHelpers
                        .FindMethod(m => m.Name == "CreateRootServiceProviderV2");
                    codeGen.EmitCall(method);
                }

                return XamlILNodeEmitResult.Type(0, Type.GetClrType());
            }
        }
    }
}
