using System;

namespace Processor
{
    public static class OpCodeEx
    {
        public static AddressingMode GetAddressingMode(this OpCode @this)
        {
            switch (@this)
            {
                case OpCode.OraAbsolute:
                case OpCode.AndAbsolute:
                case OpCode.EorAbsolute:
                case OpCode.AdcAbsolute:
                case OpCode.StaAbsolute:
                case OpCode.LdaAbsolute:
                case OpCode.CmpAbsolute:
                case OpCode.SbcAbsolute:
                case OpCode.AslAbsolute:
                case OpCode.RolAbsolute:
                case OpCode.LsrAbsolute:
                case OpCode.RorAbsolute:
                case OpCode.LdxAbsolute:
                case OpCode.DecAbsolute:
                case OpCode.IncAbsolute:
                case OpCode.BitAbsolute:
                case OpCode.JmpAbsolute:
                case OpCode.StyAbsolute:
                case OpCode.LdyAbsolute:
                case OpCode.CpyAbsolute:
                case OpCode.CpxAbsolute:
                case OpCode.JsrAbsolute:
                case OpCode.StxAbsolute:
                    return AddressingMode.Absolute;

                case OpCode.OraAbsoluteX:
                case OpCode.AndAbsoluteX:
                case OpCode.EorAbsoluteX:
                case OpCode.AdcAbsoluteX:
                case OpCode.StaAbsoluteX:
                case OpCode.LdaAbsoluteX:
                case OpCode.CmpAbsoluteX:
                case OpCode.SbcAbsoluteX:
                case OpCode.IncAbsoluteX:
                case OpCode.AslAbsoluteX:
                case OpCode.RolAbsoluteX:
                case OpCode.LsrAbsoluteX:
                case OpCode.RorAbsoluteX:
                case OpCode.DecAbsoluteX:
                    return AddressingMode.AbsoluteX;

                case OpCode.OraAbsoluteY:
                case OpCode.AndAbsoluteY:
                case OpCode.EorAbsoluteY:
                case OpCode.AdcAbsoluteY:
                case OpCode.StaAbsoluteY:
                case OpCode.LdaAbsoluteY:
                case OpCode.CmpAbsoluteY:
                case OpCode.SbcAbsoluteY:
                case OpCode.LdxAbsoluteY:
                case OpCode.LdyAbsoluteY:
                    return AddressingMode.AbsoluteY;

                case OpCode.AslAccumulator:
                case OpCode.LsrAccumulator:
                case OpCode.RolAccumulator:
                case OpCode.RorAccumulator:
                    return AddressingMode.Accumulator;

                case OpCode.OraImmediate:
                case OpCode.AndImmediate:
                case OpCode.EorImmediate:
                case OpCode.AdcImmediate:
                case OpCode.LdyImmediate:
                case OpCode.CpyImmediate:
                case OpCode.CmpImmediate:
                case OpCode.LdxImmediate:
                case OpCode.LdaImmediate:
                case OpCode.SbcImmediate:
                case OpCode.CpxImmediate:
                    return AddressingMode.Immediate;

                case OpCode.BrkImplied:
                case OpCode.ClcImplied:
                case OpCode.CldImplied:
                case OpCode.CliImplied:
                case OpCode.ClvImplied:
                case OpCode.DexImplied:
                case OpCode.DeyImplied:
                case OpCode.InxImplied:
                case OpCode.InyImplied:
                case OpCode.NopImplied:
                case OpCode.PhaImplied:
                case OpCode.PhpImplied:
                case OpCode.PlaImplied:
                case OpCode.PlpImplied:
                case OpCode.RtiImplied:
                case OpCode.RtsImplied:
                case OpCode.SecImplied:
                case OpCode.SedImplied:
                case OpCode.SeiImplied:
                case OpCode.TaxImplied:
                case OpCode.TayImplied:
                case OpCode.TsxImplied:
                case OpCode.TxaImplied:
                case OpCode.TxsImplied:
                case OpCode.TyaImplied:
                    return AddressingMode.Implied;

                case OpCode.JmpIndirect:
                    return AddressingMode.Indirect;

                case OpCode.AndIndirectX:
                case OpCode.CmpIndirectX:
                case OpCode.LdaIndirectX:
                case OpCode.OraIndirectX:
                case OpCode.StaIndirectX:
                case OpCode.SbcIndirectX:
                case OpCode.AdcIndirectX:
                case OpCode.EorIndirectX:
                    return AddressingMode.IndirectX;

                case OpCode.AdcIndirectY:
                case OpCode.AndIndirectY:
                case OpCode.CmpIndirectY:
                case OpCode.EorIndirectY:
                case OpCode.LdaIndirectY:
                case OpCode.OraIndirectY:
                case OpCode.SbcIndirectY:
                case OpCode.StaIndirectY:
                    return AddressingMode.IndirectY;

                case OpCode.BccRelative:
                case OpCode.BcsRelative:
                case OpCode.BeqRelative:
                case OpCode.BmiRelative:
                case OpCode.BneRelative:
                case OpCode.BplRelative:
                case OpCode.BvcRelative:
                case OpCode.BvsRelative:
                    return AddressingMode.Relative;

                case OpCode.AdcZeroPage:
                case OpCode.AndZeroPage:
                case OpCode.AslZeroPage:
                case OpCode.BitZeroPage:
                case OpCode.CmpZeroPage:
                case OpCode.CpxZeroPage:
                case OpCode.CpyZeroPage:
                case OpCode.DecZeroPage:
                case OpCode.EorZeroPage:
                case OpCode.IncZeroPage:
                case OpCode.LdaZeroPage:
                case OpCode.LdxZeroPage:
                case OpCode.LdyZeroPage:
                case OpCode.LsrZeroPage:
                case OpCode.OraZeroPage:
                case OpCode.RolZeroPage:
                case OpCode.RorZeroPage:
                case OpCode.SbcZeroPage:
                case OpCode.StaZeroPage:
                case OpCode.StxZeroPage:
                case OpCode.StyZeroPage:
                    return AddressingMode.ZeroPage;

                case OpCode.AdcZeroPageX:
                case OpCode.AndZeroPageX:
                case OpCode.AslZeroPageX:
                case OpCode.CmpZeroPageX:
                case OpCode.DecZeroPageX:
                case OpCode.EorZeroPageX:
                case OpCode.IncZeroPageX:
                case OpCode.LdaZeroPageX:
                case OpCode.LsrZeroPageX:
                case OpCode.OraZeroPageX:
                case OpCode.RolZeroPageX:
                case OpCode.RorZeroPageX:
                case OpCode.SbcZeroPageX:
                case OpCode.StaZeroPageX:
                case OpCode.StyZeroPageX:
                    return AddressingMode.ZeroPageX;

                case OpCode.LdxZeroPageY:
                case OpCode.LdyZeroPageY:
                case OpCode.StxZeroPageY:
                    return AddressingMode.ZeroPageY;

                default:
                    throw new NotSupportedException($"Opcode {@this} is not supported");
            }
        }
    }
}
