// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: kudu/util/compression/compression.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Knet.Kudu.Client.Protobuf {

  /// <summary>Holder for reflection information generated from kudu/util/compression/compression.proto</summary>
  public static partial class CompressionReflection {

    #region Descriptor
    /// <summary>File descriptor for kudu/util/compression/compression.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static CompressionReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CidrdWR1L3V0aWwvY29tcHJlc3Npb24vY29tcHJlc3Npb24ucHJvdG8SBGt1",
            "ZHUqeQoRQ29tcHJlc3Npb25UeXBlUEISGAoTVU5LTk9XTl9DT01QUkVTU0lP",
            "ThDnBxIXChNERUZBVUxUX0NPTVBSRVNTSU9OEAASEgoOTk9fQ09NUFJFU1NJ",
            "T04QARIKCgZTTkFQUFkQAhIHCgNMWjQQAxIICgRaTElCEARCLQoPb3JnLmFw",
            "YWNoZS5rdWR1qgIZS25ldC5LdWR1LkNsaWVudC5Qcm90b2J1Zg=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Knet.Kudu.Client.Protobuf.CompressionTypePB), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum CompressionTypePB {
    [pbr::OriginalName("UNKNOWN_COMPRESSION")] UnknownCompression = 999,
    [pbr::OriginalName("DEFAULT_COMPRESSION")] DefaultCompression = 0,
    [pbr::OriginalName("NO_COMPRESSION")] NoCompression = 1,
    [pbr::OriginalName("SNAPPY")] Snappy = 2,
    [pbr::OriginalName("LZ4")] Lz4 = 3,
    [pbr::OriginalName("ZLIB")] Zlib = 4,
  }

  #endregion

}

#endregion Designer generated code
