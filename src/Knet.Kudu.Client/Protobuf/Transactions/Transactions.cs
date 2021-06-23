// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: kudu/transactions/transactions.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Knet.Kudu.Client.Protobuf.Transactions {

  /// <summary>Holder for reflection information generated from kudu/transactions/transactions.proto</summary>
  public static partial class TransactionsReflection {

    #region Descriptor
    /// <summary>File descriptor for kudu/transactions/transactions.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TransactionsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiRrdWR1L3RyYW5zYWN0aW9ucy90cmFuc2FjdGlvbnMucHJvdG8SEWt1ZHUu",
            "dHJhbnNhY3Rpb25zIqQBChBUeG5TdGF0dXNFbnRyeVBCEiwKBXN0YXRlGAEg",
            "ASgOMh0ua3VkdS50cmFuc2FjdGlvbnMuVHhuU3RhdGVQQhIMCgR1c2VyGAIg",
            "ASgJEhgKEGNvbW1pdF90aW1lc3RhbXAYAyABKAYSFwoPc3RhcnRfdGltZXN0",
            "YW1wGAQgASgDEiEKGWxhc3RfdHJhbnNpdGlvbl90aW1lc3RhbXAYBSABKAMi",
            "RQoVVHhuUGFydGljaXBhbnRFbnRyeVBCEiwKBXN0YXRlGAEgASgOMh0ua3Vk",
            "dS50cmFuc2FjdGlvbnMuVHhuU3RhdGVQQiJQCgpUeG5Ub2tlblBCEg4KBnR4",
            "bl9pZBgBIAEoAxIYChBrZWVwYWxpdmVfbWlsbGlzGAIgASgNEhgKEGVuYWJs",
            "ZV9rZWVwYWxpdmUYAyABKAgqiAEKClR4blN0YXRlUEISCwoHVU5LTk9XThAA",
            "EggKBE9QRU4QARIVChFBQk9SVF9JTl9QUk9HUkVTUxAFEgsKB0FCT1JURUQQ",
            "AhIWChJDT01NSVRfSU5fUFJPR1JFU1MQAxIYChRGSU5BTElaRV9JTl9QUk9H",
            "UkVTUxAGEg0KCUNPTU1JVFRFRBAEQkcKHG9yZy5hcGFjaGUua3VkdS50cmFu",
            "c2FjdGlvbnOqAiZLbmV0Lkt1ZHUuQ2xpZW50LlByb3RvYnVmLlRyYW5zYWN0",
            "aW9ucw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatusEntryPB), global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatusEntryPB.Parser, new[]{ "State", "User", "CommitTimestamp", "StartTimestamp", "LastTransitionTimestamp" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Knet.Kudu.Client.Protobuf.Transactions.TxnParticipantEntryPB), global::Knet.Kudu.Client.Protobuf.Transactions.TxnParticipantEntryPB.Parser, new[]{ "State" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Knet.Kudu.Client.Protobuf.Transactions.TxnTokenPB), global::Knet.Kudu.Client.Protobuf.Transactions.TxnTokenPB.Parser, new[]{ "TxnId", "KeepaliveMillis", "EnableKeepalive" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  /// <summary>
  /// The following state changes are expected:
  ///
  ///     BeginCommit           FinalizeCommit        CompleteCommit
  ///   OPEN --> COMMIT_IN_PROGRESS --> FINALIZE_IN_PROGRESS --> COMMITTED
  ///
  ///     BeginCommit           BeginAbort            FinalizeAbort
  ///   OPEN --> COMMIT_IN_PROGRESS --> ABORT_IN_PROGRESS --> ABORTED
  ///
  ///     AbortTxn              FinalizeAbort
  ///   OPEN --> ABORT_IN_PROGRESS --> ABORTED
  /// </summary>
  public enum TxnStatePB {
    [pbr::OriginalName("UNKNOWN")] Unknown = 0,
    /// <summary>
    /// The transaction is open. Users can write to participants, and register new
    /// participants.
    /// </summary>
    [pbr::OriginalName("OPEN")] Open = 1,
    /// <summary>
    /// A user or Kudu's transaction staleness tracker has signaled that the
    /// transaction should be aborted. No further participants can be registered,
    /// and the transaction can only be moved to the ABORTED state after sending
    /// ABORT_TXN ops to all participants.
    /// </summary>
    [pbr::OriginalName("ABORT_IN_PROGRESS")] AbortInProgress = 5,
    /// <summary>
    /// The transaction has been fully aborted -- all participants have
    /// successfully replicated ABORT_TXN ops and cleared the transaction. No
    /// further tasks are required to abort the transaction.
    /// </summary>
    [pbr::OriginalName("ABORTED")] Aborted = 2,
    /// <summary>
    /// The user has signaled that the transaction should be committed. No further
    /// participants can be registered. The transaction may still be aborted if
    /// prompted by a user or if sending any BEGIN_COMMIT op fails.
    /// </summary>
    [pbr::OriginalName("COMMIT_IN_PROGRESS")] CommitInProgress = 3,
    /// <summary>
    /// Kudu has successfully sent BEGIN_COMMIT ops to all participants, and has
    /// started sending FINALIZE_COMMIT ops to participants. The transaction can
    /// only be moved to the COMMITTED state.
    /// </summary>
    [pbr::OriginalName("FINALIZE_IN_PROGRESS")] FinalizeInProgress = 6,
    /// <summary>
    /// All FINALIZE_COMMIT ops have succeeded. No further tasks are required to
    /// commit the transaction.
    /// </summary>
    [pbr::OriginalName("COMMITTED")] Committed = 4,
  }

  #endregion

  #region Messages
  /// <summary>
  /// Metadata encapsulating the status of a transaction.
  /// </summary>
  public sealed partial class TxnStatusEntryPB : pb::IMessage<TxnStatusEntryPB>
      , pb::IBufferMessage
  {
    private static readonly pb::MessageParser<TxnStatusEntryPB> _parser = new pb::MessageParser<TxnStatusEntryPB>(() => new TxnStatusEntryPB());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TxnStatusEntryPB> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Knet.Kudu.Client.Protobuf.Transactions.TransactionsReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnStatusEntryPB() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnStatusEntryPB(TxnStatusEntryPB other) : this() {
      _hasBits0 = other._hasBits0;
      state_ = other.state_;
      user_ = other.user_;
      commitTimestamp_ = other.commitTimestamp_;
      startTimestamp_ = other.startTimestamp_;
      lastTransitionTimestamp_ = other.lastTransitionTimestamp_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnStatusEntryPB Clone() {
      return new TxnStatusEntryPB(this);
    }

    /// <summary>Field number for the "state" field.</summary>
    public const int StateFieldNumber = 1;
    private readonly static global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB StateDefaultValue = global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB.Unknown;

    private global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB state_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB State {
      get { if ((_hasBits0 & 1) != 0) { return state_; } else { return StateDefaultValue; } }
      set {
        _hasBits0 |= 1;
        state_ = value;
      }
    }
    /// <summary>Gets whether the "state" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasState {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "state" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearState() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "user" field.</summary>
    public const int UserFieldNumber = 2;
    private readonly static string UserDefaultValue = "";

    private string user_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string User {
      get { return user_ ?? UserDefaultValue; }
      set {
        user_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "user" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasUser {
      get { return user_ != null; }
    }
    /// <summary>Clears the value of the "user" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearUser() {
      user_ = null;
    }

    /// <summary>Field number for the "commit_timestamp" field.</summary>
    public const int CommitTimestampFieldNumber = 3;
    private readonly static ulong CommitTimestampDefaultValue = 0UL;

    private ulong commitTimestamp_;
    /// <summary>
    /// Commit timestamp associated with this transaction.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public ulong CommitTimestamp {
      get { if ((_hasBits0 & 2) != 0) { return commitTimestamp_; } else { return CommitTimestampDefaultValue; } }
      set {
        _hasBits0 |= 2;
        commitTimestamp_ = value;
      }
    }
    /// <summary>Gets whether the "commit_timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasCommitTimestamp {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "commit_timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearCommitTimestamp() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "start_timestamp" field.</summary>
    public const int StartTimestampFieldNumber = 4;
    private readonly static long StartTimestampDefaultValue = 0L;

    private long startTimestamp_;
    /// <summary>
    /// The timestamp in seconds since the epoch that this transaction was
    /// started.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long StartTimestamp {
      get { if ((_hasBits0 & 4) != 0) { return startTimestamp_; } else { return StartTimestampDefaultValue; } }
      set {
        _hasBits0 |= 4;
        startTimestamp_ = value;
      }
    }
    /// <summary>Gets whether the "start_timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasStartTimestamp {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "start_timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearStartTimestamp() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "last_transition_timestamp" field.</summary>
    public const int LastTransitionTimestampFieldNumber = 5;
    private readonly static long LastTransitionTimestampDefaultValue = 0L;

    private long lastTransitionTimestamp_;
    /// <summary>
    /// The timestamp in seconds since the epoch that this transaction had a state
    /// change.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long LastTransitionTimestamp {
      get { if ((_hasBits0 & 8) != 0) { return lastTransitionTimestamp_; } else { return LastTransitionTimestampDefaultValue; } }
      set {
        _hasBits0 |= 8;
        lastTransitionTimestamp_ = value;
      }
    }
    /// <summary>Gets whether the "last_transition_timestamp" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasLastTransitionTimestamp {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "last_transition_timestamp" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearLastTransitionTimestamp() {
      _hasBits0 &= ~8;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TxnStatusEntryPB);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TxnStatusEntryPB other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (State != other.State) return false;
      if (User != other.User) return false;
      if (CommitTimestamp != other.CommitTimestamp) return false;
      if (StartTimestamp != other.StartTimestamp) return false;
      if (LastTransitionTimestamp != other.LastTransitionTimestamp) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasState) hash ^= State.GetHashCode();
      if (HasUser) hash ^= User.GetHashCode();
      if (HasCommitTimestamp) hash ^= CommitTimestamp.GetHashCode();
      if (HasStartTimestamp) hash ^= StartTimestamp.GetHashCode();
      if (HasLastTransitionTimestamp) hash ^= LastTransitionTimestamp.GetHashCode();
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
      if (HasState) {
        output.WriteRawTag(8);
        output.WriteEnum((int) State);
      }
      if (HasUser) {
        output.WriteRawTag(18);
        output.WriteString(User);
      }
      if (HasCommitTimestamp) {
        output.WriteRawTag(25);
        output.WriteFixed64(CommitTimestamp);
      }
      if (HasStartTimestamp) {
        output.WriteRawTag(32);
        output.WriteInt64(StartTimestamp);
      }
      if (HasLastTransitionTimestamp) {
        output.WriteRawTag(40);
        output.WriteInt64(LastTransitionTimestamp);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasState) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) State);
      }
      if (HasUser) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(User);
      }
      if (HasCommitTimestamp) {
        size += 1 + 8;
      }
      if (HasStartTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(StartTimestamp);
      }
      if (HasLastTransitionTimestamp) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(LastTransitionTimestamp);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TxnStatusEntryPB other) {
      if (other == null) {
        return;
      }
      if (other.HasState) {
        State = other.State;
      }
      if (other.HasUser) {
        User = other.User;
      }
      if (other.HasCommitTimestamp) {
        CommitTimestamp = other.CommitTimestamp;
      }
      if (other.HasStartTimestamp) {
        StartTimestamp = other.StartTimestamp;
      }
      if (other.HasLastTransitionTimestamp) {
        LastTransitionTimestamp = other.LastTransitionTimestamp;
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
            State = (global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB) input.ReadEnum();
            break;
          }
          case 18: {
            User = input.ReadString();
            break;
          }
          case 25: {
            CommitTimestamp = input.ReadFixed64();
            break;
          }
          case 32: {
            StartTimestamp = input.ReadInt64();
            break;
          }
          case 40: {
            LastTransitionTimestamp = input.ReadInt64();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// Metadata encapsulating the existence of a transaction participant.
  /// </summary>
  public sealed partial class TxnParticipantEntryPB : pb::IMessage<TxnParticipantEntryPB>
      , pb::IBufferMessage
  {
    private static readonly pb::MessageParser<TxnParticipantEntryPB> _parser = new pb::MessageParser<TxnParticipantEntryPB>(() => new TxnParticipantEntryPB());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TxnParticipantEntryPB> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Knet.Kudu.Client.Protobuf.Transactions.TransactionsReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnParticipantEntryPB() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnParticipantEntryPB(TxnParticipantEntryPB other) : this() {
      _hasBits0 = other._hasBits0;
      state_ = other.state_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnParticipantEntryPB Clone() {
      return new TxnParticipantEntryPB(this);
    }

    /// <summary>Field number for the "state" field.</summary>
    public const int StateFieldNumber = 1;
    private readonly static global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB StateDefaultValue = global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB.Unknown;

    private global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB state_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB State {
      get { if ((_hasBits0 & 1) != 0) { return state_; } else { return StateDefaultValue; } }
      set {
        _hasBits0 |= 1;
        state_ = value;
      }
    }
    /// <summary>Gets whether the "state" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasState {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "state" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearState() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TxnParticipantEntryPB);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TxnParticipantEntryPB other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (State != other.State) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasState) hash ^= State.GetHashCode();
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
      if (HasState) {
        output.WriteRawTag(8);
        output.WriteEnum((int) State);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasState) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) State);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TxnParticipantEntryPB other) {
      if (other == null) {
        return;
      }
      if (other.HasState) {
        State = other.State;
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
            State = (global::Knet.Kudu.Client.Protobuf.Transactions.TxnStatePB) input.ReadEnum();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  /// High-level metadata about a transaction. Serialized messages of this type
  /// are used to pass information on an already opened transaction among multiple
  /// Kudu clients (even if residing on different nodes), so they can issue write
  /// operations in the context of the same multi-row distributed transaction.
  /// </summary>
  public sealed partial class TxnTokenPB : pb::IMessage<TxnTokenPB>
      , pb::IBufferMessage
  {
    private static readonly pb::MessageParser<TxnTokenPB> _parser = new pb::MessageParser<TxnTokenPB>(() => new TxnTokenPB());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TxnTokenPB> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Knet.Kudu.Client.Protobuf.Transactions.TransactionsReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnTokenPB() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnTokenPB(TxnTokenPB other) : this() {
      _hasBits0 = other._hasBits0;
      txnId_ = other.txnId_;
      keepaliveMillis_ = other.keepaliveMillis_;
      enableKeepalive_ = other.enableKeepalive_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TxnTokenPB Clone() {
      return new TxnTokenPB(this);
    }

    /// <summary>Field number for the "txn_id" field.</summary>
    public const int TxnIdFieldNumber = 1;
    private readonly static long TxnIdDefaultValue = 0L;

    private long txnId_;
    /// <summary>
    /// Transaction identifier assigned to the transaction.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long TxnId {
      get { if ((_hasBits0 & 1) != 0) { return txnId_; } else { return TxnIdDefaultValue; } }
      set {
        _hasBits0 |= 1;
        txnId_ = value;
      }
    }
    /// <summary>Gets whether the "txn_id" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasTxnId {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "txn_id" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearTxnId() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "keepalive_millis" field.</summary>
    public const int KeepaliveMillisFieldNumber = 2;
    private readonly static uint KeepaliveMillisDefaultValue = 0;

    private uint keepaliveMillis_;
    /// <summary>
    /// The keep-alive interval (in milliseconds) to keep the transaction alive.
    /// To avoid auto-aborting the transaction, TxnManager should receive
    /// keep-alive heartbeats spaced by intervals equal or shorter than
    /// 'keepalive_millis' milliseconds in duration.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint KeepaliveMillis {
      get { if ((_hasBits0 & 2) != 0) { return keepaliveMillis_; } else { return KeepaliveMillisDefaultValue; } }
      set {
        _hasBits0 |= 2;
        keepaliveMillis_ = value;
      }
    }
    /// <summary>Gets whether the "keepalive_millis" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasKeepaliveMillis {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "keepalive_millis" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearKeepaliveMillis() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "enable_keepalive" field.</summary>
    public const int EnableKeepaliveFieldNumber = 3;
    private readonly static bool EnableKeepaliveDefaultValue = false;

    private bool enableKeepalive_;
    /// <summary>
    /// Whether the client should automatically send keepalive messages once
    /// this token is deserialized into a runtime transaction handle.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool EnableKeepalive {
      get { if ((_hasBits0 & 4) != 0) { return enableKeepalive_; } else { return EnableKeepaliveDefaultValue; } }
      set {
        _hasBits0 |= 4;
        enableKeepalive_ = value;
      }
    }
    /// <summary>Gets whether the "enable_keepalive" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool HasEnableKeepalive {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "enable_keepalive" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void ClearEnableKeepalive() {
      _hasBits0 &= ~4;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TxnTokenPB);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TxnTokenPB other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (TxnId != other.TxnId) return false;
      if (KeepaliveMillis != other.KeepaliveMillis) return false;
      if (EnableKeepalive != other.EnableKeepalive) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (HasTxnId) hash ^= TxnId.GetHashCode();
      if (HasKeepaliveMillis) hash ^= KeepaliveMillis.GetHashCode();
      if (HasEnableKeepalive) hash ^= EnableKeepalive.GetHashCode();
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
      if (HasTxnId) {
        output.WriteRawTag(8);
        output.WriteInt64(TxnId);
      }
      if (HasKeepaliveMillis) {
        output.WriteRawTag(16);
        output.WriteUInt32(KeepaliveMillis);
      }
      if (HasEnableKeepalive) {
        output.WriteRawTag(24);
        output.WriteBool(EnableKeepalive);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (HasTxnId) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(TxnId);
      }
      if (HasKeepaliveMillis) {
        size += 1 + pb::CodedOutputStream.ComputeUInt32Size(KeepaliveMillis);
      }
      if (HasEnableKeepalive) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TxnTokenPB other) {
      if (other == null) {
        return;
      }
      if (other.HasTxnId) {
        TxnId = other.TxnId;
      }
      if (other.HasKeepaliveMillis) {
        KeepaliveMillis = other.KeepaliveMillis;
      }
      if (other.HasEnableKeepalive) {
        EnableKeepalive = other.EnableKeepalive;
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
            TxnId = input.ReadInt64();
            break;
          }
          case 16: {
            KeepaliveMillis = input.ReadUInt32();
            break;
          }
          case 24: {
            EnableKeepalive = input.ReadBool();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
