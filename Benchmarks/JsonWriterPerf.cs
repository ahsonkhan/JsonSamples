using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace JsonPerfNumbers
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, targetCount: 5)]
    public class JsonWriterPerf
    {
        private const int ExtraArraySize = 10;
        private const int BufferSize = 1_024 + (ExtraArraySize * 64);

        private int[] _data;

        [Params(true, false)]
        public bool Formatted;

        private MemoryStream _memoryStream;
        private StreamWriter _streamWriter;
        private FileStream _fileStream;
        private StreamWriter _fileStreamWriter;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _data = new int[ExtraArraySize];
            Random rand = new Random(42);

            for (int i = 0; i < ExtraArraySize; i++)
            {
                _data[i] = rand.Next(-10000, 10000);
            }

            var buffer = new byte[BufferSize];
            _memoryStream = new MemoryStream(buffer);
            _streamWriter = new StreamWriter(_memoryStream, new UTF8Encoding(false), BufferSize, true);

            _fileStream = File.Create(@"E:\Other\trash\JsonPerfNumbers\Output.json");
            _fileStreamWriter = new StreamWriter(_fileStream, new UTF8Encoding(false), BufferSize, true);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _fileStream.Dispose();
            _memoryStream.Dispose();
            _streamWriter.Dispose();
        }

        [Benchmark]
        public void DotNetJsonWriterHelloWorld()
        {
            DotNetJsonWriterHelloWorld(Formatted);
        }

        [Benchmark]
        public void DotNetJsonWriterHelloWorldUtf8()
        {
            DotNetJsonWriterHelloWorldUtf8(Formatted);
        }

        [Benchmark(Baseline = true)]
        public void NewtonsoftHelloWorld()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            NewtonsoftHelloWorld(Formatted, _streamWriter);
        }

        //[Benchmark]
        public void DotNetJsonWriterTypical()
        {
            DotNetJsonWriterTypical(Formatted, _data);
        }

        //[Benchmark]
        public void DotNetJsonWriterTypicalUtf8()
        {
            DotNetJsonWriterTypicalUtf8(Formatted, _data);
        }

        //[Benchmark(Baseline = true)]
        public void NewtonsoftTypical()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            NewtonsoftTypical(Formatted, _streamWriter, _data);
        }

        //[Benchmark]
        public void DotNetJsonWriterReallyLarge()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            DotNetJsonWriterReallyLarge(_fileStream, Formatted, _data);
        }

        //[Benchmark]
        public void DotNetJsonWriterReallyLargeUtf8()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            DotNetJsonWriterReallyLargeUtf8(_fileStream, Formatted, _data);
        }

        //[Benchmark(Baseline = true)]
        public void NewtonsoftReallyLarge()
        {
            _fileStream.Seek(0, SeekOrigin.Begin);
            NewtonsoftReallyLarge(Formatted, _fileStreamWriter, _data);
        }

        private static void DotNetJsonWriterHelloWorld(bool formatted)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                json.WriteStartObject();
                json.WriteString("message", "Hello, World!");
                json.WriteEndObject();
                json.Flush(isFinalBlock: true);
            }
        }

        private static readonly byte[] Message = Encoding.UTF8.GetBytes("message");
        private static readonly byte[] HelloWorld = Encoding.UTF8.GetBytes("Hello, World!");

        private static void DotNetJsonWriterHelloWorldUtf8(bool formatted)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                json.WriteStartObject();
                json.WriteString(Message, HelloWorld);
                json.WriteEndObject();
                json.Flush(isFinalBlock: true);
            }
        }

        private static void NewtonsoftHelloWorld(bool formatted, TextWriter writer)
        {
            using (var json = new Newtonsoft.Json.JsonTextWriter(writer))
            {
                json.Formatting = formatted ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

                json.WriteStartObject();
                json.WritePropertyName("message");
                json.WriteValue("Hello, World!");
                json.WriteEnd();
            }
        }

        private static void DotNetJsonWriterTypical(bool formatted, ReadOnlySpan<int> data)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                json.WriteStartObject();
                json.WriteNumber("age", 42);
                json.WriteString("first", "John");
                json.WriteString("last", "Smith");
                json.WriteStartArray("phoneNumbers");
                json.WriteStringValue("425-000-1212");
                json.WriteStringValue("425-000-1213");
                json.WriteEndArray();
                json.WriteStartObject("address");
                json.WriteString("street", "1 Microsoft Way");
                json.WriteString("city", "Redmond");
                json.WriteNumber("zip", 98052);
                json.WriteEndObject();

                json.WriteStartArray("ExtraArray");
                for (int i = 0; i < data.Length; i++)
                {
                    json.WriteNumberValue(data[i]);
                }
                json.WriteEndArray();

                json.WriteEndObject();

                json.Flush(isFinalBlock: true);
            }
        }

        private static readonly byte[] Age = Encoding.UTF8.GetBytes("age");
        private static readonly byte[] First = Encoding.UTF8.GetBytes("first");
        private static readonly byte[] Last = Encoding.UTF8.GetBytes("last");
        private static readonly byte[] PhoneNumbers = Encoding.UTF8.GetBytes("phoneNumbers");
        private static readonly byte[] Address = Encoding.UTF8.GetBytes("address");
        private static readonly byte[] Street = Encoding.UTF8.GetBytes("street");
        private static readonly byte[] City = Encoding.UTF8.GetBytes("city");
        private static readonly byte[] Zip = Encoding.UTF8.GetBytes("zip");
        private static readonly byte[] ExtraArray = Encoding.UTF8.GetBytes("ExtraArray");

        private static void DotNetJsonWriterTypicalUtf8(bool formatted, ReadOnlySpan<int> data)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                json.WriteStartObject();
                json.WriteNumber(Age, 42);
                json.WriteString(First, "John");
                json.WriteString(Last, "Smith");
                json.WriteStartArray(PhoneNumbers);
                json.WriteStringValue("425-000-1212");
                json.WriteStringValue("425-000-1213");
                json.WriteEndArray();
                json.WriteStartObject(Address);
                json.WriteString(Street, "1 Microsoft Way");
                json.WriteString(City, "Redmond");
                json.WriteNumber(Zip, 98052);
                json.WriteEndObject();

                json.WriteStartArray(ExtraArray);
                for (int i = 0; i < data.Length; i++)
                {
                    json.WriteNumberValue(data[i]);
                }
                json.WriteEndArray();

                json.WriteEndObject();

                json.Flush(isFinalBlock: true);
            }
        }

        private static void NewtonsoftTypical(bool formatted, TextWriter writer, ReadOnlySpan<int> data)
        {
            using (var json = new Newtonsoft.Json.JsonTextWriter(writer))
            {
                json.Formatting = formatted ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

                json.WriteStartObject();
                json.WritePropertyName("age");
                json.WriteValue(42);
                json.WritePropertyName("first");
                json.WriteValue("John");
                json.WritePropertyName("last");
                json.WriteValue("Smith");
                json.WritePropertyName("phoneNumbers");
                json.WriteStartArray();
                json.WriteValue("425-000-1212");
                json.WriteValue("425-000-1213");
                json.WriteEnd();
                json.WritePropertyName("address");
                json.WriteStartObject();
                json.WritePropertyName("street");
                json.WriteValue("1 Microsoft Way");
                json.WritePropertyName("city");
                json.WriteValue("Redmond");
                json.WritePropertyName("zip");
                json.WriteValue(98052);
                json.WriteEnd();

                json.WritePropertyName("ExtraArray");
                json.WriteStartArray();
                for (int i = 0; i < data.Length; i++)
                {
                    json.WriteValue(data[i]);
                }
                json.WriteEnd();

                json.WriteEnd();
            }
        }

        const int IterationCount = 1_000_000;
        const int SyncWriteThreshold = 1_000_000;

        private static void DotNetJsonWriterReallyLarge(FileStream fileStream, bool formatted, ReadOnlySpan<int> data)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                long prevBytesWritten = 0;
                json.WriteStartArray();

                for (int j = 0; j < IterationCount; j++)
                {
                    json.WriteStartObject();
                    json.WriteNumber("age", 42);
                    json.WriteString("first", "John");
                    json.WriteString("last", "Smith");
                    json.WriteStartArray("phoneNumbers");
                    json.WriteStringValue("425-000-1212");
                    json.WriteStringValue("425-000-1213");
                    json.WriteEndArray();
                    json.WriteStartObject("address");
                    json.WriteString("street", "1 Microsoft Way");
                    json.WriteString("city", "Redmond");
                    json.WriteNumber("zip", 98052);
                    json.WriteEndObject();

                    json.WriteStartArray("ExtraArray");
                    for (var i = 0; i < data.Length; i++)
                    {
                        json.WriteNumberValue(data[i]);
                    }
                    json.WriteEndArray();

                    json.WriteEndObject();

                    if (json.BytesWritten - prevBytesWritten > SyncWriteThreshold)
                    {
                        json.Flush(isFinalBlock: false);
                        output.CopyTo(fileStream);
                        prevBytesWritten = json.BytesWritten;
                    }
                }

                json.WriteEndArray();
                json.Flush(isFinalBlock: true);
                output.CopyTo(fileStream);
            }
        }

        private static void DotNetJsonWriterReallyLargeUtf8(FileStream fileStream, bool formatted, ReadOnlySpan<int> data)
        {
            using (var output = new ArrayBufferWriter())
            {
                var state = new JsonWriterState(options: new JsonWriterOptions { Indented = formatted });
                var json = new Utf8JsonWriter(output, state);

                long prevBytesWritten = 0;
                json.WriteStartArray();

                for (int j = 0; j < IterationCount; j++)
                {
                    json.WriteStartObject();
                    json.WriteNumber(Age, 42);
                    json.WriteString(First, "John");
                    json.WriteString(Last, "Smith");
                    json.WriteStartArray(PhoneNumbers);
                    json.WriteStringValue("425-000-1212");
                    json.WriteStringValue("425-000-1213");
                    json.WriteEndArray();
                    json.WriteStartObject(Address);
                    json.WriteString(Street, "1 Microsoft Way");
                    json.WriteString(City, "Redmond");
                    json.WriteNumber(Zip, 98052);
                    json.WriteEndObject();

                    json.WriteStartArray("ExtraArray");
                    for (var i = 0; i < data.Length; i++)
                    {
                        json.WriteNumberValue(data[i]);
                    }
                    json.WriteEndArray();

                    json.WriteEndObject();

                    if (json.BytesWritten - prevBytesWritten > SyncWriteThreshold)
                    {
                        json.Flush(isFinalBlock: false);
                        output.CopyTo(fileStream);
                        prevBytesWritten = json.BytesWritten;
                    }
                }

                json.WriteEndArray();
                json.Flush(isFinalBlock: true);
                output.CopyTo(fileStream);
            }
        }

        private static void NewtonsoftReallyLarge(bool formatted, TextWriter writer, ReadOnlySpan<int> data)
        {
            using (var json = new Newtonsoft.Json.JsonTextWriter(writer))
            {
                json.Formatting = formatted ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None;

                json.WriteStartArray();
                for (int j = 0; j < IterationCount; j++)
                {
                    json.WriteStartObject();
                    json.WritePropertyName("age");
                    json.WriteValue(42);
                    json.WritePropertyName("first");
                    json.WriteValue("John");
                    json.WritePropertyName("last");
                    json.WriteValue("Smith");
                    json.WritePropertyName("phoneNumbers");
                    json.WriteStartArray();
                    json.WriteValue("425-000-1212");
                    json.WriteValue("425-000-1213");
                    json.WriteEnd();
                    json.WritePropertyName("address");
                    json.WriteStartObject();
                    json.WritePropertyName("street");
                    json.WriteValue("1 Microsoft Way");
                    json.WritePropertyName("city");
                    json.WriteValue("Redmond");
                    json.WritePropertyName("zip");
                    json.WriteValue(98052);
                    json.WriteEnd();

                    json.WritePropertyName("ExtraArray");
                    json.WriteStartArray();
                    for (var i = 0; i < data.Length; i++)
                    {
                        json.WriteValue(data[i]);
                    }
                    json.WriteEnd();

                    json.WriteEnd();
                }
                json.WriteEnd();
            }
        }
    }
}
