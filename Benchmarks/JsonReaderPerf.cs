using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace JsonPerfNumbers
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 5, targetCount: 10)]
    public class JsonReaderPerf
    {
        private byte[] _dataUtf8;

        [ParamsSource(nameof(TestCaseValues))]
        public TestCaseType TestCase;

        private MemoryStream _memoryStream;
        private StreamReader _streamReader;

        // Keep the JsonStrings resource names in sync with TestCaseType enum values.
        public enum TestCaseType
        {
            HelloWorld,
            BasicJson,
            Json400B,
            Json400KB
        }

        public static IEnumerable<TestCaseType> TestCaseValues() => (IEnumerable<TestCaseType>)Enum.GetValues(typeof(TestCaseType));

        [GlobalSetup]
        public void GlobalSetup()
        {
            string jsonString = JsonStrings.ResourceManager.GetString(TestCase.ToString());

            _dataUtf8 = Encoding.UTF8.GetBytes(jsonString);

            _memoryStream = new MemoryStream(_dataUtf8);
            _streamReader = new StreamReader(_memoryStream, Encoding.UTF8, false, 1024, true);
        }

        //[Benchmark(Baseline = true)]
        public void NewtonsoftEmptyLoop()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            TextReader reader = _streamReader;
            var json = new JsonTextReader(reader);
            while (json.Read()) ;
        }

        //[Benchmark]
        public void DotNetReaderEmptyLoop()
        {
            var json = new Utf8JsonReader(_dataUtf8, isFinalBlock: true, state: default);
            while (json.Read()) ;
        }

        //[Benchmark(Baseline = true)]
        public string NewtonsoftReturnString()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            TextReader reader = _streamReader;

            var sb = new StringBuilder();
            var json = new JsonTextReader(reader);
            while (json.Read())
            {
                if (json.Value != null)
                {
                    sb.Append(json.Value).Append(", ");
                }
            }

            return sb.ToString();
        }

        [Benchmark]
        public string DotNetReaderReturnString()
        {
            var sb = new StringBuilder();
            var json = new Utf8JsonReader(_dataUtf8, isFinalBlock: true, state: default);
            while (json.Read())
            {
                ReadOnlySpan<byte> valueSpan = json.ValueSpan;
                if (json.TokenType != JsonTokenType.Null)
                {
                    sb.Append(Encoding.UTF8.GetString(json.ValueSpan)).Append(", ");
                }
            }
            return sb.ToString();
        }

        //[Benchmark]
        public byte[] DotNetReaderReturnBytes()
        {
            var outputArray = new byte[_dataUtf8.Length * 2];

            Span<byte> destination = outputArray;
            var json = new Utf8JsonReader(_dataUtf8, isFinalBlock: true, state: default);
            while (json.Read())
            {
                JsonTokenType tokenType = json.TokenType;
                ReadOnlySpan<byte> valueSpan = json.ValueSpan;
                switch (tokenType)
                {
                    case JsonTokenType.PropertyName:
                        valueSpan.CopyTo(destination);
                        destination[valueSpan.Length] = (byte)',';
                        destination[valueSpan.Length + 1] = (byte)' ';
                        destination = destination.Slice(valueSpan.Length + 2);
                        break;
                    case JsonTokenType.Number:
                    case JsonTokenType.String:
                        valueSpan.CopyTo(destination);
                        destination[valueSpan.Length] = (byte)',';
                        destination[valueSpan.Length + 1] = (byte)' ';
                        destination = destination.Slice(valueSpan.Length + 2);
                        break;
                    case JsonTokenType.True:
                        // Special casing True/False so that the casing matches with Json.NET
                        destination[0] = (byte)'T';
                        destination[1] = (byte)'r';
                        destination[2] = (byte)'u';
                        destination[3] = (byte)'e';
                        destination[valueSpan.Length] = (byte)',';
                        destination[valueSpan.Length + 1] = (byte)' ';
                        destination = destination.Slice(valueSpan.Length + 2);
                        break;
                    case JsonTokenType.False:
                        destination[0] = (byte)'F';
                        destination[1] = (byte)'a';
                        destination[2] = (byte)'l';
                        destination[3] = (byte)'s';
                        destination[4] = (byte)'e';
                        destination[valueSpan.Length] = (byte)',';
                        destination[valueSpan.Length + 1] = (byte)' ';
                        destination = destination.Slice(valueSpan.Length + 2);
                        break;
                    case JsonTokenType.Null:
                        // Special casing Null so that it matches what JSON.NET does
                        break;
                    default:
                        break;
                }
            }
            return outputArray;
        }
    }
}
