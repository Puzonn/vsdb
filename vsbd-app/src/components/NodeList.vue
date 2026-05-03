<template>
  <ul class="bg-zinc-950 rounded-lg overflow-scroll w-64 shrink-0">
    <div class="flex justify-center font-semibold p-3">
      <p>Nodes</p>
    </div>

    <li v-for="node in nodes" class="px-4 py-2 text-center">
      <button
        @click="onNodeClick(node)"
        :class="
          selectedNode?.name === node.name
            ? 'bg-zinc-800 border-l-4 border-white'
            : 'bg-zinc-900 border-zinc-900'
        "
        class="px-5 py-2 rounded-lg w-full border-l-4"
      >
        {{ node.name }}
      </button>
    </li>
  </ul>
</template>

<script setup lang="ts">
defineProps<{
  nodes: ProjectNode[];
  selectedNode: NodeEditView | undefined;
  onNodeClick: (node: ProjectNode) => void;
}>();
</script>
