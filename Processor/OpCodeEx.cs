using System;

namespace Processor
{
    public static class OpCodeEx
    {
        public static AddressingMode GetAddressingMode(this OpCode @this)
        {
            // TODO: Refactor without casting to byte.
            switch ((byte)@this)
            {
                case 0x0D: //ORA
                case 0x2D: //AND
                case 0x4D: //EOR
                case 0x6D: //ADC
                case 0x8D: //STA
                case 0xAD: //LDA
                case 0xCD: //CMP
                case 0xED: //SBC
                case 0x0E: //ASL
                case 0x2E: //ROL
                case 0x4E: //LSR
                case 0x6E: //ROR
                case 0x8E: //SDX
                case 0xAE: //LDX
                case 0xCE: //DEC
                case 0xEE: //INC
                case 0x2C: //Bit
                case 0x4C: //JMP
                case 0x8C: //STY
                case 0xAC: //LDY
                case 0xCC: //CPY
                case 0xEC: //CPX
                case 0x20: //JSR
                    {
                        return AddressingMode.Absolute;
                    }
                case 0x1D: //ORA
                case 0x3D: //AND
                case 0x5D: //EOR
                case 0x7D: //ADC
                case 0x9D: //STA
                case 0xBD: //LDA
                case 0xDD: //CMP
                case 0xFD: //SBC
                case 0xBC: //LDY
                case 0xFE: //INC
                case 0x1E: //ASL
                case 0x3E: //ROL
                case 0x5E: //LSR
                case 0x7E: //ROR
                    {
                        return AddressingMode.AbsoluteX;
                    }
                case 0x19: //ORA
                case 0x39: //AND
                case 0x59: //EOR
                case 0x79: //ADC
                case 0x99: //STA
                case 0xB9: //LDA
                case 0xD9: //CMP
                case 0xF9: //SBC
                case 0xBE: //LDX
                    {
                        return AddressingMode.AbsoluteY;
                    }
                case 0x0A: //ASL
                case 0x4A: //LSR
                case 0x2A: //ROL
                case 0x6A: //ROR
                    {
                        return AddressingMode.Accumulator;
                    }

                case 0x09: //ORA
                case 0x29: //AND
                case 0x49: //EOR
                case 0x69: //ADC
                case 0xA0: //LDY
                case 0xC0: //CPY
                case 0xE0: //CMP
                case 0xA2: //LDX
                case 0xA9: //LDA
                case 0xC9: //CMP
                case 0xE9: //SBC
                    {
                        return AddressingMode.Immediate;
                    }
                case 0x00: //BRK
                case 0x18: //CLC
                case 0xD8: //CLD
                case 0x58: //CLI
                case 0xB8: //CLV
                case 0xDE: //DEC
                case 0xCA: //DEX
                case 0x88: //DEY
                case 0xE8: //INX
                case 0xC8: //INY
                case 0xEA: //NOP
                case 0x48: //PHA
                case 0x08: //PHP
                case 0x68: //PLA
                case 0x28: //PLP
                case 0x40: //RTI
                case 0x60: //RTS
                case 0x38: //SEC
                case 0xF8: //SED
                case 0x78: //SEI
                case 0xAA: //TAX
                case 0xA8: //TAY
                case 0xBA: //TSX
                case 0x8A: //TXA
                case 0x9A: //TXS
                case 0x98: //TYA
                    {
                        return AddressingMode.Implied;
                    }
                case 0x6C:
                    {
                        return AddressingMode.Indirect;
                    }

                case 0x61: //ADC
                case 0x21: //AND
                case 0xC1: //CMP
                case 0x41: //EOR
                case 0xA1: //LDA
                case 0x01: //ORA
                case 0xE1: //SBC
                case 0x81: //STA
                    {
                        return AddressingMode.IndirectX;
                    }
                case 0x71: //ADC
                case 0x31: //AND
                case 0xD1: //CMP
                case 0x51: //EOR
                case 0xB1: //LDA
                case 0x11: //ORA
                case 0xF1: //SBC
                case 0x91: //STA
                    {
                        return AddressingMode.IndirectY;
                    }
                case 0x90: //BCC
                case 0xB0: //BCS
                case 0xF0: //BEQ
                case 0x30: //BMI
                case 0xD0: //BNE
                case 0x10: //BPL
                case 0x50: //BVC
                case 0x70: //BVS
                    {
                        return AddressingMode.Relative;
                    }
                case 0x65: //ADC
                case 0x25: //AND
                case 0x06: //ASL
                case 0x24: //BIT
                case 0xC5: //CMP
                case 0xE4: //CPX
                case 0xC4: //CPY
                case 0xC6: //DEC
                case 0x45: //EOR
                case 0xE6: //INC
                case 0xA5: //LDA
                case 0xA6: //LDX
                case 0xA4: //LDY
                case 0x46: //LSR
                case 0x05: //ORA
                case 0x26: //ROL
                case 0x66: //ROR
                case 0xE5: //SBC
                case 0x85: //STA
                case 0x86: //STX
                case 0x84: //STY
                    {
                        return AddressingMode.ZeroPage;
                    }
                case 0x75: //ADC
                case 0x35: //AND
                case 0x16: //ASL
                case 0xD5: //CMP
                case 0xD6: //DEC
                case 0x55: //EOR
                case 0xF6: //INC
                case 0xB5: //LDA
                case 0xB6: //LDX
                case 0xB4: //LDY
                case 0x56: //LSR
                case 0x15: //ORA
                case 0x36: //ROL
                case 0x76: //ROR
                case 0xF5: //SBC
                case 0x95: //STA
                case 0x96: //STX
                case 0x94: //STY
                    {
                        return AddressingMode.ZeroPageX;
                    }
                default:
                    throw new NotSupportedException(string.Format("Opcode {0} is not supported", @this));
            }
        }
    }
}
