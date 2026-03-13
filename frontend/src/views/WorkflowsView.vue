<template>
  <div class="p-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Workflow</h1>
      <button @click="openCreate" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors">
        + สร้าง Workflow
      </button>
    </div>

    <div v-if="loading" class="text-center text-gray-400 dark:text-gray-500 py-12">กำลังโหลด...</div>
    <div v-else-if="workflows.length === 0" class="text-center text-gray-400 dark:text-gray-500 py-12">ไม่มีข้อมูล Workflow</div>
    <div v-else class="space-y-4">
      <div v-for="wf in workflows" :key="wf.id" class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
        <div class="flex items-center justify-between p-5">
          <div class="flex items-center gap-3">
            <span :class="wf.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'"
              class="px-2 py-0.5 rounded-full text-xs font-medium">
              {{ wf.isActive ? 'Active' : 'Inactive' }}
            </span>
            <div>
              <p class="font-semibold text-gray-900 dark:text-white">{{ wf.name }}</p>
              <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
                {{ wf.requestTypeName || 'Request Type ID: ' + wf.requestTypeId }} — {{ wf.steps.length }} ขั้นตอน
              </p>
            </div>
          </div>
          <button @click="openEdit(wf)"
            class="px-3 py-1.5 text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white border border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500 rounded-lg transition-colors">
            แก้ไข
          </button>
        </div>
        <div v-if="wf.steps.length" class="border-t border-gray-100 dark:border-gray-700 px-5 pb-4 pt-3">
          <div class="space-y-2">
            <div v-for="step in wf.steps" :key="step.id"
              class="flex items-center gap-3 p-2.5 bg-gray-50 dark:bg-gray-700/50 rounded-lg text-sm">
              <span class="w-6 h-6 bg-blue-600 text-white rounded-full flex items-center justify-center text-xs font-bold shrink-0">
                {{ step.stepOrder }}
              </span>
              <span class="font-medium text-gray-900 dark:text-white">{{ step.stepName }}</span>
              <span class="text-gray-400 dark:text-gray-500">—</span>
              <span class="text-gray-500 dark:text-gray-400">{{ APPROVER_TYPE_LABEL[step.approverType] ?? step.approverType }}</span>
              <span v-if="step.approverValue" class="text-blue-600 font-medium">{{ step.approverValue }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <div v-if="modal.open" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl w-full max-w-lg max-h-[90vh] overflow-y-auto">
        <div class="flex items-center justify-between p-6 border-b border-gray-100 dark:border-gray-700">
          <h2 class="text-base font-semibold text-gray-900 dark:text-white">
            {{ modal.editing ? 'แก้ไข Workflow' : 'สร้าง Workflow ใหม่' }}
          </h2>
          <button @click="closeModal" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 text-xl leading-none">&times;</button>
        </div>
        <form @submit.prevent="save" class="p-6 space-y-5">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">ชื่อ Workflow</label>
            <input v-model="form.name" type="text" required placeholder="เช่น Leave Approval Workflow"
              class="w-full border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Request Type</label>
            <select v-model="form.requestTypeId" required
              class="w-full border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white">
              <option value="" disabled>เลือก Request Type</option>
              <option v-for="rt in requestTypes" :key="rt.id" :value="rt.id">{{ rt.name }}</option>
            </select>
          </div>
          <div>
            <div class="flex items-center justify-between mb-2">
              <label class="text-sm font-medium text-gray-700 dark:text-gray-300">ขั้นตอนการอนุมัติ</label>
              <button type="button" @click="addStep" class="text-xs text-blue-600 hover:text-blue-800 font-medium">
                + เพิ่มขั้นตอน
              </button>
            </div>
            <div class="space-y-3">
              <div v-for="(step, i) in form.steps" :key="i"
                class="border border-gray-200 dark:border-gray-600 rounded-lg p-3 space-y-2 bg-gray-50 dark:bg-gray-700/50">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-gray-500 dark:text-gray-400">ขั้นตอนที่ {{ i + 1 }}</span>
                  <button type="button" @click="removeStep(i)"
                    class="text-xs text-red-400 hover:text-red-600"
                    :disabled="form.steps.length === 1">ลบ</button>
                </div>
                <input v-model="step.stepName" type="text" required placeholder="ชื่อขั้นตอน"
                  class="w-full border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-gray-700 dark:text-white" />
                <select v-model="step.approverType"
                  class="w-full border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-gray-700 dark:text-white">
                  <option value="DepartmentManager">หัวหน้าแผนก (DepartmentManager)</option>
                  <option value="Role">ตาม Role</option>
                  <option value="User">ระบุ User ID</option>
                </select>
                <input v-if="step.approverType !== 'DepartmentManager'"
                  v-model="step.approverValue" type="text" required
                  :placeholder="step.approverType === 'Role' ? 'เช่น Approver, HR' : 'User ID เช่น 2'"
                  class="w-full border border-gray-200 dark:border-gray-600 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white dark:bg-gray-700 dark:text-white" />
              </div>
            </div>
          </div>
          <p v-if="error" class="text-sm text-red-500">{{ error }}</p>
          <div class="flex gap-3 pt-1">
            <button type="button" @click="closeModal"
              class="flex-1 px-4 py-2 border border-gray-200 dark:border-gray-600 text-gray-600 dark:text-gray-300 text-sm rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
              ยกเลิก
            </button>
            <button type="submit" :disabled="saving"
              class="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg transition-colors disabled:opacity-50">
              {{ saving ? 'กำลังบันทึก...' : 'บันทึก' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '@/api/client'

const APPROVER_TYPE_LABEL: Record<string, string> = {
  DepartmentManager: 'หัวหน้าแผนก',
  Role: 'Role',
  User: 'User',
}

const workflows = ref<any[]>([])
const requestTypes = ref<any[]>([])
const loading = ref(true)
const modal = ref({ open: false, editing: false, editId: 0 })
const saving = ref(false)
const error = ref('')

const emptyForm = () => ({
  name: '',
  requestTypeId: '' as number | '',
  steps: [{ stepName: '', approverType: 'DepartmentManager', approverValue: '' }],
})
const form = ref(emptyForm())

async function load() {
  const [wfRes, rtRes] = await Promise.all([
    client.get('/workflows'),
    client.get('/request-types'),
  ])
  workflows.value = Array.isArray(wfRes.data) ? wfRes.data : (wfRes.data?.value ?? [])
  requestTypes.value = Array.isArray(rtRes.data) ? rtRes.data : (rtRes.data?.value ?? [])
}

onMounted(async () => {
  try { await load() } finally { loading.value = false }
})

function openCreate() {
  form.value = emptyForm()
  modal.value = { open: true, editing: false, editId: 0 }
  error.value = ''
}

function openEdit(wf: any) {
  form.value = {
    name: wf.name,
    requestTypeId: wf.requestTypeId,
    steps: wf.steps.map((s: any) => ({
      stepName: s.stepName,
      approverType: s.approverType,
      approverValue: s.approverValue ?? '',
    })),
  }
  modal.value = { open: true, editing: true, editId: wf.id }
  error.value = ''
}

function closeModal() { modal.value.open = false }
function addStep() { form.value.steps.push({ stepName: '', approverType: 'DepartmentManager', approverValue: '' }) }
function removeStep(i: number) { form.value.steps.splice(i, 1) }

async function save() {
  saving.value = true
  error.value = ''
  try {
    const payload = {
      name: form.value.name,
      requestTypeId: Number(form.value.requestTypeId),
      steps: form.value.steps.map((s, i) => ({
        stepOrder: i + 1,
        stepName: s.stepName,
        approverType: s.approverType,
        approverValue: s.approverType === 'DepartmentManager' ? '' : s.approverValue,
      })),
    }
    if (modal.value.editing) {
      await client.put('/workflows/' + modal.value.editId, payload)
    } else {
      await client.post('/workflows', payload)
    }
    closeModal()
    await load()
  } catch (e: any) {
    error.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่'
  } finally {
    saving.value = false
  }
}
</script>
