using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Gardiner.XsltTools.Weavers
{
    public class ReturnFixer
    {
        public MethodDefinition Method;
        public Instruction NopBeforeReturn;
        private Instruction _nopForHandleEnd;
        private Collection<Instruction> _instructions;
        private Instruction _sealBranchesNop;
        private VariableDefinition _returnVariable;

        public void MakeLastStatementReturn()
        {
            _instructions = Method.Body.Instructions;
            FixHangingHandlerEnd();

            _sealBranchesNop = Instruction.Create(OpCodes.Nop);
            _instructions.Add(_sealBranchesNop);

            NopBeforeReturn = Instruction.Create(OpCodes.Nop);

            if (IsMethodReturnValue())
            {
                _returnVariable = new VariableDefinition(Method.MethodReturnType.ReturnType);
                Method.Body.Variables.Add(_returnVariable);
            }

            for (var index = 0; index < _instructions.Count; index++)
            {
                var operand = _instructions[index].Operand as Instruction;
                if (operand != null)
                {
                    if (operand.OpCode == OpCodes.Ret)
                    {
                        if (IsMethodReturnValue())
                        {
                            // The C# compiler never jumps directly to a ret
                            // when returning a value from the method. But other Fody
                            // modules and other compilers might. So store the value here.
                            _instructions.Insert(index, Instruction.Create(OpCodes.Stloc, _returnVariable));
                            _instructions.Insert(index, Instruction.Create(OpCodes.Dup));
                            index += 2;
                        }

                        _instructions[index].Operand = _sealBranchesNop;
                    }
                }
            }

            if (!IsMethodReturnValue())
            {
                WithNoReturn();
                return;
            }
            WithReturnValue();
        }

        bool IsMethodReturnValue()
        {
            return Method.MethodReturnType.ReturnType.Name != "Void";
        }

        void FixHangingHandlerEnd()
        {
            if (Method.Body.ExceptionHandlers.Count == 0)
            {
                return;
            }

            _nopForHandleEnd = Instruction.Create(OpCodes.Nop);
            Method.Body.Instructions.Add(_nopForHandleEnd);
            foreach (var handler in Method.Body.ExceptionHandlers)
            {
                if (handler.HandlerStart != null && handler.HandlerEnd == null)
                {
                    handler.HandlerEnd = _nopForHandleEnd;
                }
            }
        }

        void WithReturnValue()
        {

            for (var index = 0; index < _instructions.Count; index++)
            {
                var instruction = _instructions[index];
                if (instruction.OpCode == OpCodes.Ret)
                {
                    _instructions.Insert(index, Instruction.Create(OpCodes.Stloc, _returnVariable));
                    instruction.OpCode = OpCodes.Br;
                    instruction.Operand = _sealBranchesNop;
                    index++;
                }
            }
            _instructions.Add(NopBeforeReturn);
            _instructions.Add(Instruction.Create(OpCodes.Ldloc, _returnVariable));
            _instructions.Add(Instruction.Create(OpCodes.Ret));

        }

        void WithNoReturn()
        {

            foreach (var instruction in _instructions)
            {
                if (instruction.OpCode == OpCodes.Ret)
                {
                    instruction.OpCode = OpCodes.Br;
                    instruction.Operand = _sealBranchesNop;
                }
            }
            _instructions.Add(NopBeforeReturn);
            _instructions.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}