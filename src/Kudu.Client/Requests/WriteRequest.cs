﻿using System.Buffers;
using System.IO;
using Kudu.Client.Protocol.Tserver;

namespace Kudu.Client.Requests
{
    public class WriteRequest : KuduTabletRpc<WriteResponsePB>
    {
        private readonly WriteRequestPB _request;

        public override string MethodName => "Write";

        public WriteRequest(WriteRequestPB request, string tableId, byte[] partitionKey)
        {
            _request = request;
            TableId = tableId;
            PartitionKey = partitionKey;
        }

        public override void Serialize(Stream stream)
        {
            Serialize(stream, _request);
        }

        public override void ParseProtobuf(ReadOnlySequence<byte> buffer)
        {
            Output = Deserialize(buffer);
            Error = Output.Error;
        }
    }
}
