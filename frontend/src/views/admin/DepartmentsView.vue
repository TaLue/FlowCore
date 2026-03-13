<template>
  <div class="p-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">จัดการแผนก</h1>
      <button @click="openCreate"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors">
        + เพิ่มแผนกใหม่
      </button>
    </div>

    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div v-if="loading" class="text-center text-gray-400 dark:text-gray-500 py-16">กำลังโหลด...</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 dark:bg-gray-700/50 border-b border-gray-200 dark:border-gray-700">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">ชื่อแผนก</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">ผู้จัดการแผนก</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">จำนวนผู้ใช้</th>
            <th class="px-4 py-3"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-gray-700">
          <tr v-if="departments.length === 0">
            <td colspan="4" class="text-center text-gray-400 dark:text-gray-500 py-10">ยังไม่มีแผนก</td>
          </tr>
          <tr v-for="d in departments" :key="d.id" class="hover:bg-gray-50 dark:hover:bg-gray-700">
            <td class="px-4 py-3 font-medium text-gray-900 dark:text-white">{{ d.name }}</td>
            <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ d.managerName || '-' }}</td>
            <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ d.userCount }} คน</td>
            <td class="px-4 py-3 text-right">
              <button @click="openEdit(d)" class="text-blue-600 hover:text-blue-800 text-xs font-medium">แก้ไข</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl w-full max-w-sm">
        <div class="px-6 py-4 border-b border-gray-100 dark:border-gray-700">
          <h2 class="font-semibold text-gray-900 dark:text-white">{{ editingDept ? 'แก้ไขแผนก' : 'เพิ่มแผนกใหม่' }}</h2>
        </div>
        <div class="px-6 py-4 space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">ชื่อแผนก <span class="text-red-500">*</span></label>
            <input v-model="form.name" type="text" placeholder="ชื่อแผนก" autocomplete="off"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">ผู้จัดการแผนก</label>
            <select v-model="form.managerId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white">
              <option :value="null">-- ไม่ระบุ --</option>
              <option v-for="u in users" :key="u.id" :value="u.id">{{ u.username }}</option>
            </select>
          </div>
          <p v-if="formError" class="text-sm text-red-500">{{ formError }}</p>
        </div>
        <div class="px-6 py-4 border-t border-gray-100 dark:border-gray-700 flex justify-end gap-3">
          <button @click="closeModal" class="px-4 py-2 text-sm border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
            ยกเลิก
          </button>
          <button @click="submit" :disabled="submitting"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
            {{ submitting ? 'กำลังบันทึก...' : (editingDept ? 'บันทึก' : 'เพิ่มแผนก') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '@/api/client'

interface Department {
  id: number
  name: string
  managerId: number | null
  managerName: string
  userCount: number
}

const departments = ref<Department[]>([])
const users = ref<{ id: number; username: string }[]>([])
const loading = ref(true)

const showModal = ref(false)
const editingDept = ref<Department | null>(null)
const submitting = ref(false)
const formError = ref('')

const emptyForm = () => ({ name: '', managerId: null as number | null })
const form = ref(emptyForm())

onMounted(async () => {
  try {
    const [deptRes, userRes] = await Promise.all([
      client.get('/departments'),
      client.get('/users'),
    ])
    departments.value = Array.isArray(deptRes.data) ? deptRes.data : (deptRes.data?.value ?? [])
    users.value = Array.isArray(userRes.data) ? userRes.data : (userRes.data?.value ?? [])
  } finally {
    loading.value = false
  }
})

function openCreate() {
  editingDept.value = null
  form.value = emptyForm()
  formError.value = ''
  showModal.value = true
}

function openEdit(d: Department) {
  editingDept.value = d
  form.value = { name: d.name, managerId: d.managerId }
  formError.value = ''
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingDept.value = null
}

async function submit() {
  formError.value = ''
  if (!form.value.name.trim()) {
    formError.value = 'กรุณาระบุชื่อแผนก'; return
  }

  submitting.value = true
  try {
    const payload = { name: form.value.name.trim(), managerId: form.value.managerId || null }

    if (editingDept.value) {
      const res = await client.put('/departments/' + editingDept.value.id, payload)
      const idx = departments.value.findIndex(d => d.id === editingDept.value!.id)
      if (idx >= 0) departments.value[idx] = res.data
    } else {
      const res = await client.post('/departments', payload)
      departments.value.push(res.data)
    }
    closeModal()
  } catch (e: any) {
    formError.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่'
  } finally {
    submitting.value = false
  }
}
</script>
