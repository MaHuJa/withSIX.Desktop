// <copyright company="SIX Networks GmbH" file="XmlTools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;
using NDepend.Path;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public class XmlTools
        {
            public T LoadXmlFromFile<T>(string path) {
                var serializer = new DataContractSerializer(typeof (T));
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return (T) serializer.ReadObject(fs);
            }

            public virtual T Deserialize<T>(XElement doc) {
                Contract.Requires<ArgumentNullException>(doc != null);

                var xmlSerializer = new XmlSerializer(typeof (T));
                using (var reader = doc.CreateReader())
                    return (T) xmlSerializer.Deserialize(reader);
            }

            public virtual void SaveXmlToDiskThroughMemory(object graph, IAbsoluteFilePath filePath, bool pretty = false) {
                Contract.Requires<ArgumentNullException>(graph != null);
                Contract.Requires<ArgumentNullException>(filePath != null);

                FileUtil.Ops.AddIORetryDialog(() => FileTools.SafeIO.SafeSave(x => {
                    using (var ms = new MemoryStream()) {
                        var serializer = new DataContractSerializer(graph.GetType());
                        serializer.WriteObject(ms, graph);
                        ms.Seek(0, 0);
                        using (var fs = new FileStream(x.ToString(), FileMode.Create)) {
                            ms.CopyTo(fs);
                            fs.Flush(true);
                        }
                    }
                }, filePath), filePath.ToString());
            }
        }
    }
}