using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Antlr4.Runtime;
using Microsoft.Vbe.Interop;
using Rubberduck.Extensions;
using Rubberduck.VBA;
using Rubberduck.VBA.Grammar;
using Rubberduck.VBA.Nodes;

namespace Rubberduck.Inspections
{
    public class ImplicitVariantReturnTypeInspectionResult : CodeInspectionResultBase
    {
        public ImplicitVariantReturnTypeInspectionResult(string name, CodeInspectionSeverity severity, 
            QualifiedContext<ParserRuleContext> qualifiedContext)
            : base(name, severity, qualifiedContext.QualifiedName, qualifiedContext.Context)
        {
        }

        public override IDictionary<string, Action<VBE>> GetQuickFixes()
        {
            return
                new Dictionary<string, Action<VBE>>
                {
                    {"Return explicit Variant", ReturnExplicitVariant}
                };
        }

        private void ReturnExplicitVariant(VBE vbe)
        {
            // note: turns a multiline signature into a one-liner signature.
            // bug: removes all comments.

            var node = GetNode(Context);
            var signature = node.Signature.TrimEnd();

            var procedure = Context.GetText();
            var result = procedure.Replace(signature, signature + ' ' + Tokens.As + ' ' + Tokens.Variant);
            
            var module = vbe.FindCodeModules(QualifiedName).First();
            var selection = Context.GetSelection();

            module.DeleteLines(selection.StartLine, selection.LineCount);
            module.InsertLines(selection.StartLine, result);
        }

        private ProcedureNode GetNode(ParserRuleContext context)
        {
            var result = GetNode(context as VBParser.FunctionStmtContext);
            if (result != null) return result;
            
            result = GetNode(context as VBParser.PropertyGetStmtContext);
            Debug.Assert(result != null, "result != null");

            return result;
        }

        private ProcedureNode GetNode(VBParser.FunctionStmtContext context)
        {
            if (context == null)
            {
                return null;
            }

            var scope = QualifiedName.ToString();
            var localScope = scope + "." + context.AmbiguousIdentifier().GetText();
            return new ProcedureNode(context, scope, localScope);
        }

        private ProcedureNode GetNode(VBParser.PropertyGetStmtContext context)
        {
            if (context == null)
            {
                return null;
            }

            var scope = QualifiedName.ToString();
            var localScope = scope + "." + context.AmbiguousIdentifier().GetText();
            return new ProcedureNode(context, scope, localScope);
        }
    }
}