// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: hash.proto

#pragma warning disable CS1591, CS0612, CS3021, IDE1006
namespace Knet.Kudu.Client.Protocol
{

    [global::ProtoBuf.ProtoContract()]
    public enum HashAlgorithm
    {
        [global::ProtoBuf.ProtoEnum(Name = @"UNKNOWN_HASH")]
        UnknownHash = 0,
        [global::ProtoBuf.ProtoEnum(Name = @"MURMUR_HASH_2")]
        MurmurHash2 = 1,
        [global::ProtoBuf.ProtoEnum(Name = @"CITY_HASH")]
        CityHash = 2,
    }

}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006