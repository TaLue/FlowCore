<template>
  <div class="p-8 max-w-3xl">
    <!-- Back -->
    <button @click="router.push('/requests')" class="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 dark:text-gray-400 dark:hover:text-white mb-5">
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
      กลับรายการ
    </button>

    <div v-if="loading" class="text-center text-gray-400 dark:text-gray-500 py-16">กำลังโหลด...</div>
    <div v-else-if="!request" class="text-center text-gray-400 dark:text-gray-500 py-16">ไม่พบคำขอ</div>

    <template v-else>
      <!-- Header Card -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 mb-5">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h1 class="text-xl font-bold text-gray-900 dark:text-white mb-1">{{ request.title }}</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">{{ request.requestTypeName }}</p>
          </div>
          <span :class="statusClass(request.status)" class="px-3 py-1 rounded-full text-xs font-semibold whitespace-nowrap">
            {{ statusLabel(request.status) }}
          </span>
        </div>

        <div class="grid grid-cols-2 gap-4 mt-5 pt-5 border-t border-gray-100 dark:border-gray-700 text-sm">
          <div>
            <p class="text-gray-400 dark:text-gray-500 text-xs mb-0.5">ผู้ขอ</p>
            <p class="font-medium text-gray-800 dark:text-gray-200">{{ request.requesterName }}</p>
          </div>
          <div>
            <p class="text-gray-400 dark:text-gray-500 text-xs mb-0.5">วันที่สร้าง</p>
            <p class="font-medium text-gray-800 dark:text-gray-200">{{ formatDate(request.createdAt) }}</p>
          </div>
          <div v-if="request.updatedAt !== request.createdAt">
            <p class="text-gray-400 dark:text-gray-500 text-xs mb-0.5">แก้ไขล่าสุด</p>
            <p class="font-medium text-gray-800 dark:text-gray-200">{{ formatDate(request.updatedAt) }}</p>
          </div>
          <div>
            <p class="text-gray-400 dark:text-gray-500 text-xs mb-0.5">ขั้นตอนปัจจุบัน</p>
            <p class="font-medium text-gray-800 dark:text-gray-200">{{ request.currentStep > 0 ? 'ขั้นตอนที่ ' + request.currentStep : '-' }}</p>
          </div>
        </div>

        <!-- Actions -->
        <div v-if="canSubmit" class="flex gap-3 mt-5 pt-5 border-t border-gray-100 dark:border-gray-700">
          <button @click="submitRequest" :disabled="acting"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors">
            {{ acting ? 'กำลังส่ง...' : 'ส่งคำขออนุมัติ' }}
          </button>
        </div>
        <p v-if="actionError" class="text-sm text-red-500 mt-3">{{ actionError }}</p>
      </div>

      <!-- Attachments -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6 mb-5">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-sm font-semibold text-gray-700 dark:text-gray-300">ไฟล์แนบ</h2>
          <label class="cursor-pointer text-xs text-blue-600 hover:text-blue-800 font-medium flex items-center gap-1">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            {{ uploading ? 'กำลังอัปโหลด...' : 'อัปโหลดไฟล์' }}
            <input type="file" class="hidden" @change="uploadFile" :disabled="uploading"
              accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip,.txt" />
          </label>
        </div>
        <p v-if="uploadError" class="text-xs text-red-500 mb-3">{{ uploadError }}</p>
        <div v-if="attachments.length === 0" class="text-sm text-gray-400 dark:text-gray-500 py-4 text-center">
          ยังไม่มีไฟล์แนบ
        </div>
        <div v-else class="space-y-2">
          <div v-for="att in attachments" :key="att.id"
            class="flex items-center justify-between p-2.5 rounded-lg bg-gray-50 dark:bg-gray-700/50 text-sm">
            <div class="flex items-center gap-2 min-w-0">
              <svg class="w-4 h-4 text-gray-400 dark:text-gray-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13"/>
              </svg>
              <span class="truncate text-gray-800 dark:text-gray-200">{{ att.fileName }}</span>
              <span class="text-xs text-gray-400 dark:text-gray-500 shrink-0">{{ formatSize(att.fileSize) }}</span>
            </div>
            <a :href="`/api/requests/${route.params.id}/attachments/${att.id}/download`"
              target="_blank"
              class="text-xs text-blue-600 hover:text-blue-800 font-medium shrink-0 ml-2">ดาวน์โหลด</a>
          </div>
        </div>
      </div>

      <!-- Approval History -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-6">
        <h2 class="text-sm font-semibold text-gray-700 dark:text-gray-300 mb-4">ประวัติการอนุมัติ</h2>
        <div v-if="request.approvals.length === 0" class="text-sm text-gray-400 dark:text-gray-500 py-4 text-center">
          ยังไม่มีประวัติการอนุมัติ
        </div>
        <div v-else class="space-y-3">
          <div v-for="a in sortedApprovals" :key="a.id"
            class="flex items-start gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50">
            <div class="w-7 h-7 rounded-full bg-blue-600 text-white flex items-center justify-center text-xs font-bold shrink-0 mt-0.5">
              {{ a.stepOrder }}
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2">
                <span class="text-sm font-medium text-gray-800 dark:text-gray-200">{{ a.approverName }}</span>
                <span v-if="a.action" :class="actionClass(a.action)" class="px-2 py-0.5 rounded-full text-xs font-semibold">
                  {{ actionLabel(a.action) }}
                </span>
                <span v-else class="px-2 py-0.5 rounded-full text-xs font-semibold bg-yellow-100 text-yellow-700">
                  รอดำเนินการ
                </span>
              </div>
              <p v-if="a.comment" class="text-xs text-gray-500 dark:text-gray-400 mt-1">{{ a.comment }}</p>
              <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{{ formatDate(a.createdAt) }}</p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import client from '@/api/client'

const router = useRouter()
const route = useRoute()

const request = ref<any>(null)
const loading = ref(true)
const acting = ref(false)
const actionError = ref('')
const attachments = ref<any[]>([])
const uploading = ref(false)
const uploadError = ref('')

onMounted(async () => {
  try {
    const [reqRes, attRes] = await Promise.all([
      client.get('/requests/' + route.params.id),
      client.get('/requests/' + route.params.id + '/attachments'),
    ])
    request.value = reqRes.data
    attachments.value = Array.isArray(attRes.data) ? attRes.data : (attRes.data?.value ?? [])
  } catch {
    request.value = null
  } finally {
    loading.value = false
  }
})

const canSubmit = computed(() => {
  const s = request.value?.status
  return s === 0 || s === 4
})

const sortedApprovals = computed(() =>
  [...(request.value?.approvals ?? [])].sort((a, b) => a.stepOrder - b.stepOrder)
)

async function uploadFile(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploadError.value = ''
  if (file.size > 10 * 1024 * 1024) {
    uploadError.value = 'ไฟล์ขนาดเกิน 10 MB'; return
  }
  uploading.value = true
  try {
    const fd = new FormData()
    fd.append('file', file)
    const res = await client.post('/requests/' + route.params.id + '/attachments', fd, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    attachments.value.push(res.data)
  } catch (e: any) {
    uploadError.value = e.response?.data?.message ?? 'อัปโหลดไม่สำเร็จ'
  } finally {
    uploading.value = false
    input.value = ''
  }
}

function formatSize(bytes: number) {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / 1024 / 1024).toFixed(1) + ' MB'
}

async function submitRequest() {
  acting.value = true
  actionError.value = ''
  try {
    const res = await client.post('/requests/' + route.params.id + '/submit')
    request.value = res.data
  } catch (e: any) {
    actionError.value = e.response?.data?.message ?? 'เกิดข้อผิดพลาด'
  } finally {
    acting.value = false
  }
}

const STATUS_MAP: Record<number, { label: string; cls: string }> = {
  0: { label: 'ร่าง', cls: 'bg-gray-100 text-gray-600' },
  1: { label: 'รอดำเนินการ', cls: 'bg-blue-100 text-blue-700' },
  2: { label: 'อนุมัติแล้ว', cls: 'bg-green-100 text-green-700' },
  3: { label: 'ปฏิเสธ', cls: 'bg-red-100 text-red-700' },
  4: { label: 'ส่งกลับแก้ไข', cls: 'bg-orange-100 text-orange-700' },
}

function statusClass(s: string | number) {
  return STATUS_MAP[Number(s)]?.cls ?? 'bg-gray-100 text-gray-600'
}
function statusLabel(s: string | number) {
  return STATUS_MAP[Number(s)]?.label ?? String(s)
}

const ACTION_MAP: Record<string, { label: string; cls: string }> = {
  Approve: { label: 'อนุมัติ', cls: 'bg-green-100 text-green-700' },
  Reject: { label: 'ปฏิเสธ', cls: 'bg-red-100 text-red-700' },
  Return: { label: 'ส่งกลับ', cls: 'bg-orange-100 text-orange-700' },
}

function actionClass(a: string) {
  return ACTION_MAP[a]?.cls ?? 'bg-gray-100 text-gray-600'
}
function actionLabel(a: string) {
  return ACTION_MAP[a]?.label ?? a
}

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('th-TH', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>
