<template>
  <div class="p-8">
    <div class="flex items-center justify-between mb-6">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">คำขออนุมัติ</h1>
      <button
        @click="showForm = true"
        class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium rounded-lg transition-colors"
      >
        + สร้างคำขอใหม่
      </button>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 mb-4">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ชื่อคำขอ</label>
          <input
            v-model="filterTitle"
            type="text"
            placeholder="ค้นหาหัวข้อ..."
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          />
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ประเภท</label>
          <select
            v-model="filterTypeId"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">ทั้งหมด</option>
            <option v-for="rt in requestTypes" :key="rt.id" :value="rt.id">{{ rt.name }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">สถานะ</label>
          <select
            v-model="filterStatus"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option value="">ทั้งหมด</option>
            <option v-for="(s, key) in STATUS_MAP" :key="key" :value="key">{{ s.label }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">ช่วงวันที่</label>
          <div class="flex gap-1 items-center">
            <input
              v-model="filterDateFrom"
              type="date"
              class="w-full px-2 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            />
            <span class="text-gray-400 dark:text-gray-500 text-xs shrink-0">ถึง</span>
            <input
              v-model="filterDateTo"
              type="date"
              class="w-full px-2 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
      </div>
      <div class="flex items-center justify-between mt-3">
        <span class="text-xs text-gray-400 dark:text-gray-500">
          แสดง {{ filteredRequests.length }} / {{ requests.length }} รายการ
        </span>
        <button
          v-if="hasActiveFilter"
          @click="clearFilters"
          class="text-xs text-blue-600 hover:text-blue-800 font-medium"
        >
          ล้างตัวกรอง
        </button>
      </div>
    </div>

    <!-- List -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
      <div v-if="loading" class="p-8 text-center text-gray-400 dark:text-gray-500">กำลังโหลด...</div>
      <div v-else-if="filteredRequests.length === 0" class="p-8 text-center text-gray-400 dark:text-gray-500">
        {{ requests.length === 0 ? 'ยังไม่มีคำขอ' : 'ไม่พบรายการที่ตรงกับเงื่อนไข' }}
      </div>
      <table v-else class="w-full text-sm">
        <thead class="border-b border-gray-200 dark:border-gray-700">
          <tr class="text-left text-gray-500 dark:text-gray-400">
            <th class="px-6 py-3 font-medium">หัวข้อ</th>
            <th class="px-6 py-3 font-medium">ประเภท</th>
            <th class="px-6 py-3 font-medium">สถานะ</th>
            <th class="px-6 py-3 font-medium">วันที่</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-gray-700">
          <tr
            v-for="req in filteredRequests"
            :key="req.id"
            class="hover:bg-gray-50 dark:hover:bg-gray-700 cursor-pointer"
            @click="router.push('/requests/' + req.id)"
          >
            <td class="px-6 py-4 font-medium text-gray-900 dark:text-white">{{ req.title }}</td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ req.requestTypeName }}</td>
            <td class="px-6 py-4">
              <span :class="statusClass(req.status)" class="px-2 py-1 rounded-full text-xs font-medium">
                {{ statusLabel(req.status) }}
              </span>
            </td>
            <td class="px-6 py-4 text-gray-500 dark:text-gray-400">{{ formatDate(req.createdAt) }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create Form Modal -->
    <div v-if="showForm" class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl w-full max-w-md p-6">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">สร้างคำขอใหม่</h2>
        <form @submit.prevent="submitRequest" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">หัวข้อ</label>
            <input v-model="form.title" type="text" required
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">รายละเอียด</label>
            <textarea v-model="form.description" rows="3"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"></textarea>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">ประเภทคำขอ</label>
            <select v-model="form.requestTypeId" required
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white">
              <option value="">-- เลือกประเภท --</option>
              <option v-for="rt in requestTypes" :key="rt.id" :value="rt.id">{{ rt.name }}</option>
            </select>
          </div>
          <p v-if="formError" class="text-sm text-red-600">{{ formError }}</p>
          <div class="flex gap-3 pt-2">
            <button type="button" @click="showForm = false"
              class="flex-1 px-4 py-2 border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 text-sm font-medium rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700">
              ยกเลิก
            </button>
            <button type="submit" :disabled="submitting"
              class="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white text-sm font-medium rounded-lg">
              {{ submitting ? 'กำลังส่ง...' : 'สร้างคำขอ' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import client from '@/api/client'

const router = useRouter()

const requests = ref<any[]>([])
const requestTypes = ref<any[]>([])
const loading = ref(true)
const showForm = ref(false)
const submitting = ref(false)
const formError = ref('')
const form = ref({ title: '', description: '', requestTypeId: '' })

// Filters
const filterTitle = ref('')
const filterTypeId = ref<number | ''>('')
const filterStatus = ref<number | ''>('')
const filterDateFrom = ref('')
const filterDateTo = ref('')

const hasActiveFilter = computed(
  () =>
    !!filterTitle.value ||
    filterTypeId.value !== '' ||
    filterStatus.value !== '' ||
    !!filterDateFrom.value ||
    !!filterDateTo.value,
)

const filteredRequests = computed(() =>
  requests.value.filter((req) => {
    if (filterTitle.value && !req.title.toLowerCase().includes(filterTitle.value.toLowerCase()))
      return false
    if (filterTypeId.value !== '' && req.requestTypeId !== Number(filterTypeId.value))
      return false
    if (filterStatus.value !== '' && Number(req.status) !== Number(filterStatus.value))
      return false
    if (filterDateFrom.value) {
      const from = new Date(filterDateFrom.value)
      from.setHours(0, 0, 0, 0)
      if (new Date(req.createdAt) < from) return false
    }
    if (filterDateTo.value) {
      const to = new Date(filterDateTo.value)
      to.setHours(23, 59, 59, 999)
      if (new Date(req.createdAt) > to) return false
    }
    return true
  }),
)

function clearFilters() {
  filterTitle.value = ''
  filterTypeId.value = ''
  filterStatus.value = ''
  filterDateFrom.value = ''
  filterDateTo.value = ''
}

onMounted(async () => {
  await Promise.all([loadRequests(), loadRequestTypes()])
})

async function loadRequests() {
  loading.value = true
  try {
    const res = await client.get('/requests')
    requests.value = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
  } finally {
    loading.value = false
  }
}

async function loadRequestTypes() {
  try {
    const res = await client.get('/request-types')
    requestTypes.value = Array.isArray(res.data) ? res.data : (res.data?.value ?? [])
  } catch {
    requestTypes.value = []
  }
}

async function submitRequest() {
  submitting.value = true
  formError.value = ''
  try {
    await client.post('/requests', {
      title: form.value.title,
      description: form.value.description,
      requestTypeId: Number(form.value.requestTypeId),
    })
    showForm.value = false
    form.value = { title: '', description: '', requestTypeId: '' }
    await loadRequests()
  } catch {
    formError.value = 'เกิดข้อผิดพลาด กรุณาลองใหม่'
  } finally {
    submitting.value = false
  }
}

const STATUS_MAP: Record<number, { label: string; cls: string }> = {
  0: { label: 'ร่าง', cls: 'bg-gray-100 text-gray-600' },
  1: { label: 'รอดำเนินการ', cls: 'bg-blue-100 text-blue-700' },
  2: { label: 'อนุมัติแล้ว', cls: 'bg-green-100 text-green-700' },
  3: { label: 'ปฏิเสธ', cls: 'bg-red-100 text-red-700' },
  4: { label: 'ส่งกลับแก้ไข', cls: 'bg-orange-100 text-orange-700' },
}

function statusClass(status: string | number) {
  return STATUS_MAP[Number(status)]?.cls ?? 'bg-gray-100 text-gray-600'
}

function statusLabel(status: string | number) {
  return STATUS_MAP[Number(status)]?.label ?? String(status)
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('th-TH')
}
</script>
