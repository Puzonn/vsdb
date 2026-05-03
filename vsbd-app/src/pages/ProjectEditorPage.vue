<template>
  <div class="min-h-screen w-screen bg-zinc-950 text-zinc-100 p-4">
    <div
      class="mx-auto flex h-[calc(100vh-2rem)] max-w-[1800px] flex-col gap-4"
    >
      <header
        class="flex items-center justify-between rounded-2xl border border-zinc-800 bg-zinc-900/80 px-5 py-4 shadow-lg"
      >
        <div>
          <p class="text-xs uppercase tracking-widest text-zinc-500">Project</p>
          <h1 class="text-xl font-bold">
            {{ projectId === "undefined" ? "New Project" : projectId }}
          </h1>
        </div>

        <div class="flex gap-3">
          <button
            v-if="projectId === 'undefined'"
            class="rounded-xl bg-indigo-600 px-5 py-2 font-semibold text-white hover:bg-indigo-500 active:scale-95 transition"
            @click="createProject"
          >
            Create Project
          </button>

          <template v-else>
            <button
              class="rounded-xl bg-zinc-800 px-5 py-2 font-semibold hover:bg-zinc-700 active:scale-95 transition"
              @click="saveProject"
            >
              Save
            </button>

            <button
              class="rounded-xl bg-emerald-600 px-5 py-2 font-semibold text-white hover:bg-emerald-500 active:scale-95 transition"
              @click="compileProject"
            >
              Compile
            </button>
            <button
              class="rounded-xl bg-blue-600 px-5 py-2 font-semibold text-white hover:bg-blue-500 active:scale-95 transition"
              @click="start"
            >
              Start
            </button>
          </template>
        </div>
      </header>

      <div class="grid min-h-0 flex-1 grid-cols-[320px_1fr] gap-4">
        <aside
          class="min-h-0 rounded-2xl border border-zinc-800 bg-zinc-900/70 p-3 shadow-lg"
        >
          <div class="mb-3 flex items-center justify-between px-2">
            <h2 class="font-semibold">Nodes</h2>
            <span
              class="rounded-full bg-zinc-800 px-2 py-1 text-xs text-zinc-400"
            >
              {{ projectNodes.length }}
            </span>
          </div>

          <NodeList
            :nodes="projectNodes"
            :selected-node="selectedNode"
            @node-click="onNodeClick"
          />
        </aside>

        <main
          class="min-h-0 rounded-2xl border border-zinc-800 bg-zinc-900/70 shadow-lg overflow-hidden"
        >
          <div class="flex h-full flex-col">
            <div
              class="flex items-center justify-between border-b border-zinc-800 px-4 py-3"
            >
              <div class="inline-flex rounded-xl bg-zinc-950 p-1">
                <button
                  v-for="tab in tabs"
                  :key="tab.value"
                  class="rounded-lg px-5 py-2 text-sm font-semibold transition"
                  :class="
                    display === tab.value
                      ? 'bg-zinc-800 text-white shadow'
                      : 'text-zinc-400 hover:text-white hover:bg-zinc-900'
                  "
                  @click="switchDisplay(tab.value)"
                >
                  {{ tab.label }}
                </button>
              </div>

              <div v-if="selectedNode" class="text-sm text-zinc-400">
                Selected:
                <span class="font-semibold text-zinc-200">
                  {{ selectedNode.name }}
                </span>
              </div>
            </div>

            <div class="min-h-0 flex-1 p-4">
              <div
                v-if="display === 'projects'"
                class="h-full w-full overflow-auto rounded-xl border border-zinc-800 bg-zinc-950 p-5"
              >
                <ProjectList
                  :projects="projects"
                  v-on:project-click="onProjectClick"
                />
              </div>

              <div
                v-if="!selectedNode && display !== 'flow'"
                class="flex h-full items-center justify-center rounded-xl border border-dashed border-zinc-800 bg-zinc-950 text-zinc-500"
              >
                Select a node from the left panel
              </div>

              <textarea
                v-else-if="display === 'code' && selectedNode"
                v-model="selectedNode.sourceCode"
                class="h-full w-full resize-none rounded-xl border border-zinc-800 bg-zinc-950 p-5 font-mono text-sm leading-6 text-zinc-100 outline-none focus:border-indigo-500"
                spellcheck="false"
              />

              <div
                v-else-if="display === 'code' && !selectedNode"
                class="flex h-full items-center justify-center rounded-xl border border-dashed border-zinc-800 bg-zinc-950 text-zinc-500"
              >
                Select a library node to edit code
              </div>

              <div
                v-else-if="display === 'properties' && selectedNode"
                class="h-full w-full overflow-auto rounded-xl border border-zinc-800 bg-zinc-950 p-5"
              >
                <NodeProperties
                  :node="selectedNode"
                  :is-instance="!!selectedNode"
                  @property-save="onPropertySave"
                />
              </div>

              <div
                v-else-if="display === 'flow'"
                class="h-full w-full overflow-hidden rounded-xl border border-zinc-800 bg-zinc-950"
              >
                <ProjectFlow
                  :nodes="flowNodes"
                  :edges="flowEdges"
                  v-on:connect="onFlowConnect"
                />
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as signalR from "@microsoft/signalr";
import { computed, onMounted, ref, toRaw } from "vue";
import NodeList from "../components/NodeList.vue";
import NodeProperties from "../components/NodeProperties.vue";
import ProjectFlow from "../components/ProjectFlow.vue";
import type { Connection, Edge, Node } from "@vue-flow/core";
import { flowNodeToEditView, projectNodeToEditView } from "../utils/NodeUtils";
import ProjectList from "../components/ProjectList.vue";

type Display = "properties" | "code" | "flow" | "projects";

const tabs = [
  { label: "Properties", value: "properties" as const },
  { label: "Code", value: "code" as const },
  { label: "Flow", value: "flow" as const },
  { label: "Projects", value: "projects" as const },
];

const projectId = ref("undefined");

const projectNodes = ref<ProjectNode[]>([]);
const flowNodes = ref<Node<FlowNode>[]>([]);
const projects = ref<Project[]>([]);

const flowEdges = computed<Edge[]>(() => {
  return flowNodes.value.flatMap((node) =>
    node.data!.connections.map((connection) => ({
      id: connection.id,
      source: connection.sourceId,
      target: connection.targetId,
      sourceHandle: connection.sourceHandleId,
      targetHandle: connection.targetHandleId,
    })),
  );
});

const selectedNode = ref<NodeEditView | undefined>(undefined);
const display = ref<Display>("code");

function clone<T>(value: T): T {
  return JSON.parse(JSON.stringify(toRaw(value)));
}

onMounted(async () => {
  const response = await fetch("http://localhost:5020/project/all");
  const result = (await response.json()) as Project[];

  projects.value = result;
});

async function onProjectClick(project: Project) {
  const response = await fetch(`http://localhost:5020/project/${project.id}`);

  if (!response.ok) {
    console.error("Failed to load project", response.status);
    return;
  }

  const result = (await response.json()) as ProjectLoadResponse;
  projectId.value = result.id;
  projectNodes.value = result.nodes;

  flowNodes.value = (result.flowNodes ?? []).map((node) => {
    const projectNode = result.nodes.find((x) => x.name == node.name)!;
    const nodeConnections = (result.flowEdges ?? []).filter(
      (edge) => edge.sourceId === node.id,
    );

    return {
      id: node.id,
      type: "visual",
      position: {
        x: node.position?.x ?? 100,
        y: node.position?.y ?? 100,
      },
      data: {
        id: node.id,
        name: node.name,
        inputs: projectNode.inputs,
        position: { x: 0, y: 0 },
        outputs: projectNode.outputs,
        properties: node.properties ?? [],
        connections: nodeConnections.map((edge) => ({
          id: edge.id,
          sourceId: edge.sourceId,
          targetId: edge.targetId,
          sourceHandleId: edge.sourceHandleId ?? null,
          targetHandleId: edge.targetHandleId ?? null,
        })),
        onDeleteClicked: () => onNodeRemove(node.id),
        onSettingsClicked: () => onPropertyChange(node.id),
      },
    };
  });

  console.log("edges: ", flowEdges);
  console.log("nodes: ", flowNodes);
}

async function start() {
  const hub = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5020/flow")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();

  await hub.start();

  await hub.invoke("StartFlow", projectId.value)
}

async function createProject() {
  const response = await fetch("http://localhost:5020/project/create", {
    method: "POST",
  });

  if (!response.ok) {
    console.error("Failed to create project", response.status);
    return;
  }

  const result = (await response.json()) as ProjectCreationResponse;

  projectId.value = result.projectId;

  projectNodes.value =
    result.nodes?.map((node) => ({
      ...node,
      inputs: node.inputs ?? [],
      outputs: node.outputs ?? [],
      properties: node.properties ?? [],
    })) ?? [];
}

function onNodeClick(node: ProjectNode) {
  selectedNode.value = undefined;

  if (display.value !== "flow") {
    selectedNode.value = projectNodeToEditView(node);
    return;
  }

  /* Create node instance */

  const id = crypto.randomUUID();

  const flowNodeData: FlowNode = {
    id,
    position: { x: 0, y: 0 },
    name: node.name,
    properties: clone(node.properties),
    inputs: clone(node.inputs),
    outputs: clone(node.outputs),
    onDeleteClicked: () => onNodeRemove(id),
    onSettingsClicked: () => onPropertyChange(id),
    connections: [],
  };

  for (const input in flowNodeData.inputs) {
    input;
  }

  const newNode: Node<FlowNode> = {
    id,
    type: "visual",
    position: { x: 100, y: 100 },
    data: flowNodeData,
  };

  flowNodes.value = [...flowNodes.value, newNode];
}

function onNodeRemove(id: string) {
  flowNodes.value = flowNodes.value
    .filter((node) => node.id !== id)
    .map((node) => ({
      ...node,
      data: {
        ...node.data!,
        connections: node.data!.connections.filter(
          (connection) =>
            connection.sourceId !== id && connection.targetId !== id,
        ),
      },
    }));

  if (selectedNode.value?.id === id) {
    selectedNode.value = undefined;
  }
}

function onPropertyChange(id: string) {
  const node = flowNodes.value.find((node) => node.id === id);
  const data = node?.data;

  if (!node || !data) {
    return;
  }

  const flowNode: FlowNode = {
    position: { x: 0, y: 0 },
    id: data.id,
    name: data.name,
    outputs: data.outputs,
    inputs: data.inputs,
    properties: data.properties,
    onDeleteClicked: data.onDeleteClicked,
    onSettingsClicked: data.onSettingsClicked,
    connections: data.connections,
  };

  const editNode = flowNodeToEditView(flowNode);
  editNode.onPropertySave = () => onPropertyChange(data.id);

  selectedNode.value = editNode;

  switchDisplay("properties");
}

function onFlowConnect(connection: Connection) {
  const sourceNode = flowNodes.value.find(
    (node) => node.id === connection.source,
  );

  if (!sourceNode || !connection.source || !connection.target) {
    return;
  }

  console.log(connection);

  sourceNode.data!.connections.push({
    id: crypto.randomUUID(),
    sourceId: connection.source,
    targetId: connection.target,
    sourceHandleId: connection.sourceHandle,
    targetHandleId: connection.targetHandle,
  });
}

function onPropertySave(properties: NodeProperty[], id: string) {
  const node = flowNodes.value.find((node) => node.id === id);

  if (!node || !node.data) {
    return;
  }

  node.data.properties = clone(properties);

  if (selectedNode.value?.id === id) {
    selectedNode.value.properties = clone(properties);
  }
}

function switchDisplay(state: Display) {
  display.value = state;
}

async function saveProject() {
  await fetch(`http://localhost:5020/project/save/${projectId.value}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      libraries: projectNodes.value.map((node) => ({
        name: node.name,
        sourceCode: node.sourceCode,
      })),
      flowNodes: flowNodes.value.map((node) => ({
        id: node.id,
        type: node.type,
        position: node.position,
        name: node.data!.name,
        properties: node.data!.properties,
      })),
      flowEdges: flowEdges.value.map(
        (x): FlowConnection => ({
          id: x.id,
          sourceId: x.source,
          targetId: x.target,
          sourceHandleId: x.sourceHandle,
          targetHandleId: x.targetHandle,
        }),
      ),
    }),
  });
}

async function compileProject() {
  const response = await fetch(
    `http://localhost:5020/project/compile/${projectId.value}`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    },
  );

  if (!response.ok) {
    console.error("Failed to compile project", response.status);
    return;
  }

  const result = (await response.json()) as ProjectRunResult;

  if (!result.success) {
    console.error(result.errors);
    return;
  }

  projectNodes.value =
    result.nodes?.map((node) => ({
      ...node,
      inputs: node.inputs ?? [],
      outputs: node.outputs ?? [],
      properties: node.properties ?? [],
    })) ?? [];
}
</script>
