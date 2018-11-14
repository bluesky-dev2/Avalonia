﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Avalonia.Markup.Xaml.PortableXaml;
using Avalonia.Utilities;
using Microsoft.Build.Framework;
using SPath=System.IO.Path;
namespace Avalonia.Build.Tasks
{
    public class GenerateAvaloniaResourcesTask : ITask
    {
        [Required]
        public ITaskItem[] Resources { get; set; }
        [Required]
        public string Root { get; set; }
        [Required]
        public string Output { get; set; }
        [Required]
        public ITaskItem[] EmbeddedResources { get; set; }

        class Source
        {
            public string Path { get; set; }
            public int Size { get; set; }
            private byte[] _data;
            private string _sourcePath;

            public Source(string file, string root)
            {
                file = SPath.GetFullPath(file);
                root = SPath.GetFullPath(root);
                var fileUri = new Uri(file, UriKind.Absolute);
                var rootUri = new Uri(root, UriKind.Absolute);
                rootUri = new Uri(rootUri.ToString().TrimEnd('/') + '/');
                Path = '/' + rootUri.MakeRelativeUri(fileUri).ToString().TrimStart('/');
                _sourcePath = file;
                Size = (int)new FileInfo(_sourcePath).Length;
            }

            public string SystemPath => _sourcePath ?? Path;

            public Source(string path, byte[] data)
            {
                Path = path;
                _data = data;
                Size = data.Length;
            }

            public Stream Open()
            {
                if (_data != null)
                    return new MemoryStream(_data, false);
                return File.OpenRead(_sourcePath);
            }

            public string ReadAsString()
            {
                if (_data != null)
                    return Encoding.UTF8.GetString(_data);
                return File.ReadAllText(_sourcePath);
            }
        }

        List<Source> BuildResourceSources() => Resources.Select(r => new Source(r.ItemSpec, Root)).ToList();

        void Pack(Stream output, List<Source> sources)
        {
            var offsets = new Dictionary<Source, int>();
            var coffset = 0;
            foreach (var s in sources)
            {
                offsets[s] = coffset;
                coffset += s.Size;
            }
            var index = sources.Select(s => new AvaloniaResourcesIndexEntry
            {
                Path = s.Path,
                Size = s.Size,
                Offset = offsets[s]
            }).ToList();
            var ms = new MemoryStream();
            AvaloniaResourcesIndexReaderWriter.Write(ms, index);
            new BinaryWriter(output).Write((int)ms.Length);
            ms.Position = 0;
            ms.CopyTo(output);
            foreach (var s in sources)
            {
                using(var input = s.Open())
                    input.CopyTo(output);
            }
        }

        bool PreProcessXamlFiles(List<Source> sources)
        {
            var typeToXamlIndex = new Dictionary<string, string>(); 
            
            foreach (var s in sources.ToList())
            {
                if (s.Path.ToLowerInvariant().EndsWith(".xaml") || s.Path.ToLowerInvariant().EndsWith(".paml"))
                {
                    XamlFileInfo info;
                    try
                    {
                        info = XamlFileInfo.Parse(s.ReadAsString());
                    }
                    catch(Exception e)
                    {
                        BuildEngine.LogError(s.SystemPath, "File doesn't contain valid XAML: " + e);
                        return false;
                    }

                    if (info.XClass != null)
                    {
                        if (typeToXamlIndex.ContainsKey(info.XClass))
                        {
                            
                            BuildEngine.LogError(s.SystemPath,
                                $"Duplicate x:Class directive, {info.XClass} is already used in {typeToXamlIndex[info.XClass]}");
                            return false;
                        }
                        typeToXamlIndex[info.XClass] = s.Path;
                    }
                }
            }

            var xamlInfo = new AvaloniaResourceXamlInfo
            {
                ClassToResourcePathIndex = typeToXamlIndex
            };
            var ms = new MemoryStream();
            new DataContractSerializer(typeof(AvaloniaResourceXamlInfo)).WriteObject(ms, xamlInfo);
            sources.Add(new Source("/!AvaloniaResourceXamlInfo", ms.ToArray()));
            return true;
        }
        
        public bool Execute()
        {
            foreach(var r in EmbeddedResources.Where(r=>r.ItemSpec.EndsWith(".xaml")||r.ItemSpec.EndsWith(".paml")))
                BuildEngine.LogWarning(r.ItemSpec, "XAML file is packed using legacy EmbeddedResource/resm scheme, relative URIs won't work");
            var resources = BuildResourceSources();

            if (!PreProcessXamlFiles(resources))
                return false;
            var dir = Path.GetDirectoryName(Output);
            Directory.CreateDirectory(dir);
            using (var file = File.Create(Output))
                Pack(file, resources);
            return true;
        }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
