using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace JsonPerfNumbers
{
    [MemoryDiagnoser]
    [SimpleJob(warmupCount: 3, targetCount: 5)]
    public class JsonDocumentPerf
    {
        private byte[] _dataUtf8;

        [Params(true, false)]
        public bool ParseAndAccess;

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

        [Benchmark(Baseline = true)]
        public void Newtonsoft()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            using (var jsonReader = new JsonTextReader(_streamReader))
            {
                JToken obj = JToken.ReadFrom(jsonReader);

                if (!ParseAndAccess)
                {
                    if (TestCase == TestCaseType.HelloWorld)
                    {
                        ReadHelloWorld(obj);
                    }
                    else if (TestCase == TestCaseType.Json400B)
                    {
                        ReadJson400B(obj);
                    }
                    else if (TestCase == TestCaseType.BasicJson)
                    {
                        ReadJsonBasic(obj);
                    }
                    else if (TestCase == TestCaseType.Json400KB)
                    {
                        ReadJson400KB(obj);
                    }
                }
            }
        }

        [Benchmark]
        public void DotNetDocument()
        {
            using (JsonDocument document = JsonDocument.Parse(_dataUtf8))
            {
                if (!ParseAndAccess)
                {
                    if (TestCase == TestCaseType.HelloWorld)
                    {
                        ReadHelloWorld(document.RootElement);
                    }
                    else if (TestCase == TestCaseType.Json400B)
                    {
                        ReadJson400B(document.RootElement);
                    }
                    else if (TestCase == TestCaseType.BasicJson)
                    {
                        ReadJsonBasic(document.RootElement);
                    }
                    else if (TestCase == TestCaseType.Json400KB)
                    {
                        ReadJson400KB(document.RootElement);
                    }
                }
            }
        }

        private static string ReadHelloWorld(JToken obj)
        {
            string message = (string)obj["message"];
            return message;
        }

        private static string ReadJson400KB(JToken obj)
        {
            var sb = new StringBuilder();
            foreach (JToken token in obj)
            {
                sb.Append((string)token["_id"]);
                sb.Append((int)token["index"]);
                sb.Append((string)token["guid"]);
                sb.Append((bool)token["isActive"]);
                sb.Append((string)token["balance"]);
                sb.Append((string)token["picture"]);
                sb.Append((int)token["age"]);
                sb.Append((string)token["eyeColor"]);
                sb.Append((string)token["name"]);
                sb.Append((string)token["gender"]);
                sb.Append((string)token["company"]);
                sb.Append((string)token["email"]);
                sb.Append((string)token["phone"]);
                sb.Append((string)token["address"]);
                sb.Append((string)token["about"]);
                sb.Append((string)token["registered"]);
                sb.Append((double)token["latitude"]);
                sb.Append((double)token["longitude"]);

                JToken tags = token["tags"];
                foreach (JToken tag in tags)
                {
                    sb.Append((string)tag);
                }
                JToken friends = token["friends"];
                foreach (JToken friend in friends)
                {
                    sb.Append((int)friend["id"]);
                    sb.Append((string)friend["name"]);
                }
                sb.Append((string)token["greeting"]);
                sb.Append((string)token["favoriteFruit"]);

            }
            return sb.ToString();
        }

        private static string ReadJson400B(JToken obj)
        {
            var sb = new StringBuilder();
            foreach (JToken token in obj)
            {
                sb.Append((string)token["_id"]);
                sb.Append((int)token["index"]);
                sb.Append((bool)token["isActive"]);
                sb.Append((string)token["balance"]);
                sb.Append((string)token["picture"]);
                sb.Append((int)token["age"]);
                sb.Append((string)token["email"]);
                sb.Append((string)token["phone"]);
                sb.Append((string)token["address"]);
                sb.Append((string)token["registered"]);
                sb.Append((double)token["latitude"]);
                sb.Append((double)token["longitude"]);
            }
            return sb.ToString();
        }

        private static string ReadJsonBasic(JToken obj)
        {
            var sb = new StringBuilder();
            sb.Append((int)obj["age"]);
            sb.Append((string)obj["first"]);
            sb.Append((string)obj["last"]);
            JToken phoneNumbers = obj["phoneNumbers"];
            foreach (JToken phoneNumber in phoneNumbers)
            {
                sb.Append((string)phoneNumber);
            }
            JToken address = obj["address"];
            sb.Append((string)address["street"]);
            sb.Append((string)address["city"]);
            sb.Append((string)address["zip"]);
            return sb.ToString();
        }

        private static string ReadHelloWorld(JsonElement obj)
        {
            string message = obj.GetProperty("message").GetString();
            return message;
        }

        private static string ReadJson400KB(JsonElement obj)
        {
            ReadOnlySpan<byte> _id = Encoding.UTF8.GetBytes("_id");
            ReadOnlySpan<byte> index = Encoding.UTF8.GetBytes("index");
            ReadOnlySpan<byte> guid = Encoding.UTF8.GetBytes("guid");
            ReadOnlySpan<byte> isActive = Encoding.UTF8.GetBytes("isActive");
            ReadOnlySpan<byte> balance = Encoding.UTF8.GetBytes("balance");
            ReadOnlySpan<byte> picture = Encoding.UTF8.GetBytes("picture");
            ReadOnlySpan<byte> age = Encoding.UTF8.GetBytes("age");
            ReadOnlySpan<byte> eyeColor = Encoding.UTF8.GetBytes("eyeColor");
            ReadOnlySpan<byte> name = Encoding.UTF8.GetBytes("name");
            ReadOnlySpan<byte> gender = Encoding.UTF8.GetBytes("gender");
            ReadOnlySpan<byte> company = Encoding.UTF8.GetBytes("company");
            ReadOnlySpan<byte> email = Encoding.UTF8.GetBytes("email");
            ReadOnlySpan<byte> phone = Encoding.UTF8.GetBytes("phone");
            ReadOnlySpan<byte> address = Encoding.UTF8.GetBytes("address");
            ReadOnlySpan<byte> about = Encoding.UTF8.GetBytes("about");
            ReadOnlySpan<byte> registered = Encoding.UTF8.GetBytes("registered");
            ReadOnlySpan<byte> latitude = Encoding.UTF8.GetBytes("latitude");
            ReadOnlySpan<byte> longitude = Encoding.UTF8.GetBytes("longitude");
            ReadOnlySpan<byte> tags = Encoding.UTF8.GetBytes("tags");
            ReadOnlySpan<byte> friends = Encoding.UTF8.GetBytes("friends");
            ReadOnlySpan<byte> id = Encoding.UTF8.GetBytes("id");
            ReadOnlySpan<byte> greeting = Encoding.UTF8.GetBytes("greeting");
            ReadOnlySpan<byte> favoriteFruit = Encoding.UTF8.GetBytes("favoriteFruit");

            var sb = new StringBuilder();
            foreach (var element in obj.EnumerateArray())
            {
                sb.Append(element.GetProperty(_id).GetString());
                sb.Append(element.GetProperty(index).GetInt32());
                sb.Append(element.GetProperty(guid).GetString());
                sb.Append(element.GetProperty(isActive).GetBoolean());
                sb.Append(element.GetProperty(balance).GetString());
                sb.Append(element.GetProperty(picture).GetString());
                sb.Append(element.GetProperty(age).GetInt32());
                sb.Append(element.GetProperty(eyeColor).GetString());
                sb.Append(element.GetProperty(name).GetString());
                sb.Append(element.GetProperty(gender).GetString());
                sb.Append(element.GetProperty(company).GetString());
                sb.Append(element.GetProperty(email).GetString());
                sb.Append(element.GetProperty(phone).GetString());
                sb.Append(element.GetProperty(address).GetString());
                sb.Append(element.GetProperty(about).GetString());
                sb.Append(element.GetProperty(registered).GetString());
                sb.Append(element.GetProperty(latitude).GetDouble());
                sb.Append(element.GetProperty(longitude).GetDouble());

                JsonElement tagsObject = element.GetProperty(tags);
                foreach (JsonElement tag in tagsObject.EnumerateArray())
                {
                    sb.Append(tag.GetString());
                }
                JsonElement friendsObject = element.GetProperty(friends);
                foreach (JsonElement friend in friendsObject.EnumerateArray())
                {
                    sb.Append(friend.GetProperty(id).GetInt32());
                    sb.Append(friend.GetProperty(name).GetString());
                }
                sb.Append(element.GetProperty(greeting).GetString());
                sb.Append(element.GetProperty(favoriteFruit).GetString());
            }
            return sb.ToString();
        }

        private static string ReadJson400B(JsonElement obj)
        {
            ReadOnlySpan<byte> _id = Encoding.UTF8.GetBytes("_id");
            ReadOnlySpan<byte> index = Encoding.UTF8.GetBytes("index");
            ReadOnlySpan<byte> isActive = Encoding.UTF8.GetBytes("isActive");
            ReadOnlySpan<byte> balance = Encoding.UTF8.GetBytes("balance");
            ReadOnlySpan<byte> picture = Encoding.UTF8.GetBytes("picture");
            ReadOnlySpan<byte> age = Encoding.UTF8.GetBytes("age");
            ReadOnlySpan<byte> email = Encoding.UTF8.GetBytes("email");
            ReadOnlySpan<byte> phone = Encoding.UTF8.GetBytes("phone");
            ReadOnlySpan<byte> address = Encoding.UTF8.GetBytes("address");
            ReadOnlySpan<byte> registered = Encoding.UTF8.GetBytes("registered");
            ReadOnlySpan<byte> latitude = Encoding.UTF8.GetBytes("latitude");
            ReadOnlySpan<byte> longitude = Encoding.UTF8.GetBytes("longitude");

            var sb = new StringBuilder();
            foreach (var element in obj.EnumerateArray())
            {
                sb.Append(element.GetProperty(_id).GetString());
                sb.Append(element.GetProperty(index).GetInt32());
                sb.Append(element.GetProperty(isActive).GetBoolean());
                sb.Append(element.GetProperty(balance).GetString());
                sb.Append(element.GetProperty(picture).GetString());
                sb.Append(element.GetProperty(age).GetInt32());
                sb.Append(element.GetProperty(email).GetString());
                sb.Append(element.GetProperty(phone).GetString());
                sb.Append(element.GetProperty(address).GetString());
                sb.Append(element.GetProperty(registered).GetString());
                sb.Append(element.GetProperty(latitude).GetDouble());
                sb.Append(element.GetProperty(longitude).GetDouble());
            }
            return sb.ToString();
        }

        private static string ReadJsonBasic(JsonElement obj)
        {
            var sb = new StringBuilder();
            sb.Append(obj.GetProperty("age").GetInt32());
            sb.Append(obj.GetProperty("first").GetString());
            sb.Append(obj.GetProperty("last").GetString());
            JsonElement phoneNumbers = obj.GetProperty("phoneNumbers");
            foreach (var element in phoneNumbers.EnumerateArray())
            {
                sb.Append(element.GetString());
            }
            JsonElement address = obj.GetProperty("address");
            sb.Append(address.GetProperty("street").GetString());
            sb.Append(address.GetProperty("city").GetString());
            sb.Append(address.GetProperty("zip").GetInt32());
            return sb.ToString();
        }
    }
}
