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

      <template v-if="node.isInstance">
        <div
          v-for="(property, i) in node.properties"
          :key="`property-input-${i}`"
          class="mb-3"
        >
          <label class="mb-1 block text-xs text-zinc-500">
            {{ property.name }}
            <span class="text-zinc-600">({{ property.type }})</span>
          </label>

          <input
            v-model="property.value"
            class="w-full rounded-lg border border-zinc-800 bg-zinc-900 px-3 py-2 text-sm text-zinc-100 outline-none focus:border-indigo-500"
            :placeholder="property.name"
            :value="property.value"
          />
        </div>

        <p v-if="!node.properties.length" class="text-xs text-zinc-600 italic">
          No properties
        </p>

        <button
          class="mt-4 rounded-xl bg-indigo-600 px-5 py-2 text-sm font-semibold text-white hover:bg-indigo-500 active:scale-95 transition"
          @click="onPropertySave(node.properties, node.id!)"
        >
          Save
        </button>
      </template>

      <template v-else>
        <div
          v-for="(property, i) in node.properties"
          :key="`property-${i}`"
          class="flex justify-between text-sm text-zinc-400 py-1 border-b border-zinc-800"
        >
          <span>{{ property.name }}</span>
          <span class="text-zinc-500">{{ property.type }}</span>
        </div>

        <p v-if="!node.properties.length" class="text-xs text-zinc-600 italic">
          No properties
        </p>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  node: NodeEditView;
  isInstance: boolean;
  onPropertySave: (properties: NodeProperty[], id: string) => void;
}>();
</script>
