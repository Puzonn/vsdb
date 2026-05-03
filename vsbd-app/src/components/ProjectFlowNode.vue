<template>
  <div
    class="relative min-w-[220px] rounded-lg border border-zinc-700 bg-zinc-900 text-zinc-100 shadow-lg"
  >
    <button
      v-on:click="data.onDeleteClicked(data.id)"
      class="absolute right-2 top-2 z-10 flex h-6 w-6 items-center justify-center rounded-md bg-zinc-800 text-zinc-400 hover:bg-opacity-50 hover:text-white transition"
    >
      X
    </button>
    <button
      v-on:click="data.onSettingsClicked(data.id)"
      class="absolute right-10 top-2 z-10 flex h-6 w-6 items-center justify-center rounded-md bg-zinc-800 text-zinc-400 hover:bg-opacity-50 hover:text-white transition"
    >
      ⚙
    </button>

    <div
      class="px-3 py-2 pr-10 font-semibold text-sm rounded-t-lg select-none"
      :class="headerColor"
    >
      {{ data.name }}
    </div>

    <div class="relative flex p-3 text-xs">
      <div class="relative flex-1">
        <div
          v-for="(input, i) in data.inputs"
          :key="input.id"
          class="relative h-6 flex items-center"
        >
          <Handle
            type="target"
            :id="input.id"
            :position="Position.Left"
            class="pin pin-input"
          />
          <span class="ml-4">{{ input.name }}</span>
        </div>
      </div>

      <div class="relative flex-1 text-right">
        <div
          v-for="(output, index) in data.outputs"
          :key="output.id"
          class="relative h-6 flex items-center justify-end"
        >
          <span class="mr-4">{{ output.name }}</span>
          <Handle
            type="source"
            :id="output.id"
            :position="Position.Right"
            class="pin pin-output"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Handle, Position } from "@vue-flow/core";
import { computed } from "vue";

const props = defineProps<{
  data: {
    id: string;
    name: string;
    category?: "event" | "logic" | "math";
    inputs: NodeInput[];
    outputs: NodeOutput[];
    onDeleteClicked: (id: string) => void;
    onSettingsClicked: (id: string) => void;
  };
}>();

const headerColor = computed(() => {
  switch (props.data.category) {
    case "event":
      return "bg-emerald-700";
    case "logic":
      return "bg-indigo-700";
    case "math":
      return "bg-orange-700";
    default:
      return "bg-zinc-700";
  }
});
</script>

<style scoped>
.pin {
  width: 10px;
  height: 10px;
  border-radius: 9999px;
}

.pin-input {
  background: rgb(59, 130, 246);
}

.pin-output {
  background: rgb(168, 85, 247);
}
</style>
