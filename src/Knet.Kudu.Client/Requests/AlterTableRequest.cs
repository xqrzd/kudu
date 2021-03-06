using System.Buffers;
using Google.Protobuf;
using Knet.Kudu.Client.Protobuf.Master;

namespace Knet.Kudu.Client.Requests
{
    public class AlterTableRequest : KuduMasterRpc<AlterTableResponse>
    {
        private readonly AlterTableRequestPB _request;

        public override string MethodName => "AlterTable";

        public AlterTableRequest(AlterTableRequestPB request)
        {
            _request = request;

            // We don't need to set required feature ADD_DROP_RANGE_PARTITIONS here,
            // as it's supported in Kudu 1.3, the oldest version this client supports.
        }

        public override int CalculateSize() => _request.CalculateSize();

        public override void WriteTo(IBufferWriter<byte> output) => _request.WriteTo(output);

        public override void ParseProtobuf(ReadOnlySequence<byte> buffer)
        {
            var responsePb = AlterTableResponsePB.Parser.ParseFrom(buffer);

            if (responsePb.Error is null)
            {
                Output = new AlterTableResponse(
                    responsePb.TableId.ToStringUtf8(),
                    responsePb.SchemaVersion);
            }
            else
            {
                Error = responsePb.Error;
            }
        }
    }
}
