<template>
  <ul class="bg-zinc-950 rounded-lg overflow-scroll w-full shrink-0">
    <div class="flex justify-center font-semibold p-3">
      <p>Projects</p>
    </div>

    <li v-for="project in projects" class="px-4 py-2 text-center">
      <button
        v-on:click="onProjectClick(project)"
        class="px-5 py-2 rounded-lg w-full bg-zinc-900 hover:bg-opacity-90"
      >
        {{ project.id }} - {{ project.createdAt }}
      </button>
    </li>
  </ul>
</template>

<script setup lang="ts">
defineProps<{
  projects: Project[];
  onProjectClick: (project: Project) => void;
}>();
</script>
