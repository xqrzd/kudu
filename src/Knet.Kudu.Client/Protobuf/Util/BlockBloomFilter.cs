// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: kudu/util/block_bloom_filter.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Knet.Kudu.Client.Protobuf {

  /// <summary>Holder for reflection information generated from kudu/util/block_bloom_filter.proto</summary>
  public static partial class BlockBloomFilterReflection {

    #region Descriptor
    /// <summary>File descriptor for kudu/util/block_bloom_filter.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BlockBloomFilterReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiJrdWR1L3V0aWwvYmxvY2tfYmxvb21fZmlsdGVyLnByb3RvEgRrdWR1GhRr",
            "dWR1L3V0aWwvaGFzaC5wcm90bxoXa3VkdS91dGlsL3BiX3V0aWwucHJvdG8i",
            "qwEKEkJsb2NrQmxvb21GaWx0ZXJQQhIXCg9sb2dfc3BhY2VfYnl0ZXMYASAB",
            "KAUSGAoKYmxvb21fZGF0YRgCIAEoDEIEiLUYARIUCgxhbHdheXNfZmFsc2UY",
            "AyABKAgSNgoOaGFzaF9hbGdvcml0aG0YBCABKA4yEy5rdWR1Lkhhc2hBbGdv",
            "cml0aG06CUZBU1RfSEFTSBIUCgloYXNoX3NlZWQYBSABKA06ATBCLQoPb3Jn",
            "LmFwYWNoZS5rdWR1qgIZS25ldC5LdWR1LkNsaWVudC5Qcm90b2J1Zg=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Knet.Kudu.Client.Protobuf.HashReflection.Descriptor, global::Knet.Kudu.Client.Protobuf.PbUtilReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Knet.Kudu.Client.Protobuf.BlockBloomFilterPB), global::Knet.Kudu.Client.Protobuf.BlockBloomFilterPB.Parser, new[]{ "LogSpaceBytes", "BloomData", "AlwaysFalse", "HashAlgorithm", "HashSeed" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class BlockBloomFilterPB : pb::IMessage<BlockBloomFilterPB>
      , pb::IBufferMessage
  {
    private static readonly pb::MessageParser<BlockBloomFilterPB> _parser = new pb::MessageParser<BlockBloomFilterPB>(() => new BlockBloomFilterPB());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<BlockBloomFilterPB> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Knet.Kudu.Client.Protobuf.BlockBloomFilterReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockBloomFilterPB() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockBloomFilterPB(BlockBloomFilterPB other) : this() {
      _hasBits0 = other._hasBits0;
      logSpaceBytes_ = other.logSpaceBytes_;
      bloomData_ = other.bloomData_;
      alwaysFalse_ = other.alwaysFalse_;
      hashAlgorithm_ = other.hashAlgorithm_;
      hashSeed_ = other.hashSeed_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public BlockBloomFilterPB Clone() {
      return new BlockBloomFilterPB(this);
    }

    /// <summary>Field number for the "log_space_bytes" field.</summary>
    public const int LogSpaceBytesFieldNumber = 1;
    private readonly static int LogSpaceBytesDefaultValue = 0;

    private int logSpaceBytes_;
    /// <summary>
    /// Log2 of the space required for the BlockBloomFilter.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int LogSpaceBytes {
      get { if ((_hasBits0 & 1) != 0) { return logSpaceBytes_; } else { return LogSpaceBytesDefaultValue; } }
      set {
        _hasBits0 |= 1;
        logSpaceBytes_ = value;
      }
    }
    /// <summary>Gets whether the "log_space_bytes" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasLogSpaceBytes {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "log_space_bytes" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearLogSpaceBytes() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "bloom_data" field.</summary>
    public const int BloomDataFieldNumber = 2;
    private readonly static pb::ByteString BloomDataDefaultValue = pb::ByteString.Empty;

    private pb::ByteString bloomData_;
    /// <summary>
    /// The bloom filter bitmap.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString BloomData {
      get { return bloomData_ ?? BloomDataDefaultValue; }
      set {
        bloomData_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "bloom_data" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasBloomData {
      get { return bloomData_ != null; }
    }
    /// <summary>Clears the value of the "bloom_data" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearBloomData() {
      bloomData_ = null;
    }

    /// <summary>Field number for the "always_false" field.</summary>
    public const int AlwaysFalseFieldNumber = 3;
    private readonly static bool AlwaysFalseDefaultValue = false;

    private bool alwaysFalse_;
    /// <summary>
    /// Whether the BlockBloomFilter is empty and hence always returns false for lookups.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool AlwaysFalse {
      get { if ((_hasBits0 & 2) != 0) { return alwaysFalse_; } else { return AlwaysFalseDefaultValue; } }
      set {
        _hasBits0 |= 2;
        alwaysFalse_ = value;
      }
    }
    /// <summary>Gets whether the "always_false" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasAlwaysFalse {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "always_false" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearAlwaysFalse() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "hash_algorithm" field.</summary>
    public const int HashAlgorithmFieldNumber = 4;
    private readonly static global::Knet.Kudu.Client.Protobuf.HashAlgorithm HashAlgorithmDefaultValue = global::Knet.Kudu.Client.Protobuf.HashAlgorithm.FastHash;

    private global::Knet.Kudu.Client.Protobuf.HashAlgorithm hashAlgorithm_;
    /// <summary>
    /// Hash algorithm to generate 32-bit unsigned integer hash values before inserting
    /// in the BlockBloomFilter.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Knet.Kudu.Client.Protobuf.HashAlgorithm HashAlgorithm {
      get { if ((_hasBits0 & 4) != 0) { return hashAlgorithm_; } else { return HashAlgorithmDefaultValue; } }
      set {
        _hasBits0 |= 4;
        hashAlgorithm_ = value;
      }
    }
    /// <summary>Gets whether the "hash_algorithm" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasHashAlgorithm {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "hash_algorithm" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearHashAlgorithm() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "hash_seed" field.</summary>
    public const int HashSeedFieldNumber = 5;
    private readonly static uint HashSeedDefaultValue = 0;

    private uint hashSeed_;
    /// <summary>
    /// Seed used to hash the input values in the hash algorithm.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint HashSeed {
      get { if ((_hasBits0 & 8) != 0) { return hashSeed_; } else { return HashSeedDefaultValue; } }
      set {
        _hasBits0 |= 8;
        hashSeed_ = value;
      }
    }
    /// <summary>Gets whether the "hash_seed" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasHashSeed {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "hash_seed" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearHashSeed() {
      _hasBits0 &= ~8;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as BlockBloomFilterPB);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(BlockBloomFilterPB other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (LogSpaceBytes != other.LogSpaceBytes) return false;
      if (BloomData != other.BloomData) return false;
      if (AlwaysFalse != other.AlwaysFalse) return false;
      if (HashAlgorithm != other.HashAlgorithm) return false;
      if (HashSeed != other.HashSeed) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasLogSpaceBytes) hash ^= LogSpaceBytes.GetHashCode();
      if (HasBloomData) hash ^= BloomData.GetHashCode();
      if (HasAlwaysFalse) hash ^= AlwaysFalse.GetHashCode();
      if (HasHashAlgorithm) hash ^= HashAlgorithm.GetHashCode();
      if (HasHashSeed) hash ^= HashSeed.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      output.WriteRawMessage(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasLogSpaceBytes) {
        output.WriteRawTag(8);
        output.WriteInt32(LogSpaceBytes);
      }
      if (HasBloomData) {
        output.WriteRawTag(18);
        output.WriteBytes(BloomData);
      }
      if (HasAlwaysFalse) {
        output.WriteRawTag(24);
        output.WriteBool(AlwaysFalse);
      }
      if (HasHashAlgorithm) {
        output.WriteRawTag(32);
        output.WriteEnum((int) HashAlgorithm);
      }
      if (HasHashSeed) {
        output.WriteRawTag(40);
        output.WriteUInt32(HashSeed);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasLogSpaceBytes) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(LogSpaceBytes);
      }
      if (HasBloomData) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(BloomData);
      }
      if (HasAlwaysFalse) {
        size += 1 + 1;
      }
      if (HasHashAlgorithm) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) HashAlgorithm);
      }
      if (HasHashSeed) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(HashSeed);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(BlockBloomFilterPB other) {
      if (other == null) {
        return;
      }
      if (other.HasLogSpaceBytes) {
        LogSpaceBytes = other.LogSpaceBytes;
      }
      if (other.HasBloomData) {
        BloomData = other.BloomData;
      }
      if (other.HasAlwaysFalse) {
        AlwaysFalse = other.AlwaysFalse;
      }
      if (other.HasHashAlgorithm) {
        HashAlgorithm = other.HashAlgorithm;
      }
      if (other.HasHashSeed) {
        HashSeed = other.HashSeed;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      input.ReadRawMessage(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            LogSpaceBytes = input.ReadInt32();
            break;
          }
          case 18: {
            BloomData = input.ReadBytes();
            break;
          }
          case 24: {
            AlwaysFalse = input.ReadBool();
            break;
          }
          case 32: {
            HashAlgorithm = (global::Knet.Kudu.Client.Protobuf.HashAlgorithm) input.ReadEnum();
            break;
          }
          case 40: {
            HashSeed = input.ReadUInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
