<template>
  <div class="p-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">จัดการผู้ใช้งาน</h1>
      <button @click="openCreate"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors">
        + เพิ่มผู้ใช้ใหม่
      </button>
    </div>

    <!-- Table -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div v-if="loading" class="text-center text-gray-400 dark:text-gray-500 py-16">กำลังโหลด...</div>
      <table v-else class="w-full text-sm">
        <thead class="bg-gray-50 dark:bg-gray-700/50 border-b border-gray-200 dark:border-gray-700">
          <tr>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">ชื่อผู้ใช้</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">อีเมล</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">แผนก</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">บทบาท</th>
            <th class="text-left px-4 py-3 font-semibold text-gray-600 dark:text-gray-400">สถานะ</th>
            <th class="px-4 py-3"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-gray-700">
          <tr v-if="users.length === 0">
            <td colspan="6" class="text-center text-gray-400 dark:text-gray-500 py-10">ยังไม่มีผู้ใช้งาน</td>
          </tr>
          <tr v-for="u in users" :key="u.id" class="hover:bg-gray-50 dark:hover:bg-gray-700">
            <td class="px-4 py-3 font-medium text-gray-900 dark:text-white">{{ u.username }}</td>
            <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ u.email }}</td>
            <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ u.departmentName }}</td>
            <td class="px-4 py-3">
              <span :class="u.roleName === 'Admin' ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-600'"
                class="px-2 py-0.5 rounded-full text-xs font-semibold">
                {{ u.roleName }}
              </span>
            </td>
            <td class="px-4 py-3">
              <span :class="u.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'"
                class="px-2 py-0.5 rounded-full text-xs font-semibold">
                {{ u.isActive ? 'ใช้งาน' : 'ปิดใช้งาน' }}
              </span>
            </td>
            <td class="px-4 py-3 text-right">
              <button @click="openEdit(u)" class="text-blue-600 hover:text-blue-800 text-xs font-medium">แก้ไข</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal -->
    <div v-if="showModal" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl w-full max-w-md">
        <div class="px-6 py-4 border-b border-gray-100 dark:border-gray-700">
          <h2 class="font-semibold text-gray-900 dark:text-white">{{ editingUser ? 'แก้ไขผู้ใช้งาน' : 'เพิ่มผู้ใช้งานใหม่' }}</h2>
        </div>
        <div class="px-6 py-4 space-y-4">
          <!-- Username (create only) -->
          <div v-if="!editingUser">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">ชื่อผู้ใช้ <span class="text-red-500">*</span></label>
            <input v-model="form.username" type="text" placeholder="ชื่อผู้ใช้ (3-50 ตัวอักษร)" autocomplete="off"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>

          <!-- Email -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">อีเมล <span class="text-red-500">*</span></label>
            <input v-model="form.email" type="email" placeholder="email@example.com" autocomplete="off"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>

          <!-- Password -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              {{ editingUser ? 'รหัสผ่านใหม่ (ถ้าต้องการเปลี่ยน)' : 'รหัสผ่าน' }}
              <span v-if="!editingUser" class="text-red-500">*</span>
            </label>
            <input v-model="form.password" type="password" :placeholder="editingUser ? 'เว้นว่างถ้าไม่ต้องการเปลี่ยน' : 'อย่างน้อย 6 ตัวอักษร'"
              autocomplete="new-password"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>

          <!-- Department -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">แผนก <span class="text-red-500">*</span></label>
            <select v-model="form.departmentId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white">
              <option :value="0">-- เลือกแผนก --</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
            </select>
          </div>

          <!-- Role -->
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">บทบาท <span class="text-red-500">*</span></label>
            <select v-model="form.roleId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white">
              <option :value="0">-- เลือกบทบาท --</option>
              <option v-for="r in roles" :key="r.id" :value="r.id">{{ r.name }}</option>
            </select>
          </div>

          <!-- IsActive (edit only) -->
          <div v-if="editingUser" class="flex items-center gap-3">
            <input id="isActive" v-model="form.isActive" type="checkbox"
              class="w-4 h-4 text-blue-600 rounded border-gray-300 dark:border-gray-600 focus:ring-blue-500" />
            <label for="isActive" class="text-sm font-medium text-gray-700 dark:text-gray-300">เปิดใช้งาน</label>
          </div>

          <p v-if="formError" class="text-sm text-red-500">{{ formError }}</p>
        </div>
        <div class="px-6 py-4 border-t border-gray-100 dark:border-gray-700 flex justify-end gap-3">
          <button @click="closeModal" class="px-4 py-2 text-sm border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
            ยกเลิก
          </button>
          <button @click="submit" :disabled="submitting"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
            {{ submitting ? 'กำลังบันทึก...' : (editingUser ? 'บันทึกการเปลี่ยนแปลง' : 'เพิ่มผู้ใช้') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import client from '@/api/client'

interface User {
  id: number
  username: string
  email: string
  departmentId: number
  departmentName: string
  roleId: number
  roleName: string
  isActive: boolean
  createdAt: string
}

const users = ref<User[]>([])
const departments = ref<{ id: number; name: string }[]>([])
const roles = ref<{ id: number; name: string }[]>([])
const loading = ref(true)

const showModal = ref(false)
const editingUser = ref<User | null>(null)
const submitting = ref(false)
const formError = ref('')

const emptyForm = () => ({ username: '', email: '', password: '', departmentId: 0, roleId: 0, isActive: true })
const form = ref(emptyForm())

onMounted(async () => {
  try {
    const [usersRes, deptRes, roleRes] = await Promise.all([
      client.get('/users'),
      client.get('/departments'),
      client.get('/roles'),
    ])
    users.value = Array.isArray(usersRes.data) ? usersRes.data : (usersRes.data?.value ?? [])
    departments.value = Array.isArray(deptRes.data) ? deptRes.data : (deptRes.data?.value ?? [])
    roles.value = Array.isArray(roleRes.data) ? roleRes.data : (roleRes.data?.value ?? [])
  } finally {
    loading.value = false
  }
})

function openCreate() {
  editingUser.value = null
  form.value = emptyForm()
  formError.value = ''
  showModal.value = true
}

function openEdit(u: User) {
  editingUser.value = u
  form.value = {
    username: u.username,
    email: u.email,
    password: '',
    departmentId: u.departmentId,
    roleId: u.roleId,
    isActive: u.isActive,
  }
  formError.value = ''
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  editingUser.value = null
}

async function submit() {
  formError.value = ''

  if (!editingUser.value) {
    // Create
    if (!form.value.username.trim() || form.value.username.length < 3) {
      formError.value = 'ชื่อผู้ใช้ต้องมีอย่างน้อย 3 ตัวอักษร'; return
    }
    if (!form.value.password || form.value.password.length < 6) {
      formError.value = 'รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร'; return
    }
  }
  if (!form.value.email.includes('@')) {
    formError.value = 'อีเมลไม่ถูกต้อง'; return
  }
  if (!form.value.departmentId) {
    formError.value = 'กรุณาเลือกแผนก'; return
  }
  if (!form.value.roleId) {
    formError.value = 'กรุณาเลือกบทบาท'; return
  }

  submitting.value = true
  try {
    if (editingUser.value) {
      const payload: any = {
        email: form.value.email,
        departmentId: form.value.departmentId,
        roleId: form.value.roleId,
        isActive: form.value.isActive,
      }
      if (form.value.password) payload.newPassword = form.value.password
      const res = await client.put('/users/' + editingUser.value.id, payload)
      const idx = users.value.findIndex(u => u.id === editingUser.value!.id)
      if (idx >= 0) users.value[idx] = res.data
    } else {
      const res = await client.post('/users', {
        username: form.value.username,
        email: form.value.email,
        password: form.value.password,
        departmentId: form.value.departmentId,
        roleId: form.value.roleId,
      })
      users.value.push(res.data)
    }
    closeModal()
  } catch (e: any) {
    formError.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่'
  } finally {
    submitting.value = false
  }
}
</script>
