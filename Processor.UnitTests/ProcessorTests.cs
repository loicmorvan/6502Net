using System;
using Xunit;

namespace Processor.UnitTests
{
    public class ProcessorTests
    {
        #region Initialization Tests
        [Fact]
        // ReSharper disable InconsistentNaming
        public void Processor_Status_Flags_Initialized_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(memory);
            Assert.False(processor.CarryFlag);
            Assert.False(processor.ZeroFlag);
            Assert.False(processor.DisableInterruptFlag);
            Assert.False(processor.DecimalFlag);
            Assert.False(processor.OverflowFlag);
            Assert.False(processor.NegativeFlag);
        }

        [Fact]
        public void Processor_Registers_Initialized_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(memory);
            Assert.Equal(0, processor.Accumulator);
            Assert.Equal(0, processor.XRegister);
            Assert.Equal(0, processor.YRegister);
            Assert.Equal(OpCode.BrkImplied, processor.CurrentOpCode);
            Assert.Equal<Address>(0, processor.ProgramCounter);
        }

        [Fact]
        public void ProgramCounter_Correct_When_Program_Loaded()
        {
            var memory = Memory.LoadProgram(0, new byte[1], 0x01);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0x01, processor.ProgramCounter);
        }

        [Fact]
        public void Throws_Exception_When_OpCode_Is_Invalid()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0x00, new byte[] { 0xFF }, 0x00));

            Assert.Throws<NotSupportedException>(() => processor.NextStep());
        }

        [Fact]
        public void Stack_Pointer_Initializes_To_Default_Value_After_Reset()
        {
            var memory = new Memory();
            var processor = new Processor(memory);

            // TODO: Was 0xFD, but I don't see why...
            Assert.Equal(0xFF, processor.StackPointer);
        }
        #endregion

        #region ADC - Add with Carry Tests

        [Theory]
        [InlineData(0, 0, false, 0)]
        [InlineData(0, 1, false, 1)]
        [InlineData(1, 2, false, 3)]
        [InlineData(255, 1, false, 0)]
        [InlineData(254, 1, false, 255)]
        [InlineData(255, 0, false, 255)]
        [InlineData(0, 0, true, 1)]
        [InlineData(0, 1, true, 2)]
        [InlineData(1, 2, true, 4)]
        [InlineData(254, 1, true, 0)]
        [InlineData(253, 1, true, 255)]
        [InlineData(254, 0, true, 255)]
        [InlineData(255, 255, true, 255)]
        public void ADC_Accumulator_Correct_When_Not_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd, bool CarryFlagSet, byte expectedValue)
        {
            var processor = new Processor(
                CarryFlagSet ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00));

            Assert.Equal(0x00, processor.Accumulator);

            if (CarryFlagSet)
            {
                processor.NextStep();
            }

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [Theory]
        [InlineData(0x99, 0x99, false, 0x98)]
        [InlineData(0x99, 0x99, true, 0x99)]
        [InlineData(0x90, 0x99, false, 0x89)]
        public void ADC_Accumulator_Correct_When_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd,
                                                                       bool setCarryFlag, byte expectedValue)
        {
            var memory = setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xF8, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xF8, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [Theory]
        [InlineData(254, 1, false, false)]
        [InlineData(254, 1, true, true)]
        [InlineData(253, 1, true, false)]
        [InlineData(255, 1, false, true)]
        [InlineData(255, 1, true, true)]
        public void ADC_Carry_Correct_When_Not_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd, bool setCarryFlag,
                                                                     bool expectedValue)
        {
            var memory =
                setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.CarryFlag);
        }

        // TODO: `setCarryFlag` is not used, seems to be a mistake.
        [Theory]
        [InlineData(98, 1, false)]
        [InlineData(99, 1, false)]
        public void ADC_Carry_Correct_When_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xF8, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(255, 1, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, false)]
        public void ADC_Zero_Flag_Correct_When_Not_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(126, 1, false)]
        [InlineData(1, 126, false)]
        [InlineData(1, 127, true)]
        [InlineData(127, 1, true)]
        [InlineData(1, 254, true)]
        [InlineData(254, 1, true)]
        [InlineData(1, 255, false)]
        [InlineData(255, 1, false)]
        public void ADC_Negative_Flag_Correct(byte accumlatorIntialValue, byte amountToAdd, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);


            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0, 127, false, false)]
        [InlineData(0, 128, false, false)]
        [InlineData(1, 127, false, true)]
        [InlineData(1, 128, false, false)]
        [InlineData(127, 1, false, true)]
        [InlineData(127, 127, false, true)]
        [InlineData(128, 127, false, false)]
        [InlineData(128, 128, false, true)]
        [InlineData(128, 129, false, true)]
        [InlineData(128, 255, false, true)]
        [InlineData(255, 0, false, false)]
        [InlineData(255, 1, false, false)]
        [InlineData(255, 127, false, false)]
        [InlineData(255, 128, false, true)]
        [InlineData(255, 255, false, false)]
        [InlineData(0, 127, true, true)]
        [InlineData(0, 128, true, false)]
        [InlineData(1, 127, true, true)]
        [InlineData(1, 128, true, false)]
        [InlineData(127, 1, true, true)]
        [InlineData(127, 127, true, true)]
        [InlineData(128, 127, true, false)]
        [InlineData(128, 128, true, true)]
        [InlineData(128, 129, true, true)]
        [InlineData(128, 255, true, false)]
        [InlineData(255, 0, true, false)]
        [InlineData(255, 1, true, false)]
        [InlineData(255, 127, true, false)]
        [InlineData(255, 128, true, false)]
        [InlineData(255, 255, true, false)]
        public void ADC_Overflow_Flag_Correct(byte accumlatorIntialValue, byte amountToAdd, bool setCarryFlag, bool expectedValue)
        {
            var memory = setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x69, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.OverflowFlag);
        }
        #endregion

        #region AND - Compare Memory with Accumulator
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(255, 255, 255)]
        [InlineData(255, 254, 254)]
        [InlineData(170, 85, 0)]
        public void AND_Accumulator_Correct(byte accumlatorIntialValue, byte amountToAnd, byte expectedResult)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0x29, amountToAnd }, 0x00);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.Accumulator);
        }
        #endregion

        #region ASL - Arithmetic Shift Left

        [Theory]
        [InlineData(0x0A, 109, 218, 0)] // ASL Accumulator
        [InlineData(0x0A, 108, 216, 0)] // ASL Accumulator
        [InlineData(0x06, 109, 218, 0x01)] // ASL Zero Page
        [InlineData(0x16, 109, 218, 0x01)] // ASL Zero Page X
        [InlineData(0x0E, 109, 218, 0x01)] // ASL Absolute
        [InlineData(0x1E, 109, 218, 0x01)] // ASL Absolute X
        public void ASL_Correct_Value_Stored(byte operation, byte valueToShift, byte expectedValue, byte expectedLocation)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToShift, operation, expectedLocation }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue,
                operation == 0x0A ?
                    processor.Accumulator :
                    processor.ReadMemoryValue(expectedLocation));
        }

        [Theory]
        [InlineData(127, false)]
        [InlineData(128, true)]
        [InlineData(255, true)]
        [InlineData(0, false)]
        public void ASL_Carry_Set_Correctly(byte valueToShift, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToShift, 0x0A }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.CarryFlag);
        }

        [Theory]
        [InlineData(63, false)]
        [InlineData(64, true)]
        [InlineData(127, true)]
        [InlineData(128, false)]
        [InlineData(0, false)]
        public void ASL_Negative_Set_Correctly(byte valueToShift, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToShift, 0x0A }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(127, false)]
        [InlineData(128, true)]
        [InlineData(0, true)]
        public void ASL_Zero_Set_Correctly(byte valueToShift, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToShift, 0x0A }, 0x00);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }
        #endregion

        #region BCC - Branch On Carry Clear

        [Theory]
        [InlineData(0, 1, 3)]
        [InlineData(0x80, 0x80, 2)]
        [InlineData(0, 0xFD, 0xFFFF)]
        [InlineData(0x7D, 0x80, 0xFFFF)]
        public void BCC_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0x90, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region BCS - Branch on Carry Set
        [Theory]
        [InlineData(0, 1, 4)]
        [InlineData(0x80, 0x80, 3)]
        [InlineData(0, 0xFC, 0xFFFF)]
        [InlineData(0x7C, 0x80, 0xFFFF)]
        public void BCS_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0x38, 0xB0, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region BEQ - Branch on Zero Set
        [Theory]
        [InlineData(0, 1, 5)]
        [InlineData(0x80, 0x80, 4)]
        [InlineData(0, 0xFB, 0xFFFF)]
        [InlineData(0x7B, 0x80, 0xFFFF)]
        [InlineData(2, 0xFE, 4)]
        public void BEQ_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0xA9, 0x00, 0xF0, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }

        #endregion

        #region BIT - Compare Memory with Accumulator

        [Theory]
        [InlineData(0x24, 0x7f, 0x7F, false)] // BIT Zero Page
        [InlineData(0x24, 0x80, 0x7F, false)] // BIT Zero Page
        [InlineData(0x24, 0x7F, 0x80, true)] // BIT Zero Page
        [InlineData(0x24, 0x80, 0xFF, true)] // BIT Zero Page
        [InlineData(0x24, 0xFF, 0x80, true)] // BIT Zero Page
        [InlineData(0x2C, 0x7F, 0x7F, false)] // BIT Absolute
        [InlineData(0x2C, 0x80, 0x7F, false)] // BIT Absolute
        [InlineData(0x2C, 0x7F, 0x80, true)] // BIT Absolute
        [InlineData(0x2C, 0x80, 0xFF, true)] // BIT Absolute
        [InlineData(0x2C, 0xFF, 0x80, true)] // BIT Absolute
        public void BIT_Negative_Set_When_Comparison_Is_Negative_Number(byte operation, byte accumulatorValue, byte valueToTest, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0x00, new byte[] { 0xA9, accumulatorValue, operation, 0x06, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0x24, 0x3F, 0x3F, false)] // BIT Zero Page
        [InlineData(0x24, 0x3F, 0x40, true)] // BIT Zero Page
        [InlineData(0x24, 0x40, 0x3F, false)] // BIT Zero Page
        [InlineData(0x24, 0x40, 0x7F, true)] // BIT Zero Page
        [InlineData(0x24, 0x7F, 0x40, true)] // BIT Zero Page
        [InlineData(0x24, 0x7F, 0x80, false)] // BIT Zero Page
        [InlineData(0x24, 0x80, 0x7F, true)] // BIT Zero Page
        [InlineData(0x24, 0xC0, 0xDF, true)] // BIT Zero Page
        [InlineData(0x24, 0xDF, 0xC0, true)] // BIT Zero Page
        [InlineData(0x24, 0xC0, 0xFF, true)] // BIT Zero Page
        [InlineData(0x24, 0xFF, 0xC0, true)] // BIT Zero Page
        [InlineData(0x24, 0x40, 0xFF, true)] // BIT Zero Page
        [InlineData(0x24, 0xFF, 0x40, true)] // BIT Zero Page
        [InlineData(0x24, 0xC0, 0x7F, true)] // BIT Zero Page
        [InlineData(0x24, 0x7F, 0xC0, true)] // BIT Zero Page
        [InlineData(0x2C, 0x3F, 0x3F, false)] // BIT Absolute
        [InlineData(0x2C, 0x3F, 0x40, true)] // BIT Absolute
        [InlineData(0x2C, 0x40, 0x3F, false)] // BIT Absolute
        [InlineData(0x2C, 0x40, 0x7F, true)] // BIT Absolute
        [InlineData(0x2C, 0x7F, 0x40, true)] // BIT Absolute
        [InlineData(0x2C, 0x7F, 0x80, false)] // BIT Absolute
        [InlineData(0x2C, 0x80, 0x7F, true)] // BIT Absolute
        [InlineData(0x2C, 0xC0, 0xDF, true)] // BIT Absolute
        [InlineData(0x2C, 0xDF, 0xC0, true)] // BIT Absolute
        [InlineData(0x2C, 0xC0, 0xFF, true)] // BIT Absolute
        [InlineData(0x2C, 0xFF, 0xC0, true)] // BIT Absolute
        [InlineData(0x2C, 0x40, 0xFF, true)] // BIT Absolute
        [InlineData(0x2C, 0xFF, 0x40, true)] // BIT Absolute
        [InlineData(0x2C, 0xC0, 0x7F, true)] // BIT Absolute
        [InlineData(0x2C, 0x7F, 0xC0, true)] // BIT Absolute
        public void BIT_Overflow_Set_By_Bit_Six(byte operation, byte accumulatorValue, byte valueToTest, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0x00, new byte[] { 0xA9, accumulatorValue, operation, 0x06, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.OverflowFlag);
        }

        [Theory]
        [InlineData(0x24, 0x00, 0x00, true)] // BIT Zero Page
        [InlineData(0x24, 0xFF, 0xFF, false)] // BIT Zero Page
        [InlineData(0x24, 0xAA, 0x55, true)] // BIT Zero Page
        [InlineData(0x24, 0x55, 0xAA, true)] // BIT Zero Page
        [InlineData(0x2C, 0x00, 0x00, true)] // BIT Absolute
        [InlineData(0x2C, 0xFF, 0xFF, false)] // BIT Absolute
        [InlineData(0x2C, 0xAA, 0x55, true)] // BIT Absolute
        [InlineData(0x2C, 0x55, 0xAA, true)] // BIT Absolute
        public void BIT_Zero_Set_When_Comparison_Is_Zero(byte operation, byte accumulatorValue, byte valueToTest, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0x00, new byte[] { 0xA9, accumulatorValue, operation, 0x06, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }
        #endregion

        #region BMI - Branch if Negative Set
        [Theory]
        [InlineData(0, 1, 5)]
        [InlineData(0x80, 0x80, 4)]
        [InlineData(0, 0xFB, 0xFFFF)]
        [InlineData(0x7B, 0x80, 0xFFFF)]
        public void BMI_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0xA9, 0x80, 0x30, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region BNE - Branch On Result Not Zero

        [Theory]
        [InlineData(0, 1, 5)]
        [InlineData(0x80, 0x80, 4)]
        [InlineData(0, 0xFB, 0xFFFF)]
        [InlineData(0x7B, 0x80, 0xFFFF)]
        public void BNE_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0xA9, 0x01, 0xD0, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }

        #endregion

        #region BPL - Branch if Negative Clear
        [Theory]
        [InlineData(0, 1, 5)]
        [InlineData(0x80, 0x80, 4)]
        [InlineData(0, 0xFB, 0xFFFF)]
        [InlineData(0x7B, 0x80, 0xFFFF)]
        public void BPL_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0xA9, 0x79, 0x10, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region BRK - Simulate Interrupt Request (IRQ)

        [Fact]
        public void BRK_Program_Counter_Set_To_Address_At_Break_Vector_Address()
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0x00 }, 0x00);
            var processor = new Processor(memory);

            //Manually Write the Break Address
            processor.WriteMemoryValue(0xFFFE, 0xBC);
            processor.WriteMemoryValue(0xFFFF, 0xCD);

            processor.NextStep();

            Assert.Equal((Address)0xCDBC, processor.ProgramCounter);
        }

        [Fact]
        public void BRK_Program_Counter_Stack_Correct()
        {
            var memory = Memory.LoadProgram(0xABCD, new byte[] { 0x00 }, 0xABCD);
            var processor = new Processor(memory);

            var stackLocation = processor.StackPointer;
            processor.NextStep();

            Assert.Equal(0xAB, processor.ReadMemoryValue(stackLocation + 0x100));
            Assert.Equal(0xCF, processor.ReadMemoryValue(stackLocation + 0x100 - 1));
        }

        [Fact]
        public void BRK_Stack_Pointer_Correct()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0xABCD, new byte[] { 0x00 }, 0xABCD));


            var stackLocation = processor.StackPointer;
            processor.NextStep();

            Assert.Equal(stackLocation - 3, processor.StackPointer);
        }

        [Theory]
        [InlineData(0x038, 0x31)] //SEC Carry Flag Test
        [InlineData(0x0F8, 0x38)] //SED Decimal Flag Test
        [InlineData(0x078, 0x34)] //SEI Interrupt Flag Test
        public void BRK_Stack_Set_Flag_Operations_Correctly(byte operation, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x58, operation, 0x00 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedValue, processor.ReadMemoryValue(stackLocation + 0x100 - 2));
        }

        [Theory]
        [InlineData(0x01, 0x80, 0xB0)] //Negative
        [InlineData(0x01, 0x7F, 0xF0)] //Overflow + Negative
        [InlineData(0x00, 0x00, 0x32)] //Zero
        public void BRK_Stack_Non_Set_Flag_Operations_Correctly(byte accumulatorValue, byte memoryValue, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x58, 0xA9, accumulatorValue, 0x69, memoryValue, 0x00 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedValue, processor.ReadMemoryValue(stackLocation + 0x100 - 2));
        }


        #endregion

        #region BVC - Branch if Overflow Clear
        [Theory]
        [InlineData(0, 1, 3)]
        [InlineData(0x80, 0x80, 2)]
        [InlineData(0, 0xFD, 0xFFFF)]
        [InlineData(0x7D, 0x80, 0xFFFF)]
        public void BVC_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0x50, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region BVS - Branch if Overflow Set
        [Theory]
        [InlineData(0, 1, 7)]
        [InlineData(0x80, 0x80, 6)]
        [InlineData(0, 0xF9, 0xFFFF)]
        [InlineData(0x79, 0x80, 0xFFFF)]
        public void BVS_Program_Counter_Correct(int programCounterInitalValue, byte offset, int expectedValue)
        {
            var memory = Memory.LoadProgram(programCounterInitalValue, new byte[] { 0xA9, 0x01, 0x69, 0x7F, 0x70, offset }, programCounterInitalValue);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(expectedValue, processor.ProgramCounter);
        }
        #endregion

        #region CLC - Clear Carry Flag

        [Fact]
        public void CLC_Carry_Flag_Cleared_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x18 }, 0x00));

            processor.NextStep();

            Assert.False(processor.CarryFlag);
        }

        #endregion

        #region CLD - Clear Decimal Flag

        [Fact]
        public void CLD_Carry_Flag_Set_And_Cleared_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xF8, 0xD8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.DecimalFlag);
        }

        #endregion

        #region CLI - Clear Interrupt Flag

        [Fact]
        public void CLI_Interrup_Flag_Cleared_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x58 }, 0x00));

            processor.NextStep();

            Assert.False(processor.DisableInterruptFlag);
        }

        #endregion

        #region CLV - Clear Overflow Flag

        [Fact]
        public void CLV_Overflow_Flag_Cleared_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xB8 }, 0x00));

            processor.NextStep();

            Assert.False(processor.OverflowFlag);
        }

        #endregion

        #region CMP - Compare Memory With Accumulator

        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, false)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CMP_Zero_Flag_Set_When_Values_Match(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0xC9, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, true)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0x00, 0x01, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CMP_Carry_Flag_Set_When_Accumulator_Is_Greater_Than_Or_Equal(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0xC9, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xFE, 0xFF, true)]
        [InlineData(0x81, 0x1, true)]
        [InlineData(0x81, 0x2, false)]
        [InlineData(0x79, 0x1, false)]
        [InlineData(0x00, 0x1, true)]
        public void CMP_Negative_Flag_Set_When_Result_Is_Negative(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0xC9, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }

        #endregion

        #region CPX - Compare Memory With X Register
        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, false)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CPX_Zero_Flag_Set_When_Values_Match(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, xValue, 0xE0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, true)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0x00, 0x01, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CPX_Carry_Flag_Set_When_Accumulator_Is_Greater_Than_Or_Equal(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, xValue, 0xE0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xFE, 0xFF, true)]
        [InlineData(0x81, 0x1, true)]
        [InlineData(0x81, 0x2, false)]
        [InlineData(0x79, 0x1, false)]
        [InlineData(0x00, 0x1, true)]
        public void CPX_Negative_Flag_Set_When_Result_Is_Negative(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, xValue, 0xE0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region CPY - Compare Memory With X Register
        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, false)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CPY_Zero_Flag_Set_When_Values_Match(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, xValue, 0xC0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0x00, true)]
        [InlineData(0x00, 0xFF, false)]
        [InlineData(0x00, 0x01, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void CPY_Carry_Flag_Set_When_Accumulator_Is_Greater_Than_Or_Equal(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, xValue, 0xC0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xFE, 0xFF, true)]
        [InlineData(0x81, 0x1, true)]
        [InlineData(0x81, 0x2, false)]
        [InlineData(0x79, 0x1, false)]
        [InlineData(0x00, 0x1, true)]
        public void CPY_Negative_Flag_Set_When_Result_Is_Negative(byte xValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, xValue, 0xC0, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region DEC - Decrement Memory by One

        [Theory]
        [InlineData(0x00, 0xFF)]
        [InlineData(0xFF, 0xFE)]
        public void DEC_Memory_Has_Correct_Value(byte initalMemoryValue, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xC6, 0x03, 0x00, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.ReadMemoryValue(0x03));
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x01, true)]
        [InlineData(0x02, false)]
        public void DEC_Zero_Has_Correct_Value(byte initalMemoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xC6, 0x03, 0x00, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x80, false)]
        [InlineData(0x81, true)]
        [InlineData(0x00, true)]
        public void DEC_Negative_Has_Correct_Value(byte initalMemoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xC6, 0x03, 0x00, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region DEX - Decrement X by One

        [Theory]
        [InlineData(0x00, 0xFF)]
        [InlineData(0xFF, 0xFE)]
        public void DEX_XRegister_Has_Correct_Value(byte initialXRegisterValue, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegisterValue, 0xCA }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.XRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x01, true)]
        [InlineData(0x02, false)]
        public void DEX_Zero_Has_Correct_Value(byte initialXRegisterValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegisterValue, 0xCA }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x80, false)]
        [InlineData(0x81, true)]
        [InlineData(0x00, true)]
        public void DEX_Negative_Has_Correct_Value(byte initialXRegisterValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegisterValue, 0xCA }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region DEY - Decrement Y by One

        [Theory]
        [InlineData(0x00, 0xFF)]
        [InlineData(0xFF, 0xFE)]
        public void DEY_YRegister_Has_Correct_Value(byte initialYRegisterValue, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegisterValue, 0x88 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.YRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x01, true)]
        [InlineData(0x02, false)]
        public void DEY_Zero_Has_Correct_Value(byte initialYRegisterValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegisterValue, 0x88 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x80, false)]
        [InlineData(0x81, true)]
        [InlineData(0x00, true)]
        public void DEY_Negative_Has_Correct_Value(byte initialYRegisterValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegisterValue, 0x88 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region EOR - Exclusive OR Compare Accumulator With Memory

        [Theory]
        [InlineData(0x00, 0x00, 0x00)]
        [InlineData(0xFF, 0x00, 0xFF)]
        [InlineData(0x00, 0xFF, 0xFF)]
        [InlineData(0x55, 0xAA, 0xFF)]
        [InlineData(0xFF, 0xFF, 0x00)]
        public void EOR_Accumulator_Correct(byte accumulatorValue, byte memoryValue, byte expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x49, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.Accumulator);
        }

        [Theory]
        [InlineData(0xFF, 0xFF, false)]
        [InlineData(0x80, 0x7F, true)]
        [InlineData(0x40, 0x3F, false)]
        [InlineData(0xFF, 0x7F, true)]
        public void EOR_Negative_Flag_Correct(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x49, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0xFF, 0xFF, true)]
        [InlineData(0x80, 0x7F, false)]
        public void EOR_Zero_Flag_Correct(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x49, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        #endregion

        #region INC - Increment Memory by One

        [Theory]
        [InlineData(0x00, 0x01)]
        [InlineData(0xFF, 0x00)]
        public void INC_Memory_Has_Correct_Value(byte initalMemoryValue, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xE6, 0x03, 0x00, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.ReadMemoryValue(0x03));
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0xFF, true)]
        [InlineData(0xFE, false)]
        public void INC_Zero_Has_Correct_Value(byte initalMemoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xE6, 0x03, 0x00, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x78, false)]
        [InlineData(0x80, true)]
        [InlineData(0x00, false)]
        public void INC_Negative_Has_Correct_Value(byte initalMemoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xE6, 0x02, initalMemoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region INX - Increment X by One

        [Theory]
        [InlineData(0x00, 0x01)]
        [InlineData(0xFF, 0x00)]
        public void INX_XRegister_Has_Correct_Value(byte initialXRegister, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegister, 0xE8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.XRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0xFF, true)]
        [InlineData(0xFE, false)]
        public void INX_Zero_Has_Correct_Value(byte initialXRegister, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegister, 0xE8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x78, false)]
        [InlineData(0x80, true)]
        [InlineData(0x00, false)]
        public void INX_Negative_Has_Correct_Value(byte initialXRegister, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, initialXRegister, 0xE8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region INY - Increment Y by One

        [Theory]
        [InlineData(0x00, 0x01)]
        [InlineData(0xFF, 0x00)]
        public void INY_YRegisgter_Has_Correct_Value(byte initialYRegister, byte expectedMemoryValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegister, 0xC8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedMemoryValue, processor.YRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0xFF, true)]
        [InlineData(0xFE, false)]
        public void INY_Zero_Has_Correct_Value(byte initialYRegister, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegister, 0xC8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x78, false)]
        [InlineData(0x80, true)]
        [InlineData(0x00, false)]
        public void INY_Negative_Has_Correct_Value(byte initialYRegister, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, initialYRegister, 0xC8 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }

        #endregion

        #region JMP - Jump to New Location

        [Fact]
        public void JMP_Program_Counter_Set_Correctly_After_Jump()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x4C, 0x08, 0x00 }, 0x00));

            processor.NextStep();

            Assert.Equal<Address>(0x08, processor.ProgramCounter);
        }

        [Fact]
        public void JMP_Program_Counter_Set_Correctly_After_Indirect_Jump()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x6C, 0x03, 0x00, 0x08, 0x00 }, 0x00));

            processor.NextStep();

            Assert.Equal<Address>(0x08, processor.ProgramCounter);
        }

        [Fact]
        public void JMP_Indirect_Wraps_Correct_If_MSB_IS_FF()
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0x6C, 0xFF, 0x01, 0x08, 0x00 }, 0x00);
            var processor = new Processor(memory);
            processor.WriteMemoryValue(0x01FE, 0x6C);


            processor.WriteMemoryValue(0x01FF, 0x03);
            processor.WriteMemoryValue(0x0100, 0x02);
            processor.NextStep();

            Assert.Equal<Address>(0x0203, processor.ProgramCounter);
        }

        #endregion

        #region JSR - Jump to SubRoutine

        [Fact]
        public void JSR_Stack_Loads_Correct_Value()
        {
            var memory = Memory.LoadProgram(0xBBAA, new byte[] { 0x20, 0xCC, 0xCC }, 0xBBAA);
            var processor = new Processor(memory);


            var stackLocation = processor.StackPointer;
            processor.NextStep();


            Assert.Equal(0xBB, processor.ReadMemoryValue(stackLocation + 0x100));
            Assert.Equal(0xAC, processor.ReadMemoryValue(stackLocation + 0x100 - 1));
        }

        [Fact]
        public void JSR_Program_Counter_Correct()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0xBBAA, new byte[] { 0x20, 0xCC, 0xCC }, 0xBBAA));

            processor.NextStep();


            Assert.Equal<Address>(0xCCCC, processor.ProgramCounter);
        }


        [Fact]
        public void JSR_Stack_Pointer_Correct()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0xBBAA, new byte[] { 0x20, 0xCC, 0xCC }, 0xBBAA));


            var stackLocation = processor.StackPointer;
            processor.NextStep();


            Assert.Equal(stackLocation - 2, processor.StackPointer);
        }
        #endregion

        #region LDA - Load Accumulator with Memory

        [Fact]
        public void LDA_Accumulator_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, 0x03 }, 0x00));

            processor.NextStep();

            Assert.Equal(0x03, processor.Accumulator);
        }

        [Theory]
        [InlineData(0x0, true)]
        [InlineData(0x3, false)]
        public void LDA_Zero_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x79, false)]
        [InlineData(0x80, true)]
        [InlineData(0xFF, true)]
        public void LDA_Negative_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        #endregion

        #region LDX - Load X with Memory

        [Fact]
        public void LDX_XRegister_Value_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, 0x03 }, 0x00));

            processor.NextStep();

            Assert.Equal(0x03, processor.XRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x79, false)]
        [InlineData(0x80, true)]
        [InlineData(0xFF, true)]
        public void LDX_Negative_Flag_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0x0, true)]
        [InlineData(0x3, false)]
        public void LDX_Zero_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }

        #endregion

        #region LDY - Load Y with Memory

        [Fact]
        public void STY_YRegister_Value_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, 0x03 }, 0x00));

            processor.NextStep();

            Assert.Equal(0x03, processor.YRegister);
        }

        [Theory]
        [InlineData(0x00, false)]
        [InlineData(0x79, false)]
        [InlineData(0x80, true)]
        [InlineData(0xFF, true)]
        public void LDY_Negative_Flag_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0x0, true)]
        [InlineData(0x3, false)]
        public void LDY_Zero_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, valueToLoad }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }

        #endregion

        #region LSR - Logical Shift Right

        [Theory]
        [InlineData(0xFF, false, false)]
        [InlineData(0xFE, false, false)]
        [InlineData(0xFF, true, false)]
        [InlineData(0x00, true, false)]
        public void LSR_Negative_Set_Correctly(byte accumulatorValue, bool carryBitSet, bool expectedValue)
        {
            var carryOperation = carryBitSet ? 0x38 : 0x18;
            var memory = Memory.LoadProgram(0, new byte[] { (byte)carryOperation, 0xA9, accumulatorValue, 0x4A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0x1, true)]
        [InlineData(0x2, false)]
        public void LSR_Zero_Set_Correctly(byte accumulatorValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x4A }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x1, true)]
        [InlineData(0x2, false)]
        public void LSR_Carry_Flag_Set_Correctly(byte accumulatorValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x4A }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0x4A, 0xFF, 0x7F, 0x00)] // LSR Accumulator
        [InlineData(0x4A, 0xFD, 0x7E, 0x00)] // LSR Accumulator
        [InlineData(0x46, 0xFF, 0x7F, 0x01)] // LSR Zero Page
        [InlineData(0x56, 0xFF, 0x7F, 0x01)] // LSR Zero Page X
        [InlineData(0x4E, 0xFF, 0x7F, 0x01)] // LSR Absolute
        [InlineData(0x5E, 0xFF, 0x7F, 0x01)] // LSR Absolute X
        public void LSR_Correct_Value_Stored(byte operation, byte valueToShift, byte expectedValue, byte expectedLocation)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToShift, operation, expectedLocation }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue,
                operation == 0x4A ?
                processor.Accumulator :
                processor.ReadMemoryValue(expectedLocation));
        }
        #endregion

        #region ORA - Bitwise OR Compare Memory with Accumulator

        [Theory]
        [InlineData(0x00, 0x00, 0x00)]
        [InlineData(0xFF, 0xFF, 0xFF)]
        [InlineData(0x55, 0xAA, 0xFF)]
        [InlineData(0xAA, 0x55, 0xFF)]
        public void ORA_Accumulator_Correct(byte accumulatorValue, byte memoryValue, byte expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x09, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.Accumulator);
        }

        [Theory]
        [InlineData(0x00, 0x00, true)]
        [InlineData(0xFF, 0xFF, false)]
        [InlineData(0x00, 0x01, false)]
        public void ORA_Zero_Flag_Correct(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x09, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x7F, 0x80, true)]
        [InlineData(0x79, 0x00, false)]
        [InlineData(0xFF, 0xFF, true)]
        public void ORA_Negative_Flag_Correct(byte accumulatorValue, byte memoryValue, bool expectedResult)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x09, memoryValue }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region PHA - Push Accumulator Onto Stack

        [Fact]
        public void PHA_Stack_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, 0x03, 0x48 }, 0x00));


            var stackLocation = processor.StackPointer;

            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(0x03, processor.ReadMemoryValue(stackLocation + 0x100));
        }

        [Fact]
        public void PHA_Stack_Pointer_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, 0x03, 0x48 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();
            processor.NextStep();

            //A Push will decrement the Pointer by 1
            Assert.Equal(stackLocation - 1, processor.StackPointer);
        }

        [Fact]
        public void PHA_Stack_Pointer_Has_Correct_Value_When_Wrapping()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x9A, 0x48 }, 0x00));

            processor.NextStep();
            processor.NextStep();


            Assert.Equal(0xFF, processor.StackPointer);
        }
        #endregion

        #region PHP - Push Flags Onto Stack 
        [Theory]
        [InlineData(0x038, 0x31)] //SEC Carry Flag Test
        [InlineData(0x0F8, 0x38)] //SED Decimal Flag Test
        [InlineData(0x078, 0x34)] //SEI Interrupt Flag Test
        public void PHP_Stack_Set_Flag_Operations_Correctly(byte operation, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x58, operation, 0x08 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedValue, processor.ReadMemoryValue(stackLocation + 0x100));
        }

        [Theory]
        [InlineData(0x01, 0x80, 0xB0)] //Negative
        [InlineData(0x01, 0x7F, 0xF0)] //Overflow + Negative
        [InlineData(0x00, 0x00, 0x32)] //Zero
        public void PHP_Stack_Non_Set_Flag_Operations_Correctly(byte accumulatorValue, byte memoryValue, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x58, 0xA9, accumulatorValue, 0x69, memoryValue, 0x08 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedValue, processor.ReadMemoryValue(stackLocation + 0x100));
        }

        [Fact]
        public void PHP_Stack_Pointer_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x08 }, 0x00));


            var stackLocation = processor.StackPointer;
            processor.NextStep();

            //A Push will decrement the Pointer by 1
            Assert.Equal(stackLocation - 1, processor.StackPointer);
        }

        #endregion

        #region PLA - Pull From Stack to Accumulator

        [Fact]
        public void PLA_Accumulator_Has_Correct_Value()
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x03, 0x48, 0xA9, 0x00, 0x68 }, 0x00);
            var processor = new Processor(memory);


            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(0x03, processor.Accumulator);
        }

        [Theory]
        [InlineData(0x00, true)]
        [InlineData(0x01, false)]
        [InlineData(0xFF, false)]
        public void PLA_Zero_Flag_Has_Correct_Value(byte valueToLoad, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToLoad, 0x48, 0x68 }, 0x00);
            var processor = new Processor(memory);


            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x7F, false)]
        [InlineData(0x80, true)]
        [InlineData(0xFF, true)]
        public void PLA_Negative_Flag_Has_Correct_Value(byte valueToLoad, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToLoad, 0x48, 0x68 }, 0x00);
            var processor = new Processor(memory);


            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.Equal(expectedResult, processor.NegativeFlag);
        }
        #endregion

        #region PLP - Pull From Stack to Flags

        [Fact]
        public void PLP_Carry_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x01, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.CarryFlag);
        }

        [Fact]
        public void PLP_Zero_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x02, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.ZeroFlag);
        }

        [Fact]
        public void PLP_Decimal_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x08, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.DecimalFlag);
        }

        [Fact]
        public void PLP_Interrupt_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x04, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.DisableInterruptFlag);
        }

        [Fact]
        public void PLP_Overflow_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x40, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.OverflowFlag);
        }

        [Fact]
        public void PLP_Negative_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Read From stack
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x80, 0x48, 0x28 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.NegativeFlag);
        }

        #endregion

        #region ROL - Rotate Left

        [Theory]
        [InlineData(0x40, true)]
        [InlineData(0x3F, false)]
        [InlineData(0x80, false)]
        public void ROL_Negative_Set_Correctly(byte accumulatorValue, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x2A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ROL_Zero_Set_Correctly(bool carryFlagSet, bool expectedResult)
        {
            var carryOperation = carryFlagSet ? 0x38 : 0x18;
            var memory = Memory.LoadProgram(0, new byte[] { (byte)carryOperation, 0x2A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x80, true)]
        [InlineData(0x7F, false)]
        public void ROL_Carry_Flag_Set_Correctly(byte accumulatorValue, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x2A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0x2A, 0x55, 0xAA, 0x00)] // ROL Accumulator
        [InlineData(0x26, 0x55, 0xAA, 0x01)] // ROL Zero Page
        [InlineData(0x36, 0x55, 0xAA, 0x01)] // ROL Zero Page X
        [InlineData(0x2E, 0x55, 0xAA, 0x01)] // ROL Absolute
        [InlineData(0x3E, 0x55, 0xAA, 0x01)] // ROL Absolute X
        public void ROL_Correct_Value_Stored(byte operation, byte valueToRotate, byte expectedValue, byte expectedLocation)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToRotate, operation, expectedLocation }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(
                expectedValue,
                operation == 0x2A ?
                    processor.Accumulator :
                    processor.ReadMemoryValue(expectedLocation));
        }

        #endregion

        #region ROR - Rotate Left

        [Theory]
        [InlineData(0xFF, false, false)]
        [InlineData(0xFE, false, false)]
        [InlineData(0xFF, true, true)]
        [InlineData(0x00, true, true)]
        public void ROR_Negative_Set_Correctly(byte accumulatorValue, bool carryBitSet, bool expectedValue)
        {
            var carryOperation = carryBitSet ? 0x38 : 0x18;
            var memory = Memory.LoadProgram(0, new byte[] { (byte)carryOperation, 0xA9, accumulatorValue, 0x6A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [Theory]
        [InlineData(0x00, false, true)]
        [InlineData(0x00, true, false)]
        [InlineData(0x01, false, true)]
        [InlineData(0x01, true, false)]
        public void ROR_Zero_Set_Correctly(byte accumulatorValue, bool carryBitSet, bool expectedResult)
        {
            var carryOperation = carryBitSet ? 0x38 : 0x18;
            var memory = Memory.LoadProgram(0, new byte[] { (byte)carryOperation, 0xA9, accumulatorValue, 0x6A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        [Theory]
        [InlineData(0x01, true)]
        [InlineData(0x02, false)]
        public void ROR_Carry_Flag_Set_Correctly(byte accumulatorValue, bool expectedResult)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorValue, 0x6A }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.CarryFlag);
        }

        [Theory]
        [InlineData(0x6A, 0xAA, 0x55, 0x00)] // ROR Accumulator
        [InlineData(0x66, 0xAA, 0x55, 0x01)] // ROR Zero Page
        [InlineData(0x76, 0xAA, 0x55, 0x01)] // ROR Zero Page X
        [InlineData(0x6E, 0xAA, 0x55, 0x01)] // ROR Absolute
        [InlineData(0x7E, 0xAA, 0x55, 0x01)] // ROR Absolute X
        public void ROR_Correct_Value_Stored(byte operation, byte valueToRotate, byte expectedValue, byte expectedLocation)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, valueToRotate, operation, expectedLocation }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(
                expectedValue,
                operation == 0x6A ?
                    processor.Accumulator :
                    processor.ReadMemoryValue(expectedLocation));
        }

        #endregion

        #region RTI - Return from Interrupt

        [Fact]
        public void RTI_Program_Counter_Correct()
        {
            var memory = Memory.LoadProgram(0xABCD, new byte[] { 0x00 }, 0xABCD);
            var processor = new Processor(memory);

            //The Reset Vector Points to 0x0000 by default, so load the RTI instruction there.
            processor.WriteMemoryValue(0x00, 0x40);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(0xABCF, processor.ProgramCounter);
        }

        [Fact]
        public void RTI_Carry_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x01, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.CarryFlag);
        }

        [Fact]
        public void RTI_Zero_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x02, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.ZeroFlag);
        }

        [Fact]
        public void RTI_Decimal_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x08, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.DecimalFlag);
        }

        [Fact]
        public void RTI_Interrupt_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x04, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.DisableInterruptFlag);
        }

        [Fact]
        public void RTI_Overflow_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x40, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.OverflowFlag);
        }

        [Fact]
        public void RTI_Negative_Flag_Set_Correctly()
        {
            //Load Accumulator and Transfer to Stack, Clear Accumulator, and Return from Interrupt
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, 0x80, 0x48, 0x40 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            //Accounting for the Offest in memory
            Assert.True(processor.NegativeFlag);
        }
        #endregion

        #region RTS - Return from SubRoutine

        [Fact]
        public void RTS_Program_Counter_Has_Correct_Value()
        {
            var memory = Memory.LoadProgram(0x00, new byte[] { 0x20, 0x04, 0x00, 0x00, 0x60 }, 0x00);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal<Address>(0x03, processor.ProgramCounter);
        }

        [Fact]
        public void RTS_Stack_Pointer_Has_Correct_Value()
        {
            var memory = Memory.LoadProgram(0x00, new byte[] { 0x20, 0x04, 0x00, 0x00, 0x60 }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0xFF, processor.StackPointer);

            processor.NextStep();

            Assert.Equal(0xFD, processor.StackPointer);

            processor.NextStep();

            Assert.Equal(0xFF, processor.StackPointer);
        }

        #endregion

        #region SBC - Subtraction With Borrow

        [Theory]
        [InlineData(0x0, 0x0, false, 0xFF)]
        [InlineData(0x0, 0x0, true, 0x00)]
        [InlineData(0x50, 0xf0, false, 0x5F)]
        [InlineData(0x50, 0xB0, true, 0xA0)]
        [InlineData(0xff, 0xff, false, 0xff)]
        [InlineData(0xff, 0xff, true, 0x00)]
        [InlineData(0xff, 0x80, false, 0x7e)]
        [InlineData(0xff, 0x80, true, 0x7f)]
        [InlineData(0x80, 0xff, false, 0x80)]
        [InlineData(0x80, 0xff, true, 0x81)]
        public void SBC_Accumulator_Correct_When_Not_In_BDC_Mode(byte accumlatorIntialValue, byte amountToSubtract, bool CarryFlagSet, byte expectedValue)
        {
            var memory = CarryFlagSet ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (CarryFlagSet)
            {
                processor.NextStep();
            }

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0, 0x99, false, 0)]
        [InlineData(0, 0x99, true, 1)]
        [Theory]
        public void SBC_Accumulator_Correct_When_In_BDC_Mode(byte accumlatorIntialValue, byte amountToAdd,
                                                                       bool setCarryFlag, byte expectedValue)
        {
            var memory = setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xF8, 0xA9, accumlatorIntialValue, 0xE9, amountToAdd }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xF8, 0xA9, accumlatorIntialValue, 0xE9, amountToAdd }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0xFF, 1, false, false)]
        [InlineData(0xFF, 0, false, false)]
        [InlineData(0x80, 0, false, true)]
        [InlineData(0x80, 0, true, false)]
        [InlineData(0x81, 1, false, true)]
        [InlineData(0x81, 1, true, false)]
        [InlineData(0, 0x80, false, false)]
        [InlineData(0, 0x80, true, true)]
        [InlineData(1, 0x80, true, true)]
        [InlineData(1, 0x7F, false, false)]
        [Theory]
        public void SBC_Overflow_Correct_When_Not_In_BDC_Mode(byte accumlatorIntialValue, byte amountToSubtact, bool setCarryFlag,
                                                                     bool expectedValue)
        {
            var memory = setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xA9, accumlatorIntialValue, 0xE9, amountToSubtact }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0xE9, amountToSubtact }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.OverflowFlag);
        }

        [InlineData(99, 1, false, false)]
        [InlineData(99, 0, false, false)]
        //[InlineData(0, 1, false, true)]
        //[InlineData(1, 1, true, true)]
        //[InlineData(2, 1, true, false)]
        //[InlineData(1, 1, false, false)]
        [Theory]
        public void SBC_Overflow_Correct_When_In_BDC_Mode(byte accumlatorIntialValue, byte amountToSubtract, bool setCarryFlag,
                                                                     bool expectedValue)
        {
            var memory = setCarryFlag ?
                Memory.LoadProgram(0, new byte[] { 0x38, 0xF8, 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00) :
                Memory.LoadProgram(0, new byte[] { 0xF8, 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            if (setCarryFlag)
            {
                processor.NextStep();
            }

            processor.NextStep();
            processor.NextStep();
            Assert.Equal(accumlatorIntialValue, processor.Accumulator);

            processor.NextStep();
            Assert.Equal(expectedValue, processor.OverflowFlag);
        }

        [InlineData(0, 0, false)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, true)]
        [InlineData(2, 1, true)]
        [Theory]
        public void SBC_Carry_Correct(byte accumlatorIntialValue, byte amountToSubtract, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.CarryFlag);
        }

        [InlineData(0, 0, false)]
        [InlineData(0, 1, false)]
        [InlineData(1, 0, true)]
        [InlineData(1, 1, false)]
        [Theory]
        public void SBC_Zero_Correct(byte accumlatorIntialValue, byte amountToSubtract, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }

        [InlineData(0x80, 0x01, false)]
        [InlineData(0x81, 0x01, false)]
        [InlineData(0x00, 0x01, true)]
        [InlineData(0x01, 0x01, true)]
        [Theory]
        public void SBC_Negative_Correct(byte accumlatorIntialValue, byte amountToSubtract, bool expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumlatorIntialValue, 0xE9, amountToSubtract }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }
        #endregion

        #region SEC - Set Carry Flag

        [Fact]
        public void SEC_Carry_Flag_Set_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x38 }, 0x00));

            processor.NextStep();

            Assert.True(processor.CarryFlag);
        }

        #endregion

        #region SED - Set Decimal Mode

        [Fact]
        public void SED_Decimal_Mode_Set_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xF8 }, 0x00));

            processor.NextStep();

            Assert.True(processor.DecimalFlag);
        }

        #endregion

        #region SEI - Set Interrup Flag

        [Fact]
        public void SEI_Interrupt_Flag_Set_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0x78 }, 0x00));

            processor.NextStep();

            Assert.True(processor.DisableInterruptFlag);
        }

        #endregion

        #region STA - Store Accumulator In Memory

        [Fact]
        public void STA_Memory_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA9, 0x03, 0x85, 0x05 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(0x03, processor.ReadMemoryValue(0x05));
        }

        #endregion

        #region STX - Set Memory To X

        [Fact]
        public void STX_Memory_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, 0x03, 0x86, 0x05 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(0x03, processor.ReadMemoryValue(0x05));
        }

        #endregion

        #region STY - Set Memory To Y

        [Fact]
        public void STY_Memory_Has_Correct_Value()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, 0x03, 0x84, 0x05 }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(0x03, processor.ReadMemoryValue(0x05));
        }

        #endregion

        #region TAX, TAY, TSX, TSY Tests

        [InlineData(0xAA, RegisterMode.Accumulator, RegisterMode.XRegister)]
        [InlineData(0xA8, RegisterMode.Accumulator, RegisterMode.YRegister)]
        [InlineData(0x8A, RegisterMode.XRegister, RegisterMode.Accumulator)]
        [InlineData(0x98, RegisterMode.YRegister, RegisterMode.Accumulator)]
        [Theory]
        public void Transfer_Correct_Value_Set(byte operation, RegisterMode transferFrom, RegisterMode transferTo)
        {
            byte loadOperation = transferFrom switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new[] { loadOperation, (byte)0x03, operation }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            switch (transferTo)
            {

                case RegisterMode.Accumulator:
                    Assert.Equal(0x03, processor.Accumulator);
                    break;
                case RegisterMode.XRegister:
                    Assert.Equal(0x03, processor.XRegister);
                    break;
                default:
                    Assert.Equal(0x03, processor.YRegister);
                    break;
            }
        }

        [InlineData(0xAA, 0x80, RegisterMode.Accumulator, true)]
        [InlineData(0xA8, 0x80, RegisterMode.Accumulator, true)]
        [InlineData(0x8A, 0x80, RegisterMode.XRegister, true)]
        [InlineData(0x98, 0x80, RegisterMode.YRegister, true)]
        [InlineData(0xAA, 0xFF, RegisterMode.Accumulator, true)]
        [InlineData(0xA8, 0xFF, RegisterMode.Accumulator, true)]
        [InlineData(0x8A, 0xFF, RegisterMode.XRegister, true)]
        [InlineData(0x98, 0xFF, RegisterMode.YRegister, true)]
        [InlineData(0xAA, 0x7F, RegisterMode.Accumulator, false)]
        [InlineData(0xA8, 0x7F, RegisterMode.Accumulator, false)]
        [InlineData(0x8A, 0x7F, RegisterMode.XRegister, false)]
        [InlineData(0x98, 0x7F, RegisterMode.YRegister, false)]
        [InlineData(0xAA, 0x00, RegisterMode.Accumulator, false)]
        [InlineData(0xA8, 0x00, RegisterMode.Accumulator, false)]
        [InlineData(0x8A, 0x00, RegisterMode.XRegister, false)]
        [InlineData(0x98, 0x00, RegisterMode.YRegister, false)]
        [Theory]
        public void Transfer_Negative_Value_Set(byte operation, byte value, RegisterMode transferFrom, bool expectedResult)
        {
            byte loadOperation = transferFrom switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new[] { loadOperation, value, operation }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.NegativeFlag);
        }

        [InlineData(0xAA, 0xFF, RegisterMode.Accumulator, false)]
        [InlineData(0xA8, 0xFF, RegisterMode.Accumulator, false)]
        [InlineData(0x8A, 0xFF, RegisterMode.XRegister, false)]
        [InlineData(0x98, 0xFF, RegisterMode.YRegister, false)]
        [InlineData(0xAA, 0x00, RegisterMode.Accumulator, true)]
        [InlineData(0xA8, 0x00, RegisterMode.Accumulator, true)]
        [InlineData(0x8A, 0x00, RegisterMode.XRegister, true)]
        [InlineData(0x98, 0x00, RegisterMode.YRegister, true)]
        [Theory]
        public void Transfer_Zero_Value_Set(byte operation, byte value, RegisterMode transferFrom, bool expectedResult)
        {
            byte loadOperation = transferFrom switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new[] { loadOperation, value, operation }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedResult, processor.ZeroFlag);
        }

        #endregion

        #region TSX - Transfer Stack Pointer to X Register

        [Fact]
        public void TSX_XRegister_Set_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xBA }, 0x00));


            var stackPointer = processor.StackPointer;
            processor.NextStep();

            Assert.Equal(stackPointer, processor.XRegister);
        }

        [InlineData(0x00, false)]
        [InlineData(0x7F, false)]
        [InlineData(0x80, true)]
        [InlineData(0xFF, true)]
        [Theory]
        public void TSX_Negative_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, valueToLoad, 0x9A, 0xBA }, 0x00));

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.NegativeFlag);
        }

        [InlineData(0x00, true)]
        [InlineData(0x01, false)]
        [InlineData(0xFF, false)]
        [Theory]
        public void TSX_Zero_Set_Correctly(byte valueToLoad, bool expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, valueToLoad, 0x9A, 0xBA }, 0x00));

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.ZeroFlag);
        }
        #endregion

        #region TXS - Transfer X Register to Stack Pointer

        [Fact]
        public void TXS_Stack_Pointer_Set_Correctly()
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA2, 0xAA, 0x9A }, 0x00));

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(0xAA, processor.StackPointer);
        }
        #endregion

        #region Accumulator Address Tests
        [InlineData(0x69, 0x01, 0x01, 0x02)] // ADC
        [InlineData(0x29, 0x03, 0x03, 0x03)] // AND
        [InlineData(0xA9, 0x04, 0x03, 0x03)] // LDA
        [InlineData(0x49, 0x55, 0xAA, 0xFF)] // EOR
        [InlineData(0x09, 0x55, 0xAA, 0xFF)] // ORA
        [InlineData(0xE9, 0x03, 0x01, 0x01)] // SBC
        [Theory]
        public void Immediate_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorInitialValue, operation, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x65, 0x01, 0x01, 0x02)] // ADC
        [InlineData(0x25, 0x03, 0x03, 0x03)] // AND
        [InlineData(0xA5, 0x04, 0x03, 0x03)] // LDA
        [InlineData(0x45, 0x55, 0xAA, 0xFF)] // EOR
        [InlineData(0x05, 0x55, 0xAA, 0xFF)] // ORA
        [InlineData(0xE5, 0x03, 0x01, 0x01)] // SBC
        [Theory]
        public void ZeroPage_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorInitialValue, operation, 0x05, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x75, 0x00, 0x03, 0x03)] // ADC
        [InlineData(0x35, 0x03, 0x03, 0x03)] // AND
        [InlineData(0xB5, 0x04, 0x03, 0x03)] // LDA
        [InlineData(0x55, 0x55, 0xAA, 0xFF)] // EOR
        [InlineData(0x15, 0x55, 0xAA, 0xFF)] // ORA
        [InlineData(0xF5, 0x03, 0x01, 0x01)] // SBC
        [Theory]
        public void ZeroPageX_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, byte expectedValue)
        {
            //Just remember that my value's for the STX and ADC were added to the end of the byte array. In a real program this would be invalid, as an opcode would be next and 0x03 would be somewhere else
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorInitialValue, 0xA2, 0x01, operation, 0x06, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x6D, 0x00, 0x03, 0x03)] // ADC
        [InlineData(0x2D, 0x03, 0x03, 0x03)] // AND
        [InlineData(0xAD, 0x04, 0x03, 0x03)] // LDA
        [InlineData(0x4D, 0x55, 0xAA, 0xFF)] // EOR
        [InlineData(0x0D, 0x55, 0xAA, 0xFF)] // ORA
        [InlineData(0xED, 0x03, 0x01, 0x01)] // SBC
        [Theory]
        public void Absolute_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0, new byte[] { 0xA9, accumulatorInitialValue, operation, 0x06, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x7D, 0x01, 0x01, false, 0x02)] // ADC
        [InlineData(0x3D, 0x03, 0x03, false, 0x03)] // AND
        [InlineData(0xBD, 0x04, 0x03, false, 0x03)] // LDA
        [InlineData(0x5D, 0x55, 0xAA, false, 0xFF)]  // EOR
        [InlineData(0x1D, 0x55, 0xAA, false, 0xFF)] // ORA
        [InlineData(0xFD, 0x03, 0x01, false, 0x01)] // SBC
        [InlineData(0x7D, 0x01, 0x01, true, 0x02)] // ADC
        [InlineData(0x3D, 0x03, 0x03, true, 0x03)] // AND
        [InlineData(0xBD, 0x04, 0x03, true, 0x03)] // LDA
        [InlineData(0x5D, 0x55, 0xAA, true, 0xFF)]  // EOR
        [InlineData(0x1D, 0x55, 0xAA, true, 0xFF)] // ORA
        [InlineData(0xFD, 0x03, 0x01, true, 0x01)] // SBC
        [Theory]
        public void AbsoluteX_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, bool addressWraps, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0, addressWraps
                                      ? new byte[] { 0xA9, accumulatorInitialValue, 0xA2, 0x09, operation, 0xff, 0xff, 0x00, valueToTest }
                                      : new byte[] { 0xA9, accumulatorInitialValue, 0xA2, 0x01, operation, 0x07, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);
            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x79, 0x01, 0x01, false, 0x02)] // ADC
        [InlineData(0x39, 0x03, 0x03, false, 0x03)] // AND
        [InlineData(0xB9, 0x04, 0x03, false, 0x03)] // LDA
        [InlineData(0x59, 0x55, 0xAA, false, 0xFF)]  // EOR
        [InlineData(0x19, 0x55, 0xAA, false, 0xFF)] // ORA
        [InlineData(0xF9, 0x03, 0x01, false, 0x01)] // SBC
        [InlineData(0x79, 0x01, 0x01, true, 0x02)] // ADC
        [InlineData(0x39, 0x03, 0x03, true, 0x03)] // AND
        [InlineData(0xB9, 0x04, 0x03, true, 0x03)] // LDA
        [InlineData(0x59, 0x55, 0xAA, true, 0xFF)]  // EOR
        [InlineData(0x19, 0x55, 0xAA, true, 0xFF)] // ORA
        [InlineData(0xF9, 0x03, 0x01, true, 0x01)] // SBC
        [Theory]
        public void AbsoluteY_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, bool addressWraps, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0, addressWraps
                                      ? new byte[] { 0xA9, accumulatorInitialValue, 0xA0, 0x09, operation, 0xff, 0xff, 0x00, valueToTest }
                                      : new byte[] { 0xA9, accumulatorInitialValue, 0xA0, 0x01, operation, 0x07, 0x00, 0x00, valueToTest }, 0x00);
            var processor = new Processor(memory);
            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x61, 0x01, 0x01, false, 0x02)] // ADC
        [InlineData(0x21, 0x03, 0x03, false, 0x03)] // AND
        [InlineData(0xA1, 0x04, 0x03, false, 0x03)] // LDA
        [InlineData(0x41, 0x55, 0xAA, false, 0xFF)]  // EOR
        [InlineData(0x01, 0x55, 0xAA, false, 0xFF)] // ORA
        [InlineData(0xE1, 0x03, 0x01, false, 0x01)] // SBC
        [InlineData(0x61, 0x01, 0x01, true, 0x02)] // ADC
        [InlineData(0x21, 0x03, 0x03, true, 0x03)] // AND
        [InlineData(0xA1, 0x04, 0x03, true, 0x03)] // LDA
        [InlineData(0x41, 0x55, 0xAA, true, 0xFF)]  // EOR
        [InlineData(0x01, 0x55, 0xAA, true, 0xFF)] // ORA
        [InlineData(0xE1, 0x03, 0x01, true, 0x01)] // SBC
        [Theory]
        public void Indexed_Indirect_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, bool addressWraps, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0,
                                  addressWraps
                                      ? new byte[] { 0xA9, accumulatorInitialValue, 0xA6, 0x06, operation, 0xff, 0x08, 0x9, 0x00, valueToTest }
                                      : new byte[] { 0xA9, accumulatorInitialValue, 0xA6, 0x06, operation, 0x01, 0x06, 0x9, 0x00, valueToTest },
                                  0x00);

            var processor = new Processor(memory);
            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }

        [InlineData(0x71, 0x01, 0x01, false, 0x02)] // ADC
        [InlineData(0x31, 0x03, 0x03, false, 0x03)] // AND
        [InlineData(0xB1, 0x04, 0x03, false, 0x03)] // LDA
        [InlineData(0x51, 0x55, 0xAA, false, 0xFF)]  // EOR
        [InlineData(0x11, 0x55, 0xAA, false, 0xFF)] // ORA
        [InlineData(0xF1, 0x03, 0x01, false, 0x01)] // SBC
        [InlineData(0x71, 0x01, 0x01, true, 0x02)] // ADC
        [InlineData(0x31, 0x03, 0x03, true, 0x03)] // AND
        [InlineData(0xB1, 0x04, 0x03, true, 0x03)] // LDA
        [InlineData(0x51, 0x55, 0xAA, true, 0xFF)]  // EOR
        [InlineData(0x11, 0x55, 0xAA, true, 0xFF)] // ORA
        [InlineData(0xF1, 0x03, 0x01, true, 0x01)] // SBC
        [Theory]
        public void Indirect_Indexed_Mode_Accumulator_Has_Correct_Result(byte operation, byte accumulatorInitialValue, byte valueToTest, bool addressWraps, byte expectedValue)
        {
            var memory = Memory.LoadProgram(0,
                                  addressWraps
                                      ? new byte[] { 0xA9, accumulatorInitialValue, 0xA0, 0x0A, operation, 0x07, 0x00, 0xFF, 0xFF, valueToTest }
                                      : new byte[] { 0xA9, accumulatorInitialValue, 0xA0, 0x01, operation, 0x07, 0x00, 0x08, 0x00, valueToTest },
                                  0x00);

            var processor = new Processor(memory);
            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.Equal(expectedValue, processor.Accumulator);
        }
        #endregion

        #region Index Address Tests
        [InlineData(0xA6, 0x03, true)] // LDX Zero Page
        [InlineData(0xB6, 0x03, true)] // LDX Zero Page Y
        [InlineData(0xA4, 0x03, false)] // LDY Zero Page
        [InlineData(0xB4, 0x03, false)] // LDY Zero Page X
        [Theory]
        public void ZeroPage_Mode_Index_Has_Correct_Result(byte operation, byte valueToLoad, bool testXRegister)
        {
            var memory = Memory.LoadProgram(0, new byte[] { operation, 0x03, 0x00, valueToLoad }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();

            Assert.Equal(valueToLoad, testXRegister ? processor.XRegister : processor.YRegister);
        }


        [InlineData(0xB6, 0x03, true)] // LDX Zero Page Y
        [InlineData(0xB4, 0x03, false)] // LDY Zero Page X
        [Theory]
        public void ZeroPage_Mode_Index_Has_Correct_Result_When_Wrapped(byte operation, byte valueToLoad, bool testXRegister)
        {
            var memory = Memory.LoadProgram(0, new byte[] { testXRegister ? (byte)0xA0 : (byte)0xA2, 0xFF, operation, 0x06, 0x00, valueToLoad }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();
            processor.NextStep();

            Assert.Equal(valueToLoad, testXRegister ? processor.XRegister : processor.YRegister);
        }

        [InlineData(0xAE, 0x03, true)] // LDX Absolute
        [InlineData(0xAC, 0x03, false)] // LDY Absolute
        [Theory]
        public void Absolute_Mode_Index_Has_Correct_Result(byte operation, byte valueToLoad, bool testXRegister)
        {
            var memory = Memory.LoadProgram(0, new byte[] { operation, 0x04, 0x00, 0x00, valueToLoad }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal(0x00, processor.Accumulator);

            processor.NextStep();

            Assert.Equal(valueToLoad, testXRegister ? processor.XRegister : processor.YRegister);
        }
        #endregion

        #region Compare Address Tests
        [InlineData(0xC9, 0xFF, 0x00, RegisterMode.Accumulator)] //CMP Immediate
        [InlineData(0xE0, 0xFF, 0x00, RegisterMode.XRegister)] //CPX Immediate
        [InlineData(0xC0, 0xFF, 0x00, RegisterMode.YRegister)] //CPY Immediate
        [Theory]
        public void Immediate_Mode_Compare_Operation_Has_Correct_Result(byte operation, byte accumulatorValue, byte memoryValue, RegisterMode mode)
        {
            byte loadOperation = mode switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new[] { loadOperation, accumulatorValue, operation, memoryValue }, 0x00);
            var processor = new Processor(memory);



            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.ZeroFlag);
            Assert.True(processor.NegativeFlag);
            Assert.True(processor.CarryFlag);
        }

        [InlineData(0xC5, 0xFF, 0x00, RegisterMode.Accumulator)] //CMP Zero Page
        [InlineData(0xD5, 0xFF, 0x00, RegisterMode.Accumulator)] //CMP Zero Page X
        [InlineData(0xE4, 0xFF, 0x00, RegisterMode.XRegister)] //CPX Zero Page
        [InlineData(0xC4, 0xFF, 0x00, RegisterMode.YRegister)] //CPY Zero Page
        [Theory]
        public void ZeroPage_Modes_Compare_Operation_Has_Correct_Result(byte operation, byte accumulatorValue, byte memoryValue, RegisterMode mode)
        {
            byte loadOperation = mode switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new byte[] { loadOperation, accumulatorValue, operation, 0x04, memoryValue }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.ZeroFlag);
            Assert.True(processor.NegativeFlag);
            Assert.True(processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xCD, 0xFF, 0x00, RegisterMode.Accumulator)] //CMP Absolute
        [InlineData(0xDD, 0xFF, 0x00, RegisterMode.Accumulator)] //CMP Absolute X
        [InlineData(0xEC, 0xFF, 0x00, RegisterMode.XRegister)] //CPX Absolute
        [InlineData(0xCC, 0xFF, 0x00, RegisterMode.YRegister)] //CPY Absolute
        public void Absolute_Modes_Compare_Operation_Has_Correct_Result(byte operation, byte accumulatorValue, byte memoryValue, RegisterMode mode)
        {
            byte loadOperation = mode switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new byte[] { loadOperation, accumulatorValue, operation, 0x05, 0x00, memoryValue }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.ZeroFlag);
            Assert.True(processor.NegativeFlag);
            Assert.True(processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xC1, 0xFF, 0x00, true)]
        [InlineData(0xC1, 0xFF, 0x00, false)]
        public void Indexed_Indirect_Mode_CMP_Operation_Has_Correct_Result(byte operation, byte accumulatorValue, byte memoryValue, bool addressWraps)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0,
                                      addressWraps
                                          ? new byte[] { 0xA9, accumulatorValue, 0xA6, 0x06, operation, 0xff, 0x08, 0x9, 0x00, memoryValue }
                                          : new byte[] { 0xA9, accumulatorValue, 0xA6, 0x06, operation, 0x01, 0x06, 0x9, 0x00, memoryValue },
                                      0x00));


            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.ZeroFlag);
            Assert.True(processor.NegativeFlag);
            Assert.True(processor.CarryFlag);
        }

        [Theory]
        [InlineData(0xD1, 0xFF, 0x00, true)]
        [InlineData(0xD1, 0xFF, 0x00, false)]
        public void Indirect_Indexed_Mode_CMP_Operation_Has_Correct_Result(byte operation, byte accumulatorValue, byte memoryValue, bool addressWraps)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0,
                              addressWraps
                                  ? new byte[] { 0xA9, accumulatorValue, 0x84, 0x06, operation, 0x07, 0x0A, 0xFF, 0xFF, memoryValue }
                                  : new byte[] { 0xA9, accumulatorValue, 0x84, 0x06, operation, 0x07, 0x01, 0x08, 0x00, memoryValue },
                              0x00));

            processor.NextStep();
            processor.NextStep();
            processor.NextStep();

            Assert.False(processor.ZeroFlag);
            Assert.True(processor.NegativeFlag);
            Assert.True(processor.CarryFlag);
        }
        #endregion

        #region Decrement/Increment Address Tests
        [Theory]
        [InlineData(0xC6, 0xFF, 0xFE)] //DEC Zero Page
        [InlineData(0xD6, 0xFF, 0xFE)] //DEC Zero Page X
        [InlineData(0xE6, 0xFF, 0x00)] //INC Zero Page
        [InlineData(0xF6, 0xFF, 0x00)] //INC Zero Page X
        public void Zero_Page_DEC_INC_Has_Correct_Result(byte operation, byte memoryValue, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { operation, 0x02, memoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.ReadMemoryValue(0x02));
        }

        [Theory]
        [InlineData(0xCE, 0xFF, 0xFE)] //DEC Zero Page
        [InlineData(0xDE, 0xFF, 0xFE)] //DEC Zero Page X
        [InlineData(0xEE, 0xFF, 0x00)] //INC Zero Page
        [InlineData(0xFE, 0xFF, 0x00)] //INC Zero Page X
        public void Absolute_DEC_INC_Has_Correct_Result(byte operation, byte memoryValue, byte expectedValue)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { operation, 0x03, 0x00, memoryValue }, 0x00));

            processor.NextStep();

            Assert.Equal(expectedValue, processor.ReadMemoryValue(0x03));
        }
        #endregion

        #region Store In Memory Address Tests

        [Theory]
        [InlineData(0x85, RegisterMode.Accumulator)] // STA Zero Page
        [InlineData(0x95, RegisterMode.Accumulator)] // STA Zero Page X
        [InlineData(0x86, RegisterMode.XRegister)] // STX Zero Page
        [InlineData(0x96, RegisterMode.XRegister)] // STX Zero Page Y
        [InlineData(0x84, RegisterMode.YRegister)] // STY Zero Page
        [InlineData(0x94, RegisterMode.YRegister)] // STY Zero Page X
        public void ZeroPage_Mode_Memory_Has_Correct_Result(byte operation, RegisterMode mode)
        {
            byte loadOperation = mode switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new byte[] { loadOperation, 0x04, operation, 0x00, 0x05 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(0x05, processor.ReadMemoryValue(0x04));
        }

        [Theory]
        [InlineData(0x8D, 0x03, RegisterMode.Accumulator)] // STA Absolute
        [InlineData(0x9D, 0x03, RegisterMode.Accumulator)] // STA Absolute X
        [InlineData(0x99, 0x03, RegisterMode.Accumulator)] // STA Absolute X
        [InlineData(0x8E, 0x03, RegisterMode.XRegister)] // STX Zero Page
        [InlineData(0x8C, 0x03, RegisterMode.YRegister)] // STY Zero Page
        public void Absolute_Mode_Memory_Has_Correct_Result(byte operation, byte valueToLoad, RegisterMode mode)
        {
            byte loadOperation = mode switch
            {
                RegisterMode.Accumulator => 0xA9,
                RegisterMode.XRegister => 0xA2,
                _ => 0xA0,
            };

            var memory = Memory.LoadProgram(0, new byte[] { loadOperation, valueToLoad, operation, 0x04 }, 0x00);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            Assert.Equal(valueToLoad, processor.ReadMemoryValue(0x04));
        }

        #endregion

        #region Cycle Tests
        [Theory]
        [InlineData(0x69, 2)] // ADC Immediate
        [InlineData(0x65, 3)] // ADC Zero Page
        [InlineData(0x75, 4)] // ADC Zero Page X
        [InlineData(0x6D, 4)] // ADC Absolute
        [InlineData(0x7D, 4)] // ADC Absolute X
        [InlineData(0x79, 4)] // ADC Absolute Y
        [InlineData(0x61, 6)] // ADC Indrect X
        [InlineData(0x71, 5)] // ADC Indirect Y
        [InlineData(0x29, 2)] // AND Immediate
        [InlineData(0x25, 3)] // AND Zero Page
        [InlineData(0x35, 4)] // AND Zero Page X
        [InlineData(0x2D, 4)] // AND Absolute
        [InlineData(0x3D, 4)] // AND Absolute X
        [InlineData(0x39, 4)] // AND Absolute Y
        [InlineData(0x21, 6)] // AND Indirect X
        [InlineData(0x31, 5)] // AND Indirect Y
        [InlineData(0x0A, 2)] // ASL Accumulator
        [InlineData(0x06, 5)] // ASL Zero Page
        [InlineData(0x16, 6)] // ASL Zero Page X
        [InlineData(0x0E, 6)] // ASL Absolute
        [InlineData(0x1E, 7)] // ASL Absolute X
        [InlineData(0x24, 3)] // BIT Zero Page
        [InlineData(0x2C, 4)] // BIT Absolute
        [InlineData(0x00, 7)] // BRK Implied
        [InlineData(0x18, 2)] // CLC Implied
        [InlineData(0xD8, 2)] // CLD Implied
        [InlineData(0x58, 2)] // CLI Implied
        [InlineData(0xB8, 2)] // CLV Implied
        [InlineData(0xC9, 2)] // CMP Immediate
        [InlineData(0xC5, 3)] // CMP ZeroPage
        [InlineData(0xD5, 4)] // CMP Zero Page X
        [InlineData(0xCD, 4)] // CMP Absolute
        [InlineData(0xDD, 4)] // CMP Absolute X
        [InlineData(0xD9, 4)] // CMP Absolute Y
        [InlineData(0xC1, 6)] // CMP Indirect X
        [InlineData(0xD1, 5)] // CMP Indirect Y
        [InlineData(0xE0, 2)] // CPX Immediate
        [InlineData(0xE4, 3)] // CPX ZeroPage
        [InlineData(0xEC, 4)] // CPX Absolute
        [InlineData(0xC0, 2)] // CPY Immediate
        [InlineData(0xC4, 3)] // CPY ZeroPage
        [InlineData(0xCC, 4)] // CPY Absolute
        [InlineData(0xC6, 5)] // DEC Zero Page
        [InlineData(0xD6, 6)] // DEC Zero Page X
        [InlineData(0xCE, 6)] // DEC Absolute
        [InlineData(0xDE, 7)] // DEC Absolute X
        [InlineData(0xCA, 2)] // DEX Implied
        [InlineData(0x88, 2)] // DEY Implied
        [InlineData(0x49, 2)] // EOR Immediate
        [InlineData(0x45, 3)] // EOR Zero Page
        [InlineData(0x55, 4)] // EOR Zero Page X
        [InlineData(0x4D, 4)] // EOR Absolute
        [InlineData(0x5D, 4)] // EOR Absolute X
        [InlineData(0x59, 4)] // EOR Absolute Y
        [InlineData(0x41, 6)] // EOR Indrect X
        [InlineData(0x51, 5)] // EOR Indirect Y
        [InlineData(0xE6, 5)] // INC Zero Page
        [InlineData(0xF6, 6)] // INC Zero Page X
        [InlineData(0xEE, 6)] // INC Absolute
        [InlineData(0xFE, 7)] // INC Absolute X
        [InlineData(0xE8, 2)] // INX Implied
        [InlineData(0xC8, 2)] // INY Implied
        [InlineData(0x4C, 3)] // JMP Absolute
        [InlineData(0x6C, 5)] // JMP Indirect
        [InlineData(0x20, 6)] // JSR Absolute
        [InlineData(0xA9, 2)] // LDA Immediate
        [InlineData(0xA5, 3)] // LDA Zero Page
        [InlineData(0xB5, 4)] // LDA Zero Page X
        [InlineData(0xAD, 4)] // LDA Absolute
        [InlineData(0xBD, 4)] // LDA Absolute X
        [InlineData(0xB9, 4)] // LDA Absolute Y
        [InlineData(0xA1, 6)] // LDA Indirect X
        [InlineData(0xB1, 5)] // LDA Indirect Y
        [InlineData(0xA2, 2)] // LDX Immediate
        [InlineData(0xA6, 3)] // LDX Zero Page
        [InlineData(0xB6, 4)] // LDX Zero Page Y
        [InlineData(0xAE, 4)] // LDX Absolute
        [InlineData(0xBE, 4)] // LDX Absolute Y
        [InlineData(0xA0, 2)] // LDY Immediate
        [InlineData(0xA4, 3)] // LDY Zero Page
        [InlineData(0xB4, 4)] // LDY Zero Page Y
        [InlineData(0xAC, 4)] // LDY Absolute
        [InlineData(0xBC, 4)] // LDY Absolute Y
        [InlineData(0x4A, 2)] // LSR Accumulator
        [InlineData(0x46, 5)] // LSR Zero Page
        [InlineData(0x56, 6)] // LSR Zero Page X
        [InlineData(0x4E, 6)] // LSR Absolute
        [InlineData(0x5E, 7)] // LSR Absolute X
        [InlineData(0xEA, 2)] // NOP Implied
        [InlineData(0x09, 2)] // ORA Immediate
        [InlineData(0x05, 3)] // ORA Zero Page
        [InlineData(0x15, 4)] // ORA Zero Page X
        [InlineData(0x0D, 4)] // ORA Absolute
        [InlineData(0x1D, 4)] // ORA Absolute X
        [InlineData(0x19, 4)] // ORA Absolute Y
        [InlineData(0x01, 6)] // ORA Indirect X
        [InlineData(0x11, 5)] // ORA Indirect Y
        [InlineData(0x48, 3)] // PHA Implied
        [InlineData(0x08, 3)] // PHP Implied
        [InlineData(0x68, 4)] // PLA Implied
        [InlineData(0x28, 4)] // PLP Implied
        [InlineData(0x2A, 2)] // ROL Accumulator
        [InlineData(0x26, 5)] // ROL Zero Page
        [InlineData(0x36, 6)] // ROL Zero Page X
        [InlineData(0x2E, 6)] // ROL Absolute
        [InlineData(0x3E, 7)] // ROL Absolute X
        [InlineData(0x6A, 2)] // ROR Accumulator
        [InlineData(0x66, 5)] // ROR Zero Page
        [InlineData(0x76, 6)] // ROR Zero Page X
        [InlineData(0x6E, 6)] // ROR Absolute
        [InlineData(0x7E, 7)] // ROR Absolute X
        [InlineData(0x40, 6)] // RTI Implied
        [InlineData(0x60, 6)] // RTS Implied
        [InlineData(0xE9, 2)] // SBC Immediate
        [InlineData(0xE5, 3)] // SBC Zero Page
        [InlineData(0xF5, 4)] // SBC Zero Page X
        [InlineData(0xED, 4)] // SBC Absolute
        [InlineData(0xFD, 4)] // SBC Absolute X
        [InlineData(0xF9, 4)] // SBC Absolute Y
        [InlineData(0xE1, 6)] // SBC Indrect X
        [InlineData(0xF1, 5)] // SBC Indirect Y
        [InlineData(0x38, 2)] // SEC Implied
        [InlineData(0xF8, 2)] // SED Implied
        [InlineData(0x78, 2)] // SEI Implied
        [InlineData(0x85, 3)] // STA ZeroPage
        [InlineData(0x95, 4)] // STA Zero Page X
        [InlineData(0x8D, 4)] // STA Absolute
        [InlineData(0x9D, 5)] // STA Absolute X
        [InlineData(0x99, 5)] // STA Absolute Y
        [InlineData(0x81, 6)] // STA Indirect X
        [InlineData(0x91, 6)] // STA Indirect Y
        [InlineData(0x86, 3)] // STX Zero Page
        [InlineData(0x96, 4)] // STX Zero Page Y
        [InlineData(0x8E, 4)] // STX Absolute
        [InlineData(0x84, 3)] // STY Zero Page
        [InlineData(0x94, 4)] // STY Zero Page X
        [InlineData(0x8C, 4)] // STY Absolute
        [InlineData(0xAA, 2)] // TAX Implied
        [InlineData(0xA8, 2)] // TAY Implied
        [InlineData(0xBA, 2)] // TSX Implied
        [InlineData(0x8A, 2)] // TXA Implied
        [InlineData(0x9A, 2)] // TXS Implied
        [InlineData(0x98, 2)] // TYA Implied
        public void NumberOfCyclesRemaining_Correct_After_Operations_That_Do_Not_Wrap(byte operation, int numberOfCyclesUsed)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { operation, 0x00 }, 0x00));


            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x07D, true, 5)] // ADC Absolute X
        [InlineData(0x079, false, 5)] // ADC Absolute Y
        [InlineData(0x03D, true, 5)] // AND Absolute X
        [InlineData(0x039, false, 5)] // AND Absolute Y
        [InlineData(0x1E, true, 7)] // ASL Absolute X
        [InlineData(0xDD, true, 5)] // CMP Absolute X
        [InlineData(0xD9, false, 5)] // CMP Absolute Y
        [InlineData(0xDE, true, 7)] // DEC Absolute X
        [InlineData(0x05D, true, 5)] // EOR Absolute X
        [InlineData(0x059, false, 5)] // EOR Absolute Y
        [InlineData(0xFE, true, 7)] // INC Absolute X
        [InlineData(0xBD, true, 5)] // LDA Absolute X
        [InlineData(0xB9, false, 5)] // LDA Absolute Y
        [InlineData(0xBE, false, 5)] // LDX Absolute Y
        [InlineData(0xBC, true, 5)] // LDY Absolute X
        [InlineData(0x5E, true, 7)] // LSR Absolute X
        [InlineData(0x1D, true, 5)] // ORA Absolute X
        [InlineData(0x19, false, 5)] // ORA Absolute Y
        [InlineData(0x3E, true, 7)] // ROL Absolute X
        [InlineData(0x7E, true, 7)] // ROR Absolute X
        [InlineData(0xFD, true, 5)] // SBC Absolute X
        [InlineData(0xF9, false, 5)] // SBC Absolute Y
        [InlineData(0x9D, true, 5)] // STA Absolute X
        [InlineData(0x99, true, 5)] // STA Absolute Y
        public void NumberOfCyclesRemaining_Correct_When_In_AbsoluteX_Or_AbsoluteY_And_Wrap(byte operation, bool isAbsoluteX, int numberOfCyclesUsed)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, isAbsoluteX
                                      ? new byte[] { 0xA6, 0x06, operation, 0xff, 0xff, 0x00, 0x03 }
                                      : new byte[] { 0xA4, 0x06, operation, 0xff, 0xff, 0x00, 0x03 }, 0x00));

            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x071, 6)] // ADC Indirect Y
        [InlineData(0x031, 6)] // AND Indirect Y
        [InlineData(0xB1, 6)] // LDA Indirect Y
        [InlineData(0xD1, 6)] // CMP Indirect Y
        [InlineData(0x51, 6)] // EOR Indirect Y
        [InlineData(0x11, 6)] // ORA Indirect Y
        [InlineData(0xF1, 6)] // SBC Indirect Y
        [InlineData(0x91, 6)] // STA Indirect Y
        public void NumberOfCyclesRemaining_Correct_When_In_IndirectIndexed_And_Wrap(byte operation, int numberOfCyclesUsed)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, new byte[] { 0xA0, 0x04, operation, 0x05, 0x08, 0xFF, 0xFF, 0x03 }, 0x00));

            processor.NextStep();
            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x90, 2, true)] //BCC
        [InlineData(0x90, 3, false)] //BCC
        [InlineData(0xB0, 2, false)] //BCS
        [InlineData(0xB0, 3, true)]  //BCS
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Carry(byte operation, int numberOfCyclesUsed, bool isCarrySet)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, isCarrySet
                                         ? new byte[] { 0x38, operation, 0x00 }
                                         : new byte[] { 0x18, operation, 0x00 }, 0x00));
            processor.NextStep();


            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x90, 4, false, true)]  //BCC
        [InlineData(0x90, 4, false, false)] //BCC
        [InlineData(0xB0, 4, true, true)]  //BCC
        [InlineData(0xB0, 4, true, false)] //BCC
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Carry_And_Wrap(byte operation, int numberOfCyclesUsed, bool isCarrySet, bool wrapRight)
        {
            var carryOperation = isCarrySet ? 0x38 : 0x18;
            var initialAddress = wrapRight ? 0xFFF0 : 0x00;
            var amountToMove = wrapRight ? 0x0F : 0x84;

            var memory = Memory.LoadProgram(initialAddress, new byte[] { (byte)carryOperation, operation, (byte)amountToMove, 0x00 }, initialAddress);
            var processor = new Processor(memory);


            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0xF0, 3, true)]  //BEQ
        [InlineData(0xF0, 2, false)] //BEQ
        [InlineData(0xD0, 3, false)]  //BNE
        [InlineData(0xD0, 2, true)] //BNE
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Zero(byte operation, int numberOfCyclesUsed, bool isZeroSet)
        {
            var memory = new Memory();
            var processor = new Processor(Memory.LoadProgram(0, isZeroSet
                ? new byte[] { 0xA9, 0x00, operation, 0x00 }
                : new byte[] { 0xA9, 0x01, operation, 0x00 }, 0x00));

            processor.NextStep();


            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0xF0, 4, true, true)]  //BEQ
        [InlineData(0xF0, 4, true, false)] //BEQ
        [InlineData(0xD0, 4, false, true)]  //BNE
        [InlineData(0xD0, 4, false, false)] //BNE
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Zero_And_Wrap(byte operation, int numberOfCyclesUsed, bool isZeroSet, bool wrapRight)
        {
            var newAccumulatorValue = isZeroSet ? 0x00 : 0x01;
            var initialAddress = wrapRight ? 0xFFF0 : 0x00;
            var amountToMove = wrapRight ? 0x0D : 0x84;

            var memory = Memory.LoadProgram(initialAddress, new byte[] { 0xA9, (byte)newAccumulatorValue, operation, (byte)amountToMove, 0x00 }, initialAddress);
            var processor = new Processor(memory);


            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x30, 3, true)]  //BEQ
        [InlineData(0x30, 2, false)] //BEQ
        [InlineData(0x10, 3, false)]  //BNE
        [InlineData(0x10, 2, true)] //BNE
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Negative(byte operation, int numberOfCyclesUsed, bool isNegativeSet)
        {
            var memory = Memory.LoadProgram(0, isNegativeSet
                ? new byte[] { 0xA9, 0x80, operation, 0x00 }
                : new byte[] { 0xA9, 0x79, operation, 0x00 }, 0x00);
            var processor = new Processor(memory);

            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x30, 4, true, true)]  //BEQ
        [InlineData(0x30, 4, true, false)] //BEQ
        [InlineData(0x10, 4, false, true)]  //BNE
        [InlineData(0x10, 4, false, false)] //BNE
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Negative_And_Wrap(byte operation, int numberOfCyclesUsed, bool isNegativeSet, bool wrapRight)
        {
            var newAccumulatorValue = isNegativeSet ? 0x80 : 0x79;
            var initialAddress = wrapRight ? 0xFFF0 : 0x00;
            var amountToMove = wrapRight ? 0x0D : 0x84;

            var memory = Memory.LoadProgram(initialAddress, new byte[] { 0xA9, (byte)newAccumulatorValue, operation, (byte)amountToMove, 0x00 }, initialAddress);
            var processor = new Processor(memory);


            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x50, 3, false)]  //BVC
        [InlineData(0x50, 2, true)] //BVC
        [InlineData(0x70, 3, true)]  //BVS
        [InlineData(0x70, 2, false)] //BVS
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Overflow(byte operation, int numberOfCyclesUsed, bool isOverflowSet)
        {
            var memory = Memory.LoadProgram(0, isOverflowSet
                ? new byte[] { 0xA9, 0x01, 0x69, 0x7F, operation, 0x00 }
                : new byte[] { 0xA9, 0x01, 0x69, 0x01, operation, 0x00 }, 0x00);
            var processor = new Processor(memory);

            processor.NextStep();
            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }

        [Theory]
        [InlineData(0x50, 4, false, true)]  //BVC
        [InlineData(0x50, 4, false, false)] //BVC
        [InlineData(0x70, 4, true, true)]  //BVS
        [InlineData(0x70, 4, true, false)] //BVS
        public void NumberOfCyclesRemaining_Correct_When_Relative_And_Branch_On_Overflow_And_Wrap(byte operation, int numberOfCyclesUsed, bool isOverflowSet, bool wrapRight)
        {
            var newAccumulatorValue = isOverflowSet ? 0x7F : 0x00;
            var initialAddress = wrapRight ? 0xFFF0 : 0x00;
            var amountToMove = wrapRight ? 0x0B : 0x86;

            var memory = Memory.LoadProgram(initialAddress, new byte[] { 0xA9, (byte)newAccumulatorValue, 0x69, 0x01, operation, (byte)amountToMove, 0x00 }, initialAddress);
            var processor = new Processor(memory);


            processor.NextStep();
            processor.NextStep();

            //Get the number of cycles after the register has been loaded, so we can isolate the operation under test
            var startingNumberOfCycles = processor.GetCycleCount();
            processor.NextStep();

            Assert.Equal(startingNumberOfCycles + numberOfCyclesUsed, processor.GetCycleCount());
        }
        #endregion

        #region Program Counter Tests
        [Theory]
        [InlineData(0x69, 2)] // ADC Immediate
        [InlineData(0x65, 2)] // ADC ZeroPage
        [InlineData(0x75, 2)] // ADC Zero Page X
        [InlineData(0x6D, 3)] // ADC Absolute
        [InlineData(0x7D, 3)] // ADC Absolute X
        [InlineData(0x79, 3)] // ADC Absolute Y
        [InlineData(0x61, 2)] // ADC Indirect X
        [InlineData(0x71, 2)] // ADC Indirect Y
        [InlineData(0x29, 2)] // AND Immediate
        [InlineData(0x25, 2)] // AND Zero Page
        [InlineData(0x35, 2)] // AND Zero Page X
        [InlineData(0x2D, 3)] // AND Absolute
        [InlineData(0x3D, 3)] // AND Absolute X
        [InlineData(0x39, 3)] // AND Absolute Y
        [InlineData(0x21, 2)] // AND Indirect X
        [InlineData(0x31, 2)] // AND Indirect Y
        [InlineData(0x0A, 1)] // ASL Accumulator
        [InlineData(0x06, 2)] // ASL Zero Page
        [InlineData(0x16, 2)] // ASL Zero Page X
        [InlineData(0x0E, 3)] // ASL Absolute
        [InlineData(0x1E, 3)] // ASL Absolute X
        [InlineData(0x24, 2)] // BIT Zero Page
        [InlineData(0x2C, 3)] // BIT Absolute
        [InlineData(0x18, 1)] // CLC Implied
        [InlineData(0xD8, 1)] // CLD Implied
        [InlineData(0x58, 1)] // CLI Implied
        [InlineData(0xB8, 1)] // CLV Implied
        [InlineData(0xC9, 2)] // CMP Immediate
        [InlineData(0xC5, 2)] // CMP ZeroPage
        [InlineData(0xD5, 2)] // CMP Zero Page X
        [InlineData(0xCD, 3)] // CMP Absolute
        [InlineData(0xDD, 3)] // CMP Absolute X
        [InlineData(0xD9, 3)] // CMP Absolute Y
        [InlineData(0xC1, 2)] // CMP Indirect X
        [InlineData(0xD1, 2)] // CMP Indirect Y
        [InlineData(0xE0, 2)] // CPX Immediate
        [InlineData(0xE4, 2)] // CPX ZeroPage
        [InlineData(0xEC, 3)] // CPX Absolute
        [InlineData(0xC0, 2)] // CPY Immediate
        [InlineData(0xC4, 2)] // CPY ZeroPage
        [InlineData(0xCC, 3)] // CPY Absolute
        [InlineData(0xC6, 2)] // DEC Zero Page
        [InlineData(0xD6, 2)] // DEC Zero Page X
        [InlineData(0xCE, 3)] // DEC Absolute
        [InlineData(0xDE, 3)] // DEC Absolute X
        [InlineData(0xCA, 1)] // DEX Implied
        [InlineData(0x88, 1)] // DEY Implied
        [InlineData(0x49, 2)] // EOR Immediate
        [InlineData(0x45, 2)] // EOR ZeroPage
        [InlineData(0x55, 2)] // EOR Zero Page X
        [InlineData(0x4D, 3)] // EOR Absolute
        [InlineData(0x5D, 3)] // EOR Absolute X
        [InlineData(0x59, 3)] // EOR Absolute Y
        [InlineData(0x41, 2)] // EOR Indirect X
        [InlineData(0x51, 2)] // EOR Indirect Y
        [InlineData(0xE6, 2)] // INC Zero Page
        [InlineData(0xF6, 2)] // INC Zero Page X
        [InlineData(0xEE, 3)] // INC Absolute
        [InlineData(0xFE, 3)] // INC Absolute X
        [InlineData(0xE8, 1)] // INX Implied
        [InlineData(0xC8, 1)] // INY Implied
        [InlineData(0xA9, 2)] // LDA Immediate
        [InlineData(0xA5, 2)] // LDA Zero Page
        [InlineData(0xB5, 2)] // LDA Zero Page X
        [InlineData(0xAD, 3)] // LDA Absolute
        [InlineData(0xBD, 3)] // LDA Absolute X
        [InlineData(0xB9, 3)] // LDA Absolute Y
        [InlineData(0xA1, 2)] // LDA Indirect X
        [InlineData(0xB1, 2)] // LDA Indirect Y
        [InlineData(0xA2, 2)] // LDX Immediate
        [InlineData(0xA6, 2)] // LDX Zero Page
        [InlineData(0xB6, 2)] // LDX Zero Page Y
        [InlineData(0xAE, 3)] // LDX Absolute
        [InlineData(0xBE, 3)] // LDX Absolute Y
        [InlineData(0xA0, 2)] // LDY Immediate
        [InlineData(0xA4, 2)] // LDY Zero Page
        [InlineData(0xB4, 2)] // LDY Zero Page Y
        [InlineData(0xAC, 3)] // LDY Absolute
        [InlineData(0xBC, 3)] // LDY Absolute Y
        [InlineData(0x4A, 1)] // LSR Accumulator
        [InlineData(0x46, 2)] // LSR Zero Page
        [InlineData(0x56, 2)] // LSR Zero Page X
        [InlineData(0x4E, 3)] // LSR Absolute
        [InlineData(0x5E, 3)] // LSR Absolute X
        [InlineData(0xEA, 1)] // NOP Implied
        [InlineData(0x09, 2)] // ORA Immediate
        [InlineData(0x05, 2)] // ORA Zero Page
        [InlineData(0x15, 2)] // ORA Zero Page X
        [InlineData(0x0D, 3)] // ORA Absolute
        [InlineData(0x1D, 3)] // ORA Absolute X
        [InlineData(0x19, 3)] // ORA Absolute Y
        [InlineData(0x01, 2)] // ORA Indirect X
        [InlineData(0x11, 2)] // ORA Indirect Y
        [InlineData(0x48, 1)] // PHA Implied
        [InlineData(0x08, 1)] // PHP Implied
        [InlineData(0x68, 1)] // PLA Implied
        [InlineData(0x28, 1)] // PLP Implied
        [InlineData(0x2A, 1)] // ROL Accumulator
        [InlineData(0x26, 2)] // ROL Zero Page
        [InlineData(0x36, 2)] // ROL Zero Page X
        [InlineData(0x2E, 3)] // ROL Absolute
        [InlineData(0x3E, 3)] // ROL Absolute X
        [InlineData(0x6A, 1)] // ROR Accumulator
        [InlineData(0x66, 2)] // ROR Zero Page
        [InlineData(0x76, 2)] // ROR Zero Page X
        [InlineData(0x6E, 3)] // ROR Absolute
        [InlineData(0x7E, 3)] // ROR Absolute X
        [InlineData(0xE9, 2)] // SBC Immediate
        [InlineData(0xE5, 2)] // SBC Zero Page
        [InlineData(0xF5, 2)] // SBC Zero Page X
        [InlineData(0xED, 3)] // SBC Absolute
        [InlineData(0xFD, 3)] // SBC Absolute X
        [InlineData(0xF9, 3)] // SBC Absolute Y
        [InlineData(0xE1, 2)] // SBC Indrect X
        [InlineData(0xF1, 2)] // SBC Indirect Y
        [InlineData(0x38, 1)] // SEC Implied
        [InlineData(0xF8, 1)] // SED Implied
        [InlineData(0x78, 1)] // SEI Implied
        [InlineData(0x85, 2)] // STA ZeroPage
        [InlineData(0x95, 2)] // STA Zero Page X
        [InlineData(0x8D, 3)] // STA Absolute
        [InlineData(0x9D, 3)] // STA Absolute X
        [InlineData(0x99, 3)] // STA Absolute Y
        [InlineData(0x81, 2)] // STA Indirect X
        [InlineData(0x91, 2)] // STA Indirect Y
        [InlineData(0x86, 2)] // STX Zero Page
        [InlineData(0x96, 2)] // STX Zero Page Y
        [InlineData(0x8E, 3)] // STX Absolute
        [InlineData(0x84, 2)] // STY Zero Page
        [InlineData(0x94, 2)] // STY Zero Page X
        [InlineData(0x8C, 3)] // STY Absolute
        [InlineData(0xAA, 1)] // TAX Implied
        [InlineData(0xA8, 1)] // TAY Implied
        [InlineData(0xBA, 1)] // TSX Implied
        [InlineData(0x8A, 1)] // TXA Implied
        [InlineData(0x9A, 1)] // TXS Implied
        [InlineData(0x98, 1)] // TYA Implied
        public void Program_Counter_Correct(byte operation, int expectedProgramCounter)
        {
            var memory = Memory.LoadProgram(0, new byte[] { operation, 0x0 }, 0x00);
            var processor = new Processor(memory);

            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();

            Assert.Equal<Address>(expectedProgramCounter, processor.ProgramCounter);
        }

        [Theory]
        [InlineData(0x90, true, 2)]  //BCC
        [InlineData(0xB0, false, 2)] //BCS
        public void Branch_On_Carry_Program_Counter_Correct_When_NoBranch_Occurs(byte operation, bool carrySet, byte expectedOutput)
        {
            var memory = Memory.LoadProgram(0,
                                  carrySet
                                          ? new byte[] { 0x38, operation, 0x48 }
                                      : new byte[] { 0x18, operation, 0x48 }, 0x00);

            var processor = new Processor(memory);
            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            var currentProgramCounter = processor.ProgramCounter;

            processor.NextStep();
            Assert.Equal<Address>(currentProgramCounter + expectedOutput, processor.ProgramCounter);

        }

        [Theory]
        [InlineData(0xF0, false, 2)]  //BEQ
        [InlineData(0xD0, true, 2)]  //BNE
        public void Branch_On_Zero_Program_Counter_Correct_When_NoBranch_Occurs(byte operation, bool zeroSet, byte expectedOutput)
        {
            var memory = Memory.LoadProgram(0,
                                  zeroSet
                                          ? new byte[] { 0xA9, 0x00, operation }
                                      : new byte[] { 0xA9, 0x01, operation }, 0x00);

            var processor = new Processor(memory);
            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            var currentProgramCounter = processor.ProgramCounter;

            processor.NextStep();
            Assert.Equal<Address>(currentProgramCounter + expectedOutput, processor.ProgramCounter);

        }

        [Theory]
        [InlineData(0x30, false, 2)]  //BMI
        [InlineData(0x10, true, 2)]  //BPL
        public void Branch_On_Negative_Program_Counter_Correct_When_NoBranch_Occurs(byte operation, bool negativeSet, byte expectedOutput)
        {
            var memory = Memory.LoadProgram(0,
                                  negativeSet
                                          ? new byte[] { 0xA9, 0x80, operation }
                                      : new byte[] { 0xA9, 0x79, operation }, 0x00);

            var processor = new Processor(memory);
            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            var currentProgramCounter = processor.ProgramCounter;

            processor.NextStep();
            Assert.Equal<Address>(currentProgramCounter + expectedOutput, processor.ProgramCounter);

        }

        [Theory]
        [InlineData(0x50, true, 2)]  //BVC
        [InlineData(0x70, false, 2)]  //BVS
        public void Branch_On_Overflow_Program_Counter_Correct_When_NoBranch_Occurs(byte operation, bool overflowSet, byte expectedOutput)
        {
            var memory = Memory.LoadProgram(0, overflowSet
                ? new byte[] { 0xA9, 0x01, 0x69, 0x7F, operation, 0x00 }
                : new byte[] { 0xA9, 0x01, 0x69, 0x01, operation, 0x00 }, 0x00);

            var processor = new Processor(memory);
            Assert.Equal<Address>(0, processor.ProgramCounter);

            processor.NextStep();
            processor.NextStep();
            var currentProgramCounter = processor.ProgramCounter;

            processor.NextStep();
            Assert.Equal<Address>(currentProgramCounter + expectedOutput, processor.ProgramCounter);
        }

        [Fact]
        public void Program_Counter_Wraps_Correctly()
        {
            var memory = Memory.LoadProgram(0xFFFF, new byte[] { 0x38 }, 0xFFFF);
            var processor = new Processor(memory);

            processor.NextStep();

            Assert.Equal<Address>(0, processor.ProgramCounter);
        }
        #endregion
    }
}