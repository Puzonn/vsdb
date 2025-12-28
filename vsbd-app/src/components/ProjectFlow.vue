<template>
  <div class="h-full w-full">
    <VueFlow
      v-model:nodes="nodes"
      :edges="edges"
      :node-types="nodeTypes"
      fit-view
      class="bg-zinc-950"
    />
  </div>
</template>

<script setup lang="ts">
import { markRaw } from "vue";
import { VueFlow, type Node, type Edge, useVueFlow } from "@vue-flow/core";
import type { NodeComponent } from "@vue-flow/core";
import ProjectFlowNode from "./ProjectFlowNode.vue";

const nodes = defineModel<Node[]>("nodes");
const edges = defineModel<Edge[]>("edges");

const nodeTypes = {
  visual: markRaw(ProjectFlowNode) as NodeComponent,
};

const { onConnect } = useVueFlow();

onConnect((params) => {
  edges.value!.push({
    ...params,
    id: crypto.randomUUID(),
  });
});
</script>

<style>
@import "@vue-flow/core/dist/style.css";
@import "@vue-flow/core/dist/theme-default.css";
</style>
