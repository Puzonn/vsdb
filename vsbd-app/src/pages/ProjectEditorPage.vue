<template>
  <div
    class="h-screen w-screen bg-zinc-900 p-4 flex justify-center flex-col gap-5"
  >
    <h1>Project Id: {{ projectId }}</h1>

    <div
      v-if="selectedNode"
      class="mb-3 inline-flex w-full rounded-lg border border-zinc-800 overflow-hidden"
    >
      <button
        v-on:click="switchDisplay('properties')"
        class="flex-1 p-4 flex justify-center items-center font-semibold"
      >
        Proporties
      </button>

      <button
        v-on:click="switchDisplay('code')"
        class="flex-1 p-4 flex justify-center items-center font-semibold"
      >
        Code
      </button>
      <button
        v-on:click="switchDisplay('flow')"
        class="flex-1 p-4 flex justify-center items-center font-semibold"
      >
        Flow
      </button>
    </div>

    <div class="flex flex-row h-full w-full gap-5">
      <NodeList
        :nodes="nodes"
        :selected-node="selectedNode"
        v-on:node-click="onNodeClick"
      ></NodeList>

      <div class="flex-1 h-full">
        <textarea
          v-if="display == 'code' && selectedNode"
          v-model="selectedNode.sourceCode"
          class="h-full w-full resize-none bg-zinc-950 text-zinc-100 font-mono p-4 rounded-lg outline-none text-base"
          spellcheck="false"
        />

        <div
          v-if="display == 'properties' && selectedNode"
          class="h-full w-full bg-zinc-950 rounded-lg p-4"
        >
          <NodeProperties :node="selectedNode"></NodeProperties>
        </div>

        <div
          v-if="display == 'flow' && selectedNode"
          class="h-full w-full bg-zinc-950 rounded-lg p-4"
        >
          <ProjectFlow
            v-if="display === 'flow' && selectedNode"
            v-model:nodes="flowNodes"
            v-model:edges="flowEdges"
          ></ProjectFlow>
        </div>
      </div>
    </div>

    <div class="flex justify-center gap-5">
      <button
        v-if="projectId == 'undefined'"
        class="text-white bg-zinc-950 px-5 py-2 font-bold rounded-lg"
        v-on:click="createProject"
      >
        Create Project
      </button>
      <button
        v-if="projectId !== 'undefined'"
        class="text-white bg-zinc-950 px-5 py-2 font-bold rounded-lg"
        v-on:click="sendProject"
      >
        Send Project
      </button>
      <button
        v-if="projectId !== 'undefined'"
        class="text-white bg-zinc-950 px-5 py-2 font-bold rounded-lg"
        v-on:click="compileProject"
      >
        Compile Project
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import NodeList from "../components/NodeList.vue";
import NodeProperties from "../components/NodeProperties.vue";
import ProjectFlow from "../components/ProjectFlow.vue";
import type { Edge, Node } from "@vue-flow/core";

const projectId = ref("undefined");
const nodes = ref<ProjectNode[]>([]);
const selectedNode = ref<ProjectNode | undefined>(undefined);
const display = ref<"properties" | "code" | "flow">("code");
const flowEdges = ref<Edge[]>([]);
const flowNodes = ref<Node[]>([
  {
    id: "event-1",
    type: "visual",
    position: { x: 100, y: 100 },
    data: {
      label: "On Start 1",
      category: "event",
      inputs: [
        { id: "in-exec-0", label: "Exec" },
        { id: "in-exec-1", label: "Exec" },
      ],
      outputs: [
        { id: "out-exec-0", label: "Exec" },
        { id: "out-exec-1", label: "Exec" },
      ],
    },
  },
  {
    id: "event-2",
    type: "visual",
    position: { x: 400, y: 100 },
    data: {
      label: "On Start",
      category: "event",
      inputs: [
        { id: "in-exec-0", label: "Exec" },
        { id: "in-exec-1", label: "Exec" },
      ],
      outputs: [
        { id: "out-exec-0", label: "Exec" },
        { id: "out-exec-1", label: "Exec" },
      ],
    },
  },
]);

onMounted(async () =>  {
  const response = await fetch("http://localhost:5020/project/all")
})

async function createProject() {
  const response = await fetch("http://localhost:5020/project/create", {
    method: "POST",
  });

  if (response.status == 200) {
    const result = (await response.json()) as ProjectCreationResponse;

    if (!result) {
      console.error(result);
      return;
    }

    projectId.value = result.projectId;
    nodes.value =
      result.nodes?.map((n) => ({
        ...n,
        inputs: n.inputs ?? [],
        outputs: n.outputs ?? [],
        nodeProperties: n.nodeProperties ?? [],
      })) ?? [];
  }
}

function onNodeClick(node: ProjectNode) {
  if (display.value == "flow") {
    const newNode = {
      id: crypto.randomUUID(),
      type: "visual",
      position: { x: 0, y: 0 },
      data: {
        label: node.name,
        category: "event",
        inputs: node.inputs.map((e) => {
          return {
            id: crypto.randomUUID(),
            label: e.name,
          };
        }),
        outputs: node.outputs.map((e) => {
          return {
            id: crypto.randomUUID(),
            label: e.name,
          };
        }),
      },
    };
    flowNodes.value = [...flowNodes.value, newNode];

    console.log(newNode);
  } else {
    selectedNode.value = node;
  }
}

function switchDisplay(state: "properties" | "code" | "flow") {
  display.value = state;
}

async function sendProject() {
  const response = await fetch(
    `http://localhost:5020/project/send/${projectId.value}`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(
        nodes.value.map((node) => ({
          name: node.name,
          sourceCode: node.sourceCode,
        }))
      ),
    }
  );
}

async function compileProject() {
  const response = await fetch(
    `http://localhost:5020/project/compile/${projectId.value}`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  const result = (await response.json()) as ProjectRunResult;

  if (!result) {
    console.error(result);
    return;
  }

  nodes.value =
    result.nodes?.map((n) => ({
      ...n,
      inputs: n.inputs ?? [],
      outputs: n.outputs ?? [],
      nodeProperties: n.nodeProperties ?? [],
    })) ?? [];
}
</script>
