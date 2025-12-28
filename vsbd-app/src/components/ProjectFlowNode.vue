<template>
  <div
    class="min-w-[220px] rounded-lg border border-zinc-700 bg-zinc-900 text-zinc-100 shadow-lg"
  >
    <div
      class="px-3 py-2 font-semibold text-sm rounded-t-lg select-none"
      :class="headerColor"
    >
      {{ data.label }}
    </div>

    <div class="relative flex p-3 text-xs">
      <div class="relative flex-1">
        <div
          v-for="(input, index) in data.inputs"
          :key="input.id"
          class="relative h-6 flex items-center"
        >
          <Handle
            type="target"
            :id="input.id"
            :position="Position.Left"
            class="pin pin-input"
          />
          <span class="ml-4">{{ input.label }}</span>
        </div>
      </div>

      <div class="relative flex-1 text-right">
        <div
          v-for="(output, index) in data.outputs"
          :key="output.id"
          class="relative h-6 flex items-center justify-end"
        >
          <span class="mr-4">{{ output.label }}</span>
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

type Pin = {
  id: string;
  label: string;
};

const props = defineProps<{
  data: {
    label: string;
    category?: "event" | "logic" | "math";
    inputs: Pin[];
    outputs: Pin[];
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
