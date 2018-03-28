using System;

namespace Network
{

    public class SerializeException : Exception
    {
        public SerializeException(string message) : base(message)
        {

        }
    }

    public static class NetworkHelper
    {
        private static int SIZE_INT = sizeof(int);
        private static int SIZE_LONG = sizeof(long);
        private static int SIZE_BOOL = sizeof(bool);
        private static int SIZE_FLOAT = sizeof(float);
        private static int SIZE_DOUBLE = sizeof(double);

        public static byte[] Serialize(object source)
        {
            if (source == null)
                return new byte[0];
            int size = GetBytesOfObject(source);
            byte[] data = new byte[size];
            int byteToWrite = 0;

            foreach (System.Reflection.FieldInfo fieldInfo in source.GetType().GetFields())
            {
                if (!fieldInfo.FieldType.IsArray)
                {
                    //BasicTypes
                    if (fieldInfo.FieldType == typeof(bool))
                    {
                        WriteToByteArray((bool)fieldInfo.GetValue(source), ref data, byteToWrite);
                    }
                    else if (fieldInfo.FieldType == typeof(int))
                    {
                        WriteToByteArray((int)fieldInfo.GetValue(source), ref data, byteToWrite);
                    }
                    else if (fieldInfo.FieldType == typeof(long))
                    {
                        WriteToByteArray((long)fieldInfo.GetValue(source), ref data, byteToWrite);
                    }
                    else if (fieldInfo.FieldType == typeof(double))
                    {
                        WriteToByteArray((double)fieldInfo.GetValue(source), ref data, byteToWrite);
                    }
                    else if (fieldInfo.FieldType == typeof(float))
                    {
                        WriteToByteArray((float)fieldInfo.GetValue(source), ref data, byteToWrite);

                        //Advanced Types
                    }
                    else if (fieldInfo.FieldType == typeof(string))
                    {
                        WriteToByteArray((string)fieldInfo.GetValue(source), ref data, byteToWrite);
                    }
                    else if (fieldInfo.FieldType.IsClass)
                    {
                        byte[] bytes = Serialize(fieldInfo.GetValue(source));
                        data[byteToWrite + 0] = (byte)(bytes.Length / 0xff);
                        data[byteToWrite + 1] = (byte)(bytes.Length % 0xff);
                        Array.Copy(bytes, 0, data, byteToWrite + 2, bytes.Length);
                    }
                    else if (fieldInfo.FieldType.IsEnum)
                    {
                        data[byteToWrite] = (byte)(int)fieldInfo.GetValue(source);
                    }

                }
                else
                {
                    //byte[] arrayLength = new byte[2];
                    Array array = (Array)fieldInfo.GetValue(source);
                    data[byteToWrite + 0] = (byte)(array.Length / 0xff);
                    data[byteToWrite + 1] = (byte)(array.Length % 0xff);
                    int writeTo = byteToWrite + 2;
                    foreach (object value in array)
                    {
                        byte[] arrayContent = Serialize(value);
                        Array.Copy(arrayContent, 0, data, writeTo, arrayContent.Length);
                        writeTo += arrayContent.Length;
                    }
                }

                byteToWrite += GetBytesOfObject(source, fieldInfo);
            }

            return data;
        }


        public static T Deserialize<T>(byte[] source) where T : new()
        {
            if (source.Length == 0)
                return default(T);
            try
            {
                //UnityEngine.Debug.Log(typeof(T));
                T type = new T();

                int byteToRead = 0;

                foreach (System.Reflection.FieldInfo fieldInfo in type.GetType().GetFields())
                {
                    if (!fieldInfo.FieldType.IsArray)
                    {
                        if (fieldInfo.FieldType == typeof(bool))
                        {
                            fieldInfo.SetValue(type, source[byteToRead] == 0x01 ? true : false);
                        }
                        else if (fieldInfo.FieldType == typeof(int))
                        {
                            fieldInfo.SetValue(type, BitConverter.ToInt32(source, byteToRead));
                        }
                        else if (fieldInfo.FieldType == typeof(long))
                        {
                            fieldInfo.SetValue(type, BitConverter.ToInt64(source, byteToRead));
                        }
                        else if (fieldInfo.FieldType == typeof(double))
                        {
                            fieldInfo.SetValue(type, BitConverter.ToDouble(source, byteToRead));
                        }
                        else if (fieldInfo.FieldType == typeof(float))
                        {
                            fieldInfo.SetValue(type, BitConverter.ToSingle(source, byteToRead));
                        }
                        else if (fieldInfo.FieldType == typeof(string))
                        {
                            fieldInfo.SetValue(type, (string)StringFromByteArray(ref source, byteToRead));
                        }
                        else if (fieldInfo.FieldType.IsEnum)
                        {
                            fieldInfo.SetValue(type, (int)source[byteToRead]);
                        }
                        else if (fieldInfo.FieldType.IsClass)
                        {
                            byte[] classData = new byte[source[byteToRead + 0] * 0xff + source[byteToRead + 1]];
                            Array.Copy(source, byteToRead + 2, classData, 0, classData.Length);
                            object t = typeof(NetworkHelper).GetMethod("Deserialize").MakeGenericMethod(fieldInfo.FieldType).Invoke(null, new object[] { classData });
                            fieldInfo.SetValue(type, t);
                            byteToRead += classData.Length + 2;
                            continue;
                        }
                    }
                    else
                    {
                        int arraySize = source[byteToRead + 0] * 0xff + source[byteToRead + 1];
                        Array arr = Array.CreateInstance(fieldInfo.FieldType.GetElementType(), arraySize);
                        byteToRead += 2;
                        for (int i = 0; i < arraySize; i++)
                        {
                            byte[] arrayContent = new byte[source.Length - byteToRead];
                            Array.Copy(source, byteToRead, arrayContent, 0, arrayContent.Length);
                            arr.SetValue(typeof(NetworkHelper)
                                .GetMethod("Deserialize")
                                .MakeGenericMethod(fieldInfo.FieldType.GetElementType())
                                .Invoke(null, new object[] { arrayContent }), i);
                            byteToRead += GetBytesOfObject(arr.GetValue(i));
                        }
                        fieldInfo.SetValue(type, arr);
                        continue;
                    }
                    byteToRead += GetBytesOfObject(type, fieldInfo);
                }
                return type;
            }
            catch (Exception ex)
            {
                throw new SerializeException("Internal Error: " + ex.Message);
            }
        }

        private static int GetBytesOfObject(object source, System.Reflection.FieldInfo fieldInfo)
        {

            //BasicTypes
            if (!fieldInfo.FieldType.IsArray)
            {
                if (fieldInfo.FieldType == typeof(bool))
                {
                    return SIZE_BOOL;
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    return SIZE_INT;
                }
                else if (fieldInfo.FieldType == typeof(long))
                {
                    return SIZE_LONG;
                }
                else if (fieldInfo.FieldType == typeof(double))
                {
                    return SIZE_DOUBLE;
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    return SIZE_FLOAT;

                    //Advanced Types
                }
                else if (fieldInfo.FieldType == typeof(string))
                {
                    return GetByteSize((string)fieldInfo.GetValue(source));
                }
                else if (fieldInfo.FieldType.IsClass)
                {
                    return 2 + GetBytesOfObject(fieldInfo.GetValue(source));
                }
                else if (fieldInfo.FieldType.IsEnum)
                {
                    return 1;
                }
            }
            else
            {
                int size = 2;
                foreach (object value in (Array)fieldInfo.GetValue(source))
                {
                    size += GetBytesOfObject(value);
                }
                return size;
            }

            throw new SerializeException("Internal Error: can not get size of " + fieldInfo.FieldType);
        }

        private static string StringFromByteArray(ref byte[] array, int startIndex)
        {
            string s = "";
            for (int i = 0; i < array[startIndex + 0] * 0xff + array[startIndex + 1]; i++)
            {
                s += (char)array[startIndex + i + 2];
            }
            return s;
        }

        private static void WriteToByteArray(string str, ref byte[] array, int startIndex)
        {
            if (str == null) return;
            array[startIndex + 0] = (byte)(str.Length / 0xff); // 0xff cause of 2 bytes are used for string length
            array[startIndex + 1] = (byte)(str.Length % 0xff);
            for (int i = 0; i < str.Length; i++)
            {
                array[startIndex + i + 2] = (byte)str[i];
            }
        }

        private static void WriteToByteArray(int int32, ref byte[] array, int startIndex)
        {
            byte[] content = BitConverter.GetBytes(int32);
            Array.Copy(content, 0, array, startIndex, content.Length);
        }

        private static void WriteToByteArray(long int64, ref byte[] array, int startIndex)
        {
            byte[] content = BitConverter.GetBytes(int64);
            Array.Copy(content, 0, array, startIndex, content.Length);
        }

        private static void WriteToByteArray(double dbl, ref byte[] array, int startIndex)
        {
            byte[] content = BitConverter.GetBytes(dbl);
            Array.Copy(content, 0, array, startIndex, content.Length);
        }

        private static void WriteToByteArray(float flt, ref byte[] array, int startIndex)
        {
            byte[] content = BitConverter.GetBytes(flt);
            Array.Copy(content, 0, array, startIndex, content.Length);
        }

        private static void WriteToByteArray(bool boolean, ref byte[] array, int startIndex)
        {
            array[startIndex] = (byte)(boolean == true ? 0x01 : 0x00);
        }

        private static int GetByteSize(string str)
        {
            if (str == null) return 2;
            return 2 + str.Length;
        }

        private static int GetBytesOfObject(object classInstance)
        {
            if (classInstance == null)
                return 0;
            int byteSize = 0;
            System.Reflection.FieldInfo[] fields = classInstance.GetType().GetFields();
            foreach (System.Reflection.FieldInfo fieldInfo in fields)
            {
                byteSize += GetBytesOfObject(classInstance, fieldInfo);
            }
            return byteSize;
        }

    }
}
