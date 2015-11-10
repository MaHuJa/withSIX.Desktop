using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SN.withSIX.Core;
using SN.withSIX.Sync.Core.Legacy.SixSync.CustomRepo.dtos;
using YamlDotNet.Serialization;

namespace SN.withSIX.Mini.Tests
{
    [TestFixture]
    public class Class1
    {

        [Test]
        public void TestYaml() {
            //var o = DeserializeYaml<SixRepoConfigDto>(File.ReadAllText("C:\\temp\\test_config.yml"));
            //Console.WriteLine(o);
            //var s = DeserializeYaml<SixRepoServerDto>(File.ReadAllText("C:\\temp\\test_server.yml"));
            //Console.WriteLine(s);
            var p = DownloaderExtensions.GetYaml<SixRepoServerDto>(new Uri("http://six.armaseries.cz/a3/armaseries.yml"));
            Console.WriteLine(p);
        }

        T DeserializeYaml<T>(string r) {
            return
    new Deserializer(ignoreUnmatched: true).Deserialize<T>(
        new StringReader(r));

        }
    }
}
