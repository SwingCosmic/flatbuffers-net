﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FlatBuffers
{
    internal class SerializationContext
    {
        private readonly TypeModelRegistry _typeModelRegistry;
        private readonly object _rootObject;
        private readonly TypeModel _rootTypeModel;
        private readonly FlatBufferBuilder _builder;
        private readonly Dictionary<object, int> _objectOffsets = new Dictionary<object, int>();

        public SerializationContext(TypeModelRegistry typeModelRegistry, object rootObject, FlatBufferBuilder builder)
        {
            _typeModelRegistry = typeModelRegistry;
            _rootObject = rootObject;
            _rootTypeModel = _typeModelRegistry.GetTypeModel(rootObject.GetType());
            _builder = builder;
        }

        /// <summary>
        /// Serializes a value as inline
        /// </summary>
        private int SerializeInlineValue(object obj, TypeModel typeModel)
        {
            switch (typeModel.BaseType)
            {
                case BaseType.Bool:
                {
                    _builder.AddBool((bool)obj);
                    break;
                }
                case BaseType.Char:
                {
                    _builder.AddSbyte((sbyte)obj);
                    break;
                }
                case BaseType.UChar:
                {
                    _builder.AddByte((byte)obj);
                    break;
                }
                case BaseType.Short:
                {
                    _builder.AddShort((short)obj);
                    break;
                }
                case BaseType.UShort:
                {
                    _builder.AddUshort((ushort)obj);
                    break;
                }
                case BaseType.Int:
                {
                    _builder.AddInt((int)obj);
                    break;
                }
                case BaseType.UInt:
                {
                    _builder.AddUint((uint)obj);
                    break;
                }
                case BaseType.Long:
                {
                    _builder.AddLong((long)obj);
                    break;
                }
                case BaseType.ULong:
                {
                    _builder.AddUlong((ulong)obj);
                    break;
                }
                case BaseType.Float:
                {
                    _builder.AddFloat((float)obj);
                    break;
                }
                case BaseType.Double:
                {
                    _builder.AddDouble((double)obj);
                    break;
                }
                case BaseType.Struct:
                {
                    return SerializeStruct(obj, typeModel);
                }
                default:
                {
                    throw new InvalidOperationException();
                }
            }
            return _builder.Offset;
        }

        private int SerializePropertyValue(object obj, FieldTypeDefinition field)
        {
            var typeModel = field.TypeModel;

            if (field.DefaultValueProvider.IsDefaultValue(obj))
            {
                return _builder.Offset;
            }

            switch (typeModel.BaseType)
            {
                case BaseType.Bool:
                {
                    _builder.AddBool(field.Index, (bool)obj, false);
                    break;
                }
                case BaseType.Char:
                {
                    _builder.AddSbyte(field.Index, (sbyte)obj, 0);
                    break;
                }
                case BaseType.UChar:
                {
                    _builder.AddByte(field.Index, (byte)obj, 0);
                    break;
                }
                case BaseType.Short:
                {
                    _builder.AddShort(field.Index, (short)obj, 0);
                    break;
                }
                case BaseType.UShort:
                {
                    _builder.AddUshort(field.Index, (ushort)obj, 0);
                    break;
                }
                case BaseType.Int:
                {
                    _builder.AddInt(field.Index, (int)obj, 0);
                    break;
                }
                case BaseType.UInt:
                {
                    _builder.AddUint(field.Index, (uint)obj, 0);
                    break;
                }
                case BaseType.Long:
                {
                    _builder.AddLong(field.Index, (long)obj, 0);
                    break;
                }
                case BaseType.ULong:
                {
                    _builder.AddUlong(field.Index, (ulong)obj, 0);
                    break;
                }
                case BaseType.Float:
                {
                    _builder.AddFloat(field.Index, (float)obj, 0);
                    break;
                }
                case BaseType.Double:
                {
                    _builder.AddDouble(field.Index, (double)obj, 0);
                    break;
                }
                case BaseType.Struct:
                {
                    var structOffset = SerializeStruct(obj, typeModel);
                    _builder.AddStruct(field.Index, structOffset, 0);
                    break;
                }
                case BaseType.String:
                case BaseType.Vector:
                {
                    // TODO: Handle obj == null 
                    var fieldBufferOffset = 0;
                    if (!_objectOffsets.TryGetValue(obj, out fieldBufferOffset))
                    {
                        throw new ArgumentException("Not found in map", "obj");
                    }

                    _builder.AddOffset(field.Index, fieldBufferOffset, 0);
                    break;
                }
                case BaseType.Union:
                {
                    throw new NotImplementedException();
                }
                default:
                {
                    throw new InvalidOperationException();
                }
            }
            return _builder.Offset;
        }

        private void SerializeFieldValue(object obj, StructTypeDefinition structDef, FieldTypeDefinition field)
        {
            if (structDef.IsFixed)
            {
                SerializeInlineValue(obj, field.TypeModel);
            }
            else
            {
                SerializePropertyValue(obj, field);
            }
        }

        private void SerializeStructField(object obj, StructTypeDefinition structDef, FieldTypeDefinition field)
        {
            if (field.Padding > 0)
            {
                _builder.Pad(field.Padding);
            }
            var val = field.ValueProvider.GetValue(obj);
            SerializeFieldValue(val, structDef, field);
        }


        private int SerializeStruct(object obj, TypeModel typeModel)
        {
            var structDef = typeModel.StructDef;

            if (structDef.IsFixed)
            {
                _builder.Prep(structDef.ByteSize, 0);
                foreach (var field in structDef.Fields.Reverse())
                {
                    SerializeStructField(obj, structDef, field);
                }
            }
            else
            {
                _builder.StartObject(structDef.Fields.Count());
                foreach (var field in structDef.Fields.OrderByDescending(i => i.TypeModel.InlineSize))
                {
                    SerializeStructField(obj, structDef, field);
                }
                var rootTable = _builder.EndObject();
                _builder.Finish(rootTable);
            }
            return _builder.Offset;
        }

        private int SerializeVector(ICollection collection)
        {
            var elementType = TypeHelpers.GetEnumerableElementType(collection.GetType());
            var elementTypeModel = _typeModelRegistry.GetTypeModel(elementType);
            _builder.StartVector(elementTypeModel.InlineSize, collection.Count, elementTypeModel.InlineAlignment);

            foreach(var element in collection)
            {
                SerializeInlineValue(element, elementTypeModel);
            }

            return _builder.EndVector().Value;
        }

        private int SerializeVector(object obj, TypeModel typeModel)
        {
            var collection = obj as ICollection;
            if (collection != null)
            {
                return SerializeVector(collection);
            }
            throw new NotSupportedException("Vector type not supported");
        }

        private int SerializeReferenceType(object obj, TypeModel typeModel)
        {
            if (typeModel.BaseType == BaseType.String)
            {
                return _builder.CreateString((string)obj).Value;
            }
            if (typeModel.BaseType == BaseType.Vector)
            {
                return SerializeVector(obj, typeModel);
            }
            throw new NotSupportedException();
        }

        private void SerializeReferenceTypeFields(object obj, TypeModel typeModel)
        {
            var structDef = typeModel.StructDef;

            foreach (var field in structDef.Fields)
            {
                if (!field.TypeModel.BaseType.IsScalar())
                {
                    // get object field
                    var val = field.ValueProvider.GetValue(obj);

                    if (val == null)
                    {
                        continue;
                    }

                    if (field.TypeModel.IsTable)
                    {
                        SerializeReferenceTypeFields(val, field.TypeModel);
                    }

                    var fieldBufferOffset = 0;
                    if (!_objectOffsets.TryGetValue(val, out fieldBufferOffset))
                    {
                        fieldBufferOffset = SerializeReferenceType(val, field.TypeModel);
                        _objectOffsets.Add(val, fieldBufferOffset);
                    }

                }
            }
        }

        public int Serialize()
        {
            // Todo: support more root types?
            if (_rootTypeModel.BaseType != BaseType.Struct)
            {
                throw new NotSupportedException();
            }

            var hasRefTypes = !_rootTypeModel.StructDef.IsFixed 
                && _rootTypeModel.StructDef.Fields.Any(i => !i.TypeModel.BaseType.IsFixed());

            if (hasRefTypes)
            {
                // if we have ref types, we want to serialize them first and store their offsets in an object map
                SerializeReferenceTypeFields(_rootObject, _rootTypeModel);
            }

            SerializeStruct(_rootObject, _rootTypeModel);
            return _builder.Offset;
        }
    }
}