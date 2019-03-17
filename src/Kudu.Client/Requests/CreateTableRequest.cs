﻿using Kudu.Client.Protocol.Master;

namespace Kudu.Client.Requests
{
    public class CreateTableRequest : KuduRpc<CreateTableRequestPB, CreateTableResponsePB>
    {
        public override string ServiceName => MasterServiceName;

        public override string MethodName => "CreateTable";

        public CreateTableRequest(CreateTableRequestPB request)
        {
            Request = request;
        }
    }
}
