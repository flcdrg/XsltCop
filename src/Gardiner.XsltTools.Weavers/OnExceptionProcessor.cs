using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Gardiner.XsltTools.Weavers
{
    public class OnExceptionProcessor
    {
        public MethodDefinition Method;
        public ModuleWeaver ModuleWeaver;
        private MethodBody _body;

        AttributeFinder _attributeFinder;
        VariableDefinition _exceptionVariable;

        public void Process()
        {

            _attributeFinder = new AttributeFinder(Method);
            if (!_attributeFinder.LogException)
            {
                return;
            }

            ContinueProcessing();
        }

        void ContinueProcessing()
        {
            _body = Method.Body;

            _body.SimplifyMacros();

            var ilProcessor = _body.GetILProcessor();

            _exceptionVariable = new VariableDefinition(ModuleWeaver.ExceptionType);

            var returnFixer = new ReturnFixer
            {
                Method = Method
            };
            returnFixer.MakeLastStatementReturn();

            _body.Variables.Add(_exceptionVariable);

            var tryBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);
            var catchBlockLeaveInstructions = Instruction.Create(OpCodes.Leave, returnFixer.NopBeforeReturn);

            var methodBodyFirstInstruction = GetMethodBodyFirstInstruction();

            var catchBlockInstructions = GetCatchInstructions(catchBlockLeaveInstructions).ToList();

            ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, tryBlockLeaveInstructions);

            ilProcessor.InsertBefore(returnFixer.NopBeforeReturn, catchBlockInstructions);

            var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                CatchType = ModuleWeaver.ExceptionType,
                TryStart = methodBodyFirstInstruction,
                TryEnd = tryBlockLeaveInstructions.Next,
                HandlerStart = catchBlockInstructions.First(),
                HandlerEnd = catchBlockInstructions.Last().Next
            };

            _body.ExceptionHandlers.Add(handler);

            _body.InitLocals = true;
            _body.OptimizeMacros();
        }

        Instruction GetMethodBodyFirstInstruction()
        {
            if (Method.IsConstructor)
            {
                return _body.Instructions.First(i => i.OpCode == OpCodes.Call).Next;
            }
            return _body.Instructions.First();
        }

        IEnumerable<Instruction> GetCatchInstructions(Instruction catchBlockLeaveInstructions)
        {
            yield return Instruction.Create(OpCodes.Stloc, _exceptionVariable);
            yield return Instruction.Create(OpCodes.Ldloc, _exceptionVariable);
            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod);
            yield return catchBlockLeaveInstructions;
        }
    }
}