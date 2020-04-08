// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: rpc_introspection.proto

#pragma warning disable CS1591, CS0612, CS3021, IDE1006
namespace Knet.Kudu.Client.Protocol.Rpc
{

    [global::ProtoBuf.ProtoContract()]
    public partial class RpcCallInProgressPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"header", IsRequired = true)]
        public RequestHeader Header { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"trace_buffer")]
        [global::System.ComponentModel.DefaultValue("")]
        public string TraceBuffer
        {
            get { return __pbn__TraceBuffer ?? ""; }
            set { __pbn__TraceBuffer = value; }
        }
        public bool ShouldSerializeTraceBuffer() => __pbn__TraceBuffer != null;
        public void ResetTraceBuffer() => __pbn__TraceBuffer = null;
        private string __pbn__TraceBuffer;

        [global::ProtoBuf.ProtoMember(3, Name = @"micros_elapsed")]
        public ulong MicrosElapsed
        {
            get { return __pbn__MicrosElapsed.GetValueOrDefault(); }
            set { __pbn__MicrosElapsed = value; }
        }
        public bool ShouldSerializeMicrosElapsed() => __pbn__MicrosElapsed != null;
        public void ResetMicrosElapsed() => __pbn__MicrosElapsed = null;
        private ulong? __pbn__MicrosElapsed;

        [global::ProtoBuf.ProtoMember(4)]
        [global::System.ComponentModel.DefaultValue(State.Unknown)]
        public State state
        {
            get { return __pbn__state ?? State.Unknown; }
            set { __pbn__state = value; }
        }
        public bool ShouldSerializestate() => __pbn__state != null;
        public void Resetstate() => __pbn__state = null;
        private State? __pbn__state;

        [global::ProtoBuf.ProtoContract()]
        public enum State
        {
            [global::ProtoBuf.ProtoEnum(Name = @"UNKNOWN")]
            Unknown = 999,
            [global::ProtoBuf.ProtoEnum(Name = @"ON_OUTBOUND_QUEUE")]
            OnOutboundQueue = 1,
            [global::ProtoBuf.ProtoEnum(Name = @"SENDING")]
            Sending = 2,
            [global::ProtoBuf.ProtoEnum(Name = @"SENT")]
            Sent = 3,
            [global::ProtoBuf.ProtoEnum(Name = @"TIMED_OUT")]
            TimedOut = 4,
            [global::ProtoBuf.ProtoEnum(Name = @"FINISHED_ERROR")]
            FinishedError = 5,
            [global::ProtoBuf.ProtoEnum(Name = @"FINISHED_SUCCESS")]
            FinishedSuccess = 6,
            [global::ProtoBuf.ProtoEnum(Name = @"NEGOTIATION_TIMED_OUT")]
            NegotiationTimedOut = 7,
            [global::ProtoBuf.ProtoEnum(Name = @"FINISHED_NEGOTIATION_ERROR")]
            FinishedNegotiationError = 8,
            [global::ProtoBuf.ProtoEnum(Name = @"CANCELLED")]
            Cancelled = 9,
        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class SocketStatsPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"rtt")]
        public uint Rtt
        {
            get { return __pbn__Rtt.GetValueOrDefault(); }
            set { __pbn__Rtt = value; }
        }
        public bool ShouldSerializeRtt() => __pbn__Rtt != null;
        public void ResetRtt() => __pbn__Rtt = null;
        private uint? __pbn__Rtt;

        [global::ProtoBuf.ProtoMember(2, Name = @"rttvar")]
        public uint Rttvar
        {
            get { return __pbn__Rttvar.GetValueOrDefault(); }
            set { __pbn__Rttvar = value; }
        }
        public bool ShouldSerializeRttvar() => __pbn__Rttvar != null;
        public void ResetRttvar() => __pbn__Rttvar = null;
        private uint? __pbn__Rttvar;

        [global::ProtoBuf.ProtoMember(3, Name = @"snd_cwnd")]
        public uint SndCwnd
        {
            get { return __pbn__SndCwnd.GetValueOrDefault(); }
            set { __pbn__SndCwnd = value; }
        }
        public bool ShouldSerializeSndCwnd() => __pbn__SndCwnd != null;
        public void ResetSndCwnd() => __pbn__SndCwnd = null;
        private uint? __pbn__SndCwnd;

        [global::ProtoBuf.ProtoMember(4, Name = @"total_retrans")]
        public uint TotalRetrans
        {
            get { return __pbn__TotalRetrans.GetValueOrDefault(); }
            set { __pbn__TotalRetrans = value; }
        }
        public bool ShouldSerializeTotalRetrans() => __pbn__TotalRetrans != null;
        public void ResetTotalRetrans() => __pbn__TotalRetrans = null;
        private uint? __pbn__TotalRetrans;

        [global::ProtoBuf.ProtoMember(5, Name = @"pacing_rate")]
        public ulong PacingRate
        {
            get { return __pbn__PacingRate.GetValueOrDefault(); }
            set { __pbn__PacingRate = value; }
        }
        public bool ShouldSerializePacingRate() => __pbn__PacingRate != null;
        public void ResetPacingRate() => __pbn__PacingRate = null;
        private ulong? __pbn__PacingRate;

        [global::ProtoBuf.ProtoMember(6, Name = @"max_pacing_rate")]
        public ulong MaxPacingRate
        {
            get { return __pbn__MaxPacingRate.GetValueOrDefault(); }
            set { __pbn__MaxPacingRate = value; }
        }
        public bool ShouldSerializeMaxPacingRate() => __pbn__MaxPacingRate != null;
        public void ResetMaxPacingRate() => __pbn__MaxPacingRate = null;
        private ulong? __pbn__MaxPacingRate;

        [global::ProtoBuf.ProtoMember(7, Name = @"bytes_acked")]
        public ulong BytesAcked
        {
            get { return __pbn__BytesAcked.GetValueOrDefault(); }
            set { __pbn__BytesAcked = value; }
        }
        public bool ShouldSerializeBytesAcked() => __pbn__BytesAcked != null;
        public void ResetBytesAcked() => __pbn__BytesAcked = null;
        private ulong? __pbn__BytesAcked;

        [global::ProtoBuf.ProtoMember(8, Name = @"bytes_received")]
        public ulong BytesReceived
        {
            get { return __pbn__BytesReceived.GetValueOrDefault(); }
            set { __pbn__BytesReceived = value; }
        }
        public bool ShouldSerializeBytesReceived() => __pbn__BytesReceived != null;
        public void ResetBytesReceived() => __pbn__BytesReceived = null;
        private ulong? __pbn__BytesReceived;

        [global::ProtoBuf.ProtoMember(9, Name = @"segs_out")]
        public uint SegsOut
        {
            get { return __pbn__SegsOut.GetValueOrDefault(); }
            set { __pbn__SegsOut = value; }
        }
        public bool ShouldSerializeSegsOut() => __pbn__SegsOut != null;
        public void ResetSegsOut() => __pbn__SegsOut = null;
        private uint? __pbn__SegsOut;

        [global::ProtoBuf.ProtoMember(10, Name = @"segs_in")]
        public uint SegsIn
        {
            get { return __pbn__SegsIn.GetValueOrDefault(); }
            set { __pbn__SegsIn = value; }
        }
        public bool ShouldSerializeSegsIn() => __pbn__SegsIn != null;
        public void ResetSegsIn() => __pbn__SegsIn = null;
        private uint? __pbn__SegsIn;

        [global::ProtoBuf.ProtoMember(11, Name = @"send_queue_bytes")]
        public ulong SendQueueBytes
        {
            get { return __pbn__SendQueueBytes.GetValueOrDefault(); }
            set { __pbn__SendQueueBytes = value; }
        }
        public bool ShouldSerializeSendQueueBytes() => __pbn__SendQueueBytes != null;
        public void ResetSendQueueBytes() => __pbn__SendQueueBytes = null;
        private ulong? __pbn__SendQueueBytes;

        [global::ProtoBuf.ProtoMember(12, Name = @"receive_queue_bytes")]
        public ulong ReceiveQueueBytes
        {
            get { return __pbn__ReceiveQueueBytes.GetValueOrDefault(); }
            set { __pbn__ReceiveQueueBytes = value; }
        }
        public bool ShouldSerializeReceiveQueueBytes() => __pbn__ReceiveQueueBytes != null;
        public void ResetReceiveQueueBytes() => __pbn__ReceiveQueueBytes = null;
        private ulong? __pbn__ReceiveQueueBytes;

        [global::ProtoBuf.ProtoMember(13, Name = @"send_bytes_per_sec")]
        public long SendBytesPerSec
        {
            get { return __pbn__SendBytesPerSec.GetValueOrDefault(); }
            set { __pbn__SendBytesPerSec = value; }
        }
        public bool ShouldSerializeSendBytesPerSec() => __pbn__SendBytesPerSec != null;
        public void ResetSendBytesPerSec() => __pbn__SendBytesPerSec = null;
        private long? __pbn__SendBytesPerSec;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RpcConnectionPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"remote_ip", IsRequired = true)]
        public string RemoteIp { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"state", IsRequired = true)]
        public StateType State { get; set; } = StateType.Unknown;

        [global::ProtoBuf.ProtoMember(3, Name = @"remote_user_credentials")]
        [global::System.ComponentModel.DefaultValue("")]
        public string RemoteUserCredentials
        {
            get { return __pbn__RemoteUserCredentials ?? ""; }
            set { __pbn__RemoteUserCredentials = value; }
        }
        public bool ShouldSerializeRemoteUserCredentials() => __pbn__RemoteUserCredentials != null;
        public void ResetRemoteUserCredentials() => __pbn__RemoteUserCredentials = null;
        private string __pbn__RemoteUserCredentials;

        [global::ProtoBuf.ProtoMember(4, Name = @"calls_in_flight")]
        public global::System.Collections.Generic.List<RpcCallInProgressPB> CallsInFlights { get; } = new global::System.Collections.Generic.List<RpcCallInProgressPB>();

        [global::ProtoBuf.ProtoMember(5, Name = @"outbound_queue_size")]
        public long OutboundQueueSize
        {
            get { return __pbn__OutboundQueueSize.GetValueOrDefault(); }
            set { __pbn__OutboundQueueSize = value; }
        }
        public bool ShouldSerializeOutboundQueueSize() => __pbn__OutboundQueueSize != null;
        public void ResetOutboundQueueSize() => __pbn__OutboundQueueSize = null;
        private long? __pbn__OutboundQueueSize;

        [global::ProtoBuf.ProtoMember(6, Name = @"socket_stats")]
        public SocketStatsPB SocketStats { get; set; }

        [global::ProtoBuf.ProtoContract()]
        public enum StateType
        {
            [global::ProtoBuf.ProtoEnum(Name = @"UNKNOWN")]
            Unknown = 999,
            [global::ProtoBuf.ProtoEnum(Name = @"NEGOTIATING")]
            Negotiating = 0,
            [global::ProtoBuf.ProtoEnum(Name = @"OPEN")]
            Open = 1,
        }

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class DumpConnectionsRequestPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"include_traces")]
        [global::System.ComponentModel.DefaultValue(false)]
        public bool IncludeTraces
        {
            get { return __pbn__IncludeTraces ?? false; }
            set { __pbn__IncludeTraces = value; }
        }
        public bool ShouldSerializeIncludeTraces() => __pbn__IncludeTraces != null;
        public void ResetIncludeTraces() => __pbn__IncludeTraces = null;
        private bool? __pbn__IncludeTraces;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class DumpConnectionsResponsePB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"inbound_connections")]
        public global::System.Collections.Generic.List<RpcConnectionPB> InboundConnections { get; } = new global::System.Collections.Generic.List<RpcConnectionPB>();

        [global::ProtoBuf.ProtoMember(2, Name = @"outbound_connections")]
        public global::System.Collections.Generic.List<RpcConnectionPB> OutboundConnections { get; } = new global::System.Collections.Generic.List<RpcConnectionPB>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class TraceMetricPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"child_path")]
        [global::System.ComponentModel.DefaultValue("")]
        public string ChildPath
        {
            get { return __pbn__ChildPath ?? ""; }
            set { __pbn__ChildPath = value; }
        }
        public bool ShouldSerializeChildPath() => __pbn__ChildPath != null;
        public void ResetChildPath() => __pbn__ChildPath = null;
        private string __pbn__ChildPath;

        [global::ProtoBuf.ProtoMember(2, Name = @"key")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Key
        {
            get { return __pbn__Key ?? ""; }
            set { __pbn__Key = value; }
        }
        public bool ShouldSerializeKey() => __pbn__Key != null;
        public void ResetKey() => __pbn__Key = null;
        private string __pbn__Key;

        [global::ProtoBuf.ProtoMember(3, Name = @"value")]
        public long Value
        {
            get { return __pbn__Value.GetValueOrDefault(); }
            set { __pbn__Value = value; }
        }
        public bool ShouldSerializeValue() => __pbn__Value != null;
        public void ResetValue() => __pbn__Value = null;
        private long? __pbn__Value;

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RpczSamplePB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"header")]
        public RequestHeader Header { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"trace")]
        [global::System.ComponentModel.DefaultValue("")]
        public string Trace
        {
            get { return __pbn__Trace ?? ""; }
            set { __pbn__Trace = value; }
        }
        public bool ShouldSerializeTrace() => __pbn__Trace != null;
        public void ResetTrace() => __pbn__Trace = null;
        private string __pbn__Trace;

        [global::ProtoBuf.ProtoMember(3, Name = @"duration_ms")]
        public int DurationMs
        {
            get { return __pbn__DurationMs.GetValueOrDefault(); }
            set { __pbn__DurationMs = value; }
        }
        public bool ShouldSerializeDurationMs() => __pbn__DurationMs != null;
        public void ResetDurationMs() => __pbn__DurationMs = null;
        private int? __pbn__DurationMs;

        [global::ProtoBuf.ProtoMember(4, Name = @"metrics")]
        public global::System.Collections.Generic.List<TraceMetricPB> Metrics { get; } = new global::System.Collections.Generic.List<TraceMetricPB>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class RpczMethodPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"method_name", IsRequired = true)]
        public string MethodName { get; set; }

        [global::ProtoBuf.ProtoMember(2, Name = @"samples")]
        public global::System.Collections.Generic.List<RpczSamplePB> Samples { get; } = new global::System.Collections.Generic.List<RpczSamplePB>();

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class DumpRpczStoreRequestPB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

    }

    [global::ProtoBuf.ProtoContract()]
    public partial class DumpRpczStoreResponsePB : global::ProtoBuf.IExtensible
    {
        private global::ProtoBuf.IExtension __pbn__extensionData;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => global::ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [global::ProtoBuf.ProtoMember(1, Name = @"methods")]
        public global::System.Collections.Generic.List<RpczMethodPB> Methods { get; } = new global::System.Collections.Generic.List<RpczMethodPB>();

    }

}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006
