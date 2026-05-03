<template>
  <div class="h-full w-full">
    <VueFlow
      v-model:nodes="nodes"
      :edges="edges"
      :node-types="nodeTypes"
      fit-view
      class="bg-zinc-950"
      @connect="handleConnect"
    />
  </div>
</template>

<script setup lang="ts">
import { markRaw } from "vue";
import {
  VueFlow,
  type Node,
  type Edge,
  type Connection,
  type NodeComponent,
} from "@vue-flow/core";

import ProjectFlowNode from "./ProjectFlowNode.vue";

const nodes = defineModel<Node<FlowNode>[]>("nodes", {
  default: [],
});

defineProps<{
  edges: Edge[];
}>();

const emit = defineEmits<{
  connect: [connection: Connection];
}>();

const nodeTypes = {
  visual: markRaw(ProjectFlowNode) as NodeComponent,
};

function handleConnect(connection: Connection) {
  emit("connect", connection);
}
</script>

<style>
@import "@vue-flow/core/dist/style.css";
@import "@vue-flow/core/dist/theme-default.css";
</style>
