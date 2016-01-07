using System;
using System.Collections.Generic;
using FlatBuffers.Tests.TestTypes;

namespace FlatBuffers.Tests
{
    /// <summary>
    /// The Test Oracle provides a simple way of wrapping the code generated by flatc.exe
    /// It's used to verify that flatbuffers written by the serializer can be read by existing code
    /// and that existing buffers can be deserialized
    /// </summary>
    public class SerializationTestOracle
    {
        public TestStruct1 ReadTestStruct1(byte[] buffer)
        {
            var test = new SerializationTests.TestStruct1();
            var bb = new ByteBuffer(buffer);
            test.__init(0, bb);
            var result = new TestStruct1()
            {
                IntProp = test.IntProp,
                ByteProp = test.ByteProp,
                ShortProp = test.ShortProp
            };
            return result;
        }

        public TestStruct2 ReadTestStruct2(byte[] buffer)
        {
            var test = new SerializationTests.TestStruct2();
            var bb = new ByteBuffer(buffer);
            test.__init(0, bb);
            var result = new TestStruct2()
            {
                IntProp = test.IntProp,
                TestStructProp =  new TestStruct1()
                {
                    IntProp = test.TestStruct1Prop.IntProp,
                    ByteProp = test.TestStruct1Prop.ByteProp,
                    ShortProp = test.TestStruct1Prop.ShortProp
                }
            };
            return result;
        }

        public TestTable1 ReadTestTable1(byte[] buffer)
        {
            var test = SerializationTests.TestTable1.GetRootAsTestTable1(new ByteBuffer(buffer));
            var result = new TestTable1()
            {
                IntProp = test.IntProp,
                ByteProp = test.ByteProp,
                ShortProp = test.ShortProp
            };
            return result;
        }

        public TestTableWithDefaults ReadTestTableWithDefaults(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithDefaults.GetRootAsTestTableWithDefaults(new ByteBuffer(buffer));
            var result = new TestTableWithDefaults()
            {
                IntProp = test.IntProp,
                ByteProp = test.ByteProp,
                ShortProp = test.ShortProp
            };
            return result;
        }
        
        public TestTableWithUserOrdering ReadTestTable1WithUserOrdering(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithUserOrdering.GetRootAsTestTableWithUserOrdering(new ByteBuffer(buffer));
            var result = new TestTableWithUserOrdering()
            {
                IntProp = test.IntProp,
                ByteProp = test.ByteProp,
                ShortProp = test.ShortProp
            };
            return result;
        }

        public TestTableWithOriginalOrdering ReadTestTableWithOriginalOrdering(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithOriginalOrdering.GetRootAsTestTableWithOriginalOrdering(new ByteBuffer(buffer));
            var result = new TestTableWithOriginalOrdering()
            {
                IntProp = test.IntProp,
                ByteProp = test.ByteProp,
                ShortProp = test.ShortProp
            };
            return result;
        }

        public TestTableWithTable ReadTestTableWithTable(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithTable.GetRootAsTestTableWithTable(new ByteBuffer(buffer));
            var result = new TestTableWithTable()
            {
                IntProp = test.IntProp,
            };

            if (test.TableProp != null)
            {
                result.TableProp = new TestTable1()
                {
                    IntProp = test.TableProp.IntProp,
                    ByteProp = test.TableProp.ByteProp,
                    ShortProp = test.TableProp.ShortProp,
                };
            }

            return result;
        }
        

        public TestTable2 ReadTestTable2(byte[] buffer)
        {
            var test = SerializationTests.TestTable2.GetRootAsTestTable2(new ByteBuffer(buffer));
            var result = new TestTable2()
            {
                StringProp = test.StringProp
            };
            return result;
        }

        public TestTable3 ReadTestTable3(byte[] buffer)
        {
            var test = SerializationTests.TestTable3.GetRootAsTestTable3(new ByteBuffer(buffer));
            var result = new TestTable3()
            {
                BoolProp = test.BoolProp,
                LongProp = test.LongProp,
                DoubleProp = test.DoubleProp,
                EnumProp = (TestEnum)test.EnumProp,
                FloatProp = test.FloatProp,
                SByteProp = test.SByteProp,
                ULongProp = test.ULongProp,
                UShortProp = test.UShortProp
            };
            return result;
        }

        public TestTableWithArray ReadTestTableWithArray(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithArray.GetRootAsTestTableWithArray(new ByteBuffer(buffer));
            var intArray = new int[test.IntArrayLength];
            for (var i = 0; i < intArray.Length; ++i)
            {
                intArray[i] = test.GetIntArray(intArray.Length - i - 1);
            }
            var intListLength = test.IntListLength;
            var intList = new List<int>(intListLength);
            for (var i = 0; i < intListLength; ++i)
            {
                intList.Add(test.GetIntList(intListLength - i - 1));
            }
            return new TestTableWithArray()
            {
                IntArray = intArray,
                IntList = intList
            };
        }

        public TestTableWithStruct ReadTestTableWithStruct(byte[] buffer)
        {
            var test = SerializationTests.TestTableWithStruct.GetRootAsTestTableWithStruct(new ByteBuffer(buffer));

            var testStruct1 = test.StructProp;

            return new TestTableWithStruct()
            {
                IntProp = test.IntProp,
                StructProp =FromTestSchema(testStruct1)
            };
        }

        private TestStruct1 FromTestSchema(SerializationTests.TestStruct1 testStruct1)
        {
            return new TestStruct1()
            {
                IntProp = testStruct1.IntProp,
                ByteProp = testStruct1.ByteProp,
                ShortProp = testStruct1.ShortProp
            };
        }
        
        public TestTableWithArrayOfStructs ReadTestTableWithArrayOfStructs(byte[] buffer)
        {
            var test =
                SerializationTests.TestTableWithArrayOfStructs.GetRootAsTestTableWithArrayOfStructs(
                    new ByteBuffer(buffer));

            var array = new TestStruct1[test.StructArrayLength];
            for (var i = 0; i < array.Length; ++i)
            {
                array[i] = FromTestSchema(test.GetStructArray(array.Length - i - 1));
            }
            return new TestTableWithArrayOfStructs() {StructArray = array};
        }

        public byte[] GenerateTestStruct1(int intProp, byte byteProp, short shortProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestStruct1.CreateTestStruct1(fbb, intProp, byteProp, shortProp);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestStruct2(int intProp, TestStruct1 testStruct)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestStruct2.CreateTestStruct2(fbb, intProp, testStruct.IntProp, 
                testStruct.ByteProp, testStruct.ShortProp);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTable1(int intProp, byte byteProp, short shortProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestTable1.CreateTestTable1(fbb, intProp, byteProp, shortProp);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTableWithDefaults()
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestTableWithDefaults.CreateTestTableWithDefaults(fbb);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTableWithUserOrdering(int intProp, byte byteProp, short shortProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestTableWithUserOrdering.CreateTestTableWithUserOrdering(fbb, byteProp, shortProp, intProp);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTableWithOriginalOrdering(int intProp, byte byteProp, short shortProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestTableWithOriginalOrdering.CreateTestTableWithOriginalOrdering(fbb, intProp, byteProp, shortProp);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTable2(string stringProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var stringOffset = default(StringOffset);

            if (stringProp != null)
            {
                stringOffset = fbb.CreateString(stringProp);
            }
            var offset = SerializationTests.TestTable2.CreateTestTable2(fbb, stringOffset);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTable3(bool boolProp, long longProp, sbyte sbyteProp, ushort ushortProp, ulong ulongProp,
            TestEnum enumProp, float floatProp, double doubleProp)
        {
            var fbb = new FlatBufferBuilder(8);
            var offset = SerializationTests.TestTable3.CreateTestTable3(fbb, boolProp, longProp, sbyteProp, ushortProp, ulongProp,
                (SerializationTests.TestEnum)enumProp, floatProp, doubleProp);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTableWithStruct(TestStruct1 testStructProp, int intProp)
        {
            var fbb = new FlatBufferBuilder(8);

            SerializationTests.TestTableWithStruct.StartTestTableWithStruct(fbb);
            SerializationTests.TestTableWithStruct.AddIntProp(fbb, intProp);
            var structOffset = SerializationTests.TestStruct1.CreateTestStruct1(fbb, testStructProp.IntProp,
                testStructProp.ByteProp, testStructProp.ShortProp);
            SerializationTests.TestTableWithStruct.AddStructProp(fbb, structOffset);
            var offset = SerializationTests.TestTableWithStruct.EndTestTableWithStruct(fbb);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        public byte[] GenerateTestTableWithTable(TestTable1 testTableProp, int intProp)
        {
            var fbb = new FlatBufferBuilder(8);

            var tableOffset = SerializationTests.TestTable1.CreateTestTable1(fbb, testTableProp.IntProp,
                testTableProp.ByteProp, testTableProp.ShortProp);

            SerializationTests.TestTableWithTable.StartTestTableWithTable(fbb);
            SerializationTests.TestTableWithTable.AddIntProp(fbb, intProp);
            
            SerializationTests.TestTableWithTable.AddTableProp(fbb, tableOffset);
            var offset = SerializationTests.TestTableWithTable.EndTestTableWithTable(fbb);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }

        private static byte[] GetBytes(FlatBufferBuilder fbb)
        {
            var data = new byte[fbb.Offset];
            Buffer.BlockCopy(fbb.DataBuffer.Data, fbb.DataBuffer.Length - fbb.Offset, data, 0, fbb.Offset);
            return data;
        }


        public byte[] GenerateTestTableWithArray(int[] intArray, List<int> intList)
        {
            var fbb = new FlatBufferBuilder(8);

            var intArrayOffset = SerializationTests.TestTableWithArray.CreateIntArrayVector(fbb, intArray);
            var intListOffset = SerializationTests.TestTableWithArray.CreateIntListVector(fbb, intList.ToArray());

            SerializationTests.TestTableWithArray.StartTestTableWithArray(fbb);
            SerializationTests.TestTableWithArray.AddIntArray(fbb, intArrayOffset);
            SerializationTests.TestTableWithArray.AddIntList(fbb, intListOffset);
            var offset = SerializationTests.TestTableWithArray.EndTestTableWithArray(fbb);
            fbb.Finish(offset.Value);
            return GetBytes(fbb);
        }
    }
}