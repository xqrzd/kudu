﻿using System.IO;
using Knet.Kudu.Client.Protocol.Tools;

namespace Knet.Kudu.Client.FunctionalTests.MiniCluster
{
    public class MiniKuduClusterBuilder
    {
        private readonly CreateClusterRequestPB _options;

        public MiniKuduClusterBuilder()
        {
            _options = new CreateClusterRequestPB();
        }

        /// <summary>
        /// Builds and starts a new <see cref="MiniKuduCluster"/>.
        /// </summary>
        public MiniKuduCluster Build()
        {
            if (string.IsNullOrWhiteSpace(_options.ClusterRoot))
            {
                _options.ClusterRoot = Path.Combine(
                    Path.GetTempPath(),
                    $"mini-kudu-cluster-{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}");
            }

            var miniCluster = new MiniKuduCluster(_options);
            miniCluster.Start();
            return miniCluster;
        }

        public MiniKuduClusterBuilder NumMasters(int numMasters)
        {
            _options.NumMasters = numMasters;
            return this;
        }

        public MiniKuduClusterBuilder NumTservers(int numTservers)
        {
            _options.NumTservers = numTservers;
            return this;
        }

        /// <summary>
        /// Adds a new flag to be passed to the Master daemons on start.
        /// </summary>
        /// <param name="flag">The flag to pass.</param>
        public MiniKuduClusterBuilder AddMasterServerFlag(string flag)
        {
            _options.ExtraMasterFlags.Add(flag);
            return this;
        }

        /// <summary>
        /// Adds a new flag to be passed to the Tablet Server daemons on start.
        /// </summary>
        /// <param name="flag">The flag to pass.</param>
        public MiniKuduClusterBuilder AddTabletServerFlag(string flag)
        {
            _options.ExtraTserverFlags.Add(flag);
            return this;
        }
    }
}
