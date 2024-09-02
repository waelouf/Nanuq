<template>
    <v-card>
      <v-tabs
        v-model="tab"
        bg-color="primary"
      >
        <v-tab value="server">Server</v-tab>
        <v-tab value="replication">Replication</v-tab>
        <v-tab value="databases">Databases</v-tab>
        <v-tab value="stats">Stats</v-tab>
      </v-tabs>

      <v-card-text>
        <v-tabs-window v-model="tab">
          <v-tabs-window-item value="server">
            <v-row>
            <v-card
                class="mx-auto"
                max-width="344"
                hover
            >
                <v-card-item>
                <v-card-title>
                    Redis Server details
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <p>
                        <b>Redis Version:</b> {{ serverDetails.server.redisVersion }}
                    </p>
                    <p>
                        <b>OS:</b> {{ serverDetails.server.os }}
                    </p>
                    <p>
                        <b>Uptime:</b> {{ serverDetails.server.uptimeInDays }} Days
                    </p>
                </v-card-text>
            </v-card>
            <v-card
                class="mx-auto"
                max-width="344"
                hover >
                <v-card-item>
                <v-card-title>
                    Clients
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <p>
                        <b>Connected clients:</b> {{   serverDetails.clients.connectedClients }}
                    </p>
                    <p>
                        <b>Blocked clients:</b> {{ serverDetails.clients.blockedClients }}
                    </p>
                </v-card-text>
            </v-card>
            <v-card
                class="mx-auto"
                max-width="344"
                hover >
                <v-card-item>
                <v-card-title>
                    Memory
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <p>
                        <b>System memory:</b> {{ serverDetails.memory.totalSystemMemoryHuman }}
                    </p>
                    <p>
                        <b>Used memory:</b> {{ serverDetails.memory.usedMemoryHuman }}
                    </p>
                </v-card-text>
            </v-card>
            <v-card
                    class="mx-auto"
                    max-width="344"
                    hover >
                    <v-card-item>
                    <v-card-title>
                        CPU
                    </v-card-title>
                    </v-card-item>
                    <v-card-text>
                        <p>
                            <b>CPU usage:</b> {{ (serverDetails.cpu.usedCpuUser / serverDetails.cpu.usedCpuSys) * 100 }} %
                        </p>
                    </v-card-text>
            </v-card>
        </v-row>
        </v-tabs-window-item>

          <v-tabs-window-item value="replication">
            <v-card
                class="mx-auto"
                max-width="344"
                hover >
                <v-card-item>
                <v-card-title>
                    Replication details
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <p>
                        <b>Role:</b> {{ serverDetails.replication.role }}
                    </p>
                    <p>
                        <b>Connected slaves:</b> {{ serverDetails.replication.connectedSlaves }}
                    </p>
                    <p>
                        <b>Master Failover State:</b> {{ serverDetails.replication.masterFailoverState }}
                    </p>
                </v-card-text>
            </v-card>
          </v-tabs-window-item>

          <v-tabs-window-item value="databases">
            <v-card
                class="mx-auto"
                max-width="344"
                hover >
                <v-card-item>
                <v-card-title>
                    Databases
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <v-table v-if="serverDetails.databases">
                        <thead>
                          <tr>
                            <th class="text-left">
                                Database
                            </th>
                            <th class="text-left">
                                Keys
                            </th>
                            <th class="text-left">
                                Expires
                            </th>
                            <th class="text-left">
                                Average TTL
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr
                            v-for="db in serverDetails.databases"
                            :key="db.database"
                          >
                            <td>{{ db.database }}</td>
                            <td>{{ db.keys }}</td>
                            <td>{{ db.expires }}</td>
                            <td>{{ db.avgTtl }}</td>
                          </tr>
                        </tbody>
                      </v-table>
                </v-card-text>
            </v-card>
          </v-tabs-window-item>

          <v-tabs-window-item value="stats">
            <v-card
                class="mx-auto"
                max-width="344"
                hover >
                <v-card-item>
                <v-card-title>
                    Stats
                </v-card-title>
                </v-card-item>
                <v-card-text>
                    <p>
                        <b>Total Connections Received:</b> {{ serverDetails.stats.totalConnectionsReceived }}
                    </p>
                    <p>
                        <b>Total Commands Processed:</b> {{ serverDetails.stats.totalCommandsProcessed }}
                    </p>
                    <p>
                        <b>Total Net Input:</b> {{ serverDetails.stats.totalNetInputBytes }} Bytes
                    </p>
                    <p>
                        <b>Total Net Output:</b> {{ serverDetails.stats.totalNetOutputBytes }} Bytes
                    </p>
                    <p>
                        <b>Keyspace Hits:</b> {{ serverDetails.stats.keyspaceHits }}
                    </p>
                    <p>
                        <b>Keyspace Misses:</b> {{ serverDetails.stats.keyspaceMisses }}
                    </p>
                    <p>
                        <b>Pub/Sub Channels:</b> {{ serverDetails.stats.pubsubChannels }}
                    </p>
                    <p>
                        <b>Pub/Sub Shared Channels:</b> {{ serverDetails.stats.pubsubshardChannels }}
                    </p>
                    <p>
                        <b>Total Reads Processed:</b> {{ serverDetails.stats.totalReadsProcessed }}
                    </p>
                    <p>
                        <b>Total Writes Processed:</b> {{ serverDetails.stats.totalWritesProcessed }}
                    </p>
                    <p>
                        <b>Total Error Replies:</b> {{ serverDetails.stats.totalErrorReplies }}
                    </p>
                </v-card-text>
            </v-card>
          </v-tabs-window-item>
        </v-tabs-window>
        <v-btn class="mt-2" @click="handleClose" type="submit" block>Close</v-btn>
      </v-card-text>
    </v-card>
  </template>
<script>
export default {
  name: 'ViewServerFullDetails',
  data: () => ({
    tab: null,
  }),
  computed: {
    serverDetails() {
      console.log(this.$store.state.redis.redisServers);
      console.log(this.$store.state.redis.redisServers[this.serverUrl]);
      return this.$store.state.redis.redisServers[this.serverUrl];
    },
  },
  props: {
    serverUrl: {
      Type: String,
      Required: true,
    },
  },
  methods: {
    handleClose() {
      this.$emit('showServerDetails', false);
    },
  },
};
</script>
