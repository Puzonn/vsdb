<template>
  <div class="h-full w-full bg-zinc-950 rounded-lg p-4 overflow-y-auto">
    <div class="mb-6">
      <h2 class="text-sm font-semibold text-zinc-300 mb-2">Inputs</h2>

      <div
        v-for="(input, i) in node.inputs"
        :key="`input-${i}`"
        class="flex justify-between text-sm text-zinc-400 py-1 border-b border-zinc-800"
      >
        <span>{{ input.name }}</span>
        <span class="text-zinc-500">{{ input.type }}</span>
      </div>

      <p v-if="!node.inputs.length" class="text-xs text-zinc-600 italic">
        No inputs
      </p>
    </div>

    <div class="mb-6">
      <h2 class="text-sm font-semibold text-zinc-300 mb-2">Outputs</h2>

      <div
        v-for="(output, i) in node.outputs"
        :key="`output-${i}`"
        class="flex justify-between text-sm text-zinc-400 py-1 border-b border-zinc-800"
      >
        <span>{{ output.name }}</span>
        <span class="text-zinc-500">{{ output.type }}</span>
      </div>

      <p v-if="!node.outputs.length" class="text-xs text-zinc-600 italic">
        No outputs
      </p>
    </div>

    <div>
      <h2 class="text-sm font-semibold text-zinc-300 mb-2">Properties</h2>

      <div
        v-for="(prop, i) in node.nodeProperties"
        :key="`prop-${i}`"
        class="flex flex-col gap-1 py-2 border-b border-zinc-800"
      >
        <div class="flex justify-between text-sm text-zinc-400">
          <span>{{ prop.name }}</span>
          <span class="text-zinc-500">{{ prop.type }}</span>
        </div>

        <input
          v-if="prop.defaultValue !== undefined"
          v-model="prop.defaultValue"
          class="bg-zinc-900 border border-zinc-800 rounded px-2 py-1 text-sm text-zinc-200"
          placeholder="Default value"
        />
      </div>

      <p v-if="node.nodeProperties.length" class="text-xs text-zinc-600 italic">
        No properties
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  node: ProjectNode;
}>();
</script>
