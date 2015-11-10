// <copyright company="SIX Networks GmbH" file="JsonTools.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using NDepend.Path;
using Newtonsoft.Json;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core
{
    public partial class Tools
    {
        public class JsonTools
        {
            public T LoadJsonFromFile<T>(IAbsoluteFilePath file) {
                return LoadJson<T>(LoadTextFromFile(file));
            }

            public T LoadJsonFromFile<T>(IAbsoluteFilePath file, JsonSerializerSettings settings) {
                return LoadJson<T>(LoadTextFromFile(file), settings);
            }

            public async Task<T> LoadJsonFromFileAsync<T>(IAbsoluteFilePath file) {
                return LoadJson<T>(await LoadTextFromFileAsync(file).ConfigureAwait(false));
            }

            public async Task<T> LoadJsonFromFileAsync<T>(IAbsoluteFilePath file, JsonSerializerSettings settings) {
                return LoadJson<T>(await LoadTextFromFileAsync(file).ConfigureAwait(false), settings);
            }

            public T LoadJson<T>(string json) {
                return LoadJson<T>(json, SerializationExtension.DefaultSettings);
            }

            public T LoadJson<T>(string json, JsonSerializerSettings settings) {
                Contract.Requires<ArgumentNullException>(json != null);

                return JsonConvert.DeserializeObject<T>(json, settings);
            }

            public string LoadTextFromFile(IAbsoluteFilePath path) {
                Contract.Requires<ArgumentNullException>(path != null);

                var text = FileUtil.Ops.ReadTextFile(path);
                return string.IsNullOrWhiteSpace(text)
                    ? null
                    : text;
            }

            public async Task<string> LoadTextFromFileAsync(IAbsoluteFilePath path) {
                Contract.Requires<ArgumentNullException>(path != null);

                var text = await FileUtil.Ops.ReadTextFileAsync(path).ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(text)
                    ? null
                    : text;
            }

            public void SaveJsonToDiskThroughMemory(object graph, IAbsoluteFilePath filePath, bool pretty = false) {
                Contract.Requires<ArgumentNullException>(graph != null);
                Contract.Requires<ArgumentNullException>(filePath != null);
                SaveJsonToDiskThroughMemory(graph, filePath, SerializationExtension.DefaultSettings, pretty);
            }

            void SaveJsonToDiskThroughMemory(object graph, IAbsoluteFilePath filePath,
                JsonSerializerSettings settings, bool pretty = false) {
                var json = graph.ToJson(settings, pretty);
                FileTools.SafeIO.SafeSave(x => SaveTextToFile(x, json), filePath);
            }

            static void SaveTextToFile(IAbsoluteFilePath x, string json) {
                FileUtil.Ops.CreateText(x, json);
            }
        }
    }
}