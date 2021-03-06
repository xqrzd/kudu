using System.Buffers;
using Google.Protobuf;
using Knet.Kudu.Client.Protobuf.Master;

namespace Knet.Kudu.Client.Requests
{
    public class CreateTableRequest : KuduMasterRpc<CreateTableResponsePB>
    {
        private readonly CreateTableRequestPB _request;

        public override string MethodName => "CreateTable";

        public CreateTableRequest(CreateTableRequestPB request)
        {
            _request = request;

            // We don't need to set required feature ADD_DROP_RANGE_PARTITIONS here,
            // as it's supported in Kudu 1.3, the oldest version this client supports.
        }

        public override int CalculateSize() => _request.CalculateSize();

        public override void WriteTo(IBufferWriter<byte> output) => _request.WriteTo(output);

        public override void ParseProtobuf(ReadOnlySequence<byte> buffer)
        {
            Output = CreateTableResponsePB.Parser.ParseFrom(buffer);
            Error = Output.Error;
        }
    }
}
